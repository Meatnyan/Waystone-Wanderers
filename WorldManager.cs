using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public enum AreaType
{
    unassigned,
    wasteland
}

public enum StatusEffect
{
    none,
    burn,
    chill,
    bleed
}

public enum Orientation
{
    left,
    right,
    up,
    down
}

public class WorldManager : MonoBehaviour
{
    [Header("Waystone Settings")]
    public GameObject waystoneObject;

    [Tooltip("Both horizontally and vertically")]
    public int minBlockDistFromWaystone;

    public int maxXBlockDistFromWaystone;

    public int maxYBlockDistFromWaystone;

    [Tooltip("In % of enemies left")]
    public float spawnWaystoneThreshold;

    [Header("Pointer Arrow Settings")]
    public GameObject enemyArrow;

    public float enemyArrowExtraYPosition;

    public float distBetweenTextAndArrow;

    [Tooltip("In % of enemies left")]
    public float showEnemyArrowThreshold;

    [Space(20f)]
    public GameObject treasureRoomArrow;

    public float treasureRoomArrowExtraYPosition;

    [Header("Scaling Settings")]
    public float enemyDmgMultiplierPerLevel;

    public float enemyHealthMultiplierPerLevel;

    [Header("Status Effects & Particle Effects")]
    public GameObject burnParticleSystem;

    public GameObject bleedParticleSystem;

    public GameObject chillParticleSystem;

    [Space(10f)]
    public GameObject bloodExplosionParticleSystem;

    [Space(30f)]
    public GameObject itemPedestal;

    [Space(30f)]
    public GameObject[] enemies;

    [System.NonSerialized]
    public GameObject playerObject = null;

    [System.NonSerialized]
    public PlayerController playerController = null;

    [System.NonSerialized]
    public List<GameObject> enemiesInCurrentLevel = new List<GameObject>();

    [System.NonSerialized]
    public int startingAmountOfEnemiesInCurrentLevel = 0;

    [System.NonSerialized]
    public float currentLevelStartTime = 0f;

    [System.NonSerialized]
    public bool allowWaystoneSpawning = false;

    [System.NonSerialized]
    public bool waystoneIsPresent = false;

    Vector2 waystoneSpawnPos;

    [HideInInspector]
    public LoadingController loadingController;

    GameObject enemyArrowInstance = null;

    [HideInInspector]
    public int levelsBeaten = 0;

    RestartManager restartManager;

    [HideInInspector]
    public Dictionary<Vector2Int, bool> blockedPositions = new Dictionary<Vector2Int, bool>();

    [HideInInspector]
    public List<Vector2Int> possibleWaystoneSpawnPos = new List<Vector2Int>();

    float blockSize = 0.64f;

    [HideInInspector]
    public Vector2 startingBlockPosition;

    [HideInInspector]
    public int mapWidthGrid;

    [HideInInspector]
    public int mapHeightGrid;

    [HideInInspector]
    public List<GameObject> singularHoles = new List<GameObject>();

    [HideInInspector]
    public WaystoneController waystoneController;

    [HideInInspector]
    public List<GameObject> allWeaponsOnTheGround = new List<GameObject>();

    [System.NonSerialized]
    public int moveOnGroundAttempts = 0;

    [HideInInspector]
    public Color latestPlayerColor = new Color(1f, 1f, 1f);

    [HideInInspector]
    public bool levelIsLoaded = false;

    [HideInInspector]
    public UICanvasController uICanvasController;

    Camera cam;

    [HideInInspector]
    public AreaType currentAreaType = AreaType.unassigned;

    [HideInInspector]
    public List<GameObject> closedTreasureRooms = new List<GameObject>();

    [HideInInspector]
    public GameObject treasureRoomArrowInstance = null;

    private void Awake()
    {
        restartManager = FindObjectOfType<RestartManager>();
        restartManager.DontDestroyOnLoadButDestroyWhenRestarting(gameObject);

        cam = FindObjectOfType<Camera>();

        showEnemyArrowThreshold /= 100f;
        spawnWaystoneThreshold /= 100f;
    }

    private void Update()
    {
        if (allowWaystoneSpawning && !waystoneIsPresent && enemiesInCurrentLevel.Count <= startingAmountOfEnemiesInCurrentLevel * spawnWaystoneThreshold
            && playerController != null && playerController.allowControl && !playerController.isInRoom)
        {
            GenerateWaystoneSpawnPos();
            SpawnWaystone(waystoneSpawnPos);
        }

        if (waystoneIsPresent && closedTreasureRooms.Count > 0 && playerController.heldKeys > 0)
            if (treasureRoomArrowInstance == null)
                treasureRoomArrowInstance = Instantiate(treasureRoomArrow, new Vector2(playerController.transform.position.x,
                        playerController.transform.position.y - playerController.spriteRenderer.sprite.bounds.extents.y / 2f - treasureRoomArrowExtraYPosition),
                        Quaternion.identity, playerController.transform);
            else if (!treasureRoomArrowInstance.activeSelf)
                treasureRoomArrowInstance.SetActive(true);

        if (treasureRoomArrowInstance != null && treasureRoomArrowInstance.activeSelf)
        {
            if (closedTreasureRooms.Count == 0 || playerController.heldKeys == 0)
                treasureRoomArrowInstance.SetActive(false);
            else
            {
                Vector2 diff = GenericExtensions.GetClosestGameObject(playerController.transform.position, closedTreasureRooms).transform.position - enemyArrowInstance.transform.position;
                diff.Normalize();
                float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
                treasureRoomArrowInstance.transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90f);    // rotate towards closest treasure room
            }
        }

        // check for enemy pointer arrow conditions
        if (playerController != null && !playerController.isInRoom && playerController.amountOfEnemiesOnScreen == 0 && playerController.allowControl    // allowControl is a temporary solution for situations like the transition between outside and inside a room
            && enemiesInCurrentLevel.Count > startingAmountOfEnemiesInCurrentLevel * spawnWaystoneThreshold
            && (enemiesInCurrentLevel.Count <= startingAmountOfEnemiesInCurrentLevel * showEnemyArrowThreshold || enemiesInCurrentLevel.Count <= 9))
        {
            if (enemyArrowInstance == null)
                enemyArrowInstance = Instantiate(enemyArrow, new Vector2(playerController.transform.position.x,
                    playerController.transform.position.y + playerController.spriteRenderer.sprite.bounds.extents.y / 2f + enemyArrowExtraYPosition),
                    Quaternion.identity, playerController.transform);

            enemyArrowInstance.SetActive(true);

            enemyArrowInstance.transform.rotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(enemyArrowInstance.transform.position,
                GenericExtensions.GetClosestGameObject(playerController.transform.position, enemiesInCurrentLevel).transform.position) + 90f);    // rotate towards closest enemy


            if (enemiesInCurrentLevel.Count <= 9)
            {
                uICanvasController.enemiesRemainingText.gameObject.SetActive(true);

                // set enemiesRemainingText anchoredPosition between player and arrow
                uICanvasController.enemiesRemainingText.rectTransform.anchoredPosition = new Vector2(cam.WorldToScreenPoint(enemyArrowInstance.transform.position).x,
                    cam.WorldToScreenPoint(new Vector2(enemyArrowInstance.transform.position.x, enemyArrowInstance.transform.position.y - distBetweenTextAndArrow)).y);

                uICanvasController.enemiesRemainingText.text = "" + enemiesInCurrentLevel.Count;
            }
        }
        else if (enemyArrowInstance != null)  // if conditions for enemy arrow aren't met, but the arrow instance exists
        {
            enemyArrowInstance.SetActive(false);
            uICanvasController.enemiesRemainingText.gameObject.SetActive(false);
        }
    }

    void GenerateWaystoneSpawnPos()
    {
        Vector2Int playerPosInGrid = new Vector2Int(mapWidthGrid / 2 + Mathf.RoundToInt(playerObject.transform.position.x / blockSize),
            mapHeightGrid / 2 + Mathf.RoundToInt(playerObject.transform.position.y / blockSize));


        Vector2Int waystoneSpawnPosInGrid = Vector2Int.zero;

        int rectangleWidth = maxXBlockDistFromWaystone * 2 + 1;
        int rectangleHeight = maxYBlockDistFromWaystone * 2 + 1;

        for(int curRectangleY = 0; curRectangleY < rectangleHeight; curRectangleY++)
        {
            for(int curRectangleX = 0; curRectangleX < rectangleWidth; curRectangleX++) // player position index in rectangle is equal to maxXBlockDistFromWaystone and maxYBlockDistFromWaystone
            {
                if (curRectangleX > maxXBlockDistFromWaystone - minBlockDistFromWaystone && curRectangleX < maxXBlockDistFromWaystone + minBlockDistFromWaystone
                    && curRectangleY > maxYBlockDistFromWaystone - minBlockDistFromWaystone && curRectangleY < maxYBlockDistFromWaystone + minBlockDistFromWaystone)
                    continue;

                Vector2Int curIntPos = new Vector2Int(playerPosInGrid.x - maxXBlockDistFromWaystone + curRectangleX, playerPosInGrid.y - maxYBlockDistFromWaystone + curRectangleY);

                if (curIntPos.x < 0 || curIntPos.x > mapWidthGrid - 1 || curIntPos.y < 0 || curIntPos.y > mapHeightGrid - 1)
                    continue;

                if (blockedPositions.ContainsKey(curIntPos) == false)
                    possibleWaystoneSpawnPos.Add(curIntPos);
            }
        }


        if (possibleWaystoneSpawnPos.Count > 0)
        {
            waystoneSpawnPosInGrid = possibleWaystoneSpawnPos[Random.Range(0, possibleWaystoneSpawnPos.Count)];
            waystoneSpawnPos = new Vector2(startingBlockPosition.x + waystoneSpawnPosInGrid.x * blockSize, startingBlockPosition.y + waystoneSpawnPosInGrid.y * blockSize);
        }
        else
        {
            waystoneSpawnPos = new Vector2(startingBlockPosition.x + playerPosInGrid.x * blockSize, startingBlockPosition.y + playerPosInGrid.y * blockSize);
            Debug.LogWarning("Attempted generating waystoneSpawnPos using possibleWaystoneSpawnPos but failed. Defaulting to waystoneSpawnPos based on playerPosInGrid");
        }

        Debug.Log("playerPosInGrid = " + playerPosInGrid + "; waystoneSpawnPosInGrid = " + waystoneSpawnPosInGrid);
    }

    public void SpawnWaystone(Vector2 spawnPos)
    {
        waystoneController = Instantiate(waystoneObject, spawnPos, Quaternion.identity, null).GetComponent<WaystoneController>();

        waystoneController.StartWarp(targetObject: waystoneController.gameObject, warpingIn: true);

        waystoneIsPresent = true;
    }

    /*  old way of handling waystoneSpawnPosInGrid
        waystoneSpawnPosInGrid = new Vector2Int(Random.Range(Mathf.Clamp(playerPosInGrid.x - maxBlockDistFromWaystone, 0, mapWidthGrid - 2), Mathf.Clamp(playerPosInGrid.x + maxBlockDistFromWaystone + 1, 0, mapWidthGrid - 2)),
            Random.Range(Mathf.Clamp(playerPosInGrid.y - maxBlockDistFromWaystone / 2, 0, mapHeightGrid - 2), Mathf.Clamp(playerPosInGrid.y + maxBlockDistFromWaystone / 2 + 1, 0, mapHeightGrid - 2)));

        Debug.LogWarning("waystoneSpawnPosInGrid = " + waystoneSpawnPosInGrid);

        if ((waystoneSpawnPosInGrid.x >= playerPosInGrid.x + minBlockDistFromWaystone || waystoneSpawnPosInGrid.x <= playerPosInGrid.x - minBlockDistFromWaystone)
            && (waystoneSpawnPosInGrid.y >= playerPosInGrid.y + minBlockDistFromWaystone || waystoneSpawnPosInGrid.y <= playerPosInGrid.y - minBlockDistFromWaystone)
            && blockedPositions.ContainsKey(waystoneSpawnPosInGrid) == false)
        {
            waystoneSpawnPos = new Vector2(startingBlockPosition.x + waystoneSpawnPosInGrid.x * blockSize, startingBlockPosition.y + waystoneSpawnPosInGrid.y * blockSize);
            Debug.LogWarning("waystoneSpawnPos = " + waystoneSpawnPos);
            break;
        }
    */

    /* and another one
            Vector2Int playerPosInGrid = new Vector2Int(mapWidthGrid / 2 + Mathf.RoundToInt(playerObject.transform.position.x / blockSize),
            mapHeightGrid / 2 + Mathf.RoundToInt(playerObject.transform.position.y / blockSize));


        Vector2Int waystoneSpawnPosInGrid = Vector2Int.zero;
        int attempts = 0;
        while (true)
        {
            int xSide = 0;  // 0 - undefined, 1 - left, 2 - right
            int ySide = 0;  // 0 - undefined, 1 - down, 2 - up

            if (playerPosInGrid.x - minBlockDistFromWaystone <= 0)
                xSide = 2;  // right
            else if (playerPosInGrid.x + minBlockDistFromWaystone >= mapWidthGrid - 2)
                xSide = 1;  // left

            if (playerPosInGrid.y - minBlockDistFromWaystone <= 0)
                ySide = 2;  // up
            else if (playerPosInGrid.y + minBlockDistFromWaystone >= mapHeightGrid - 2)
                ySide = 1;  // down

            if (xSide == 0)
                xSide = Random.Range(1, 3); // 1 or 2

            if (ySide == 0)
                ySide = Random.Range(1, 3); // 1 or 2


            if (xSide == 1)  // generate to the left of player
                waystoneSpawnPosInGrid.x = Mathf.Max(Random.Range(playerPosInGrid.x - maxXBlockDistFromWaystone, playerPosInGrid.x - minBlockDistFromWaystone + 1), 0);
            else   // generate to the right of player
                waystoneSpawnPosInGrid.x = Mathf.Min(Random.Range(playerPosInGrid.x + minBlockDistFromWaystone, playerPosInGrid.x + maxXBlockDistFromWaystone + 1), mapWidthGrid - 2);

            if (ySide == 1)  // generate down from player
                waystoneSpawnPosInGrid.y = Mathf.Max(Random.Range(playerPosInGrid.y - maxYBlockDistFromWaystone, playerPosInGrid.y - minBlockDistFromWaystone + 1), 0);
            else   // generate up from player
                waystoneSpawnPosInGrid.y = Mathf.Min(Random.Range(playerPosInGrid.y + minBlockDistFromWaystone, playerPosInGrid.y + maxYBlockDistFromWaystone + 1), mapHeightGrid - 2);


            Debug.Log("playerPosInGrid = " + playerPosInGrid + "; waystoneSpawnPosInGrid = " + waystoneSpawnPosInGrid);


            if (blockedPositions.ContainsKey(waystoneSpawnPosInGrid) == false)
            {
                waystoneSpawnPos = new Vector2(startingBlockPosition.x + waystoneSpawnPosInGrid.x * blockSize, startingBlockPosition.y + waystoneSpawnPosInGrid.y * blockSize);
                break;
            }

            attempts++;

            if(attempts == 1000)
            {
                waystoneSpawnPos = new Vector2(startingBlockPosition.x + playerPosInGrid.x * blockSize, startingBlockPosition.y + playerPosInGrid.y * blockSize);
                Debug.LogWarning("Attempted generating waystoneSpawnPos " + attempts + " times and failed. Defaulting to playerPosInGrid");
                break;
            }
        }
        */

    public void StartDamagedTinter()
    {
        if(playerController.damagedTinterCoroutine == null)
        {
            latestPlayerColor = playerController.spriteRenderer.material.color;
            playerController.damagedTinterCoroutine = StartCoroutine(DamagedTinter());
        }
    }

    IEnumerator DamagedTinter()     // right now only works on player, might wanna convert it so that it works on anything
    {
        playerController.spriteRenderer.material.color = new Color(1f, 0f, 0f);
        yield return new WaitForSeconds(playerController.damagedTintTime / 3);
        playerController.spriteRenderer.material.color = new Color(1f, 0.33f, 0.33f);
        yield return new WaitForSeconds(playerController.damagedTintTime / 3);
        playerController.spriteRenderer.material.color = new Color(1f, 0.66f, 0.66f);
        yield return new WaitForSeconds(playerController.damagedTintTime / 3);
        playerController.spriteRenderer.material.color = latestPlayerColor;
        playerController.damagedTinterCoroutine = null;
    }

    public void StartInvincibilityTinter()
    {
        if(playerController.invincibilityTinterCoroutine == null)
        {
            playerController.invincibilityTinterCoroutine = StartCoroutine(InvincibilityTinter());
        }
    }

    public void StopInvincibilityTinter()
    {
        if (playerController.invincibilityTinterCoroutine != null)
        {
            StopCoroutine(playerController.invincibilityTinterCoroutine);
            playerController.invincibilityTinterCoroutine = null;
            playerController.spriteRenderer.material.color = latestPlayerColor;
        }
        else
            Debug.LogWarning("playerController.invincibilityTinterCoroutine = null; aborting StopInvincibilityTinter");
    }

    IEnumerator InvincibilityTinter()
    {
        if (playerController.damagedTinterCoroutine != null)
            yield return playerController.damagedTinterCoroutine;

        latestPlayerColor = playerController.spriteRenderer.material.color;

        while (true)
        {
            playerController.spriteRenderer.material.color = new Color(0.3f, 0.3f, 0.3f);
            yield return new WaitForSeconds(playerController.invincibilityTintTime);
            playerController.spriteRenderer.material.color = new Color(0.6f, 0.6f, 0.6f);
            yield return new WaitForSeconds(playerController.invincibilityTintTime);
            playerController.spriteRenderer.material.color = new Color(0.9f, 0.9f, 0.9f);
            yield return new WaitForSeconds(playerController.invincibilityTintTime);
        }
    }

    public void StartAddInvincibilityLayer(float duration)
    {
        StartCoroutine(AddInvincibilityLayer(duration));
    }

    IEnumerator AddInvincibilityLayer(float duration)
    {
        playerController.invincibilityLayers++;
        yield return new WaitForSeconds(duration);
        playerController.invincibilityLayers--;
    }

    public void StartRemoveInvincibilityLayer(float duration)
    {
        StartCoroutine(RemoveInvincibilityLayer(duration));
    }

    IEnumerator RemoveInvincibilityLayer(float duration)
    {
        yield return new WaitForSeconds(duration);
        playerController.invincibilityLayers--;
    }

    public void MoveOnGround(Transform targetTransform, float maxMoveDist)
    {
        moveOnGroundAttempts++;

        if (targetTransform.GetComponent<Shooter>())
            targetTransform.GetComponent<Shooter>().moveOnGroundId = moveOnGroundAttempts;

        if (targetTransform.GetComponent<MeleeAttacker>())
        {
            targetTransform.SetParent(null);
            targetTransform.GetComponent<MeleeAttacker>().moveOnGroundId = moveOnGroundAttempts;
        }

        Vector2 randomPosNearby = new Vector2(targetTransform.position.x + Random.Range(-maxMoveDist, maxMoveDist), targetTransform.position.y + Random.Range(-maxMoveDist, maxMoveDist));
        targetTransform.DOMove(randomPosNearby, Random.Range(1f, 1.5f)).SetId(moveOnGroundAttempts);

        if (targetTransform.GetComponent<Pickup>() == null)
            targetTransform.DORotate(new Vector3(0f, 0f, Random.Range(360f, 720f)), Random.Range(1f, 1.5f), RotateMode.FastBeyond360).SetId(moveOnGroundAttempts);
    }

    public void UpdateWeaponSpriteLayerAndCollider(Shooter shooter, bool ownedByPlayer)
    {
        shooter.polygonCollider2D.enabled = ownedByPlayer ? false : true;
        shooter.spriteRenderer.sortingLayerName = ownedByPlayer ? "Player" : "Weapons";
        shooter.spriteRenderer.sortingOrder = ownedByPlayer ? 50 : 0;
    }

    public void UpdateWeaponSpriteLayerAndCollider(MeleeAttacker meleeAttacker, bool ownedByPlayer)
    {
        meleeAttacker.polygonCollider2D.enabled = ownedByPlayer ? false : true;
        meleeAttacker.spriteRenderer.sortingLayerName = ownedByPlayer ? "Player" : "Weapons";
        meleeAttacker.spriteRenderer.sortingOrder = ownedByPlayer ? 50 : 0;
    }

    public int GenerateRandomChestID(List<GameObject> chestObjects)
    {
        float chestsTotalWeight = 0f;

        for (int chestID = 0; chestID < chestObjects.Count; chestID++)
        {
            ChestController chestController = chestObjects[chestID].GetComponent<ChestController>();
            if (chestController.unlocked)
            {
                chestController.minRangeToBePicked = chestsTotalWeight;
                chestController.maxRangeToBePicked = chestsTotalWeight + chestController.spawnWeight;
                chestsTotalWeight += chestController.spawnWeight;
            }
        }

        float randomWeight = Random.Range(0f, chestsTotalWeight - 0.0001f);

        for (int chestID = 0; chestID < chestObjects.Count; chestID++)
        {
            ChestController chestController = chestObjects[chestID].GetComponent<ChestController>();
            if (randomWeight >= chestController.minRangeToBePicked && randomWeight < chestController.maxRangeToBePicked)
                return chestID;
        }

        Debug.LogWarning("Couldn't return random chest ID, returning 0 instead");
        return 0;
    }

    public int GenerateRandomEnemyPackID(List<GameObject> enemyPacks)
    {
        float enemyPacksTotalWeight = 0f;

        for (int enemyPackID = 0; enemyPackID < enemyPacks.Count; enemyPackID++)
        {
            EnemyPackController enemyPackController = enemyPacks[enemyPackID].GetComponent<EnemyPackController>();
            if (enemyPackController.unlocked)
            {
                enemyPackController.minRangeToBePicked = enemyPacksTotalWeight;
                enemyPackController.maxRangeToBePicked = enemyPacksTotalWeight + enemyPackController.spawnWeight;
                enemyPacksTotalWeight += enemyPackController.spawnWeight;
            }
        }

        float randomWeight = Random.Range(0f, enemyPacksTotalWeight - 0.0001f);

        for (int enemyPackID = 0; enemyPackID < enemyPacks.Count; enemyPackID++)
        {
            EnemyPackController enemyPackController = enemyPacks[enemyPackID].GetComponent<EnemyPackController>();
            if (randomWeight >= enemyPackController.minRangeToBePicked && randomWeight < enemyPackController.maxRangeToBePicked)
                return enemyPackID;
        }

        Debug.LogWarning("Couldn't return random enemy pack ID, returning 0 instead");
        return 0;
    }

    public int GenerateRandomRoomID(List<GameObject> rooms)
    {
        float roomsTotalWeight = 0f;

        for (int roomID = 0; roomID < rooms.Count; roomID++)
        {
            RoomController roomController = rooms[roomID].GetComponent<RoomController>();
            if (roomController.unlocked)
            {
                roomController.minRangeToBePicked = roomsTotalWeight;
                roomController.maxRangeToBePicked = roomsTotalWeight + roomController.spawnWeight;
                roomsTotalWeight += roomController.spawnWeight;
            }
        }

        float randomWeight = Random.Range(0f, roomsTotalWeight - 0.0001f);

        for (int roomID = 0; roomID < rooms.Count; roomID++)
        {
            RoomController roomController = rooms[roomID].GetComponent<RoomController>();
            if (randomWeight >= roomController.minRangeToBePicked && randomWeight < roomController.maxRangeToBePicked)
                return roomID;
        }

        Debug.LogWarning("Couldn't return random room ID, returning 0 instead");
        return 0;
    }
}
