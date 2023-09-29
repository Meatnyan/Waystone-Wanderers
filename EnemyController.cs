using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenericExtensions;
public class EnemyController : MonoBehaviour {

    public string internalName;

    public bool isBoss = false;

    public float maxHealth;

    public float armor;

    public float alertPartThreshold = 0.75f;

    public float scaredPartThreshold = 0f;

    public float scaredMaxAngleForward = 0f;

    public float scaredAccuracyMultiplier = 1f;

    public float contactDamage;

    public float timeBetweenContactDmgTicks;

    public float moveSpeed;

    public bool hasScaledMoveSpeed = false;

    public float idleTime;

    public float moveTime;

    public bool isFlying = false;

    public float visionRange;

    public LayerMask sightBlockerLayers;

    public float hearingRange;

    public bool flipsToFacePlayer = true;

    public float stillCaresTime;

    public float seenMoveSpeedMultiplier;

    public float seenIdleTimeMultiplier;

    public float seenMoveTimeMultiplier;

    [System.Serializable]
    public struct OnDeathEvent
    {
        public GameObject[] childrenToDetach;   // non-projectile children that shouldn't be destroyed once the parent is destroyed

        [Space(20f)]
        public GameObject[] onDeathProjectiles; // disabled projectile children that will be enabled once the parent is destroyed

        [Header("Projectile Settings")]
        public float damage;
        public float shotSpeed;
        public float maxDuration;
    }

    public OnDeathEvent[] onDeathEvents;

    [Space(10f)]
    public GameObject rotator;

    public float bloodExplosionXSize = 1f;

    public float bloodExplosionYSize = 1f;

    public GameObject shadowObject;

    public float pickupBaseDropChance = 20f;

    public float pickupMinDropChance = 5f;

    public float pickupMaxDropChance = 500f;

    [System.NonSerialized]
    public bool isScared = false;

    [System.NonSerialized]
    public bool isAlert = false;

    float currentHealth = 0f;

    public float CurrentHealth {
        get => currentHealth;
        set
        {
            int damageDealt = Mathf.RoundToInt(currentHealth - value);
            if (damageDealt > playerController.achievementManager.reqs[(int)ReqName.MostDamageDealtInSingleHit])
                playerController.achievementManager.reqs[(int)ReqName.MostDamageDealtInSingleHit] = damageDealt;

            currentHealth = value;
        }
    }

    [System.NonSerialized]
    public float moveSpeedMultiplier = 1f;

    [System.NonSerialized]
    public float attackSpeedMultiplier = 1f;

    [System.NonSerialized]
    public float shotSpeedMultiplier = 1f;

    [System.NonSerialized]
    public float accuracyMultiplier = 1f;

    [System.NonSerialized]
    public int spawnedID = 0;

    [System.NonSerialized]
    public Vector2 shadowLocalPos = Vector2.zero;

    float damagedTintTime = 0.12f;

    [System.NonSerialized]
    public float bloodExplosionAvgSize = 1f;

    [System.NonSerialized]
    public float latestContactDmgTick = 0f;

    [System.NonSerialized]
    public float latestCurrentHealth = 0f;

    [System.NonSerialized]
    public float startingMoveSpeed = 0f;

    [System.NonSerialized]
    public bool flipped = false;

    [System.NonSerialized]
    public bool noticesPlayer = false;

    [System.NonSerialized]
    public bool hasNoticedPlayer = false;

    [System.NonSerialized]
    public bool hasNoticedPlayerRecently = false;

    [System.NonSerialized]
    public bool stillCares = false;

    [System.NonSerialized]
    bool enableAggro = true;

    public bool EnableAggro
    {
        get => enableAggro;
        set
        {
            enableAggro = value;

            if (!enableAggro)
                isAggrod = false;
        }
    }

    [System.NonSerialized]
    bool isAggrod = false;

    public bool IsAggrod
    {
        get => isAggrod;
        set
        {
            if (playerController && !playerController.enableEnemyAggro)
                enableAggro = false;

            isAggrod = enableAggro ? value : false;
        }
    }

    [System.NonSerialized]
    public float stoppedNoticingTime = Mathf.NegativeInfinity;

    [System.NonSerialized]
    public float sqrMagPlayerEnemy = Mathf.Infinity;

    [System.NonSerialized]
    public float sqrMagPlayerShotEnemy = Mathf.Infinity;

    [System.NonSerialized]
    public int movingHash = Animator.StringToHash("moving");

    [System.NonSerialized]
    public bool isMoving = false;

    [System.NonSerialized]
    public Rigidbody2D rb = null;

    [System.NonSerialized]
    public Animator animator = null;

    [System.NonSerialized]
    public GameObject playerObject = null;

    [System.NonSerialized]
    public PlayerController playerController = null;

    [System.NonSerialized]
    public SpriteRenderer spriteRenderer = null;

    Color defaultColor;

    [System.NonSerialized]
    public Vector2 startingLocalScale = new Vector2(1f, 1f);

    [System.NonSerialized]
    public float spawnTime = 0f;

    [System.NonSerialized]
    public EnemyShooter enemyShooter = null;

    [System.NonSerialized]
    public bool isIdle = true;

    [System.NonSerialized]
    public int burnStacks = 0;

    [HideInInspector]
    public List<BurnController> burnControllers = new List<BurnController>();

    [System.NonSerialized]
    public int chillStacks = 0;

    [HideInInspector]
    public List<ChillController> chillControllers = new List<ChillController>();

    [System.NonSerialized]
    public int bleedStacks = 0;

    [HideInInspector]
    public List<BleedController> bleedControllers = new List<BleedController>();

    [HideInInspector]
    public WorldManager worldManager = null;

    [HideInInspector]
    public float damageMultiplier = 1f;

    [HideInInspector]
    new public Collider2D collider2D = null;

    [HideInInspector]
    public bool overrideAllowNoticingPlayer = false;

    [HideInInspector]
    public bool allowNoticingPlayer = true;

    private void OnValidate()
    {
        internalName = internalName.ToLowerInvariant();
    }

    private void Awake()
    {
        spawnTime = Time.time;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        collider2D = GetComponent<Collider2D>();
        playerObject = GameObject.FindWithTag("Player");
        playerController = playerObject.GetComponent<PlayerController>();
        worldManager = FindObjectOfType<WorldManager>();
        playerController.totalSpawnedID++;
        spawnedID = playerController.totalSpawnedID;
        worldManager.enemiesInCurrentLevel.Add(gameObject);

        for(int levelScalingCycles = 0; levelScalingCycles < worldManager.levelsBeaten; levelScalingCycles++)
        {
            maxHealth *= worldManager.enemyHealthMultiplierPerLevel;
            damageMultiplier *= worldManager.enemyDmgMultiplierPerLevel;
        }

        damageMultiplier = Mathf.Round(damageMultiplier);

        CurrentHealth = maxHealth;
        latestCurrentHealth = CurrentHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultColor = spriteRenderer.material.color;
        startingLocalScale = transform.localScale;
        startingMoveSpeed = moveSpeed;
        shadowLocalPos = shadowObject.transform.localPosition;


        // make a list of all holes in current level for worldManager. TODO: put this somewhere where it makes more sense probably
        if (worldManager.singularHoles.Count == 0)
            worldManager.singularHoles = new List<GameObject>(GameObject.FindGameObjectsWithTag("Hole"));


        if (isFlying)
            foreach(GameObject holeObj in worldManager.singularHoles)
                Physics2D.IgnoreCollision(collider2D, holeObj.GetComponent<Collider2D>());


        // set up enemyShotMover properties for on-death events
        // 
        foreach(OnDeathEvent onDeathEvent in onDeathEvents)
            foreach (GameObject onDeathProjectile in onDeathEvent.onDeathProjectiles)
                onDeathProjectile.GetComponent<EnemyShotMover>().SetStats(
                    newDamage: onDeathEvent.damage,
                    newShotSpeed: onDeathEvent.shotSpeed,
                    newMaxDuration: onDeathEvent.maxDuration);
                    // since the projectiles are !isAttached,
                    // their Awake() will deparent them and set spawn time once they're instantiated
    }

    private void OnBecameVisible()
    {
        playerController.amountOfEnemiesOnScreen++;
    }

    private void OnBecameInvisible()
    {
        playerController.amountOfEnemiesOnScreen--;
    }

    public void FixShadowPosition() // actual garbage, fix armoreddillo script instead
    {
        shadowObject.transform.position = (Vector2)transform.position + shadowLocalPos;
        shadowObject.transform.rotation = Quaternion.identity;
    }

    public void OverrideAllowNoticingPlayer(bool newAllowNoticingPlayer)
    {
        overrideAllowNoticingPlayer = true;
        allowNoticingPlayer = newAllowNoticingPlayer;
    }

    public void StopOverrideAllowNoticingPlayer()
    {
        overrideAllowNoticingPlayer = false;
        allowNoticingPlayer = spriteRenderer.isVisible;
    }

    private void Update()
    {
        allowNoticingPlayer = overrideAllowNoticingPlayer ? allowNoticingPlayer : spriteRenderer.isVisible;

        if (CurrentHealth <= maxHealth * alertPartThreshold)    // once alert due to low health, won't go back to non-alert automatically if health is restored
            isAlert = true;

        if (CurrentHealth <= maxHealth * scaredPartThreshold)   // once scared due to low health, won't go back to non-scared automatically if health is restored
            isScared = true;

        moveSpeed = startingMoveSpeed * moveSpeedMultiplier;
        if (hasScaledMoveSpeed)
            moveSpeed *= playerController.enemyScaledMoveSpeedMultiplier;

        sqrMagPlayerEnemy = (playerObject.transform.position - transform.position).sqrMagnitude;

        sqrMagPlayerShotEnemy = GetClosestSqrMagnitude(transform.position, playerController.shotObjects);

        Collider2D raycastHitCollider = Physics2D.Raycast(transform.position, playerController.transform.position - transform.position, visionRange * (isAlert ? 2f : 1f), sightBlockerLayers).collider;

        //if (raycastHitCollider != null)
        //    Debug.Log("raycastHitCollider.tag = " + raycastHitCollider.tag);

        if (((raycastHitCollider != null && raycastHitCollider.CompareTag("Player"))
           || SqrMagIsInDistance(sqrMagPlayerShotEnemy, hearingRange))
           && (allowNoticingPlayer || isAlert))
        {
            noticesPlayer = true;
            hasNoticedPlayer = true;
            hasNoticedPlayerRecently = true;
        }
        else
        {
            noticesPlayer = false;
            if (hasNoticedPlayerRecently)
            {
                stoppedNoticingTime = Time.time;
                hasNoticedPlayerRecently = false;
            }
        }

        if (Time.time - stillCaresTime <= stoppedNoticingTime)
            stillCares = true;
        else
            stillCares = false;

        if (noticesPlayer || stillCares)
            IsAggrod = true;
        else
            IsAggrod = false;


        if (flipsToFacePlayer && playerController && IsAggrod)
        {
            if (transform.position.x > playerController.transform.position.x)
            {
                flipped = true;
                transform.localScale = new Vector2(-Mathf.Abs(transform.localScale.x), transform.localScale.y);
            }
            else
            {
                flipped = false;
                transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
            }
        }


        if (CurrentHealth <= 0)
        {
            ParticleSystem particleSystem = worldManager.bloodExplosionParticleSystem.Spawn(null, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
            particleSystem.transform.localScale = new Vector2(particleSystem.transform.localScale.x * bloodExplosionXSize, particleSystem.transform.localScale.y * bloodExplosionYSize);
            bloodExplosionAvgSize *= (bloodExplosionXSize + bloodExplosionYSize) / 2f;
            var mainSettings = particleSystem.main;
            mainSettings.startSizeMultiplier = 0.1f * bloodExplosionAvgSize;


            float pickupDropChance = Random.Range(0f, Mathf.Clamp(pickupBaseDropChance + playerController.luck * 5f, pickupMinDropChance, pickupMaxDropChance));
            int amountOfPickupsToDrop = 0;

            while(pickupDropChance > 0f)
            {
                if(DetermineIfPercentChancePasses(pickupDropChance))
                    amountOfPickupsToDrop++;

                pickupDropChance -= 100f;
            }

            if (playerController.gunNut == null)
                playerController.lootManager.DropOnlyPickups(amountOfPickupsToDrop, transform.position);
            else
                playerController.lootManager.DropPickupsAndWeapons(amountOfPickupsToDrop, transform.position);


            for (int eventID = 0; eventID < onDeathEvents.Length; ++eventID)
            {
                for (int childID = 0; childID < onDeathEvents[eventID].childrenToDetach.Length; ++childID)
                    if(onDeathEvents[eventID].childrenToDetach[childID])
                        onDeathEvents[eventID].childrenToDetach[childID].transform.SetParent(null);

                for (int projID = 0; projID < onDeathEvents[eventID].onDeathProjectiles.Length; ++projID)
                    onDeathEvents[eventID].onDeathProjectiles[projID].SetActive(true);   // their properties were already set in Awake(), and they'll deparent and set spawn time on their Awake
            }


            playerController.totalKillCount++;
            playerController.achievementManager.reqs[(int)ReqName.TotalEnemiesKilled]++;
            playerController.killCountThisLevel++;

            playerController.timeOfLatestKill = Time.time;
            playerController.recentKillTimes.Enqueue(Time.time);

            if (bleedStacks > 0 || burnStacks > 0 || chillStacks > 0)
                playerController.UpdateOnKillDURINGStatusEffectEffects(enemyController: this);

            worldManager.enemiesInCurrentLevel.Remove(gameObject);


            Destroy(gameObject);
            return;
        }

        if (CurrentHealth < latestCurrentHealth)
        {
            latestCurrentHealth = CurrentHealth;
            StartCoroutine(DamagedTinter());
        }

    }

    IEnumerator DamagedTinter()
    {
        spriteRenderer.material.color = new Color(1f, 0f, 0f);
        yield return new WaitForSeconds(damagedTintTime / 3);
        spriteRenderer.material.color = new Color(1f, 0.33f, 0.33f);
        yield return new WaitForSeconds(damagedTintTime / 3);
        spriteRenderer.material.color = new Color(1f, 0.66f, 0.66f);
        yield return new WaitForSeconds(damagedTintTime / 3);
        spriteRenderer.material.color = defaultColor;
    }

    public void ApplyBurn()
    {
        if (burnStacks < playerController.maxBurnStacks)
        {
            burnStacks++;
            burnControllers.Add(Instantiate(worldManager.burnParticleSystem, new Vector2(transform.position.x, transform.position.y - 0.05f), Quaternion.identity, transform).GetComponent<BurnController>());
            burnControllers[burnControllers.Count - 1].playerController = playerController;
            burnControllers[burnControllers.Count - 1].enemyController = this;
        }
        else
            burnControllers[burnControllers.Count - 1].beginTime = Time.time;
    }

    public void ApplyBleed(GameObject sourceOfBleed)
    {
        ShotMover sourceShotMover = sourceOfBleed.GetComponent<ShotMover>();
        AttackWaveController sourceAttackWaveController = sourceOfBleed.GetComponent<AttackWaveController>();

        float sourceDmg = 0f;
        if (sourceShotMover != null)
            sourceDmg = sourceShotMover.damage;
        else if (sourceAttackWaveController != null)
            sourceDmg = sourceAttackWaveController.damage;

        if (bleedStacks < playerController.maxBleedStacks)
        {
            bleedStacks++;
            bleedControllers.Add(Instantiate(worldManager.bleedParticleSystem, new Vector2(transform.position.x, transform.position.y - 0.05f), Quaternion.identity, transform).GetComponent<BleedController>());
            bleedControllers[bleedControllers.Count - 1].playerController = playerController;
            bleedControllers[bleedControllers.Count - 1].enemyController = this;
            bleedControllers[bleedControllers.Count - 1].sourceDmg = sourceDmg;
        }
        else
        {
            bleedControllers[bleedControllers.Count - 1].beginTime = Time.time;
            if (sourceDmg > bleedControllers[bleedControllers.Count - 1].sourceDmg)
                bleedControllers[bleedControllers.Count - 1].sourceDmg = sourceDmg;
        }
    }

    public void ApplyChill()
    {
        if(chillStacks < playerController.maxChillStacks)
        {
            chillStacks++;
            chillControllers.Add(Instantiate(worldManager.chillParticleSystem, new Vector2(transform.position.x, transform.position.y - 0.05f), Quaternion.identity, transform).GetComponent<ChillController>());
            chillControllers[chillControllers.Count - 1].playerController = playerController;
            chillControllers[chillControllers.Count - 1].enemyController = this;
        }
        else
            chillControllers[chillControllers.Count - 1].beginTime = Time.time;
    }

    void TryApplyDamageToPlayer()
    {
        if (Time.time > latestContactDmgTick + timeBetweenContactDmgTicks)
        {
            if (playerController.invincibilityLayers < 1)
            {
                float damage = contactDamage * damageMultiplier - playerController.Armor;

                if (damage < 1f)
                    damage = 1f;

                playerController.currentHealth -= damage;

                latestContactDmgTick = Time.time;

                playerController.UpdateAfterGettingHitEffects();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            TryApplyDamageToPlayer();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            TryApplyDamageToPlayer();
    }

    //private void LateUpdate()
    //{
    //    if(flipsToFacePlayer && playerController && isAggrod)
    //    {
    //        if(transform.position.x > playerController.transform.position.x)
    //        {
    //            flipped = true;
    //            transform.localScale = new Vector2(-Mathf.Abs(transform.localScale.x), transform.localScale.y);
    //        }
    //        else
    //        {
    //            flipped = false;
    //            transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
    //        }
    //    }

    //    FixShadowPosition();    // probably shouldn't fix shadows every frame
    //}
}
