using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatModifier : MonoBehaviour {

    [Header("Internal")]
    public string internalName;

    public bool unlocked = true;

    public bool obtainableMultipleTimes = false;

    public float dropWeight = 10f;

    [Header("Appearance")]
    public string displayedName;

    public string flavorText;

    public Sprite appearanceOnPlayer;

    public int appearanceOrder = 1;

    [Header("Base Stats")]
    public int maxHealth = 0;

    public int armor = 0;

    public int weaponSlots = 0;

    [Header("Primary Stats")]
    [SerializeField]
    float globalDamageMultiplier = 1f;

    [SerializeField]
    float globalDamageFlat = 0f;

    [Space(10f)]
    [SerializeField]
    float attackSpeedMultiplier = 1f;

    [SerializeField]
    float attackSpeedFlat = 0f;

    [Space(10f)]
    [SerializeField]
    float rangeMultiplier = 1f;

    [SerializeField]
    float rangeFlat = 0f;

    [Space(10f)]
    [SerializeField]
    float reloadSpeedMultiplier = 1f;

    [SerializeField]
    float reloadSpeedFlat = 0f;

    [Space(10f)]
    [SerializeField]
    float accuracyMultiplier = 1f;

    [SerializeField]
    float accuracyFlat = 0f;

    [Space(10f)]
    [SerializeField]
    float shotSpeedMultiplier = 1f;

    [SerializeField]
    float shotSpeedFlat = 0f;

    [Space(10f)]
    [SerializeField]
    float moveSpeedMultiplier = 1f;

    [SerializeField]
    float moveSpeedFlat = 0f;

    [Header("Weapon-specific Stats")]

    [SerializeField]
    float rangedDamageMultiplier = 1f;

    [SerializeField]
    float usamDamageMultiplier = 1f;

    [SerializeField]
    float shotgunAmmoDamageMultiplier = 1f;

    [SerializeField]
    float energyDamageMultiplier = 1f;

    [SerializeField]
    float rangedAttackSpeedMultiplier = 1f;

    [Space(20f)]
    [SerializeField]
    float meleeDamageMultiplier = 1f;

    [SerializeField]
    float meleeAttackSpeedMultiplier = 1f;

    [SerializeField]
    float unarmedDamageMultiplier = 1f;

    [SerializeField]
    float unarmedAttackSpeedMultiplier = 1f;

    [Header("Effect Chances (in %)")]
    [SerializeField]
    float burnChance = 0f;

    [SerializeField]
    float chillChance = 0f;

    [SerializeField]
    float bleedChance = 0f;

    [Space(10f)]
    public float doubleShotsChance = 0f;    // public because it affects item weight

    [SerializeField]
    float pierceChance = 0f;

    [SerializeField]
    float ghostlyChance = 0f;

    [Header("Misc. Stats")]
    [SerializeField]
    float zoomMultiplier = 1f;

    [SerializeField]
    float knockbackMultiplier = 1f;

    [SerializeField]
    float abilityDurationMultiplier = 1f;

    [SerializeField]
    float abilityCooldownMultiplier = 1f;

    [Space(20f)]
    [SerializeField]
    bool flying = false;

    [Space(20f)]
    [SerializeField]
    int homingStrength = 0;

    [Space(20f)]
    [SerializeField]
    float luck = 0f;

    [Space(20f)]
    [SerializeField]
    float playerSizeMultiplier = 1f;

    [Space(20f)]
    public int shotsPerWaveMultiplier = 1;  // also public because it affects item weight

    [System.NonSerialized]
    public int spawnedID;

    PlayerController playerController;

    SpriteRenderer spriteRenderer;

    [System.NonSerialized]
    public bool ownedByPlayer = false;

    [System.NonSerialized]
    public float minRangeToBePicked = Mathf.Infinity;

    [System.NonSerialized]
    public float maxRangeToBePicked = Mathf.NegativeInfinity;

    Animator animator;

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

    void Start ()
    {
        playerController = FindObjectOfType<PlayerController>();
        playerController.totalSpawnedID++;
        spawnedID = playerController.totalSpawnedID;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    public void ApplyItemAppearance()
    {
        spriteRenderer.sprite = appearanceOnPlayer;
        spriteRenderer.sortingLayerName = "Player";
        spriteRenderer.sortingOrder = appearanceOrder;

        if (spriteRenderer.sprite != null)
        {
            playerController.spritesVisibleOnPlayer.Enqueue(spriteRenderer);
            if (playerController.spritesVisibleOnPlayer.Count > 10)
            {
                playerController.spritesVisibleOnPlayer.Peek().enabled = false;
                playerController.spritesVisibleOnPlayer.Dequeue();
            }
        }
        if (animator != null)
            animator.enabled = true;
    }

    public void ApplyItemStats()
    {
        // Base Stats
        playerController.maxHealth += maxHealth;
        if (playerController.maxHealth < 1)
            playerController.maxHealth = 1;

        playerController.Armor += armor;

        playerController.weaponSlots += weaponSlots;
        if (playerController.weaponSlots < 0)
            playerController.weaponSlots = 0;


        // Primary Stats
        playerController.GlobalDamageMultiplier *= globalDamageMultiplier;
        playerController.GlobalDamageFlat += globalDamageFlat;

        playerController.AttackSpeedMultiplier *= attackSpeedMultiplier;
        playerController.AttackSpeedFlat += attackSpeedFlat;

        playerController.RangeMultiplier *= rangeMultiplier;
        playerController.RangeFlat += rangeFlat;

        playerController.ReloadSpeedMultiplier *= reloadSpeedMultiplier;
        playerController.ReloadSpeedFlat += reloadSpeedFlat;

        playerController.AccuracyMultiplier *= accuracyMultiplier;
        playerController.AccuracyFlat += accuracyFlat;

        playerController.ShotSpeedMultiplier *= shotSpeedMultiplier;
        playerController.ShotSpeedFlat += shotSpeedFlat;

        playerController.MoveSpeedMultiplier *= moveSpeedMultiplier;
        playerController.MoveSpeedFlat += moveSpeedFlat;


        // Weapon-specific Stats
        playerController.rangedDamageMultiplier *= rangedDamageMultiplier;
        playerController.usamDamageMultiplier *= usamDamageMultiplier;
        playerController.shotgunAmmoDamageMultiplier *= shotgunAmmoDamageMultiplier;
        playerController.energyDamageMultiplier *= energyDamageMultiplier;
        playerController.rangedAttackSpeedMultiplier *= rangedAttackSpeedMultiplier;
        playerController.meleeDamageMultiplier *= meleeDamageMultiplier;
        playerController.meleeAttackSpeedMultiplier *= meleeAttackSpeedMultiplier;
        playerController.unarmedDamageMultiplier *= unarmedDamageMultiplier;
        playerController.unarmedAttackSpeedMultiplier *= unarmedAttackSpeedMultiplier;


        // Effect Chances (in %)
        playerController.burnChance += burnChance;
        playerController.chillChance += chillChance;
        playerController.bleedChance += bleedChance;

        playerController.doubleShotsChance += doubleShotsChance;
        playerController.pierceChance += pierceChance;
        playerController.ghostlyChance += ghostlyChance;


        // Misc. Stats
        playerController.zoomMultiplier *= zoomMultiplier;
        playerController.knockbackMultiplier *= knockbackMultiplier;
        playerController.abilityDurationMultiplier *= abilityDurationMultiplier;
        playerController.abilityCooldownMultiplier *= abilityCooldownMultiplier;

        if (flying)
            playerController.flying = true;

        playerController.homingStrength += homingStrength;

        playerController.luck += luck;

        playerController.transform.localScale = new Vector2(playerController.transform.localScale.x * playerSizeMultiplier, playerController.transform.localScale.y * playerSizeMultiplier);

        playerController.shotsPerWaveMultiplier *= shotsPerWaveMultiplier;
    }
}
