using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootManager : MonoBehaviour
{
    public GameObject itemPedestalObj;

#pragma warning disable 0649
    [Space(20f)]
    [SerializeField]
    List<GameObject> treasureItemPool;

    [SerializeField]
    List<GameObject> woodenChestPickupPool;

    [SerializeField]
    List<GameObject> woodenChestWeaponPool;
#pragma warning restore 0649

    [System.NonSerialized]
    public List<GameObject> currentTreasureItemPool;

    [System.NonSerialized]
    public List<GameObject> currentWoodenChestPickupPool;

    [System.NonSerialized]
    public List<GameObject> currentWoodenChestWeaponPool;

    [HideInInspector]
    public PlayerController playerController;

    RestartManager restartManager;

    WorldManager worldManager;

    private void Awake()
    {
        restartManager = FindObjectOfType<RestartManager>();
        restartManager.DontDestroyOnLoadButDestroyWhenRestarting(gameObject);

        worldManager = FindObjectOfType<WorldManager>();

        currentTreasureItemPool = treasureItemPool;
        currentWoodenChestPickupPool = woodenChestPickupPool;
        currentWoodenChestWeaponPool = woodenChestWeaponPool;
    }

    public void GenerateItem(GameObject objItemWillAttachTo)
    {
        float totalWeight = 0f;
        int shotsMultiplierRarity = 1;
        StatModifier generatedStatModifier = null;

        for (int curObjID = 0; curObjID < currentTreasureItemPool.Count; curObjID++)
        {
            StatModifier curStatModifier = currentTreasureItemPool[curObjID].GetComponent<StatModifier>();
            if (curStatModifier.unlocked)
            {
                curStatModifier.minRangeToBePicked = totalWeight;

                if (curStatModifier.shotsPerWaveMultiplier == 1 && curStatModifier.doubleShotsChance == 0f) // to reduce chance of a ridiculous amount of shot multipliers
                    curStatModifier.maxRangeToBePicked = totalWeight + curStatModifier.dropWeight;
                else
                {
                    curStatModifier.maxRangeToBePicked = totalWeight + (curStatModifier.dropWeight / (shotsMultiplierRarity * shotsMultiplierRarity));
                    shotsMultiplierRarity++;
                }

                totalWeight = curStatModifier.maxRangeToBePicked;
            }
        }

        float randomWeight = Random.Range(0f, totalWeight - 0.0001f);

        for (int curObjID = 0; curObjID < currentTreasureItemPool.Count; curObjID++)
        {
            StatModifier curStatModifier = currentTreasureItemPool[curObjID].GetComponent<StatModifier>();

            if (randomWeight >= curStatModifier.minRangeToBePicked && randomWeight < curStatModifier.maxRangeToBePicked)
            {
                generatedStatModifier = Instantiate(currentTreasureItemPool[curObjID], new Vector2(objItemWillAttachTo.transform.position.x, objItemWillAttachTo.transform.position.y + 0.2f), Quaternion.identity, objItemWillAttachTo.transform).GetComponent<StatModifier>();

                if (!curStatModifier.obtainableMultipleTimes)
                    currentTreasureItemPool.RemoveAt(curObjID);

                break;
            }
        }

        ItemPedestal itemPedestal = objItemWillAttachTo.GetComponent<ItemPedestal>();
        ChestController chestController = objItemWillAttachTo.GetComponent<ChestController>();

        if (itemPedestal != null)
        {
            itemPedestal.heldStatModifier = generatedStatModifier;
            itemPedestal.currentlyHoldingAnItem = true;
            itemPedestal.StartItemFloater();
        }

        if(chestController != null)
            chestController.heldStatModifier = generatedStatModifier;
    }

    public bool SpawnSpecificItemOnNewPedestal(string itemName, Vector2 spawnPos)
    {
        StatModifier generatedStatModifier = null;
        GameObject pedestalInstance = null;

        for (int curObjID = 0; curObjID < treasureItemPool.Count; ++curObjID)
        {
            StatModifier curStatModifier = treasureItemPool[curObjID].GetComponent<StatModifier>();

            if (curStatModifier.internalName == itemName)    // itemName should already be lowercase if accessed through dev console
            {
                pedestalInstance = Instantiate(itemPedestalObj, spawnPos, Quaternion.identity, null);

                generatedStatModifier = Instantiate(treasureItemPool[curObjID], new Vector2(pedestalInstance.transform.position.x, pedestalInstance.transform.position.y + 0.2f),
                    Quaternion.identity, pedestalInstance.transform).GetComponent<StatModifier>();

                if (curStatModifier.obtainableMultipleTimes == false && currentTreasureItemPool.Contains(curStatModifier.gameObject))
                    currentTreasureItemPool.Remove(curStatModifier.gameObject);

                break;
            }
        }

        if (generatedStatModifier != null)
        {
            ItemPedestal itemPedestal = pedestalInstance.GetComponent<ItemPedestal>();

            itemPedestal.spawnedNaturally = false;
            itemPedestal.heldStatModifier = generatedStatModifier;
            itemPedestal.currentlyHoldingAnItem = true;
            itemPedestal.StartItemFloater();

            return true;
        }
        else
            return false;
    }

    public void DropOnlyPickups(int amountToDrop, Vector2 posToDropAt)
    {
        float pickupTotalWeight = 0f;
        float randomWeight = 0f;

        for (int pickupID = 0; pickupID < currentWoodenChestPickupPool.Count; pickupID++)
        {
            Pickup pickup = currentWoodenChestPickupPool[pickupID].GetComponent<Pickup>();
            pickup.minRangeToBePicked = pickupTotalWeight;
            pickup.maxRangeToBePicked = pickupTotalWeight + pickup.dropWeight;
            pickupTotalWeight += pickup.dropWeight;
        }

        for (int curDropID = 0; curDropID < amountToDrop; curDropID++)
        {
            randomWeight = Random.Range(0f, pickupTotalWeight - 0.0001f);
            for (int pickupID = 0; pickupID < currentWoodenChestPickupPool.Count; pickupID++)
            {
                Pickup pickup = currentWoodenChestPickupPool[pickupID].GetComponent<Pickup>();
                if (randomWeight >= pickup.minRangeToBePicked && randomWeight < pickup.maxRangeToBePicked)
                {
                    worldManager.MoveOnGround(Instantiate(currentWoodenChestPickupPool[pickupID], posToDropAt, Quaternion.identity, null).transform, 0.2f);    // instantiate then immediately move on ground
                    break;
                }
            }
        }
    }

    public void DropOnlyWeapons(int amountToDrop, Vector2 posToDropAt)
    {
        float weaponRangedTotalWeight = 0f;
        float weaponMeleeTotalWeight = 0f;
        float randomWeight = 0f;

        for (int weaponID = 0; weaponID < currentWoodenChestWeaponPool.Count; weaponID++)
        {
            if (currentWoodenChestWeaponPool[weaponID].GetComponent<Shooter>() != null)
            {
                Shooter shooter = currentWoodenChestWeaponPool[weaponID].GetComponent<Shooter>();
                if (shooter.unlocked)
                {
                    shooter.minRangeToBePicked = weaponRangedTotalWeight;
                    shooter.maxRangeToBePicked = weaponRangedTotalWeight + shooter.dropWeight;
                    weaponRangedTotalWeight += shooter.dropWeight;
                }
            }

            if (currentWoodenChestWeaponPool[weaponID].GetComponentInChildren<MeleeAttacker>() != null)
            {
                MeleeAttacker meleeAttacker = currentWoodenChestWeaponPool[weaponID].GetComponentInChildren<MeleeAttacker>();
                if (meleeAttacker.unlocked)
                {
                    meleeAttacker.minRangeToBePicked = weaponMeleeTotalWeight;
                    meleeAttacker.maxRangeToBePicked = weaponMeleeTotalWeight + meleeAttacker.dropWeight;
                    weaponMeleeTotalWeight += meleeAttacker.dropWeight;
                }
            }
        }

        for (int curDropID = 0; curDropID < amountToDrop; curDropID++)
        {
            if (GenericExtensions.DetermineIfPercentChancePasses(playerController.rangedWeaponChance))
            {
                randomWeight = Random.Range(0f, weaponRangedTotalWeight - 0.0001f);
                for (int weaponID = 0; weaponID < currentWoodenChestWeaponPool.Count; weaponID++)
                {
                    if (currentWoodenChestWeaponPool[weaponID].GetComponent<Shooter>() != null)
                    {
                        Shooter shooter = currentWoodenChestWeaponPool[weaponID].GetComponent<Shooter>();
                        if (randomWeight >= shooter.minRangeToBePicked && randomWeight < shooter.maxRangeToBePicked)
                        {
                            worldManager.MoveOnGround(Instantiate(currentWoodenChestWeaponPool[weaponID], posToDropAt, Quaternion.identity, null).transform, 0.4f);
                            break;
                        }
                    }
                }
            }
            else
            {
                randomWeight = Random.Range(0f, weaponMeleeTotalWeight - 0.0001f);
                for (int weaponID = 0; weaponID < currentWoodenChestWeaponPool.Count; weaponID++)
                {
                    if (currentWoodenChestWeaponPool[weaponID].GetComponentInChildren<MeleeAttacker>() != null)
                    {
                        MeleeAttacker meleeAttacker = currentWoodenChestWeaponPool[weaponID].GetComponentInChildren<MeleeAttacker>();
                        if (randomWeight >= meleeAttacker.minRangeToBePicked && randomWeight < meleeAttacker.maxRangeToBePicked)
                        {
                            worldManager.MoveOnGround(Instantiate(currentWoodenChestWeaponPool[weaponID], posToDropAt, Quaternion.identity, null).GetComponentInChildren<MeleeAttacker>().transform, 0.4f);
                            break;
                        }
                    }
                }
            }
        }
    }

    public void DropPickupsAndWeapons(int amountToDrop, Vector2 posToDropAt)
    {
        float pickupTotalWeight = 0f;
        float weaponRangedTotalWeight = 0f;
        float weaponMeleeTotalWeight = 0f;
        float randomWeight = 0f;

        for (int pickupID = 0; pickupID < currentWoodenChestPickupPool.Count; pickupID++)
        {
            Pickup pickup = currentWoodenChestPickupPool[pickupID].GetComponent<Pickup>();
            pickup.minRangeToBePicked = pickupTotalWeight;
            pickup.maxRangeToBePicked = pickupTotalWeight + pickup.dropWeight;
            pickupTotalWeight += pickup.dropWeight;
        }

        for (int weaponID = 0; weaponID < currentWoodenChestWeaponPool.Count; weaponID++)
        {
            if (currentWoodenChestWeaponPool[weaponID].GetComponent<Shooter>() != null)
            {
                Shooter shooter = currentWoodenChestWeaponPool[weaponID].GetComponent<Shooter>();
                if (shooter.unlocked)
                {
                    shooter.minRangeToBePicked = weaponRangedTotalWeight;
                    shooter.maxRangeToBePicked = weaponRangedTotalWeight + shooter.dropWeight;
                    weaponRangedTotalWeight += shooter.dropWeight;
                }
            }

            if (currentWoodenChestWeaponPool[weaponID].GetComponentInChildren<MeleeAttacker>() != null)
            {
                MeleeAttacker meleeAttacker = currentWoodenChestWeaponPool[weaponID].GetComponentInChildren<MeleeAttacker>();
                if (meleeAttacker.unlocked)
                {
                    meleeAttacker.minRangeToBePicked = weaponMeleeTotalWeight;
                    meleeAttacker.maxRangeToBePicked = weaponMeleeTotalWeight + meleeAttacker.dropWeight;
                    weaponMeleeTotalWeight += meleeAttacker.dropWeight;
                }
            }
        }

        for (int curDropID = 0; curDropID < amountToDrop; curDropID++)
        {
            if (!GenericExtensions.DetermineIfPercentChancePasses(playerController.woodenChestWeaponDropChance))
            {
                randomWeight = Random.Range(0f, pickupTotalWeight - 0.0001f);
                for (int pickupID = 0; pickupID < currentWoodenChestPickupPool.Count; pickupID++)
                {
                    Pickup pickup = currentWoodenChestPickupPool[pickupID].GetComponent<Pickup>();
                    if (randomWeight >= pickup.minRangeToBePicked && randomWeight < pickup.maxRangeToBePicked)
                    {
                        worldManager.MoveOnGround(Instantiate(currentWoodenChestPickupPool[pickupID], posToDropAt, Quaternion.identity, null).transform, 0.2f);
                        break;
                    }
                }
            }
            else
            {
                if (GenericExtensions.DetermineIfPercentChancePasses(playerController.rangedWeaponChance))
                {
                    randomWeight = Random.Range(0f, weaponRangedTotalWeight - 0.0001f);
                    for (int weaponID = 0; weaponID < currentWoodenChestWeaponPool.Count; weaponID++)
                    {
                        if (currentWoodenChestWeaponPool[weaponID].GetComponent<Shooter>() != null)
                        {
                            Shooter shooter = currentWoodenChestWeaponPool[weaponID].GetComponent<Shooter>();
                            if (randomWeight >= shooter.minRangeToBePicked && randomWeight < shooter.maxRangeToBePicked)
                            {
                                worldManager.MoveOnGround(Instantiate(currentWoodenChestWeaponPool[weaponID], posToDropAt, Quaternion.identity, null).transform, 0.4f);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    randomWeight = Random.Range(0f, weaponMeleeTotalWeight - 0.0001f);
                    for (int weaponID = 0; weaponID < currentWoodenChestWeaponPool.Count; weaponID++)
                    {
                        if (currentWoodenChestWeaponPool[weaponID].GetComponentInChildren<MeleeAttacker>() != null)
                        {
                            MeleeAttacker meleeAttacker = currentWoodenChestWeaponPool[weaponID].GetComponentInChildren<MeleeAttacker>();
                            if (randomWeight >= meleeAttacker.minRangeToBePicked && randomWeight < meleeAttacker.maxRangeToBePicked)
                            {
                                worldManager.MoveOnGround(Instantiate(currentWoodenChestWeaponPool[weaponID], posToDropAt, Quaternion.identity, null).GetComponentInChildren<MeleeAttacker>().transform, 0.4f);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

}
