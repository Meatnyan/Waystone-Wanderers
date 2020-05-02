using System.Collections.Generic;
using UnityEngine;
using static GenericExtensions;

public class PlayerController : MonoBehaviour {

    [Header("Weapons")]
    public GameObject unarmedObject;

    public GameObject[] startingWeapons;

    [Header("Shadow")]
    public GameObject shadowObj;

    public Sprite deadShadowSprite;

    [Header("Starting Pickups")]
    public int heldAmmoUSAM;

    public int heldAmmoShells;

    public int heldAmmoEnergy;

    public int heldKeys;

    public int heldMoney;

    [Header("Base Stats")]
    public int maxHealth = 20;

    [SerializeField]
    int armor = 0;

    public int Armor
    {
        get => armor;
        set
        {
            armor = value;
            if (armor != 0)
            {
                uiCanvasController.armorIconObj.SetActive(true);
                uiCanvasController.armorText.text = $"{armor}";
                if (armor < 0)
                    uiCanvasController.armorText.color = uiCanvasController.negativeArmorColor;
            }
            else
                uiCanvasController.armorIconObj.SetActive(false);
        }
    }

    public int weaponSlots = 2;

    [Header("Primary Stats")]   // currently 7: GlobalDamage, AttackSpeed, Range, ReloadSpeed, Accuracy, ShotSpeed, MoveSpeed - probably expand with Luck in the future
    [SerializeField]
    float globalDamageFlat = 1f;

    public float GlobalDamageFlat { get => globalDamageFlat;
        set { if (globalDamageFlat != value) { globalDamageFlat = value; RecalculateStat(PrimaryStat.GlobalDamage, StatChangeType.AddFlat); } } }

    [SerializeField]
    float globalDamageMultiplier = 1f;

    public float GlobalDamageMultiplier { get => globalDamageMultiplier;
        set { if (globalDamageMultiplier != value) { globalDamageMultiplier = value; RecalculateStat(PrimaryStat.GlobalDamage, StatChangeType.Multiply); } } }

    public float GlobalDamageTotal { get; private set; }

    [SerializeField]
    float attackSpeedFlat = 1f;

    public float AttackSpeedFlat { get => attackSpeedFlat;
        set { if (attackSpeedFlat != value) { attackSpeedFlat = value; RecalculateStat(PrimaryStat.AttackSpeed, StatChangeType.AddFlat); } } }

    [SerializeField]
    float attackSpeedMultiplier = 1f;

    public float AttackSpeedMultiplier { get => attackSpeedMultiplier;
        set { if (attackSpeedMultiplier != value) { attackSpeedMultiplier = value; RecalculateStat(PrimaryStat.AttackSpeed, StatChangeType.Multiply); } } }

    public float AttackSpeedTotal { get; private set; }

    [SerializeField]
    float rangeFlat = 1f;

    public float RangeFlat { get => rangeFlat;
        set { if (rangeFlat != value) { rangeFlat = value; RecalculateStat(PrimaryStat.Range, StatChangeType.AddFlat); } } }

    [SerializeField]
    float rangeMultiplier = 1f;

    public float RangeMultiplier { get => rangeMultiplier;
        set { if (rangeMultiplier != value) { rangeMultiplier = value; RecalculateStat(PrimaryStat.Range, StatChangeType.Multiply); } } }

    public float RangeTotal { get; private set; }

    [SerializeField]
    float reloadSpeedFlat = 1f;

    public float ReloadSpeedFlat { get => reloadSpeedFlat;
        set { if (reloadSpeedFlat != value) { reloadSpeedFlat = value; RecalculateStat(PrimaryStat.ReloadSpeed, StatChangeType.AddFlat); } } }

    [SerializeField]
    float reloadSpeedMultiplier = 1f;

    public float ReloadSpeedMultiplier { get => reloadSpeedMultiplier;
        set { if (reloadSpeedMultiplier != value) { reloadSpeedMultiplier = value; RecalculateStat(PrimaryStat.ReloadSpeed, StatChangeType.Multiply); } } }

    public float ReloadSpeedTotal { get; private set; }

    [SerializeField]
    float accuracyFlat = 1f;

    public float AccuracyFlat { get => accuracyFlat;
        set { if (accuracyFlat != value) { accuracyFlat = value; RecalculateStat(PrimaryStat.Accuracy, StatChangeType.AddFlat); } } }

    [SerializeField]
    float accuracyMultiplier = 1f;

    public float AccuracyMultiplier { get => accuracyMultiplier;
        set { if (accuracyMultiplier != value) { accuracyMultiplier = value; RecalculateStat(PrimaryStat.Accuracy, StatChangeType.Multiply); } } }

    public float AccuracyTotal { get; private set; }

    [SerializeField]
    float shotSpeedFlat = 1f;

    public float ShotSpeedFlat { get => shotSpeedFlat;
        set { if (shotSpeedFlat != value) { shotSpeedFlat = value; RecalculateStat(PrimaryStat.ShotSpeed, StatChangeType.AddFlat); } } }

    [SerializeField]
    float shotSpeedMultiplier = 1f;

    public float ShotSpeedMultiplier { get => shotSpeedMultiplier;
        set { if (shotSpeedMultiplier != value) { shotSpeedMultiplier = value; RecalculateStat(PrimaryStat.ShotSpeed, StatChangeType.Multiply); } } }

    public float ShotSpeedTotal { get; private set; }

    [SerializeField]
    float moveSpeedFlat = 2f;

    public float MoveSpeedFlat { get => moveSpeedFlat;
        set { if (moveSpeedFlat != value) { moveSpeedFlat = value; RecalculateStat(PrimaryStat.MoveSpeed, StatChangeType.AddFlat); } } }

    [SerializeField]
    float moveSpeedMultiplier = 1f;

    public float MoveSpeedMultiplier { get => moveSpeedMultiplier;
        set { if (moveSpeedMultiplier != value) { moveSpeedMultiplier = value; RecalculateStat(PrimaryStat.MoveSpeed, StatChangeType.Multiply); } } }

    public float MoveSpeedTotal { get; private set; }

    [Header("Weapon-specific Stats")]
    public float rangedDamageMultiplier = 1f;

    public float usamDamageMultiplier = 1f;

    public float shotgunAmmoDamageMultiplier = 1f;

    public float energyDamageMultiplier = 1f;

    public float rangedAttackSpeedMultiplier = 1f;

    [Space(20f)]
    public float meleeDamageMultiplier = 1f;

    public float meleeAttackSpeedMultiplier = 1f;

    public float unarmedDamageMultiplier = 1f;

    public float unarmedAttackSpeedMultiplier = 1f;

    [Header("Misc. Stats")]
    public float zoomMultiplier = 1f;

    public float knockbackMultiplier = 1f;

    public float abilityDurationMultiplier = 1f;

    public float abilityCooldownMultiplier = 1f;

    [Space(20f)]
    public bool flying = false;

    [Space(20f)]
    public int homingStrength = 0;

    [Space(20f)]
    public float luck = 0f; // move this to primary stats once it actually does something

    [HideInInspector]
    public float currentHealth;

    [HideInInspector]
    public int lastWeaponSlots;

    [HideInInspector]
    public int knockbackDirectionMultiplier = 1; // -1 for opposite direction, 0 for disabling knockback, 1 for normal direction

    [HideInInspector]
    public float damagedTintTime = 0.12f;

    [HideInInspector]
    public float invincibilityTintTime = 0.03f;

    [System.NonSerialized]
    public int shotsPerWaveMultiplier = 1;

    [System.NonSerialized]
    public float weaponPickupRange = 0.2f;

    [System.NonSerialized]
    public bool slippery = false;

    [System.NonSerialized]
    public int amountOfItemsObtained = 0;

    [System.NonSerialized]
    public int totalHealthRestored = 0;

    [HideInInspector]
    public float timeOfLatestKill;

    [System.NonSerialized]
    public int totalKillCount = 0;

    [System.NonSerialized]
    public int killCountThisLevel = 0;

    [System.NonSerialized]
    public int recentKillCount = 0;

    [System.NonSerialized]
    public Queue<float> recentKillTimes = new Queue<float>();

    int movingHash = Animator.StringToHash("moving");

    int flyingHash = Animator.StringToHash("flying");

    int deadAnimHash = Animator.StringToHash("Player_Nomad_DeadAnim1");

    [System.NonSerialized]
    public float latestCurrentHealth;

    [System.NonSerialized]
    public float latestMaxHealth;

    [System.NonSerialized]
    public bool flipped = false;

    [System.NonSerialized]
    public Rigidbody2D rb;

    [System.NonSerialized]
    public bool enableEnemyAggro = true;

    GameObject cameraObject;

    Camera cam;

    [System.NonSerialized]
    public Animator animator;

    [System.NonSerialized]
    public SpriteRenderer spriteRenderer;

    Color defaultColor;

    [System.NonSerialized]
    public float timeBeforeCleaningOffscreenChests = 5f;

    [System.NonSerialized]
    public float woodenChestWeaponDropChance = 10f;

    [System.NonSerialized]
    public float goldenChestItemChance = 25f;

    [System.NonSerialized]
    public float rangedWeaponChance = 50f;

    [System.NonSerialized]
    public List<GameObject> playerWeapons = new List<GameObject>();

    [System.NonSerialized]
    public List<GameObject> shotObjects = new List<GameObject>();

    [System.NonSerialized]
    public GameObject heldWeapon;   // for now, this is always the top object in the hierarchy, so for melee weapons this will be the parent of the MeleeAttacker

    [System.NonSerialized]
    public Shooter heldShooter;

    [System.NonSerialized]
    public MeleeAttacker heldMeleeAttacker;

    Vector3 mousePos;

    float moveHorizontal, moveVertical;

    [System.NonSerialized]
    public int curWeaponSlot = 1;

    [System.NonSerialized]
    public int actualPlayerWeaponsCount = 0;

    [System.NonSerialized]
    public UnarmedAttacker unarmedAttacker;

    [System.NonSerialized]
    public bool droppedWeaponOnThisFrame = false;

    [System.NonSerialized]
    public Coroutine damagedTinterCoroutine = null;

    [System.NonSerialized]
    public Coroutine invincibilityTinterCoroutine = null;

    [System.NonSerialized]
    public ItemPickupPopup itemPickupPopup;

    [System.NonSerialized]
    public WeaponPickupPopup weaponPickupPopup;

    [System.NonSerialized]
    public Queue<SpriteRenderer> spritesVisibleOnPlayer = new Queue<SpriteRenderer>();

    [System.NonSerialized]
    public int totalSpawnedID = 0;

    [System.NonSerialized]
    public CursorController cursorController;

    [System.NonSerialized]
    public bool zooming = false;

    [System.NonSerialized]
    public PauseMenu pauseMenu;

    [System.NonSerialized]
    public AudioManager audioManager;

    [System.NonSerialized]
    public LootManager lootManager;

    [System.NonSerialized]
    public int playerIndex = 0;

    [HideInInspector]
    static int numberOfPlayers = 0;

    [System.NonSerialized]
    public int invincibilityLayers = 0;

    [System.NonSerialized]
    public int preventDeathLayers = 0;

    [HideInInspector]
    public PolygonCollider2D polygonCollider2D;

    [System.NonSerialized]
    public bool allowControl = true;

    [HideInInspector]
    public WorldManager worldManager;

    [HideInInspector]
    public bool allowedToLeaveLevel = true;

    [HideInInspector]
    public bool isInRoom = false;

    [HideInInspector]
    public int amountOfEnemiesOnScreen = 0;

    [HideInInspector]
    public UICanvasController uiCanvasController;

    [HideInInspector]
    public PrimaryStatDisplayer primaryStatDisplayer;

    [HideInInspector]
    public GameOverController gameOverController;

    [HideInInspector]
    public RestartManager restartManager;

    [HideInInspector]
    public AchievementManager achievementManager;

    [System.NonSerialized]
    public int shotsMissedInARow = 0;

    [System.NonSerialized]
    public int shotsHitInARow = 0;

    float instaRestartButtonStartTime = Mathf.Infinity;

    SpriteRenderer shadowSpriteRenderer;

    [System.NonSerialized]
    public float enemyScaledMoveSpeedMultiplier = 1f;


    // status effects & other chance-based effects section

    [System.NonSerialized]
    public float burnChance = 0f;

    [System.NonSerialized]
    public int maxBurnStacks = 1;

    [System.NonSerialized]
    public float burnDuration = 5.05f;

    [System.NonSerialized]
    public float burnTickTime = 1f;

    [System.NonSerialized]
    public float burnTickDmg = 5f;

    [System.NonSerialized]
    public float bleedChance = 0f;

    [System.NonSerialized]
    public int maxBleedStacks = 1;

    [System.NonSerialized]
    public float bleedDuration = 5.05f;

    [System.NonSerialized]
    public float bleedTickTime = 0.75f;

    [System.NonSerialized]
    public float bleedTickDmgMultiplier = 0.2f;

    [System.NonSerialized]
    public float chillChance = 0f;

    [System.NonSerialized]
    public int maxChillStacks = 1;

    [System.NonSerialized]
    public float chillDuration = 5.05f;

    [System.NonSerialized]
    public float chillSlowdown = 0.7f;

    [System.NonSerialized]
    public float doubleShotsChance = 0f;

    [System.NonSerialized]
    public float pierceChance = 0f;

    [System.NonSerialized]
    public float ghostlyChance = 0f;


    // item scripts section

    [System.NonSerialized]
    public ChilledChilli chilledChilli = null;

    [System.NonSerialized]
    public KillerCereal killerCereal = null;

    [System.NonSerialized]
    public UnstableConcoction unstableConcoction = null;

    [System.NonSerialized]
    public AbyssalGaze abyssalGaze = null;

    [System.NonSerialized]
    public InvincibilityGlitter invincibilityGlitter = null;

    [System.NonSerialized]
    public AdrenalGland adrenalGland = null;

    [System.NonSerialized]
    public RubberBandBall rubberBandBall = null;

    [System.NonSerialized]
    public DeathsBeckoning deathsBeckoning = null;

    [System.NonSerialized]
    public GunNut gunNut = null;

    [System.NonSerialized]
    public SniperGuildBadge sniperGuildBadge = null;

    [System.NonSerialized]
    public EmptyCase emptyCase = null;

    [System.NonSerialized]
    public VampireFang vampireFang = null;

    [System.NonSerialized]
    public ExtendedMagazine extendedMagazine = null;

    [System.NonSerialized]
    public GodParticle godParticle = null;

    public float GetPrimaryStatValueByID(PrimaryStat primaryStat, StatRetrieveType statRetrieveType)
    {
        switch(primaryStat)
        {
            case PrimaryStat.GlobalDamage:
                switch (statRetrieveType)
                {
                    case StatRetrieveType.Total: return GlobalDamageTotal;
                    case StatRetrieveType.Flat: return GlobalDamageFlat;
                    case StatRetrieveType.Multiplier: return GlobalDamageMultiplier;
                }
                break;
            case PrimaryStat.AttackSpeed:
                switch (statRetrieveType)
                {
                    case StatRetrieveType.Total: return AttackSpeedTotal;
                    case StatRetrieveType.Flat: return AttackSpeedFlat;
                    case StatRetrieveType.Multiplier: return AttackSpeedMultiplier;
                }
                break;
            case PrimaryStat.Range:
                switch (statRetrieveType)
                {
                    case StatRetrieveType.Total: return RangeTotal;
                    case StatRetrieveType.Flat: return RangeFlat;
                    case StatRetrieveType.Multiplier: return RangeMultiplier;
                }
                break;
            case PrimaryStat.ReloadSpeed:
                switch (statRetrieveType)
                {
                    case StatRetrieveType.Total: return ReloadSpeedTotal;
                    case StatRetrieveType.Flat: return ReloadSpeedFlat;
                    case StatRetrieveType.Multiplier: return ReloadSpeedMultiplier;
                }
                break;
            case PrimaryStat.Accuracy:
                switch (statRetrieveType)
                {
                    case StatRetrieveType.Total: return AccuracyTotal;
                    case StatRetrieveType.Flat: return AccuracyFlat;
                    case StatRetrieveType.Multiplier: return AccuracyMultiplier;
                }
                break;
            case PrimaryStat.ShotSpeed:
                switch (statRetrieveType)
                {
                    case StatRetrieveType.Total: return ShotSpeedTotal;
                    case StatRetrieveType.Flat: return ShotSpeedFlat;
                    case StatRetrieveType.Multiplier: return ShotSpeedMultiplier;
                }
                break;
            case PrimaryStat.MoveSpeed:
                switch (statRetrieveType)
                {
                    case StatRetrieveType.Total: return MoveSpeedTotal;
                    case StatRetrieveType.Flat: return MoveSpeedFlat;
                    case StatRetrieveType.Multiplier: return MoveSpeedMultiplier;
                }
                break;
        }
        return -1f;
    }

    void RecalculateStat(PrimaryStat stat, StatChangeType changeType)
    {
        float valueBefore = -1f;
        float valueAfter = -1f;

        switch(stat)
        {
            case PrimaryStat.GlobalDamage: valueBefore = GlobalDamageTotal; GlobalDamageTotal = globalDamageFlat * globalDamageMultiplier; valueAfter = GlobalDamageTotal; break;
            case PrimaryStat.AttackSpeed: valueBefore = AttackSpeedTotal; AttackSpeedTotal = attackSpeedFlat * attackSpeedMultiplier; valueAfter = AttackSpeedTotal; break;
            case PrimaryStat.Range: valueBefore = RangeTotal; RangeTotal = rangeFlat * rangeMultiplier; valueAfter = RangeTotal; break;
            case PrimaryStat.ReloadSpeed: valueBefore = ReloadSpeedTotal; ReloadSpeedTotal = reloadSpeedFlat * reloadSpeedMultiplier; valueAfter = ReloadSpeedTotal; break;
            case PrimaryStat.Accuracy: valueBefore = AccuracyTotal; AccuracyTotal = accuracyFlat * accuracyMultiplier; valueAfter = AccuracyTotal; break;
            case PrimaryStat.ShotSpeed: valueBefore = ShotSpeedTotal; ShotSpeedTotal = shotSpeedFlat * shotSpeedMultiplier; valueAfter = ShotSpeedTotal; break;
            case PrimaryStat.MoveSpeed: valueBefore = MoveSpeedTotal; MoveSpeedTotal = moveSpeedFlat * moveSpeedMultiplier; valueAfter = MoveSpeedTotal; break;
        }

        if (changeType != StatChangeType.DontDisplay)
            primaryStatDisplayer.StartDisplayPrimaryStatChange(stat, valueBefore, valueAfter, isFlat: changeType == StatChangeType.AddFlat ? true : false);
    }    

    public void SetPrimaryStatByString(string statName, float value)
    {
        switch(statName.ToLowerInvariant())
        {
            case "globaldamageflat": GlobalDamageFlat = value;
                break;
            case "globaldamagemulti": GlobalDamageMultiplier = value;
                break;
            case "attackspeedflat": AttackSpeedFlat = value;
                break;
            case "attackspeedmulti": AttackSpeedMultiplier = value;
                break;
            case "rangeflat": RangeFlat = value;
                break;
            case "rangemulti": RangeMultiplier = value;
                break;
            case "reloadspeedflat": ReloadSpeedFlat = value;
                break;
            case "reloadspeedmulti": ReloadSpeedMultiplier = value;
                break;
            case "accuracyflat": AccuracyFlat = value;
                break;
            case "accuracymulti": AccuracyMultiplier = value;
                break;
            case "shotspeedflat": ShotSpeedFlat = value;
                break;
            case "shotspeedmulti": ShotSpeedMultiplier = value;
                break;
            case "movespeedflat": MoveSpeedFlat = value;
                break;
            case "movespeedmulti": MoveSpeedMultiplier = value;
                break;

            default:
                Debug.LogWarning($"Unrecognized stat name \"{statName.ToLowerInvariant()}\". Remember that primary stat names end with \"flat\" or \"multi\"");
                break;
        }
    }

    public int GetHeldAmmoCount(AmmoType ammoType)
    {
        switch(ammoType)
        {
            case AmmoType.USAM:
                return heldAmmoUSAM;
            case AmmoType.Shells:
                return heldAmmoShells;
            case AmmoType.Energy:
                return heldAmmoEnergy;

            default:
                LogWarningForUnrecognizedValue(ammoType);
                return 0;
        }
    }

    public void AddToHeldAmmoCount(AmmoType ammoType, int count)
    {
        switch (ammoType)
        {
            case AmmoType.USAM:
                heldAmmoUSAM += count;
                break;
            case AmmoType.Shells:
                heldAmmoShells += count;
                break;
            case AmmoType.Energy:
                heldAmmoEnergy += count;
                break;

            default:
                LogWarningForUnrecognizedValue(ammoType);
                break;
        }
    }

    public void SetHeldAmmoCount(AmmoType ammoType, int count)
    {
        switch (ammoType)
        {
            case AmmoType.USAM:
                heldAmmoUSAM = count;
                break;
            case AmmoType.Shells:
                heldAmmoShells = count;
                break;
            case AmmoType.Energy:
                heldAmmoEnergy = count;
                break;

            default:
                LogWarningForUnrecognizedValue(ammoType);
                break;
        }
    }

    private void Awake()
    {
        for (int i = 0; i < System.Enum.GetNames(typeof(PrimaryStat)).Length; i++)  // initialize PrimaryStat properties
            RecalculateStat((PrimaryStat)i, StatChangeType.DontDisplay);

        restartManager = FindObjectOfType<RestartManager>();
        restartManager.DontDestroyOnLoadButDestroyWhenRestarting(gameObject);
        achievementManager = FindObjectOfType<AchievementManager>();
        numberOfPlayers = 1;
        playerIndex = numberOfPlayers - 1;
        audioManager = FindObjectOfType<AudioManager>();
        lootManager = FindObjectOfType<LootManager>();
        rb = GetComponent<Rigidbody2D>();
        polygonCollider2D = GetComponent<PolygonCollider2D>();
        cameraObject = GameObject.FindWithTag("MainCamera");
        cam = cameraObject.GetComponent<Camera>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        latestCurrentHealth = currentHealth;
        latestMaxHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        unarmedAttacker = unarmedObject.GetComponent<UnarmedAttacker>();
        cursorController = FindObjectOfType<CursorController>();
        worldManager = FindObjectOfType<WorldManager>();
        worldManager.playerObject = gameObject;
        worldManager.playerController = this;
        worldManager.allowWaystoneSpawning = true;

        shadowSpriteRenderer = shadowObj.GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        unarmedObject.SetActive(false);

        for(int weaponId = 0; weaponId < startingWeapons.Length; weaponId++)
        {
            playerWeapons.Add(startingWeapons[weaponId]);
            startingWeapons[weaponId].SetActive(false);
            actualPlayerWeaponsCount++;
        }

        if (playerWeapons.Count >= 1)
        {
            heldWeapon = playerWeapons[0];
            heldWeapon.SetActive(true);

            if (heldWeapon.GetComponent<Shooter>())
            {
                heldShooter = heldWeapon.GetComponent<Shooter>();
                heldShooter.currentlyHeldByPlayer = true;

                worldManager.UpdateWeaponSpriteLayerAndCollider(heldShooter, ownedByPlayer: true);
            }
            else
            {
                heldMeleeAttacker = heldWeapon.GetComponentInChildren<MeleeAttacker>();
                heldMeleeAttacker.currentlyHeldByPlayer = true;

                worldManager.UpdateWeaponSpriteLayerAndCollider(heldMeleeAttacker, ownedByPlayer: true);
            }
        }
        else
            GoUnarmed();

        if (weaponSlots > playerWeapons.Count)
        {
            for (int i = weaponSlots - playerWeapons.Count; i > 0; i--)
                playerWeapons.Add(null);
        }
        lastWeaponSlots = weaponSlots;
    }

    public void UpdateDamageGlobal(ref float damage)
    {
        damage *= GlobalDamageTotal;

        if (killerCereal != null)
            damage *= 1 + Mathf.Min(killerCereal.dmgBonusPerRecentKill * recentKillCount, killerCereal.maxDmgBonus);
    }

    public void UpdateDamageRanged(ref float damage)
    {
        damage *= rangedDamageMultiplier;
    }

    public void UpdateDamageUSAM(ref float damage)
    {
        damage *= usamDamageMultiplier;
    }

    public void UpdateDamageShotgunAmmo(ref float damage)
    {
        damage *= shotgunAmmoDamageMultiplier;
    }

    public void UpdateDamageEnergy(ref float damage)
    {
        damage *= energyDamageMultiplier;
    }

    public void UpdateDamageMelee(ref float damage)
    {
        damage *= meleeDamageMultiplier;
    }

    public void UpdateDamageUnarmed(ref float damage)
    {
        damage *= unarmedDamageMultiplier;
    }

    public void UpdateOnShootEffects(ShotMover shotMover)
    {
        if(godParticle != null)
        {
            shotMover.transform.localScale = new Vector2(shotMover.transform.localScale.x / 5f, shotMover.transform.localScale.y);
            float distBetweenParticleShots = shotMover.spriteRenderer.sprite.bounds.size.x / 2.5f;
            float distFromInitialShot = 0f;
            for (int extraParticleShotID = 0; extraParticleShotID < godParticle.extraParticles; extraParticleShotID++)
            {
                distFromInitialShot += distBetweenParticleShots;
                Transform curParticleShotTransform = Instantiate(shotMover.gameObject, shotMover.transform.position, shotMover.transform.rotation, shotMover.parentObject.transform).transform;
                curParticleShotTransform.position += curParticleShotTransform.right.normalized * -distFromInitialShot;
            }
        }

        if (rubberBandBall != null)
            shotMover.bouncesLeft += rubberBandBall.maxBounces;

        if(shotMover.bouncesLeft > 0)
            shotMover.destroyOnHittingCollider = false;
    }

    public void UpdateOnShotWaveEffects(ShotMover lastShotMover, float lastShotRotationZ, int amountOfShotsInWave)
    {
        if (emptyCase != null)
            emptyCase.ShootCase(lastShotRotationZ, lastShotMover.damage, amountOfShotsInWave);
    }

    public void UpdatePropertiesOfShot(ShotMover shotMover)
    {
        int shotHomingStrength = (shotMover.shooter == null ? 0 : shotMover.shooter.homingStrength) + homingStrength;

        if(shotHomingStrength > 0)
        {
            /*  // brackey's solution, angle doesn't work
            Vector2 direction = ((Vector2)GenericExtensions.GetClosestGameObject(shotMover.transform.position, worldManager.enemiesInCurrentLevel).transform.position - shotMover.rb.position).normalized;
            float rotateAmount = Vector3.Cross(direction, shotMover.transform.up).z;
            rb.angularVelocity = -rotateAmount * 1000;
            */

            float angleDifference = GenericExtensions.GetRotationToFaceTargetPosition(shotMover.transform.position,
                GenericExtensions.GetClosestGameObject(shotMover.transform.position, worldManager.enemiesInCurrentLevel).transform.position) - shotMover.transform.rotation.eulerAngles.z;

            if (angleDifference < -180)
                angleDifference += 360;
            else if (angleDifference > 180)
                angleDifference -= 360;

            shotMover.rb.angularVelocity = angleDifference * shotHomingStrength;

            shotMover.rb.velocity = shotMover.rb.velocity.magnitude * shotMover.transform.right;
        }

        if(unstableConcoction != null)
        {
            shotMover.unstableConcoctionMultiplier = Random.Range(unstableConcoction.minMultiplier, unstableConcoction.maxMultiplier);
            shotMover.transform.localScale = new Vector2(shotMover.unstableConcoctionMultiplier, shotMover.unstableConcoctionMultiplier);
        }
    }

    public void UpdateDamageShot(GameObject shot, ref float damage, GameObject hitObj)
    {
        ShotMover shotMover = shot.GetComponent<ShotMover>();
        if (unstableConcoction != null)
            damage *= shotMover.unstableConcoctionMultiplier;

        if (sniperGuildBadge != null)
            damage *= 1 + Mathf.Clamp(Vector2.Distance(shotMover.spawnPos, hitObj.transform.position) / sniperGuildBadge.distanceBonusDivider, 0f, sniperGuildBadge.maxDmgMultiplierBonus);
    }

    public void UpdateDamageAttackWave(GameObject attackWave, ref float damage)
    {

    }

    public void UpdateTotalDamage(GameObject damageDealer, ref float totalDamage, GameObject hitObject)
    {
        ShotMover shotMover = damageDealer.GetComponent<ShotMover>();
        AttackWaveController attackWaveController = damageDealer.GetComponent<AttackWaveController>();
        if(shotMover != null)
        {
            if (shotMover.shooter != null)
                totalDamage = shotMover.shooter.damage;
            if(shotMover.meleeAttacker != null)
            {
                GlassShank glassShank = shotMover.meleeAttacker.GetComponent<GlassShank>();
                if (glassShank != null)
                    totalDamage = shotMover.meleeAttacker.damage * glassShank.shardDamageMultiplier;
            }
        }
        if(attackWaveController != null)
        {
            if (attackWaveController.meleeAttacker != null)
                totalDamage = attackWaveController.meleeAttacker.damage;
            if (attackWaveController.unarmedAttacker != null)
                totalDamage = attackWaveController.unarmedAttacker.damage;

            SpectralBlade spectralBlade = attackWaveController.GetComponent<SpectralBlade>();
            if (spectralBlade != null)
                totalDamage *= spectralBlade.partOfTimeLeft;
        }

        if(deathsBeckoning != null)
            totalDamage *= 1 + Mathf.Clamp(deathsBeckoning.maxDmgMultiplierBonus / (Vector2.Distance(transform.position, hitObject.transform.position) * deathsBeckoning.distancePenaltyMultiplier), 0f, deathsBeckoning.maxDmgMultiplierBonus);
    }

    public void UpdateOnHitEffects(GameObject weaponObj, GameObject hitterObj, GameObject hitObj, bool hitWall)
    {
        if (!hitWall)
        {
            EnemyController enemyController = hitObj.GetComponent<EnemyController>();
            ShotMover shotMover = hitterObj.GetComponent<ShotMover>();
            AttackWaveController attackWaveController = hitterObj.GetComponent<AttackWaveController>();

            if (enemyController != null)
            {
                float burnRoll = GenericExtensions.RollRandom0To100();
                float chillRoll = GenericExtensions.RollRandom0To100();
                float bleedRoll = GenericExtensions.RollRandom0To100();

                float totalBurnChance = burnChance;
                float totalChillChance = chillChance;
                float totalBleedChance = bleedChance;

                if (shotMover != null)
                {
                    totalBurnChance += shotMover.burnChance;
                    totalChillChance += shotMover.chillChance;
                    totalBleedChance += shotMover.bleedChance;
                    if (shotMover.shooter != null)
                    {
                        totalBurnChance += shotMover.shooter.burnChance;
                        totalChillChance += shotMover.shooter.chillChance;
                        totalBleedChance += shotMover.shooter.bleedChance;
                    }
                }
                else if (attackWaveController != null)
                {
                    totalBurnChance += attackWaveController.burnChance;
                    totalChillChance += attackWaveController.chillChance;
                    totalBleedChance += attackWaveController.bleedChance;
                    if (attackWaveController.meleeAttacker != null)
                    {
                        totalBurnChance += attackWaveController.meleeAttacker.burnChance;
                        totalChillChance += attackWaveController.meleeAttacker.chillChance;
                        totalBleedChance += attackWaveController.meleeAttacker.bleedChance;
                    }
                }

                if (burnRoll <= totalBurnChance)
                {
                    enemyController.ApplyBurn();

                    if (chilledChilli != null)
                        enemyController.ApplyChill();
                }
                if (chillRoll <= totalChillChance)
                {
                    enemyController.ApplyChill();

                    if (chilledChilli != null)
                        enemyController.ApplyBurn();
                }
                if (bleedRoll <= totalBleedChance)
                {
                    enemyController.ApplyBleed(sourceOfBleed: hitterObj);
                }
            }
        }


        GodRaySpawner godRaySpawner = hitterObj.GetComponent<GodRaySpawner>();
        if (godRaySpawner != null && godRaySpawner.currentAmountSpawned < godRaySpawner.maxAmountToSpawn)
            godRaySpawner.SpawnGodRay(hitObj.transform.position);
    }

    public void UpdateOnMeleeHitEffects(GameObject hitterObj, GameObject hitObj, Vector2 contactPoint)
    {
        AttackWaveController attackWaveController = hitterObj.GetComponent<AttackWaveController>();
        if (attackWaveController != null)
        {
            MeleeAttacker meleeAttacker = attackWaveController.meleeAttacker;
            EnemyController enemyController = hitObj.GetComponent<EnemyController>();
            ObstacleController obstacleController = hitObj.GetComponent<ObstacleController>();

            if (meleeAttacker != null)
            {
                GlassShank glassShank = meleeAttacker.GetComponent<GlassShank>();
                if (glassShank != null)
                {
                    for (int i = Random.Range(glassShank.minAmountOfShards, glassShank.maxAmountOfShards + 1); i > 0; i--)
                    {
                        ShotMover glassShardShotMover = Instantiate(glassShank.glassShards[Random.Range(0, glassShank.glassShards.Length)], contactPoint,
                            Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)), hitterObj.transform).GetComponent<ShotMover>();

                        if(enemyController != null)
                            glassShardShotMover.spawnedIDsToIgnore.Add(enemyController.spawnedID);
                        if(obstacleController != null)
                            glassShardShotMover.spawnedIDsToIgnore.Add(obstacleController.spawnedID);

                        glassShardShotMover.maxDuration = glassShank.shardMaxDuration;
                        glassShardShotMover.rb.velocity = -glassShardShotMover.transform.right * glassShank.shardShotSpeed * ShotSpeedTotal;
                        glassShardShotMover.damage = meleeAttacker.damage * glassShank.shardDamageMultiplier;
                    }
                }
            }
        }
    }

    public void UpdateOnShotHitEffects(GameObject shotObj, GameObject hitObj, Vector2 contactPoint, bool hitWall)
    {
        ShotMover shotMover = shotObj.GetComponent<ShotMover>();
        Shooter shooter = shotMover.shooter;

        if (!hitWall)
        {
            EnemyController enemyController = hitObj.GetComponent<EnemyController>();

            shotsMissedInARow = 0;
            shotsHitInARow++;

            if (shotsHitInARow > achievementManager.reqs[(int)ReqName.HighestShotsHitInARow])
                achievementManager.reqs[(int)ReqName.HighestShotsHitInARow] = shotsHitInARow;

            if (enemyController != null)
            {
                if (shooter != null)
                {
                    enemyController.CurrentHealth -= 0f; // placeholder to not have unused variable warning
                }
            }
        }

        if (shotMover.bouncesLeft > 0)
        {
            shotMover.bouncesLeft--;

            Vector2 diff = (Vector2)hitObj.transform.position - contactPoint;
            diff.Normalize();
            float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            shotMover.transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 180f + Random.Range(-45f, 45f));

            shotMover.rb.velocity = shotMover.rb.velocity.magnitude * shotObj.transform.right;

            if (shotMover.bouncesLeft == 0)
                shotMover.destroyOnHittingCollider = true;
        }
    }

    public void UpdateOnShotDestroyedWithoutHittingEnemyOrObstacleEffects()
    {
        shotsHitInARow = 0;
        shotsMissedInARow++;

        if(shotsMissedInARow > achievementManager.reqs[(int)ReqName.HighestShotsMissedInARow])
            achievementManager.reqs[(int)ReqName.HighestShotsMissedInARow] = shotsMissedInARow;
    }

    public void UpdateOnKillEffects()
    {
        if(adrenalGland != null && adrenalGland.countingDown && currentHealth <= 0)
        {
            currentHealth = 1;
            preventDeathLayers--;
            adrenalGland.countingDown = false;
            adrenalGland.countdownDuration = adrenalGland.remainingTime;
            adrenalGland.countdownStartTime = Mathf.NegativeInfinity;
            adrenalGland.StopCountdownHighlighter();
            adrenalGland.adrenalGlandCountdownObject.SetActive(false);
        }
    }

    public void UpdateOnKillWITHStatusEffectEffects(StatusEffect statusEffectSourceOfKill, EnemyController enemyController)
    {
        if(statusEffectSourceOfKill == StatusEffect.bleed)
        {

        }

        if(statusEffectSourceOfKill == StatusEffect.burn)
        {

        }
    }

    public void UpdateOnKillDURINGStatusEffectEffects(EnemyController enemyController)
    {
        if(vampireFang != null && currentHealth < maxHealth)
        {
            if (enemyController.isBoss == false)
                currentHealth = Mathf.Min(currentHealth + vampireFang.amountToHealFromRegularEnemies, maxHealth);
            else
                currentHealth = Mathf.Min(currentHealth + vampireFang.amountToHealFromBosses, maxHealth);
        }
    }

    public void UpdateOnMeleeKillEffects(GameObject attackWaveObj, GameObject enemyObj)
    {
        AttackWaveController attackWaveController = attackWaveObj.GetComponent<AttackWaveController>();
        MeleeAttacker meleeAttacker = attackWaveController.meleeAttacker;
        EnemyController enemyController = enemyObj.GetComponent<EnemyController>();
        if (meleeAttacker != null)
        {
            TheReapersGrasp theReapersGrasp = meleeAttacker.GetComponent<TheReapersGrasp>();
            LifeGainOnKill lifeGainOnKill = meleeAttacker.GetComponent<LifeGainOnKill>();

            if (!enemyController.isBoss)
            {
                if (theReapersGrasp != null)
                {
                    GlobalDamageTotal += theReapersGrasp.regularEnemyDamageBonus;
                    theReapersGrasp.StartConnectSoulStealParticlesWithPlayer(enemyObj.transform.position);
                }

                if(lifeGainOnKill != null)
                {
                    if (lifeGainOnKill.regularIsFlat)
                    {
                        currentHealth += lifeGainOnKill.regularKillLifeGain;
                        totalHealthRestored += Mathf.RoundToInt(lifeGainOnKill.regularKillLifeGain);
                    }
                    else
                    {
                        currentHealth += maxHealth * lifeGainOnKill.regularKillLifeGain;
                        totalHealthRestored += Mathf.RoundToInt(maxHealth * lifeGainOnKill.regularKillLifeGain);
                    }
                }
            }
            else
            {
                if(theReapersGrasp != null)
                    GlobalDamageTotal += theReapersGrasp.bossDamageBonus;

                if (lifeGainOnKill != null)
                {
                    if (lifeGainOnKill.bossIsFlat)
                    {
                        currentHealth += lifeGainOnKill.bossKillLifeGain;
                        totalHealthRestored += Mathf.RoundToInt(lifeGainOnKill.bossKillLifeGain);
                    }
                    else
                    {
                        currentHealth += maxHealth * lifeGainOnKill.bossKillLifeGain;
                        totalHealthRestored += Mathf.RoundToInt(maxHealth * lifeGainOnKill.bossKillLifeGain);
                    }
                }
            }
        }
    }

    public void UpdateOnRangedKillEffects(GameObject shotObj, GameObject enemyObj)
    {
        ShotMover shotMover = shotObj.GetComponent<ShotMover>();
        Shooter shooter = shotMover.shooter;
        EnemyController enemyController = enemyObj.GetComponent<EnemyController>();

        if(shooter != null) // placeholder
        {

        }

        if(enemyController != null) // placeholder
        {

        }
    }

    public void UpdateAttackWaveEffects(GameObject attackWave)
    {
        AttackWaveController attackWaveController = attackWave.GetComponent<AttackWaveController>();
        MeleeAttacker meleeAttacker = attackWaveController.meleeAttacker;
        if(meleeAttacker != null)
        {

        }
    }

    public void UpdateAfterGettingHitEffects()
    {
        if(adrenalGland != null && adrenalGland.countingDown)
        {
            adrenalGland.countdownDuration--;
            adrenalGland.CountdownUpdate();
            adrenalGland.StartCountdownHighlighter();
        }
        if(invincibilityGlitter != null)
        {
            worldManager.StartAddInvincibilityLayer(invincibilityGlitter.invincibilityPeriod);
        }
    }

    public void UpdateOnDeathEffects()
    {
        if(adrenalGland != null)
        {
            preventDeathLayers++;
            adrenalGland.countingDown = true;
            adrenalGland.CountdownUpdate();
        }
    }

    public void UpdateOnGlobalWeaponChangingEffects(Shooter shooter)
    {
        if(extendedMagazine && !extendedMagazine.spawnedIDsOfShootersWithExtendedMagApplied.Contains(shooter.spawnedID))
        {
            shooter.maxAmmo *= 2;
            extendedMagazine.spawnedIDsOfShootersWithExtendedMagApplied.Add(shooter.spawnedID);
        }
    }

    public void UpdateOnGlobalWeaponChangingEffects(MeleeAttacker meleeAttacker)
    {

    }

    void ChangeWeapon(int weaponSlot)
    {
        if (weaponSlots >= weaponSlot && curWeaponSlot != weaponSlot)
        {
            if (playerWeapons[weaponSlot-1])    // if chosen slot has a weapon (isn't unarmed)
            {
                if (unarmedObject.activeSelf)
                    unarmedObject.SetActive(false);

                if (heldWeapon)
                    UnequipHeldWeapon(reactivate: false);

                heldWeapon = playerWeapons[weaponSlot - 1];
                heldWeapon.SetActive(true);

                heldShooter = heldWeapon.GetComponent<Shooter>();
                heldMeleeAttacker = heldWeapon.GetComponentInChildren<MeleeAttacker>();

                if (heldShooter)
                {
                    heldShooter.currentlyHeldByPlayer = true;

                    worldManager.UpdateWeaponSpriteLayerAndCollider(heldShooter, true);

                    UpdateOnGlobalWeaponChangingEffects(heldShooter);

                    heldShooter.Update();
                }
                else
                {
                    heldMeleeAttacker.currentlyHeldByPlayer = true;

                    worldManager.UpdateWeaponSpriteLayerAndCollider(heldMeleeAttacker, true);

                    UpdateOnGlobalWeaponChangingEffects(heldMeleeAttacker);

                    heldMeleeAttacker.Update();
                }
            }
            else
                GoUnarmed();


            curWeaponSlot = weaponSlot;
        }
    }

    object UnequipHeldWeapon(bool reactivate)  // returns unequipped weapon script
    {
        if (heldWeapon)
        {
            if (heldShooter)
            {
                heldShooter.reloading = false;
                heldShooter.reloadingOneShot = false;

                heldShooter.currentlyHeldByPlayer = false;
            }
            else
            {
                heldMeleeAttacker.attacking = false;
                heldMeleeAttacker.curAttackPhase = MeleeAttacker.AttackPhase.NotSwinging;

                heldMeleeAttacker.currentlyHeldByPlayer = false;
            }


            heldWeapon.SetActive(false);    // don't need to stop coroutines because disabling an object stops all coroutines

            if (reactivate)
                heldWeapon.SetActive(true);


            Shooter droppedShooter = heldShooter;
            MeleeAttacker droppedMeleeAttacker = heldMeleeAttacker;


            heldWeapon = null;
            heldShooter = null;
            heldMeleeAttacker = null;

            if(droppedShooter)
                return droppedShooter;
            return droppedMeleeAttacker;
        }
        return null;
    }

    void GoUnarmed()
    {
        if(!unarmedObject.activeSelf)
        {
            UnequipHeldWeapon(reactivate: false);

            unarmedObject.SetActive(true);
        }
    }

    public void DropCurrentWeapon()
    {
        droppedWeaponOnThisFrame = true;

        if (heldShooter)
        {
            heldShooter.transform.SetParent(null);

            Shooter droppedShooter = (Shooter)UnequipHeldWeapon(reactivate: true);

            worldManager.UpdateWeaponSpriteLayerAndCollider(droppedShooter, ownedByPlayer: false);

            worldManager.MoveOnGround(droppedShooter.transform, 0.4f);

            worldManager.allWeaponsOnTheGround.Add(droppedShooter.gameObject);
        }
        else
        {
            heldMeleeAttacker.transform.parent.SetParent(null);

            MeleeAttacker droppedMeleeAttacker = (MeleeAttacker)UnequipHeldWeapon(reactivate: true);

            worldManager.UpdateWeaponSpriteLayerAndCollider(droppedMeleeAttacker, ownedByPlayer: false);

            worldManager.MoveOnGround(droppedMeleeAttacker.transform, 0.4f);

            worldManager.allWeaponsOnTheGround.Add(droppedMeleeAttacker.gameObject);
        }

    }

    private void Update()
    {   // update achievements first
        if (totalHealthRestored > achievementManager.reqs[(int)ReqName.HighestHealthRestored])
            achievementManager.reqs[(int)ReqName.HighestHealthRestored] = totalHealthRestored;

        if (Input.GetKeyDown(KeyCode.I))
            instaRestartButtonStartTime = Time.time;
        else
        {
            if (Input.GetKey(KeyCode.I))
            {
                if (instaRestartButtonStartTime == Mathf.Infinity)
                    instaRestartButtonStartTime = Time.time;
                else if (Time.time >= instaRestartButtonStartTime + 1f)
                {
                    gameOverController.gameObject.SetActive(true);
                    gameOverController.TryAgain();
                }
            }
            else
                instaRestartButtonStartTime = Mathf.Infinity;
        }

        if (allowControl)
        {
            enemyScaledMoveSpeedMultiplier = Mathf.Min(MoveSpeedTotal * moveSpeedMultiplier, 5f) / 2; // base move speed to consider is 2f, so 2f/2 = 1f; 5f/2 = 2.5f

            if (enemyScaledMoveSpeedMultiplier > 1)
                enemyScaledMoveSpeedMultiplier = enemyScaledMoveSpeedMultiplier * (2f / 3f) + (1f / 3f);    // scales linearly up to 2.5x player move speed = 2x enemy move speed

            Vector3 screenPos = cam.WorldToScreenPoint(transform.localPosition);
            if (!cursorController.arrowKeyAiming)
            {
                if (Input.mousePosition.x < screenPos.x && !flipped)
                {
                    transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
                    flipped = true;
                }
                else if (Input.mousePosition.x >= screenPos.x && flipped)
                {
                    transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
                    flipped = false;
                }
            }
            else
            {
                if (cam.WorldToScreenPoint(cursorController.transform.position).x < screenPos.x && !flipped)
                {
                    transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
                    flipped = true;
                }
                else if (cam.WorldToScreenPoint(cursorController.transform.position).x >= screenPos.x && flipped)
                {
                    transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
                    flipped = false;
                }
            }


            if (!slippery)
            {
                moveHorizontal = Input.GetAxisRaw("Horizontal");
                moveVertical = Input.GetAxisRaw("Vertical");
            }
            else
            {
                moveHorizontal = Input.GetAxis("Horizontal");
                moveVertical = Input.GetAxis("Vertical");
            }

            Vector2 movement = new Vector2(moveHorizontal, moveVertical);

            rb.velocity = movement * MoveSpeedTotal * moveSpeedMultiplier;
        }
        else
            rb.velocity = Vector2.zero;


        if (rb.velocity == Vector2.zero)
            animator.SetBool(movingHash, false);
        else
            animator.SetBool(movingHash, true);


        if (allowControl && Input.GetKeyDown(KeyCode.E) && curWeaponSlot != 0 && weaponSlots > 0 && !droppedWeaponOnThisFrame)  // pick up closest weapon
        {
            GameObject closestWeapon = null;
            foreach (GameObject weaponObj in worldManager.allWeaponsOnTheGround)
            {
                float shortestSqrMag = Mathf.Infinity;
                float sqrMag = (transform.position - weaponObj.transform.position).sqrMagnitude;
                if (weaponObj.transform.root != transform && sqrMag <= weaponPickupRange)
                {
                    if (sqrMag < shortestSqrMag)
                        closestWeapon = weaponObj;
                }
            }
            if(closestWeapon)
            {
                Shooter closestShooter = closestWeapon.GetComponent<Shooter>();
                if (closestShooter)
                    closestShooter.PickUp();
                else
                    closestWeapon.GetComponentInChildren<MeleeAttacker>().PickUp();
            }            
        }


        for (int i = 0; i < recentKillTimes.Count; i++)
        {
            if (recentKillTimes.Peek() < Time.time - 10f)
                recentKillTimes.Dequeue();
        }

        recentKillCount = recentKillTimes.Count;
        if (recentKillCount > achievementManager.reqs[(int)ReqName.MostEnemiesKilledRecently])
            achievementManager.reqs[(int)ReqName.MostEnemiesKilledRecently] = recentKillCount;


        if (flying && (!Physics2D.GetIgnoreLayerCollision(8, 11) || !animator.GetBool(flyingHash)))
        {
            Physics2D.IgnoreLayerCollision(8, 11);
            animator.SetBool(flyingHash, true);
        }
        else if(!flying && (Physics2D.GetIgnoreLayerCollision(8, 11) || animator.GetBool(flyingHash)))
        {
            Physics2D.IgnoreLayerCollision(8, 11, false);
            animator.SetBool(flyingHash, false);
        }

        if (maxHealth > 9999)
            maxHealth = 9999;

        if (maxHealth > latestMaxHealth)
        {
            currentHealth += maxHealth - latestMaxHealth;   // gain as much current health as you got max health

            if (maxHealth > achievementManager.reqs[(int)ReqName.HighestMaxHealth])
                achievementManager.reqs[(int)ReqName.HighestMaxHealth] = maxHealth;
        }

        latestMaxHealth = maxHealth;


        if (currentHealth > maxHealth)
            currentHealth = maxHealth;


        if (heldKeys > 999)
            heldKeys = 999;


        if (weaponSlots > 9)
            weaponSlots = 9;

        if (weaponSlots > lastWeaponSlots)
        {
            for(int i = weaponSlots - lastWeaponSlots; i>0; i--)
                playerWeapons.Add(null);
            lastWeaponSlots = weaponSlots;
        }


        if (allowControl)
        {
            for(int i = 0; i < 9; i++)
            {
                if(Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    ChangeWeapon(i + 1);
                    break;
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                GoUnarmed();
                curWeaponSlot = 0;
            }

            if (Input.GetAxisRaw("Mouse ScrollWheel") > 0)
            {
                if (curWeaponSlot + 1 <= playerWeapons.Count)
                    ChangeWeapon(curWeaponSlot + 1);
                else if (playerWeapons.Count >= 1)
                    ChangeWeapon(1);
            }
            else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
            {
                if (curWeaponSlot - 1 > 0)
                    ChangeWeapon(curWeaponSlot - 1);
                else if (playerWeapons.Count >= 1)
                    ChangeWeapon(playerWeapons.Count);
            }


            if (Input.GetKey(KeyCode.Mouse1) || Input.GetKey(KeyCode.LeftControl))
            {
                if (!cursorController.arrowKeyAiming)
                {
                    mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                    mousePos = cam.ScreenToViewportPoint(mousePos);
                }
                else
                {
                    mousePos = new Vector2(cursorController.transform.position.x, cursorController.transform.position.y);
                    mousePos = cam.WorldToViewportPoint(mousePos);
                }

                if (heldShooter)
                {
                    cam.transform.position = new Vector3(transform.position.x + (mousePos.x - 0.5f) * zoomMultiplier * heldShooter.zoomMultiplier,
                    transform.position.y + (mousePos.y - 0.5f) * zoomMultiplier * heldShooter.zoomMultiplier,
                    cam.transform.position.z);
                }
                else if (heldMeleeAttacker)
                {
                    cam.transform.position = new Vector3(transform.position.x + (mousePos.x - 0.5f) * zoomMultiplier * heldMeleeAttacker.zoomMultiplier,
                    transform.position.y + (mousePos.y - 0.5f) * zoomMultiplier * heldMeleeAttacker.zoomMultiplier,
                    cam.transform.position.z);
                }
                else
                {
                    cam.transform.position = new Vector3(transform.position.x + (mousePos.x - 0.5f) * zoomMultiplier * unarmedAttacker.zoomMultiplier,
                    transform.position.y + (mousePos.y - 0.5f) * zoomMultiplier * unarmedAttacker.zoomMultiplier,
                    cam.transform.position.z);
                }
                zooming = true;
            }
        }
        if (Input.GetKeyUp(KeyCode.Mouse1) || Input.GetKeyUp(KeyCode.LeftControl))
            zooming = false;

        if (currentHealth <= 0 && latestCurrentHealth > 0)  // die
        {
            UpdateOnDeathEffects();

            if (preventDeathLayers < 0)
            {
                Debug.LogWarning("preventDeathLayers on PlayerController is below 0, setting to 0");
                preventDeathLayers = 0;
            }
            if (preventDeathLayers < 1)
            {
                allowControl = false;
                allowedToLeaveLevel = false;

                latestCurrentHealth = currentHealth;

                if (flipped)
                    transform.rotation = Quaternion.Euler(0f, 0f, -90f);
                else
                    transform.rotation = Quaternion.Euler(0f, 0f, 90f);

                rb.mass = 1000;

                UnequipHeldWeapon(reactivate: false);

                gameOverController.gameObject.SetActive(true);
                gameOverController.StartDisplayGameOverScreen();

                pauseMenu.pauseMenuBgImage.enabled = true;


                animator.Play(deadAnimHash);
                shadowSpriteRenderer.sprite = deadShadowSprite;
                if(flipped)
                    shadowObj.transform.position = new Vector2(transform.position.x - 0.05f, transform.position.y - spriteRenderer.sprite.bounds.extents.x * 0.35f);
                else
                    shadowObj.transform.position = new Vector2(transform.position.x + 0.05f, transform.position.y - spriteRenderer.sprite.bounds.extents.x * 0.35f);
                shadowObj.transform.rotation = Quaternion.identity;

                return;
            }
        }

        if (currentHealth < latestCurrentHealth && currentHealth < maxHealth && (currentHealth > 0 || preventDeathLayers > 0))
            worldManager.StartDamagedTinter();

        latestCurrentHealth = currentHealth;


        if (heldShooter != null)
            heldMeleeAttacker = null;
        if (heldMeleeAttacker != null)
            heldShooter = null;


        droppedWeaponOnThisFrame = false;

        if(invincibilityLayers < 0)
        {
            Debug.LogWarning("invincibilityLayers on PlayerController is below 0, setting to 0");
            invincibilityLayers = 0;
        }

        if (invincibilityLayers > 0 && invincibilityTinterCoroutine == null)
            worldManager.StartInvincibilityTinter();

        if (invincibilityLayers < 1 && invincibilityTinterCoroutine != null)
            worldManager.StopInvincibilityTinter();
    }

    public void PickUpItem(GameObject itemHolderObject)
    {
        StatModifier statModifier = itemHolderObject.GetComponentInChildren<StatModifier>();

        itemPickupPopup.StartPopup(statModifier);


        Vector2 oldPlayerScale = transform.localScale;
        transform.localScale = new Vector2(1f, 1f);


        statModifier.transform.SetParent(transform);
        statModifier.transform.localPosition = Vector2.zero;


        transform.localScale = oldPlayerScale;


        statModifier.ApplyItemAppearance();
        statModifier.ApplyItemStats();



        amountOfItemsObtained++;


        ChestController chestController = itemHolderObject.GetComponent<ChestController>();
        ItemPedestal itemPedestal = itemHolderObject.GetComponent<ItemPedestal>();

        if (chestController != null)
        {
            chestController.particleSystemObject.SetActive(false);
            chestController.StopItemFloater();
            chestController.currentlyHoldingAnItem = false;
        }

        if(itemPedestal != null)
        {
            itemPedestal.particleSystemObject.SetActive(false);
            itemPedestal.StopItemFloater();
            itemPedestal.currentlyHoldingAnItem = false;
        }
    }
}
