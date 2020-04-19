using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChestType
{
    wooden,
    golden,
    diamond,
    weaponsCache
}

public class ChestController : MonoBehaviour {

    public bool unlocked = true;

    public float spawnWeight;

    [Space(10f)]
    public ChestType chestType;

    [Space(10f)]
    public int amountOfKeysRequired;

    [Space(10f)]
    public int amountOfNonItemDrops;

    [Space(20f)]
    public Sprite[] openingSprites;

    [Space(30f)]
    public GameObject particleSystemObject;

    [Space(20f)]
    public float openingTime;

    public float unlockingTime = 0.2f;

    public float floatStepDistance;

    public float floatStepDuration;

    public float maxFloatSteps;

    [System.NonSerialized]
    public bool open = false;

    [System.NonSerialized]
    public bool currentlyHoldingAnItem = false;

    [System.NonSerialized]
    public bool currentlyOpening = false;

    [System.NonSerialized]
    public float minRangeToBePicked = Mathf.Infinity;

    [System.NonSerialized]
    public float maxRangeToBePicked = Mathf.NegativeInfinity;

    AudioSource openingSound;

    AudioSource lockSound;

    GameObject playerObject;

    PlayerController playerController;

    [HideInInspector]
    public StatModifier heldStatModifier;

    [HideInInspector]
    public SpriteRenderer spriteRenderer;

    Coroutine emptyOffscreenChestCleanerCoroutine;

    LootManager lootManager;

    public Coroutine itemFloaterCoroutine = null;

    [HideInInspector]
    public int currentAmountOfLocks;

    private void Awake()
    {
        AudioSource[] audioSources = GetComponents<AudioSource>();
        openingSound = audioSources[0];
        if(audioSources.Length > 1)
            lockSound = audioSources[1];
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentAmountOfLocks = amountOfKeysRequired;
    }

    private void Start()
    {
        playerObject = GameObject.FindWithTag("Player");
        playerController = playerObject.GetComponent<PlayerController>();
        lootManager = playerController.lootManager;
    }

    public void UpdateHeldItem()
    {
        lootManager.GenerateItem(objItemWillAttachTo: gameObject);

        currentlyHoldingAnItem = true;

        particleSystemObject.SetActive(true);

        StartItemFloater();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!open && !currentlyOpening && collision.gameObject.CompareTag("Player") && playerController.allowControl && (currentAmountOfLocks == 0 || playerController.heldKeys > 0))
            StartCoroutine(Opener());

        if (currentlyHoldingAnItem && collision.gameObject.CompareTag("Player") && playerController.allowControl)
            playerController.PickUpItem(itemHolderObject: gameObject);
    }

    private void OnBecameInvisible()
    {
        if (gameObject.activeSelf && open && !currentlyHoldingAnItem && emptyOffscreenChestCleanerCoroutine == null)
            emptyOffscreenChestCleanerCoroutine = StartCoroutine(EmptyOffscreenChestCleaner());
    }

    private void OnBecameVisible()
    {
        if(emptyOffscreenChestCleanerCoroutine != null)
        {
            StopCoroutine(emptyOffscreenChestCleanerCoroutine);
            emptyOffscreenChestCleanerCoroutine = null;
        }
    }

    IEnumerator EmptyOffscreenChestCleaner()
    {
        yield return new WaitForSeconds(playerController.timeBeforeCleaningOffscreenChests);
        if (!spriteRenderer.isVisible)
            Destroy(gameObject);
    }

    public void StartItemFloater()
    {
        if (itemFloaterCoroutine == null)
            itemFloaterCoroutine = StartCoroutine(ItemFloater());
    }

    IEnumerator ItemFloater()
    {
        int directionMultiplier = 1;    // 1 = up, -1 = down
        while (true)
        {
            for (int i = 0; i < maxFloatSteps; i++)
            {
                heldStatModifier.transform.localPosition = new Vector2(heldStatModifier.transform.localPosition.x,
                    heldStatModifier.transform.localPosition.y + floatStepDistance * directionMultiplier);
                yield return new WaitForSeconds(floatStepDuration);
            }
            directionMultiplier *= -1;
        }
    }

    public void StopItemFloater()
    {
        if(itemFloaterCoroutine != null)
        {
            StopCoroutine(itemFloaterCoroutine);
            itemFloaterCoroutine = null;
        }
    }

    IEnumerator Opener()
    {
        currentlyOpening = true;

        if (currentAmountOfLocks > 0)
        {
            playerController.heldKeys--;
            currentAmountOfLocks--;
            lockSound.Play();
            yield return new WaitForSeconds(unlockingTime);
        }
        if (currentAmountOfLocks == 0)
        {
            openingSound.Play();
            yield return new WaitForSeconds(openingTime);
        }
        spriteRenderer.sprite = openingSprites[openingSprites.Length - 1 - currentAmountOfLocks];
        currentlyOpening = false;

        if (currentAmountOfLocks == 0)
        {
            open = true;
            switch (chestType)
            {
                case ChestType.golden:
                    if (GenericExtensions.DetermineIfPercentChancePasses(playerController.goldenChestItemChance))
                        UpdateHeldItem();
                    else
                        lootManager.DropPickupsAndWeapons(amountOfNonItemDrops, transform.position);
                    break;

                case ChestType.diamond:
                    UpdateHeldItem();
                    break;

                case ChestType.weaponsCache:
                    lootManager.DropOnlyWeapons(amountOfNonItemDrops, transform.position);
                    break;

                case ChestType.wooden:
                    lootManager.DropPickupsAndWeapons(amountOfNonItemDrops, transform.position);
                    break;

                default:
                    Debug.LogWarning("ChestController of object " + gameObject + " encountered an unrecognized ChestType and can't operate properly");
                    break;
            }
        }
    }

}
