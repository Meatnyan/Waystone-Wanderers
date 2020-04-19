using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotMover : MonoBehaviour {

    public bool motherShot = true;

    public Color defaultColor = new Color(1f, 1f, 1f);

    [System.NonSerialized]
    public float maxDuration = 0f;

    [System.NonSerialized]
    public Rigidbody2D rb = null;

    [System.NonSerialized]
    public Shooter shooter = null;

    [System.NonSerialized]
    public AttackWaveController attackWaveController = null;

    [System.NonSerialized]
    public MeleeAttacker meleeAttacker = null;

    PlayerController playerController;

    [System.NonSerialized]
    public GameObject parentObject = null;

    [System.NonSerialized]
    public SpriteRenderer spriteRenderer = null;

    float distanceTravelled = 0f;

    [System.NonSerialized]
    public float burnChance = 0f;

    [System.NonSerialized]
    public float chillChance = 0f;

    [System.NonSerialized]
    public float bleedChance = 0f;

    [System.NonSerialized]
    public float damage = 0f;

    Coroutine invisibleDestroyCoroutine;

    [HideInInspector]
    public int spawnedID;

    [System.NonSerialized]
    public float spawnTime = Mathf.Infinity;

    [HideInInspector]
    public Vector2 spawnPos;

    Vector2 latestPos;

    [System.NonSerialized]
    public int frameCountSinceSpawn = 0;

    //new Collider2D collider2D;    // might be useful in the future

    [System.NonSerialized]
    public bool destroyOnHittingCollider = true;

    [System.NonSerialized]
    public List<int> spawnedIDsToIgnore = new List<int>();

    [System.NonSerialized]
    public int bouncesLeft = 0;

    [System.NonSerialized]
    public bool hitEnemyThisFrame = false;

    [System.NonSerialized]
    public bool hitObstacleThisFrame = false;

    [System.NonSerialized]
    public bool hitWallThisFrame = false;

    [System.NonSerialized]
    public bool isPiercing = false;

    [System.NonSerialized]
    public bool isGhostly = false;


    // item-dependent modifiers start here

    [HideInInspector]
    public static int thePrismShotCount = 0;

    [System.NonSerialized]
    public float unstableConcoctionMultiplier = 0f;


    private void Awake()
    {
        playerController = FindObjectOfType<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
        //collider2D = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = defaultColor;

        playerController.totalSpawnedID++;
        spawnedID = playerController.totalSpawnedID;

        if (motherShot)
            playerController.shotObjects.Add(gameObject);

        if (transform.parent != null)
        {
            parentObject = transform.parent.gameObject;

            transform.SetParent(null);

            shooter = parentObject.GetComponent<Shooter>();
            if (shooter != null)
            {
                maxDuration = shooter.shotMaxDuration;
                rb.velocity = transform.right * shooter.shotSpeed * playerController.ShotSpeedTotal;
                damage = shooter.damage;

                if (GenericExtensions.DetermineIfPercentChancePasses(shooter.ghostlyChance + playerController.ghostlyChance))
                    isGhostly = true;

                if (GenericExtensions.DetermineIfPercentChancePasses(shooter.pierceChance + playerController.pierceChance))
                    isPiercing = true;

                if (!shooter.isGeneric)
                    UpdateShooterEffectsOnShots();
            }

            attackWaveController = parentObject.GetComponent<AttackWaveController>();   // this is for when the source is an attack wave, use a different one when it's a melee attacker directly
            if (attackWaveController != null)
            {
                meleeAttacker = attackWaveController.meleeAttacker; // make sure to somehow get the damage value
            }
        }

        spawnPos = transform.position;
        latestPos = spawnPos;

        spawnTime = Time.time;
    }

    public void UpdateShooterEffectsOnShots()
    {
        if(shooter.isThePrism)
        {
            thePrismShotCount++;
            if (thePrismShotCount > 3)
                thePrismShotCount = 1;

            switch(thePrismShotCount)
            {
                case 1:
                    burnChance += 33.33333f;
                    spriteRenderer.color = Color.yellow;
                    break;
                case 2:
                    chillChance += 33.33333f;
                    spriteRenderer.color = Color.blue;
                    break;
                case 3:
                    bleedChance += 33.33333f;
                    spriteRenderer.color = Color.red;
                    break;
            }
        }
    }

    private void Update()
    {
        frameCountSinceSpawn++;

        hitEnemyThisFrame = false;
        hitObstacleThisFrame = false;
        hitWallThisFrame = false;

        if(Time.time >= spawnTime + maxDuration)
        {
            Destroy(gameObject);
            if (motherShot)
                playerController.UpdateOnShotDestroyedWithoutHittingEnemyOrObstacleEffects();

            return;
        }

        distanceTravelled += Vector2.Distance(latestPos, transform.position);
        latestPos = transform.position;
        if (shooter && distanceTravelled > shooter.range * playerController.RangeTotal)
        {
            Destroy(gameObject);
            if (motherShot)
                playerController.UpdateOnShotDestroyedWithoutHittingEnemyOrObstacleEffects();

            return;
        }

        playerController.UpdatePropertiesOfShot(this);
    }

    public void UpdateOnScreenLeaveEffects()
    {
        if (playerController.abyssalGaze != null)
            rb.velocity *= -1 * playerController.abyssalGaze.speedMultiplier;
    }

    IEnumerator DestroyIfNotVisible(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(gameObject);
        if (motherShot)
            playerController.UpdateOnShotDestroyedWithoutHittingEnemyOrObstacleEffects();
    }

    private void OnBecameInvisible()
    {
        if(gameObject.activeSelf && shooter != null)
            invisibleDestroyCoroutine = StartCoroutine(DestroyIfNotVisible(shooter.shotDurAfterLeavingCam));
        UpdateOnScreenLeaveEffects();
    }

    private void OnBecameVisible()
    {
        if (invisibleDestroyCoroutine != null)
            StopCoroutine(invisibleDestroyCoroutine);
    }

    private void OnDestroy()
    {
        if(motherShot)
            playerController.shotObjects.Remove(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D otherCollider)        
    {
        Vector2 contactPoint = otherCollider.ClosestPoint(transform.position);
        float totalDamage = damage;

        if (otherCollider.CompareTag("Enemy") && !spawnedIDsToIgnore.Contains(otherCollider.GetComponent<EnemyController>().spawnedID))
        {
            GameObject enemyObject = otherCollider.gameObject;
            EnemyController enemyController = enemyObject.GetComponent<EnemyController>();

            playerController.UpdateDamageShot(gameObject, ref totalDamage, enemyObject);
            playerController.UpdateTotalDamage(gameObject, ref totalDamage, enemyObject);
            totalDamage -= enemyController.armor;
            if (totalDamage < 1)
                totalDamage = 1;

            playerController.UpdateOnShotHitEffects(shotObj: gameObject, hitObj: enemyObject, contactPoint: contactPoint, hitWall: false);
            playerController.UpdateOnHitEffects(weaponObj: parentObject, hitterObj: gameObject, hitObj: enemyObject, hitWall: false);

            enemyController.CurrentHealth -= totalDamage;

            if (shooter != null)
                enemyController.rb.AddForce(rb.velocity * shooter.knockbackMultiplier * playerController.knockbackMultiplier * playerController.knockbackDirectionMultiplier, ForceMode2D.Impulse);

            if (enemyController.CurrentHealth <= 0)
            {
                playerController.UpdateOnRangedKillEffects(shotObj: gameObject, enemyObj: enemyObject);
                playerController.UpdateOnKillEffects();
            }

            if (!isPiercing && destroyOnHittingCollider)
                Destroy(gameObject);

            hitEnemyThisFrame = true;
        }

        if (otherCollider.CompareTag("Obstacle"))
        {
            GameObject obstacleObject = otherCollider.gameObject;
            ObstacleController obstacleController = obstacleObject.GetComponent<ObstacleController>();

            playerController.UpdateDamageShot(gameObject, ref totalDamage, obstacleObject);
            playerController.UpdateTotalDamage(gameObject, ref totalDamage, obstacleObject);
            totalDamage -= obstacleController.armor;
            if (totalDamage < 1)
                totalDamage = 1;

            obstacleController.currentHealth -= totalDamage;

            playerController.UpdateOnShotHitEffects(shotObj: gameObject, hitObj: otherCollider.gameObject, contactPoint: contactPoint, hitWall: false);
            playerController.UpdateOnHitEffects(weaponObj: parentObject, hitterObj: gameObject, hitObj: otherCollider.gameObject, hitWall: false);

            if (!isGhostly && destroyOnHittingCollider)
                Destroy(gameObject);

            hitObstacleThisFrame = true;
        }

        if (!hitWallThisFrame && otherCollider.CompareTag("Wall"))  // can only ever hit 1 wall in a single frame, even if collided on the edge between 2 or more walls
        {
            playerController.UpdateOnShotHitEffects(shotObj: gameObject, hitObj: otherCollider.gameObject, contactPoint: contactPoint, hitWall: true);
            playerController.UpdateOnHitEffects(weaponObj: parentObject, hitterObj: gameObject, hitObj: otherCollider.gameObject, hitWall: true);

            if (!isGhostly && destroyOnHittingCollider)
            {
                Destroy(gameObject);
                if(!hitEnemyThisFrame && !hitObstacleThisFrame)
                    playerController.UpdateOnShotDestroyedWithoutHittingEnemyOrObstacleEffects();
            }

            hitWallThisFrame = true;
        }

    }
}
