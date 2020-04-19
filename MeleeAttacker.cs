using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class MeleeAttacker : MonoBehaviour {

    [Header("Internal")]
    public bool unlocked = true;

    public string internalName;

    public float dropWeight;

    [Space(20f)]
    public string displayedName;

    [Header("Main Stats")]
    public float baseDamage;

    public float durationBeforeSwing;

    public float attackWaveDuration;

    public float waitAfterAttDuration;

    public float zoomMultiplier = 1f;

    public float knockbackMultiplier = 1f;

    [Header("Special Effects")]
    public float burnChance = 0f;

    public float chillChance = 0f;

    public float bleedChance = 0f;

    public bool allowMultipleHitsOnSameEnemy = false;

    [Space(10f)]
    [Header("Attack Wave (functional)")]
    public GameObject attackWave;

    [Space(10f)]
    public Transform[] attackWaveSpawns;

    [Space(10f)]
    [Header("Weapon Swing (mostly cosmetic)")]
    public Transform swingStartPoint;

    public Transform swingEndPoint;

    [HideInInspector]
    public int spawnedID;

    [System.NonSerialized]
    public float minRangeToBePicked = Mathf.Infinity;

    [System.NonSerialized]
    public float maxRangeToBePicked = Mathf.NegativeInfinity;

    [System.NonSerialized]
    public bool attacking = false;

    [HideInInspector]
    public float damage;

    [HideInInspector]
    public PlayerController playerController;

    [System.NonSerialized]
    public bool flipped;

    [System.NonSerialized]
    public Vector2 defaultLocalPos;

    [System.NonSerialized]
    public Quaternion defaultLocalRotation;

    AudioSource swingSound;

    [System.NonSerialized]
    public int moveOnGroundId;

    Vector2 startingLocalScale = new Vector2(1f, 1f);

    Transform parentBoxTransform;

    [System.NonSerialized]
    public float defaultSwingSoundPitch;

    [System.NonSerialized]
    public float defaultDurationBeforeSwing;

    [System.NonSerialized]
    public float defaultWaitAfterAttDuration;

    [HideInInspector]
    public PolygonCollider2D polygonCollider2D;

    [System.NonSerialized]
    public Vector2 necessaryPosition;

    [System.NonSerialized]
    public Quaternion necessaryRotation;

    Vector2 diff;

    [HideInInspector]
    public bool currentlyHeldByPlayer = false;

    [HideInInspector]
    public SpriteRenderer spriteRenderer;

    [HideInInspector]
    public Coroutine swingerCoroutine = null;

    [HideInInspector]
    public float timeOfNextAllowedSwing = Mathf.NegativeInfinity;

    [HideInInspector]
    public Action additionalOnSwingEffects;

    public enum AttackPhase
    {
        NotSwinging,
        StartingSwing,
        FinishingSwing
    }

    public AttackPhase curAttackPhase = AttackPhase.NotSwinging;

    private void OnValidate()
    {
        internalName = internalName.ToLowerInvariant();

        if(attackWaveSpawns == null)
            attackWaveSpawns = new Transform[1];

        //if (GetComponent<GenericItem>() == null)    // move GenericItem just below sprite renderer, as first script
        //{
        //    GenericItem genericItem = gameObject.AddComponent<GenericItem>();
        //    for (int i = 0; i < 20; i++)
        //        UnityEditorInternal.ComponentUtility.MoveComponentUp(genericItem);
        //    for (int i = 0; i < 1; i++)
        //        UnityEditorInternal.ComponentUtility.MoveComponentDown(genericItem);
        //}
    }

    private void Reset()
    {
        attackWaveSpawns = new Transform[1];
    }

    private void Awake()
    {
        defaultLocalPos = transform.localPosition;
        defaultLocalRotation = transform.localRotation;
        parentBoxTransform = transform.parent;

        polygonCollider2D = GetComponent<PolygonCollider2D>();
        swingSound = GetComponent<AudioSource>();
        defaultSwingSoundPitch = swingSound.pitch;
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerController = FindObjectOfType<PlayerController>();

        playerController.totalSpawnedID++;
        spawnedID = playerController.totalSpawnedID;

        defaultDurationBeforeSwing = durationBeforeSwing;
        defaultWaitAfterAttDuration = waitAfterAttDuration;

        playerController.UpdateOnGlobalWeaponChangingEffects(this);


        if(transform.root != playerController.transform)
            playerController.worldManager.allWeaponsOnTheGround.Add(gameObject);


        playerController.restartManager.objectsToDestroyWhenRestarting.Add(gameObject);
    }

    void UpdateParentRotation()
    {
        transform.parent.rotation = Quaternion.Euler(0f, 0f, GenericExtensions.GetRotationToFaceTargetPosition(playerController.transform.position, playerController.cursorController.arrowKeyAiming ?
            (Vector2)playerController.cursorController.transform.position : GenericExtensions.GetMousePositionInWorld()) - (playerController.flipped ? 180f : 0f));
    }

    void UpdateLocalPosAndRotation()
    {
        switch(curAttackPhase)
        {
            case AttackPhase.NotSwinging:
                transform.localPosition = defaultLocalPos;
                transform.localRotation = defaultLocalRotation;
                break;
            case AttackPhase.StartingSwing:
                transform.localPosition = swingStartPoint.localPosition;
                transform.localRotation = swingStartPoint.localRotation;
                break;
            case AttackPhase.FinishingSwing:
                transform.localPosition = swingEndPoint.localPosition;
                transform.localRotation = swingEndPoint.localRotation;
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Hole"))
            DOTween.Kill(moveOnGroundId);
    }

    public void Update()
    {
        if (currentlyHeldByPlayer && playerController.allowControl)
        {
            // update damage and attack speed
            damage = baseDamage;
            playerController.UpdateDamageGlobal(ref damage);
            playerController.UpdateDamageMelee(ref damage);

            durationBeforeSwing = defaultDurationBeforeSwing / (playerController.meleeAttackSpeedMultiplier * playerController.AttackSpeedTotal);
            waitAfterAttDuration = defaultWaitAfterAttDuration / (playerController.meleeAttackSpeedMultiplier * playerController.AttackSpeedTotal);


            UpdateParentRotation(); // rotate parent box
            UpdateLocalPosAndRotation();    // rotate and change pos of attacker obj


            // attack
            if ((Input.GetKey(KeyCode.Mouse0) || playerController.cursorController.targetingMove != Vector2.zero) && !attacking && Time.time >= timeOfNextAllowedSwing)
                swingerCoroutine = StartCoroutine(Swinger());
        }       
    }

    public void PickUp()
    {
        DOTween.Kill(moveOnGroundId);

        if (playerController.heldWeapon)
            playerController.DropCurrentWeapon();
        else
        {
            playerController.unarmedObject.SetActive(false);
            playerController.actualPlayerWeaponsCount++;
        }

        transform.SetParent(parentBoxTransform);


        curAttackPhase = AttackPhase.NotSwinging;
        UpdateLocalPosAndRotation();


        playerController.playerWeapons[playerController.curWeaponSlot - 1] = gameObject;
        playerController.heldWeapon = gameObject;
        playerController.heldShooter = null;
        playerController.heldMeleeAttacker = this;


        transform.parent.SetParent(playerController.transform);

        transform.parent.localPosition = Vector2.zero;

        transform.parent.localScale = startingLocalScale;

        UpdateParentRotation();

        playerController.worldManager.UpdateWeaponSpriteLayerAndCollider(this, ownedByPlayer: true);   // makes the weapon not collide with the environment and keeps the sprite on top


        playerController.weaponPickupPopup.StartPopup(this);

        if (playerController.worldManager.allWeaponsOnTheGround.Contains(gameObject))
            playerController.worldManager.allWeaponsOnTheGround.Remove(gameObject);

        currentlyHeldByPlayer = true;


        playerController.UpdateOnGlobalWeaponChangingEffects(this);
    }

    IEnumerator Swinger()
    {
        attacking = true;

        curAttackPhase = AttackPhase.StartingSwing;
        UpdateLocalPosAndRotation();    // no need to update parent box rotation

        yield return new WaitForSeconds(durationBeforeSwing);

        swingSound.pitch = defaultSwingSoundPitch * (playerController.meleeAttackSpeedMultiplier * playerController.AttackSpeedTotal);
        swingSound.Play();

        for (int i = 0; i < attackWaveSpawns.Length; i++)
            Instantiate(attackWave, attackWaveSpawns[i].position, attackWaveSpawns[i].rotation, transform);


        additionalOnSwingEffects?.Invoke(); // none by default


        curAttackPhase = AttackPhase.FinishingSwing;
        UpdateLocalPosAndRotation();

        timeOfNextAllowedSwing = Time.time + waitAfterAttDuration;
        yield return new WaitForSeconds(waitAfterAttDuration);

        curAttackPhase = AttackPhase.NotSwinging;
        UpdateLocalPosAndRotation();

        attacking = false;
    }

}
