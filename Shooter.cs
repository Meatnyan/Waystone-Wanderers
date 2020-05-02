using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public enum AmmoType
{
    USAM,
    Shells,
    Energy
};

public class Shooter : MonoBehaviour {
    [Header("Internal")]
    public bool unlocked = true;

    public string internalName;

    public float dropWeight;

    [Space(20f)]
    public string displayedName;

    [Header("Ammo & Reloading")]
    public AmmoType ammoType;

    [Space(10f)]
    public bool multiStepReloading = false;

    public float reloadOneShotTime;

    [Header("Shot Series")]
    public int shotsPerWave = 1;

    [Space(10f)]
    public int wavesInBarrage = 1;

    public float delayBetweenWavesInBarrage = 0f;

    [Header("Main Stats")]
    public float baseDamage;

    public float fireRate;

    public int maxAmmo;

    public int ammoConsumptionPerShot = 1;

    public float reloadTime;

    public float shotSpread;

    public float shotSpeed;

    public float range;

    public float shotMaxDuration = 5f;

    public float shotDurAfterLeavingCam = 0.25f;

    public float zoomMultiplier = 1f;

    public float knockbackMultiplier = 1f;

    public float shakeCam = 0f; // Values of shakeThreshold and higher will trigger the camera shake on every shot, lower than that will add to the camera shake, triggering it when it reaches shakeThreshold, but only if it shoots at or faster than once every 0.2s

    [Header("Special Effects")]
    public float burnChance = 0f;

    public float chillChance = 0f;

    public float bleedChance = 0f;

    [Space(10f)]
    public float pierceChance = 0f;

    public float ghostlyChance = 0f;

    [Space(10f)]
    public int homingStrength = 0;

    [Space(20f)]
    public GameObject shot;

    [Space(20f)]
    public bool simultaneousShooting = true;

    public List<Transform> shotSpawns;

    [Space(20f)]
    public Vector2 startingLocalPos = new Vector2(0.14f, 0f);

    [NonSerialized]
    public int spawnedID = 0;

    [NonSerialized]
    public float minRangeToBePicked = Mathf.Infinity;

    [NonSerialized]
    public float maxRangeToBePicked = Mathf.NegativeInfinity;

    [NonSerialized]
    public float charge = 0f;

    [NonSerialized]
    public int currentAmmo = 0;

    [NonSerialized]
    public float trueShotSpread = 0f;

    [NonSerialized]
    public float damage = 0f;

    [NonSerialized]
    public bool flipped = false;

    [NonSerialized]
    public bool reloading = false;

    [NonSerialized]
    public bool reloadingOneShot = false;

    [NonSerialized]
    public float timeOfLastShot = 0f;

    [NonSerialized]
    public float timeOfNextAllowedShot = 0f;

    [NonSerialized]
    public float shotReloadStartTime = 0f;

    [HideInInspector]
    public AudioSource shotSound, reloadSound, reloadOneShot;

    PlayerController playerController;

    CameraShaker cameraShaker;

    [NonSerialized]
    public Vector2 startingLocalScale = new Vector2(1f, 1f);

    [HideInInspector]
    public int moveOnGroundId;

    [HideInInspector]
    public float defaultReloadSoundPitch;

    [HideInInspector]
    public float defaultReloadOneShotPitch;

    [HideInInspector]
    public float defaultReloadTime;

    [HideInInspector]
    public float defaultReloadOneShotTime;

    [HideInInspector]
    public float defaultFireRate;

    float rot_z;

    [HideInInspector]
    public PolygonCollider2D polygonCollider2D;

    WeaponPickupPopup weaponPickupPopup;

    Vector2 diff;

    [HideInInspector]
    public bool currentlyHeldByPlayer = false;

    [HideInInspector]
    public SpriteRenderer spriteRenderer;

    [HideInInspector]
    public Coroutine shootCoroutine = null;

    [HideInInspector]
    public Coroutine reloaderCoroutine = null;

    [HideInInspector]
    public Coroutine multiStepReloaderCoroutine = null;

    [HideInInspector]
    public Action onWeaponHeldEffects;

    [HideInInspector]
    public Action onWeaponIdleEffects;

    [HideInInspector]
    public Action onButtonHoldEffects;

    int latestShotSpawnID = -1;


    // weapon-identifying booleans start here. TODO: this is a super crappy way of doing this lmao 

    [NonSerialized]
    public bool isGeneric = false;

    [NonSerialized]
    public bool isThePrism = false;

    private void OnValidate()
    {
        internalName = internalName.ToLowerInvariant();

        //if (GetComponent<GenericItem>() == null)    // move GenericItem just below sprite renderer, as first script
        //{
        //    GenericItem genericItem = gameObject.AddComponent<GenericItem>();
        //    for (int i = 0; i < 20; i++)
        //        UnityEditorInternal.ComponentUtility.MoveComponentUp(genericItem);
        //    for (int i = 0; i < 1; i++)
        //        UnityEditorInternal.ComponentUtility.MoveComponentDown(genericItem);
        //}
    }

    private void Awake()
    {
        switch(internalName)    // maybe think of a better way to do this for the future? not top priority though because it's only performed once
        {
            case "ThePrism":
                isThePrism = true;
                break;
            default:
                isGeneric = true;
                break;
        }

        polygonCollider2D = GetComponent<PolygonCollider2D>();
        AudioSource[] audios = GetComponents<AudioSource>();
        shotSound = audios[0];
        reloadSound = audios[1];
        defaultReloadSoundPitch = reloadSound.pitch;
        if (multiStepReloading)
        {
            reloadOneShot = audios[2];
            defaultReloadOneShotPitch = reloadOneShot.pitch;
        }
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerController = FindObjectOfType<PlayerController>();
        cameraShaker = Camera.main.transform.parent.GetComponent<CameraShaker>();

        playerController.totalSpawnedID++;
        spawnedID = playerController.totalSpawnedID;


        defaultReloadTime = reloadTime;
        defaultReloadOneShotTime = reloadOneShotTime;
        defaultFireRate = fireRate;

        playerController.UpdateOnGlobalWeaponChangingEffects(this);

        currentAmmo = maxAmmo;



        if (transform.root != playerController.transform)
            playerController.worldManager.allWeaponsOnTheGround.Add(gameObject);


        playerController.restartManager.objectsToDestroyWhenRestarting.Add(gameObject);
    }

    public void Update()
    {
        if (currentlyHeldByPlayer && playerController.allowControl)
        {
            damage = baseDamage;
            playerController.UpdateDamageGlobal(ref damage);
            playerController.UpdateDamageRanged(ref damage);

            reloadTime = defaultReloadTime / playerController.ReloadSpeedTotal;
            reloadOneShotTime = defaultReloadOneShotTime / playerController.ReloadSpeedTotal;

            fireRate = defaultFireRate / (playerController.rangedAttackSpeedMultiplier * playerController.AttackSpeedTotal);


            UpdateTransform();


            onWeaponHeldEffects?.Invoke();


            if (((Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
                || ((Input.GetKeyDown(KeyCode.Mouse0) || playerController.cursorController.targetingMove != Vector2.zero) && currentAmmo == 0))
                && !reloading && HeldAmmoIsAvailable())
            {
                if (multiStepReloading)
                    multiStepReloaderCoroutine = StartCoroutine(MultiStepReloader());
                else
                    reloaderCoroutine = StartCoroutine(Reloader());
            }


            if ((Input.GetKey(KeyCode.Mouse0) || playerController.cursorController.targetingMove != Vector2.zero) && currentAmmo > 0)
            {
                if (reloading)
                {
                    if (multiStepReloading)
                    {
                        StopCoroutine(multiStepReloaderCoroutine);
                        reloadingOneShot = false;
                        reloadOneShot.Stop();
                    }
                    else
                        StopCoroutine(reloaderCoroutine);

                    reloadSound.Stop();

                    reloading = false;
                }

                onButtonHoldEffects?.Invoke();

                if (Time.time >= timeOfNextAllowedShot)
                    shootCoroutine = StartCoroutine(Shoot(wavesInBarrage, delayBetweenWavesInBarrage));
            }
            else if(!reloading)    // if not shooting or reloading
                onWeaponIdleEffects?.Invoke();


            if (currentAmmo < 0)
                currentAmmo = 0;
            if (playerController.heldAmmoUSAM < 0)
                playerController.heldAmmoUSAM = 0;
            if (playerController.heldAmmoShells < 0)
                playerController.heldAmmoShells = 0;
            if (playerController.heldAmmoEnergy < 0)
                playerController.heldAmmoEnergy = 0;
        }
    }

    public bool HeldAmmoIsAvailable()
    {
        return playerController.GetHeldAmmoCount(ammoType) > 0;
    }

    IEnumerator Reloader()
    {
        reloading = true;

        int ammoBeforeReloading = currentAmmo;

        reloadSound.pitch = defaultReloadSoundPitch * playerController.ReloadSpeedTotal;
        reloadSound.Play();

        shotReloadStartTime = Time.time;

        yield return new WaitForSeconds(reloadTime);

        currentAmmo += playerController.GetHeldAmmoCount(ammoType);

        if (currentAmmo > maxAmmo)
            currentAmmo = maxAmmo;

        int ammoGained = currentAmmo - ammoBeforeReloading;

        playerController.AddToHeldAmmoCount(ammoType, -ammoGained);

        reloading = false;
    }

    IEnumerator MultiStepReloader()
    {
        reloading = true;

        if(reloadSound != null)
            reloadSound.Play();

        yield return new WaitForSeconds(reloadTime);

        while (currentAmmo < maxAmmo && HeldAmmoIsAvailable())
        {
            reloadingOneShot = true;

            reloadOneShot.pitch = defaultReloadOneShotPitch * playerController.ReloadSpeedTotal;
            reloadOneShot.Play();

            shotReloadStartTime = Time.time;

            yield return new WaitForSeconds(reloadOneShotTime);

            currentAmmo++;

            playerController.AddToHeldAmmoCount(ammoType, -1);
        }

        reloading = false;
        reloadingOneShot = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Hole"))
            DOTween.Kill(moveOnGroundId);
    }

    public void UpdateTransform()
    {
        transform.localPosition = startingLocalPos;

        if (playerController.flipped && !flipped)
        {
            transform.localScale = new Vector2(-transform.localScale.x, -transform.localScale.y);
            flipped = true;
        }
        else if (!playerController.flipped && flipped)
        {
            transform.localScale = new Vector2(-transform.localScale.x, -transform.localScale.y);
            flipped = false;
        }

        if (!playerController.cursorController.arrowKeyAiming)
            diff = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        else
            diff = playerController.cursorController.transform.position - transform.position;
        diff.Normalize();
        rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rot_z);
    }

    public IEnumerator Shoot(int amountOfWaves, float delayBetweenWavesInBarrage)
    {
        for (int waveID = 0; waveID < amountOfWaves; waveID++)
        {
            if (Time.time - timeOfLastShot <= 0.2f)
            {
                cameraShaker.shake += shakeCam;
                cameraShaker.constantShaking = true;
            }
            if (Time.time - timeOfLastShot >= 0.4f && cameraShaker.shake < cameraShaker.shakeThreshold)
            {
                cameraShaker.shake = 0f;
                cameraShaker.constantShaking = false;
            }
            if (shakeCam >= cameraShaker.shakeThreshold)
                cameraShaker.shake += shakeCam;

            timeOfLastShot = Time.time;


            int shotMultiplierFromDoubleShotsChance = 1;

            if (GenericExtensions.DetermineIfPercentChancePasses(playerController.doubleShotsChance))
                shotMultiplierFromDoubleShotsChance = 2;

            Quaternion shotRotation = Quaternion.identity;
            ShotMover shotMoverInstance = null;

            if (shotSpawns.Count == 0)
                Debug.LogWarning("shotSpawns list for " + gameObject + " is empty, shooting can't continue normally");

            int amountOfShotSpawns = simultaneousShooting ? shotSpawns.Count : 1;
            for (int i = 0; i < amountOfShotSpawns; i++)
            {
                int shotSpawnID;
                if (simultaneousShooting)
                    shotSpawnID = i;
                else
                {
                    latestShotSpawnID++;
                    if (latestShotSpawnID >= shotSpawns.Count)
                        latestShotSpawnID = 0;
                    shotSpawnID = latestShotSpawnID;
                }

                int totalAmountOfShotsInWave = 0;
                for (int shotID = 0; shotID < (shotsPerWave * playerController.shotsPerWaveMultiplier * shotMultiplierFromDoubleShotsChance); shotID++)
                {
                    trueShotSpread = shotSpread / playerController.AccuracyTotal * UnityEngine.Random.Range(-0.5f, 0.5f);
                    shotRotation = Quaternion.Euler(0f, 0f, rot_z + trueShotSpread + shotSpawns[shotSpawnID].localEulerAngles.z);   // takes into account rotation of the shotSpawn
                    totalAmountOfShotsInWave++;

                    shotMoverInstance = Instantiate(shot, shotSpawns[shotSpawnID].position, shotRotation, transform).GetComponent<ShotMover>();  // instantiate a single shot...
                    playerController.UpdateOnShootEffects(shotMoverInstance);  // ...and update its initial properties
                }
                playerController.UpdateOnShotWaveEffects(shotMoverInstance, shotRotation.eulerAngles.z, totalAmountOfShotsInWave);
            }

            shotSound.Play();

            timeOfNextAllowedShot = Time.time + fireRate;

            currentAmmo -= ammoConsumptionPerShot;

            if (delayBetweenWavesInBarrage > 0f)
                yield return new WaitForSeconds(delayBetweenWavesInBarrage);
        }
    }

    public void PickUp()
    {
        DOTween.Kill(moveOnGroundId);

        if (playerController.heldWeapon != null)
            playerController.DropCurrentWeapon();
        else
        {
            playerController.unarmedObject.SetActive(false);
            playerController.actualPlayerWeaponsCount++;
        }

        playerController.playerWeapons[playerController.curWeaponSlot - 1] = gameObject;


        transform.SetParent(playerController.transform);


        playerController.worldManager.UpdateWeaponSpriteLayerAndCollider(this, ownedByPlayer: true);

        if (flipped != playerController.flipped)
        {
            transform.localScale = new Vector2(transform.localScale.x, -transform.localScale.y);
            flipped = !flipped;
        }

        UpdateTransform();

        playerController.heldWeapon = gameObject;
        playerController.heldShooter = this;
        playerController.heldMeleeAttacker = null;

        playerController.weaponPickupPopup.StartPopup(this);

        if (playerController.worldManager.allWeaponsOnTheGround.Contains(gameObject))
            playerController.worldManager.allWeaponsOnTheGround.Remove(gameObject);

        currentlyHeldByPlayer = true;


        playerController.UpdateOnGlobalWeaponChangingEffects(this);
    }
}
