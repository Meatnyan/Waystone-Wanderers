using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnarmedAttacker : MonoBehaviour {

    public float baseDamage;

    [System.NonSerialized]
    public float damage;

    public float durationBeforeAttack;

    public float attackWaveDuration;

    public float waitAfterAttDuration;

    public bool allowMultipleHitsOnSameEnemy = false;

    public float zoomMultiplier = 1f;

    public float knockbackMultiplier = 1f;

    [System.NonSerialized]
    public bool attacking = false;

    PlayerController playerController;

    [System.NonSerialized]
    public bool flipped;

    public GameObject attackWave;

    public Transform attackWaveSpawn;

    AudioSource[] attackSounds;

    AudioSource attackSound;

    [System.NonSerialized]
    public float defaultDurationBeforeAttack;

    [System.NonSerialized]
    public float defaultWaitAfterAttDuration;

    Vector2 diff;

    [HideInInspector]
    public float timeOfNextAllowedAttack = Mathf.NegativeInfinity;

	void Awake () {
        playerController = FindObjectOfType<PlayerController>();
        attackSounds = GetComponents<AudioSource>();

        defaultDurationBeforeAttack = durationBeforeAttack;
        defaultWaitAfterAttDuration = waitAfterAttDuration;
    }

    private void OnDisable()
    {
        attacking = false;
    }

    public void UpdateTransform()
    {
        flipped = playerController.flipped;

        if (!attacking)
        {
            if (!playerController.cursorController.arrowKeyAiming)
                diff = Camera.main.ScreenToWorldPoint(Input.mousePosition) - playerController.transform.position;
            else
                diff = playerController.cursorController.transform.position - playerController.transform.position;
            diff.Normalize();
            float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            if (flipped == false)
                transform.rotation = Quaternion.Euler(0f, 0f, rot_z);
            else
                transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 180);
        }
    }
	
	void Update () {
        damage = baseDamage;
        playerController.UpdateDamageGlobal(ref damage);
        playerController.UpdateDamageUnarmed(ref damage);

        durationBeforeAttack = defaultDurationBeforeAttack / (playerController.unarmedAttackSpeedMultiplier * playerController.AttackSpeedTotal);
        waitAfterAttDuration = defaultWaitAfterAttDuration / (playerController.unarmedAttackSpeedMultiplier * playerController.AttackSpeedTotal);


        UpdateTransform();


        if (playerController.allowControl && (Input.GetKey(KeyCode.Mouse0) || playerController.cursorController.targetingMove != Vector2.zero) && !attacking && Time.time >= timeOfNextAllowedAttack)
            StartCoroutine(Attacker());

    }

    IEnumerator Attacker()
    {
        attacking = true;
        yield return new WaitForSeconds(durationBeforeAttack);
        attackSound = attackSounds[Random.Range(0, attackSounds.Length)];
        attackSound.pitch = Random.Range(0.9f, 1.1f) * (playerController.unarmedAttackSpeedMultiplier * playerController.AttackSpeedTotal);
        attackSound.Play();
        Instantiate(attackWave, attackWaveSpawn.position, transform.rotation, transform);
        timeOfNextAllowedAttack = Time.time + waitAfterAttDuration;
        yield return new WaitForSeconds(waitAfterAttDuration);
        attacking = false;
    }
}
