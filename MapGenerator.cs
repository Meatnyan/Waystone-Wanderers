using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapGenerator : MonoBehaviour
{
    public GameObject playerCharacter;

    public float invincibilityPeriodAfterFinishedLoading;

    [Header("Map Settings")]
    public AreaType areaType = AreaType.unassigned;

    [Space(10f)]
    public int mapWidth;

    public int mapHeight;

    [Space(10f)]
    public float percentageOfBlocks;

    [Space(20f)]
    public byte maxFloodFillAttempts;

    [Space(10f)]
    public float minOpenSpacePercentage;

    [System.Serializable]
    public struct MapGenerationPhase
    {
        public int amountOfCycles;
        public int blocksWithinOneStep;
        public int blocksWithinTwoSteps;
    }

    [Space(25f)]
    public MapGenerationPhase[] mapGenerationPhases;

    [Space(15f)]
    [Header("Blocks")]
    public GameObject block;

    public GameObject blockEdge;

    public GameObject roomEntrance;

    public GameObject floor;

    public GameObject fakeWall;

    [Space(10f)]
    public float obstacleChanceInEmptySpace;

    public List<GameObject> obstacles;

    [Space(20f)]
    public float holeChanceInEmptySpace;

    public List<GameObject> holes3x3;

    public List<GameObject> holes5x5;

    [Space(15f)]
    public bool generateMapEdges;

    [Header("Chests")]
    public float anyChestChanceInEmptySpace;

    public List<GameObject> chests;

    [Header("Rooms")]
    public int minTreasureRoomsCount;

    public float extraTreasureRoomsChance;

    [Space(10f)]
    [Header("Enemies")]

    public float enemyPack3x3Chance;

    public float enemyPack3x3RareChance;

    public float extra3x3RareChancePerLevel;

    [Space(10f)]
    public List<GameObject> enemyPacks3x3Common;

    public List<GameObject> enemyPacks3x3Rare;

    [Space(30f)]
    public float enemyPack5x5Chance;

    public float enemyPack5x5RareChance;

    public float extra5x5RareChancePerLevel;

    [Space(10f)]
    public List<GameObject> enemyPacks5x5Common;

    public List<GameObject> enemyPacks5x5Rare;

    [Space(40f)]
    public float minDistFromPlayerToSpawnEnemiesAt;

    [Space(15f)]
    [Header("Blanking Settings (in %)")]
    public float blankingWidth;

    public float blankingHorizontalChance;

    public float blankingMaxDistFromMiddle;

    public float blankingMaxEdgeOffset;

    Vector2 startingBlockPosition;

    [System.NonSerialized]
    public float blockSize = 0.64f;

    Dictionary<Vector2Int, bool> blocksDictionary = new Dictionary<Vector2Int, bool>(); // used until and including flood fill, after that finalBlockPositions is used

    int blankingHalfWidth;

    int blankingPartWidthOne, blankingPartWidthTwo;

    int blankingXPos, blankingYPos;

    bool blankingHorizontal;

    float blankingEdgeOffsetOne, blankingEdgeOffsetTwo;

    Vector2Int floodFillStartingPoint;

    Dictionary<Vector2Int, bool> finalBlockPositions = new Dictionary<Vector2Int, bool>();

    Dictionary<Vector2, int> finalHoles5x5 = new Dictionary<Vector2, int>();

    Dictionary<Vector2, int> finalHoles3x3 = new Dictionary<Vector2, int>();

    Dictionary<Vector2, int> finalEnemyPacks5x5Common = new Dictionary<Vector2, int>();

    Dictionary<Vector2, int> finalEnemyPacks5x5Rare = new Dictionary<Vector2, int>();

    Dictionary<Vector2, int> finalEnemyPacks3x3Common = new Dictionary<Vector2, int>();

    Dictionary<Vector2, int> finalEnemyPacks3x3Rare = new Dictionary<Vector2, int>();

    Dictionary<Vector2, int> finalObstacles = new Dictionary<Vector2, int>();

    Dictionary<Vector2, int> finalChests = new Dictionary<Vector2, int>();

    Dictionary<Vector2Int, bool> finalEmpty1x1 = new Dictionary<Vector2Int, bool>();

    Dictionary<Vector2Int, bool> finalEmpty3x3 = new Dictionary<Vector2Int, bool>();

    Dictionary<Vector2Int, bool> finalEmpty5x5 = new Dictionary<Vector2Int, bool>();

    Dictionary<Vector2Int, bool> finalEmpty7x7 = new Dictionary<Vector2Int, bool>();

    Dictionary<Vector2Int, int> possibleRoomEntrancePositions = new Dictionary<Vector2Int, int>();

    Dictionary<Vector2, int> finalRoomEntrancePositions = new Dictionary<Vector2, int>();

    int currentTreasureRoomsCount = 0;

    int totalOpenSpace = 0;

    float baseBlankingWidth;

    LoadingController loadingController;

    int actualMinTreasureRoomsCount = 0;

    float actualExtraTreasureRoomsChance = 0f;

    [HideInInspector]
    public GameObject playerObject;

    WorldManager worldManager;

    private void Awake()
    {
        loadingController = transform.parent.GetComponent<LoadingController>();
        worldManager = FindObjectOfType<WorldManager>();

        startingBlockPosition = new Vector2(-mapWidth * blockSize / 2, -mapHeight * blockSize / 2);

        worldManager.levelIsLoaded = false;
        worldManager.startingBlockPosition = startingBlockPosition;
        worldManager.mapWidthGrid = mapWidth;
        worldManager.mapHeightGrid = mapHeight;

        minOpenSpacePercentage /= 100;
        blankingWidth /= 100;
        baseBlankingWidth = blankingWidth;
        blankingMaxDistFromMiddle /= 100;
        blankingMaxEdgeOffset /= 100;

        enemyPack3x3RareChance = Mathf.Min(enemyPack3x3RareChance + extra3x3RareChancePerLevel * worldManager.levelsBeaten, 50f);
        enemyPack5x5RareChance = Mathf.Min(enemyPack5x5RareChance + extra5x5RareChancePerLevel * worldManager.levelsBeaten, 50f);
    }

    public void StartLoadBaseScene()
    {
        StartCoroutine(LoadBaseScene());
    }

    IEnumerator LoadBaseScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1);  // 1 = Base

        while (!asyncLoad.isDone)   // wait until done loading scene
            yield return null;

        StartCoroutine(GenerateLevel());
    }

    /* Slower and less reliable, probably not worth using
    void FloodFillRecursive(Vector2Int node, bool nodeBool)
    {
        floodFillCount++;
        Debug.Log("floodFillCount = " + floodFillCount);

        if (nodeBool == true)
            return;

        floodFillReplacementCount++;
        Debug.Log("floodFillReplacementCount = " + floodFillReplacementCount);

        blocksDictionary[node] = true;
        finalEmpty1x1.Add(node, true);

        Vector2Int southNode = new Vector2Int(node.x, node.y - 1);
        Vector2Int northNode = new Vector2Int(node.x, node.y + 1);
        Vector2Int westNode = new Vector2Int(node.x - 1, node.y);
        Vector2Int eastNode = new Vector2Int(node.x + 1, node.y);

        if (blocksDictionary.ContainsKey(southNode))
            FloodFillRecursive(southNode, blocksDictionary[southNode]);
        if (blocksDictionary.ContainsKey(northNode))
            FloodFillRecursive(northNode, blocksDictionary[northNode]);
        if (blocksDictionary.ContainsKey(westNode))
            FloodFillRecursive(westNode, blocksDictionary[westNode]);
        if (blocksDictionary.ContainsKey(eastNode))
            FloodFillRecursive(eastNode, blocksDictionary[eastNode]);
    }
    */

    void FloodFillAlternativeOne(Vector2Int node)
    {
        Queue<Vector2Int> positionsQueue = new Queue<Vector2Int>();
        finalEmpty1x1 = new Dictionary<Vector2Int, bool>();

        blocksDictionary[node] = true;
        finalEmpty1x1.Add(node, true);
        positionsQueue.Enqueue(node);

        totalOpenSpace = 0;

        while (positionsQueue.Count > 0)
        {
            totalOpenSpace++;


            Vector2Int currentNode = positionsQueue.Peek();
            positionsQueue.Dequeue();

            Vector2Int southNode = new Vector2Int(currentNode.x, currentNode.y - 1);
            Vector2Int northNode = new Vector2Int(currentNode.x, currentNode.y + 1);
            Vector2Int westNode = new Vector2Int(currentNode.x - 1, currentNode.y);
            Vector2Int eastNode = new Vector2Int(currentNode.x + 1, currentNode.y);

            if (blocksDictionary.ContainsKey(westNode) && blocksDictionary[westNode] == false)
            {
                blocksDictionary[westNode] = true;
                finalEmpty1x1.Add(westNode, true);
                positionsQueue.Enqueue(westNode);
            }
            if (blocksDictionary.ContainsKey(eastNode) && blocksDictionary[eastNode] == false)
            {
                blocksDictionary[eastNode] = true;
                finalEmpty1x1.Add(eastNode, true);
                positionsQueue.Enqueue(eastNode);
            }
            if (blocksDictionary.ContainsKey(northNode) && blocksDictionary[northNode] == false)
            {
                blocksDictionary[northNode] = true;
                finalEmpty1x1.Add(northNode, true);
                positionsQueue.Enqueue(northNode);
            }
            if (blocksDictionary.ContainsKey(southNode) && blocksDictionary[southNode] == false)
            {
                blocksDictionary[southNode] = true;
                finalEmpty1x1.Add(southNode, true);
                positionsQueue.Enqueue(southNode);
            }
        }
    }

    IEnumerator GenerateLevel()
    {
        if (generateMapEdges)   // rectangular edge around the map
        {
            Vector2Int curIntPos = new Vector2Int(-1, -1);

            for (int curEdge = 0; curEdge < 4; curEdge++)
            {
                int curLength;
                if (curEdge == 0 || curEdge == 2)
                    curLength = mapWidth + 1;
                else
                    curLength = mapHeight + 1;

                for (int curDist = 0; curDist < curLength; curDist++)
                {
                    switch (curEdge)
                    {
                        case 0:
                            curIntPos = new Vector2Int(curIntPos.x + 1, curIntPos.y);
                            break;
                        case 1:
                            curIntPos = new Vector2Int(curIntPos.x, curIntPos.y + 1);
                            break;
                        case 2:
                            curIntPos = new Vector2Int(curIntPos.x - 1, curIntPos.y);
                            break;
                        case 3:
                            curIntPos = new Vector2Int(curIntPos.x, curIntPos.y - 1);
                            break;
                    }

                    finalBlockPositions.Add(curIntPos, true);
                }
            }
        }

        for (int curY = 0; curY < mapHeight * 2; curY++) // fill up a bunch of space outside the map with fake walls
        {
            for (int curX = 0; curX < mapWidth * 2; curX++)
            {
                if ((curY < mapHeight / 2 || curY > mapHeight * 1.5f) || (curX < mapWidth / 2 || curX > mapWidth * 1.5f))
                {
                    Vector2 curBlockPosition = new Vector2(startingBlockPosition.x * 2 + curX * blockSize, startingBlockPosition.y * 2 + curY * blockSize);
                    Instantiate(fakeWall, curBlockPosition, Quaternion.identity, null);
                }
            }
        }

        if (baseBlankingWidth > 0)  // "blanking" refers to creating a line of empty space across the map so that the individual caves are more connected
        {
            if (GenericExtensions.DetermineIfPercentChancePasses(blankingHorizontalChance))
            {
                blankingHorizontal = true;
                blankingWidth = baseBlankingWidth * mapHeight;
            }
            else
            {
                blankingHorizontal = false; // this means that the blanking is Vertical instead
                blankingWidth = baseBlankingWidth * mapWidth;
            }
            blankingWidth = Mathf.Round(blankingWidth);

            blankingHalfWidth = Mathf.FloorToInt(blankingWidth / 2);

            blankingPartWidthOne = blankingHalfWidth;
            blankingPartWidthTwo = blankingHalfWidth;
            if (Mathf.RoundToInt(blankingWidth) % 2 == 1)     // if blankingWidth is odd, add 1 to either the first or second half (part)
            {
                if (Random.Range(0, 2) == 0)
                    blankingPartWidthOne += 1;
                else
                    blankingPartWidthTwo += 1;
            }

            blankingEdgeOffsetOne = Random.Range(ExtConst.justAbove0, blankingMaxEdgeOffset);
            blankingEdgeOffsetTwo = blankingMaxEdgeOffset - blankingEdgeOffsetOne;

            if (blankingHorizontal)
            {
                blankingYPos = mapHeight / 2 + Random.Range(Mathf.FloorToInt(mapHeight * -blankingMaxDistFromMiddle), Mathf.FloorToInt(mapHeight * blankingMaxDistFromMiddle));

                blankingEdgeOffsetOne = Mathf.Floor(blankingEdgeOffsetOne * mapWidth);
                blankingEdgeOffsetTwo = Mathf.Floor(mapWidth - (blankingEdgeOffsetTwo * mapWidth));
            }
            else   // if blanking is Vertical
            {
                blankingXPos = mapWidth / 2 + Random.Range(Mathf.FloorToInt(mapWidth * -blankingMaxDistFromMiddle), Mathf.FloorToInt(mapWidth * blankingMaxDistFromMiddle));

                blankingEdgeOffsetOne = Mathf.Floor(blankingEdgeOffsetOne * mapHeight);
                blankingEdgeOffsetTwo = Mathf.Floor(mapHeight - (blankingEdgeOffsetTwo * mapHeight));
            }
        }

        for (int curY = 0; curY < mapHeight; curY++)    // initial generation
        {
            for (int curX = 0; curX < mapWidth; curX++)
            {
                if (GenericExtensions.DetermineIfPercentChancePasses(percentageOfBlocks) && (blankingWidth <= 0 ||
                    ((blankingHorizontal == true && ((curX <= blankingEdgeOffsetOne || curX >= blankingEdgeOffsetTwo) || (curY > blankingYPos + blankingPartWidthOne || curY < blankingYPos - blankingPartWidthTwo))) ||
                    ((blankingHorizontal == false && ((curY <= blankingEdgeOffsetOne || curY >= blankingEdgeOffsetTwo) || (curX > blankingXPos + blankingPartWidthOne || curX < blankingXPos - blankingPartWidthTwo)))))))
                    blocksDictionary.Add(new Vector2Int(curX, curY), true);
                else
                    blocksDictionary.Add(new Vector2Int(curX, curY), false);

                Vector2 curBlockPosition = new Vector2(startingBlockPosition.x + curX * blockSize, startingBlockPosition.y + curY * blockSize);
                Instantiate(floor, curBlockPosition, Quaternion.identity, null);
            }
        }



        Debug.Log("done with initial generation");




        for (int curPhase = 0; curPhase < mapGenerationPhases.Length; curPhase++)   // smoothing phases
        {
            MapGenerationPhase curMapGenPhase = mapGenerationPhases[curPhase];

            for (int curCycle = 0; curCycle < curMapGenPhase.amountOfCycles; curCycle++)
            {
                for (int curY = 0; curY < mapHeight; curY++)
                {
                    for (int curX = 0; curX < mapWidth; curX++)
                    {
                        int blocksWithinOneStep = 0;
                        int blocksWithinTwoSteps = 0;

                        if (curMapGenPhase.blocksWithinTwoSteps > 0)
                        {
                            for (int curTileY = 0; curTileY < 5; curTileY++)
                            {
                                for (int curTileX = 0; curTileX < 5; curTileX++)
                                {
                                    Vector2Int curTile = new Vector2Int(curX - 2 + curTileX, curY - 2 + curTileY);

                                    if (!blocksDictionary.ContainsKey(curTile) || blocksDictionary[curTile])
                                    {
                                        blocksWithinTwoSteps++;
                                        if (curTileY >= 1 && curTileY <= 3 && curTileX >= 1 && curTileX <= 3)
                                            blocksWithinOneStep++;
                                    }
                                }
                            }
                        }
                        else
                        {
                            for (int curTileY = 0; curTileY < 3; curTileY++)
                            {
                                for (int curTileX = 0; curTileX < 3; curTileX++)
                                {
                                    Vector2Int curTile = new Vector2Int(curX - 1 + curTileX, curY - 1 + curTileY);

                                    if (!blocksDictionary.ContainsKey(curTile) || blocksDictionary[curTile])
                                        blocksWithinOneStep++;
                                }
                            }
                        }

                        if (blocksWithinOneStep >= curMapGenPhase.blocksWithinOneStep || blocksWithinTwoSteps <= curMapGenPhase.blocksWithinTwoSteps)
                            blocksDictionary[new Vector2Int(curX, curY)] = true;
                        else
                            blocksDictionary[new Vector2Int(curX, curY)] = false;
                    }
                }
            }
        }

        byte floodFillAttempts = 0;
        while (true)
        {
            byte randomPosGenAttempts = 0;
            byte maxRandomPosGenAttempts = 20;
            while (true)
            {
                Vector2Int randomPos = new Vector2Int(Random.Range(0, mapWidth), Random.Range(0, mapHeight));
                if (blocksDictionary[randomPos] == false)
                {
                    floodFillStartingPoint = randomPos;
                    break;
                }
                randomPosGenAttempts++;
                if(randomPosGenAttempts == maxRandomPosGenAttempts) // if floodFillStartingPoint can't be established within maxRandomPosGenAttempts, generate a new map and try again
                {
                    Debug.Log("Called loadingController.GenerateAnotherLevel() - maxRandomPosGenAttempts was exceeded");

                    loadingController.tipController.DisplayTip();
                    loadingController.GenerateAnotherMap(gameObject, ref playerObject);
                    yield break;    // break entire coroutine
                }
            }
            Debug.Log("floodFillStartingPoint = " + floodFillStartingPoint);

            FloodFillAlternativeOne(floodFillStartingPoint);    // flood fill to only leave one large open area, determining finalEmpty1x1
            floodFillAttempts++;

            Debug.Log("FloodFill is over; blocksDictionary.Count = " + blocksDictionary.Count + "; totalOpenSpace = " + totalOpenSpace);

            if(totalOpenSpace >= blocksDictionary.Count * minOpenSpacePercentage)
                break;  // if it's big enough, stop flood filling attempts and continue

            if (floodFillAttempts == maxFloodFillAttempts)   // if there were too many unsuccessful flood fill attempts, generate a new map and try again
            {
                Debug.Log("Called loadingController.GenerateAnotherLevel() - totalOpenSpace is too small (covers only " + ((float)totalOpenSpace / blocksDictionary.Count * 100f)
                    + "% of the map, while " + (minOpenSpacePercentage * 100) + "% is required)");

                loadingController.tipController.DisplayTip();
                loadingController.GenerateAnotherMap(gameObject, ref playerObject);
                yield break;    // break entire coroutine
            }
        }

        for (int curY = 0; curY < mapHeight; curY++) // determine finalBlockPositions
        {
            for (int curX = 0; curX < mapWidth; curX++)
            {
                Vector2Int currentIntPos = new Vector2Int(curX, curY);

                if (finalEmpty1x1.ContainsKey(currentIntPos))   // if tile is supposed to be empty, continue
                    continue;

                finalBlockPositions.Add(currentIntPos, true);   // if it's not supposed to be empty, add the position to final block positions
                if (worldManager.blockedPositions.ContainsKey(currentIntPos) == false)   // ...and add it to blockedPositions for purposes of waystone spawning
                    worldManager.blockedPositions.Add(currentIntPos, true);
            }
        }

        foreach (KeyValuePair<Vector2Int, bool> keyValuePair in finalEmpty1x1)  // add finalEmpty3x3, 5x5, 7x7
        {
            bool isEmpty3x3 = true;
            bool isEmpty5x5 = true;
            bool isEmpty7x7 = true;

            int squareSide = 7;
            int halfSquareSide = squareSide / 2;

            for (int curTileY = 0; curTileY < squareSide; curTileY++)
            {
                if (isEmpty3x3)
                {
                    for (int curTileX = 0; curTileX < squareSide; curTileX++)
                    {
                        Vector2Int curTile = new Vector2Int(keyValuePair.Key.x - halfSquareSide + curTileX, keyValuePair.Key.y - halfSquareSide + curTileY);

                        if (!finalEmpty1x1.ContainsKey(curTile))
                        {
                            isEmpty7x7 = false;

                            if (curTileY >= 1 && curTileY <= 5 && curTileX >= 1 && curTileX <= 5)
                                isEmpty5x5 = false;

                            if (curTileY >= 2 && curTileY <= 4 && curTileX >= 2 && curTileX <= 4)
                            {
                                isEmpty3x3 = false;
                                break;
                            }
                        }
                    }
                }
            }

            if (isEmpty3x3)
                finalEmpty3x3.Add(keyValuePair.Key, true);

            if (isEmpty5x5)
                finalEmpty5x5.Add(keyValuePair.Key, true);

            if (isEmpty7x7)
                finalEmpty7x7.Add(keyValuePair.Key, true);
        }

        while (true)    // add 5x5 (7x7 taking into account empty space) enemy packs and holes using finalEmpty7x7
        {
            List<Vector2Int> finalEmpty7x7ToRemove = new List<Vector2Int>();

            foreach (KeyValuePair<Vector2Int, bool> keyValuePair in finalEmpty7x7)
            {
                bool clearAdjacentTiles = false;
                int chosenEnemyPackID = -1;
                int chosenRarity = -1;  // 0 = common, 1 = rare (, 2 = special?)
                Vector2 curBlockPosition = new Vector2(startingBlockPosition.x + (keyValuePair.Key.x * blockSize), startingBlockPosition.y + (keyValuePair.Key.y * blockSize));

                if (GenericExtensions.DetermineIfPercentChancePasses(enemyPack5x5Chance))
                {
                    if (GenericExtensions.DetermineIfPercentChancePasses(enemyPack5x5RareChance))
                    {
                        chosenEnemyPackID = worldManager.GenerateRandomEnemyPackID(enemyPacks5x5Rare);
                        chosenRarity = 1;

                        finalEnemyPacks5x5Rare.Add(curBlockPosition, chosenEnemyPackID);
                    }
                    else
                    {
                        chosenEnemyPackID = worldManager.GenerateRandomEnemyPackID(enemyPacks5x5Common);
                        chosenRarity = 0;

                        finalEnemyPacks5x5Common.Add(curBlockPosition, chosenEnemyPackID);
                    }

                    clearAdjacentTiles = true;
                }

                if((chosenEnemyPackID == -1 || (chosenRarity == 0 && enemyPacks5x5Common[chosenEnemyPackID].GetComponent<EnemyPackController>().isFlying)
                    || (chosenRarity == 1 && enemyPacks5x5Rare[chosenEnemyPackID].GetComponent<EnemyPackController>().isFlying == true)) && GenericExtensions.DetermineIfPercentChancePasses(holeChanceInEmptySpace))
                {
                    finalHoles5x5.Add(curBlockPosition, Random.Range(0, holes5x5.Count));

                    for (int blockedY = 0; blockedY < 5; blockedY++)
                        for (int blockedX = 0; blockedX < 5; blockedX++)
                        {
                            Vector2Int curBlockedPos = new Vector2Int(keyValuePair.Key.x - 2 + blockedX, keyValuePair.Key.y - 2 + blockedY);
                            if (!worldManager.blockedPositions.ContainsKey(curBlockedPos))
                                worldManager.blockedPositions.Add(curBlockedPos, true);
                        }

                    clearAdjacentTiles = true;
                }

                if (clearAdjacentTiles)
                {
                    int squareSide = 13;
                    int halfSquareSide = squareSide / 2;

                    for (int curTileY = 0; curTileY < squareSide; curTileY++)
                    {
                        for (int curTileX = 0; curTileX < squareSide; curTileX++)
                        {
                            Vector2Int curTile = new Vector2Int(keyValuePair.Key.x - halfSquareSide + curTileX, keyValuePair.Key.y - halfSquareSide + curTileY);

                            finalEmpty7x7ToRemove.Add(curTile);

                            if (curTileY >= 1 && curTileY <= squareSide - 2 && curTileX >= 1 && curTileX <= squareSide - 2)
                                finalEmpty5x5.Remove(curTile);

                            if (curTileY >= 2 && curTileY <= squareSide - 3 && curTileX >= 2 && curTileX <= squareSide - 3)
                                finalEmpty3x3.Remove(curTile);

                            if (curTileY >= 3 && curTileY <= squareSide - 4 && curTileX >= 3 && curTileX <= squareSide - 4)
                                finalEmpty1x1.Remove(curTile);
                        }
                    }
                }

                if (finalEmpty7x7ToRemove.Count > 0)
                    break;
            }

            if (finalEmpty7x7ToRemove.Count == 0)
                break;

            foreach (Vector2Int pos in finalEmpty7x7ToRemove)
                finalEmpty7x7.Remove(pos);
        }


        while (true)    // add 3x3 (5x5 taking into account empty space) enemy packs using finalEmpty5x5
        {
            List<Vector2Int> finalEmpty5x5ToRemove = new List<Vector2Int>();

            foreach (KeyValuePair<Vector2Int, bool> keyValuePair in finalEmpty5x5)
            {
                bool clearAdjacentTiles = false;
                int chosenEnemyPackID = -1;
                int chosenRarity = -1;  // 0 = common, 1 = rare (, 2 = special?)
                Vector2 curBlockPosition = new Vector2(startingBlockPosition.x + (keyValuePair.Key.x * blockSize), startingBlockPosition.y + (keyValuePair.Key.y * blockSize));

                if (GenericExtensions.DetermineIfPercentChancePasses(enemyPack3x3Chance))
                {
                    if (GenericExtensions.DetermineIfPercentChancePasses(enemyPack3x3RareChance))
                    {
                        chosenEnemyPackID = worldManager.GenerateRandomEnemyPackID(enemyPacks3x3Rare);
                        chosenRarity = 1;

                        finalEnemyPacks3x3Rare.Add(curBlockPosition, chosenEnemyPackID);
                    }
                    else
                    {
                        chosenEnemyPackID = worldManager.GenerateRandomEnemyPackID(enemyPacks3x3Common);
                        chosenRarity = 0;

                        finalEnemyPacks3x3Common.Add(curBlockPosition, chosenEnemyPackID);
                    }

                    clearAdjacentTiles = true;
                }

                if ((chosenEnemyPackID == -1 || (chosenRarity == 0 && enemyPacks3x3Common[chosenEnemyPackID].GetComponent<EnemyPackController>().isFlying == true)
                    || (chosenRarity == 1 && enemyPacks3x3Rare[chosenEnemyPackID].GetComponent<EnemyPackController>().isFlying == true)) && GenericExtensions.DetermineIfPercentChancePasses(holeChanceInEmptySpace))
                {
                    finalHoles3x3.Add(curBlockPosition, Random.Range(0, holes3x3.Count));

                    for (int blockedY = 0; blockedY < 3; blockedY++)
                        for (int blockedX = 0; blockedX < 3; blockedX++)
                        {
                            Vector2Int curBlockedPos = new Vector2Int(keyValuePair.Key.x - 1 + blockedX, keyValuePair.Key.y - 1 + blockedY);
                            if (worldManager.blockedPositions.ContainsKey(curBlockedPos) == false)
                                worldManager.blockedPositions.Add(curBlockedPos, true);
                        }

                    clearAdjacentTiles = true;
                }

                if (clearAdjacentTiles)
                {
                    int squareSide = 11;
                    int halfSquareSide = squareSide / 2;

                    for (int curTileY = 0; curTileY < squareSide; curTileY++)
                    {
                        for (int curTileX = 0; curTileX < squareSide; curTileX++)
                        {
                            Vector2Int curTile = new Vector2Int(keyValuePair.Key.x - halfSquareSide + curTileX, keyValuePair.Key.y - halfSquareSide + curTileY);

                            finalEmpty7x7.Remove(curTile);

                            if (curTileY >= 1 && curTileY <= squareSide - 2 && curTileX >= 1 && curTileX <= squareSide - 2)
                                finalEmpty5x5ToRemove.Add(curTile);

                            if (curTileY >= 2 && curTileY <= squareSide - 3 && curTileX >= 2 && curTileX <= squareSide - 3)
                                finalEmpty3x3.Remove(curTile);

                            if (curTileY >= 3 && curTileY <= squareSide - 4 && curTileX >= 3 && curTileX <= squareSide - 4)
                                finalEmpty1x1.Remove(curTile);
                        }
                    }
                }

                if (finalEmpty5x5ToRemove.Count > 0)
                    break;
            }

            if (finalEmpty5x5ToRemove.Count == 0)
                break;

            foreach (Vector2Int pos in finalEmpty5x5ToRemove)
                finalEmpty5x5.Remove(pos);
        }


        while (true)    // add 1x1 (3x3 taking into account empty space) obstacles and chests using finalEmpty3x3
        {
            List<Vector2Int> finalEmpty3x3ToRemove = new List<Vector2Int>();

            foreach (KeyValuePair<Vector2Int, bool> keyValuePair in finalEmpty3x3)
            {
                Vector2 curBlockPosition = new Vector2(startingBlockPosition.x + (keyValuePair.Key.x * blockSize), startingBlockPosition.y + (keyValuePair.Key.y * blockSize));
                bool clearAdjacentTiles = false;

                if (GenericExtensions.DetermineIfPercentChancePasses(obstacleChanceInEmptySpace))
                {
                    finalObstacles.Add(curBlockPosition, Random.Range(0, obstacles.Count));
                    if(worldManager.blockedPositions.ContainsKey(keyValuePair.Key) == false)
                        worldManager.blockedPositions.Add(keyValuePair.Key, true);

                    clearAdjacentTiles = true;
                }

                if(clearAdjacentTiles == false && GenericExtensions.DetermineIfPercentChancePasses(anyChestChanceInEmptySpace))
                {
                    finalChests.Add(curBlockPosition, worldManager.GenerateRandomChestID(chests));

                    clearAdjacentTiles = true;
                }

                if (clearAdjacentTiles)
                {
                    int squareSide = 9;
                    int halfSquareSide = squareSide / 2;

                    for (int curTileY = 0; curTileY < squareSide; curTileY++)
                    {
                        for (int curTileX = 0; curTileX < squareSide; curTileX++)
                        {
                            Vector2Int curTile = new Vector2Int(keyValuePair.Key.x - halfSquareSide + curTileX, keyValuePair.Key.y - halfSquareSide + curTileY);

                            finalEmpty7x7.Remove(curTile);

                            if (curTileY >= 1 && curTileY <= squareSide - 2 && curTileX >= 1 && curTileX <= squareSide - 2)
                                finalEmpty5x5.Remove(curTile);

                            if (curTileY >= 2 && curTileY <= squareSide - 3 && curTileX >= 2 && curTileX <= squareSide - 3)
                                finalEmpty3x3ToRemove.Add(curTile);

                            if (curTileY >= 3 && curTileY <= squareSide - 4 && curTileX >= 3 && curTileX <= squareSide - 4)
                                finalEmpty1x1.Remove(curTile);
                        }
                    }
                }

                if (finalEmpty3x3ToRemove.Count > 0)
                    break;
            }

            if (finalEmpty3x3ToRemove.Count == 0)
                break;

            foreach (Vector2Int pos in finalEmpty3x3ToRemove)
                finalEmpty3x3.Remove(pos);
        }



        foreach (KeyValuePair<Vector2Int, bool> keyValuePair in finalEmpty1x1)
        {
            bool foundBlock = false;
            Vector2Int foundBlockPos = new Vector2Int();
            bool breakSearchLoop = false;

            for (int curY = 0; curY < 3; curY++)
            {
                for (int curX = 0; curX < 3; curX++)
                {
                    Vector2Int curTile = new Vector2Int(keyValuePair.Key.x - 1 + curX, keyValuePair.Key.y - 1 + curY);

                    if (finalEmpty1x1.ContainsKey(curTile) == false)
                    {
                        if (foundBlock)
                        {
                            possibleRoomEntrancePositions.Remove(foundBlockPos);
                            breakSearchLoop = true;
                            break;
                        }

                        if (possibleRoomEntrancePositions.ContainsKey(curTile) == false)
                        {
                            if (curY == 0)
                                possibleRoomEntrancePositions.Add(curTile, 180);

                            if (curY == 1)
                            {
                                if (curX == 0)
                                    possibleRoomEntrancePositions.Add(curTile, 90);

                                if (curX == 2)
                                    possibleRoomEntrancePositions.Add(curTile, 270);
                            }

                            if (curY == 2)
                                possibleRoomEntrancePositions.Add(curTile, 0);

                            foundBlock = true;
                            foundBlockPos = curTile;
                        }
                        else
                        {
                            breakSearchLoop = true;
                            break;
                        }
                    }
                }

                if (breakSearchLoop)
                    break;
            }
        }


        actualMinTreasureRoomsCount = minTreasureRoomsCount;
        actualExtraTreasureRoomsChance = extraTreasureRoomsChance;
        while(true)
        {
            if (GenericExtensions.DetermineIfPercentChancePasses(actualExtraTreasureRoomsChance))
            {
                actualMinTreasureRoomsCount++;
                actualExtraTreasureRoomsChance /= 2;
            }
            else
                break;
        }

        if (possibleRoomEntrancePositions.Count < actualMinTreasureRoomsCount)  // make sure there's enough possible room entrance positions
        {
            Debug.Log("not enough treasure rooms, generating another map");

            loadingController.tipController.DisplayTip();
            loadingController.GenerateAnotherMap(gameObject, ref playerObject);
            yield break;
        }


        List<int> indexesOfPossibleRoomEntrancePositionsToAddToFinal = new List<int>();
        int generatedIndex = -1;
        for(int i = 0; i < actualMinTreasureRoomsCount; i++)    // generate random indexes to add to finalRoomEntrancePositions from
        {
            while(true)
            {
                generatedIndex = Random.Range(0, possibleRoomEntrancePositions.Count);
                if(indexesOfPossibleRoomEntrancePositionsToAddToFinal.Contains(generatedIndex) == false)
                {
                    indexesOfPossibleRoomEntrancePositionsToAddToFinal.Add(generatedIndex);
                    break;
                }
            }
        }

        int curKeyValuePairIndex = -1;
        foreach(KeyValuePair<Vector2Int, int> keyValuePair in possibleRoomEntrancePositions)    // add to finalRoomEntrancePositions
        {
            if (currentTreasureRoomsCount < actualMinTreasureRoomsCount)
            {
                curKeyValuePairIndex++;

                if (indexesOfPossibleRoomEntrancePositionsToAddToFinal.Contains(curKeyValuePairIndex))
                {
                    Vector2 curBlockPosition = new Vector2(startingBlockPosition.x + keyValuePair.Key.x * blockSize, startingBlockPosition.y + keyValuePair.Key.y * blockSize);

                    finalRoomEntrancePositions.Add(curBlockPosition, keyValuePair.Value);
                    currentTreasureRoomsCount++;

                    finalBlockPositions.Remove(keyValuePair.Key);
                }
            }
            else
                break;
        }


        if (currentTreasureRoomsCount < actualMinTreasureRoomsCount)
        {
            Debug.Log("not enough treasure rooms, generating another map");

            loadingController.tipController.DisplayTip();
            loadingController.GenerateAnotherMap(gameObject, ref playerObject);
            yield break;
        }


        int finalEmpty3x3ToPickForSpawn = Random.Range(0, finalEmpty3x3.Count);
        int currentKeyValuePair = -1;
        Vector2 spawnPosition = Vector2.zero;

        foreach(KeyValuePair<Vector2Int, bool> keyValuePair in finalEmpty3x3)   // determine player character spawn position
        {
            currentKeyValuePair++;
            if (currentKeyValuePair == finalEmpty3x3ToPickForSpawn)
            {
                spawnPosition = new Vector2(startingBlockPosition.x + keyValuePair.Key.x * blockSize, startingBlockPosition.y + keyValuePair.Key.y * blockSize);
                break;
            }
        }

        if (playerObject == null)
        {
            Debug.Log("playerObject = " + playerObject);
            playerObject = Instantiate(playerCharacter, spawnPosition, Quaternion.identity, null);   // instantiate player character or use the existing one...
        }
        else
        {
            playerObject.SetActive(true);
            playerObject.transform.position = spawnPosition;
        }

        loadingController.uiCanvasObject.SetActive(true);   // ...enable UI...

        worldManager.uICanvasController = FindObjectOfType<UICanvasController>();


        PlayerController playerController = playerObject.GetComponent<PlayerController>();


        playerController.killCountThisLevel = 0;
        playerController.allowControl = false;  // ...make sure player character can't be controlled and is invincible until finished loading...
        playerController.invincibilityLayers++;
        playerController.pauseMenu = FindObjectOfType<PauseMenu>();
        playerController.primaryStatDisplayer = playerController.uiCanvasController.primaryStatDisplayerObj.GetComponent<PrimaryStatDisplayer>();
        playerController.gameOverController = playerController.uiCanvasController.gameOverObj.GetComponent<GameOverController>();

        CursorController cursorController = FindObjectOfType<CursorController>();    // ...allow for arrowKeyAiming via CursorController...
        cursorController.playerController = playerObject.GetComponent<PlayerController>();
        cursorController.allowArrowKeyAiming = true;

        FindObjectOfType<LootManager>().playerController = playerController;

        FindObjectOfType<AchievementManager>().playerController = playerController;

        foreach (KeyValuePair<Vector2Int, bool> keyValuePair in finalBlockPositions)   // instantiate the blocks, room entrances, enemy packs and obstacles in their final positions
        {
            Instantiate(block, new Vector2(startingBlockPosition.x + keyValuePair.Key.x * blockSize, startingBlockPosition.y + keyValuePair.Key.y * blockSize),
                Quaternion.identity, null);

            if (keyValuePair.Key.y <= mapHeight - 1 && finalBlockPositions.ContainsKey(new Vector2Int(keyValuePair.Key.x, keyValuePair.Key.y + 1)) == false)
                Instantiate(blockEdge, new Vector2(startingBlockPosition.x + keyValuePair.Key.x * blockSize, startingBlockPosition.y + keyValuePair.Key.y * blockSize + blockSize), Quaternion.Euler(0f, 0f, 270f), null);
            if (keyValuePair.Key.x <= mapWidth - 1 && finalBlockPositions.ContainsKey(new Vector2Int(keyValuePair.Key.x + 1, keyValuePair.Key.y)) == false)
                Instantiate(blockEdge, new Vector2(startingBlockPosition.x + keyValuePair.Key.x * blockSize + blockSize, startingBlockPosition.y + keyValuePair.Key.y * blockSize), Quaternion.Euler(0f, 0f, 180f), null);
            if (keyValuePair.Key.y >= 0 && finalBlockPositions.ContainsKey(new Vector2Int(keyValuePair.Key.x, keyValuePair.Key.y - 1)) == false)
                Instantiate(blockEdge, new Vector2(startingBlockPosition.x + keyValuePair.Key.x * blockSize, startingBlockPosition.y + keyValuePair.Key.y * blockSize - blockSize), Quaternion.Euler(0f, 0f, 90f), null);
            if (keyValuePair.Key.x >= 0 && finalBlockPositions.ContainsKey(new Vector2Int(keyValuePair.Key.x - 1, keyValuePair.Key.y)) == false)
                Instantiate(blockEdge, new Vector2(startingBlockPosition.x + keyValuePair.Key.x * blockSize - blockSize, startingBlockPosition.y + keyValuePair.Key.y * blockSize), Quaternion.Euler(0f, 0f, 0f), null);
        }

        foreach (KeyValuePair<Vector2, int> keyValuePair in finalRoomEntrancePositions)
        {
            worldManager.closedTreasureRooms.Add(Instantiate(roomEntrance, keyValuePair.Key, Quaternion.Euler(0f, 0f, keyValuePair.Value), null));
        }

        foreach (KeyValuePair<Vector2, int> keyValuePair in finalHoles5x5)
        {
            Instantiate(holes5x5[keyValuePair.Value], keyValuePair.Key, Quaternion.identity, null);
        }

        foreach (KeyValuePair<Vector2, int> keyValuePair in finalHoles3x3)
        {
            Instantiate(holes3x3[keyValuePair.Value], keyValuePair.Key, Quaternion.identity, null);
        }

        worldManager.singularHoles = new List<GameObject>(GameObject.FindGameObjectsWithTag("Hole"));


        foreach (KeyValuePair<Vector2, int> keyValuePair in finalEnemyPacks5x5Rare)
        {
            if(Vector2.Distance(playerObject.transform.position, keyValuePair.Key) >= minDistFromPlayerToSpawnEnemiesAt)
                Instantiate(enemyPacks5x5Rare[keyValuePair.Value], keyValuePair.Key, Quaternion.identity, null);
        }

        foreach (KeyValuePair<Vector2, int> keyValuePair in finalEnemyPacks5x5Common)
        {
            if (Vector2.Distance(playerObject.transform.position, keyValuePair.Key) >= minDistFromPlayerToSpawnEnemiesAt)
                Instantiate(enemyPacks5x5Common[keyValuePair.Value], keyValuePair.Key, Quaternion.identity, null);
        }

        foreach (KeyValuePair<Vector2, int> keyValuePair in finalEnemyPacks3x3Rare)
        {
            if (Vector2.Distance(playerObject.transform.position, keyValuePair.Key) >= minDistFromPlayerToSpawnEnemiesAt)
                Instantiate(enemyPacks3x3Rare[keyValuePair.Value], keyValuePair.Key, Quaternion.identity, null);
        }

        foreach (KeyValuePair<Vector2, int> keyValuePair in finalEnemyPacks3x3Common)
        {
            if (Vector2.Distance(playerObject.transform.position, keyValuePair.Key) >= minDistFromPlayerToSpawnEnemiesAt)
                Instantiate(enemyPacks3x3Common[keyValuePair.Value], keyValuePair.Key, Quaternion.identity, null);
        }

        foreach (KeyValuePair<Vector2, int> keyValuePair in finalObstacles)
        {
            Instantiate(obstacles[keyValuePair.Value], keyValuePair.Key, Quaternion.identity, null);
        }

        foreach(KeyValuePair<Vector2, int> keyValuePair in finalChests)
        {
            Instantiate(chests[keyValuePair.Value], keyValuePair.Key, Quaternion.identity, null);
        }


        SmoothCamera2D smoothCamera2D = FindObjectOfType<SmoothCamera2D>();
        smoothCamera2D.playerController = playerController;
        smoothCamera2D.transform.position = new Vector3(playerController.transform.position.x, playerController.transform.position.y, smoothCamera2D.transform.position.z);

        float waitTime = 0f;
        if (Time.time < loadingController.timeOfBeginLoading + loadingController.minLoadingTime)
            yield return new WaitForSecondsRealtime(waitTime = loadingController.timeOfBeginLoading + loadingController.minLoadingTime - Time.time);

        Debug.Log("Waited for " + waitTime + "s Realtime");



        loadingController.StopLoadingDotsAnimation();
        loadingController.gameObject.SetActive(false);




        playerController.allowControl = true;

        worldManager.StartRemoveInvincibilityLayer(invincibilityPeriodAfterFinishedLoading);

        worldManager.levelIsLoaded = true;
        worldManager.currentAreaType = areaType;
        worldManager.currentLevelStartTime = Time.time;
        worldManager.startingAmountOfEnemiesInCurrentLevel = worldManager.enemiesInCurrentLevel.Count;
        worldManager.allowWaystoneSpawning = true;




        Debug.Log("IEnumerator GenerateLevel() is over");

        Destroy(gameObject);
    }
}
