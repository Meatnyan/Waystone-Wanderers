using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaystoneController : MonoBehaviour
{
    public GameObject warpBeamObject;

    public float warpTime;

    WorldManager worldManager;

    AudioManager audioManager;

    private void Awake()
    {
        worldManager = FindObjectOfType<WorldManager>();
        audioManager = FindObjectOfType<AudioManager>();
    }

    public void StartWarp(GameObject targetObject, bool warpingIn)
    {
        StartCoroutine(Warp(targetObject, warpingIn));
    }

    IEnumerator Warp(GameObject targetObject, bool warpingIn)   // warpingIn == false means warpingOut
    {
        PlayerController playerControllerInstance = targetObject.GetComponent<PlayerController>();

        List<Color> startingColors = new List<Color>();
        SpriteRenderer[] spriteRenderers = targetObject.GetComponentsInChildren<SpriteRenderer>();  // GetComponentsInChildren() also gets the parent's components

        foreach (SpriteRenderer childSpriteRenderer in spriteRenderers)
        {
            startingColors.Add(childSpriteRenderer.color);
            childSpriteRenderer.color = new Color(0f, 0f, 0f);
        }

        if (warpingIn)
            audioManager.Play("WarpingIn");
        else
            audioManager.Play("WarpingOut");


        if (playerControllerInstance != null)
            playerControllerInstance.allowControl = false;

        GameObject warpBeamInstance = Instantiate(warpBeamObject, new Vector2(targetObject.transform.position.x,
            targetObject.transform.position.y + targetObject.GetComponent<SpriteRenderer>().sprite.bounds.extents.y / 2f + 3.35f), Quaternion.identity, targetObject.transform);
        yield return new WaitForSeconds(warpTime);
        Destroy(warpBeamInstance);


        for (int i = 0; i < spriteRenderers.Length; i++)
            spriteRenderers[i].color = startingColors[i];


        if (playerControllerInstance != null)   // advance to next level
        {
            GameObject playerObject = playerControllerInstance.gameObject;

            if(playerControllerInstance.adrenalGland != null)
            {
                playerControllerInstance.adrenalGland.countdownDuration = playerControllerInstance.adrenalGland.baseCountdownDuration;
                playerControllerInstance.adrenalGland.countdownStartTime = Mathf.NegativeInfinity;
                playerControllerInstance.adrenalGland.remainingTime = Mathf.Infinity;
            }

            playerControllerInstance.restartManager.DestroyAllUnownedWeapons(playerControllerInstance);

            worldManager.blockedPositions = new Dictionary<Vector2Int, bool>();
            worldManager.possibleWaystoneSpawnPos = new List<Vector2Int>();
            worldManager.enemiesInCurrentLevel = new List<GameObject>();
            worldManager.allWeaponsOnTheGround = new List<GameObject>();
            worldManager.closedTreasureRooms = new List<GameObject>();
            worldManager.levelsBeaten++;

            AchievementManager achievementManager = playerControllerInstance.achievementManager;

            if (worldManager.levelsBeaten > achievementManager.reqs[(int)ReqName.HighestLevelBeaten])
                achievementManager.reqs[(int)ReqName.HighestLevelBeaten] = worldManager.levelsBeaten;

            int levelBeatTime = Mathf.FloorToInt(Time.time - worldManager.currentLevelStartTime);
            if (levelBeatTime < achievementManager.reqs[(int)ReqName.FastestLevelBeatenTime])
                achievementManager.reqs[(int)ReqName.FastestLevelBeatenTime] = levelBeatTime;


            achievementManager.UpdateSaveFile();


            worldManager.loadingController.gameObject.SetActive(true);
            worldManager.loadingController.BeginLoadingLevel(ref playerObject);
            Debug.Log("Warp()'s playerObject = " + playerObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player") && collision.gameObject.GetComponent<PlayerController>().allowedToLeaveLevel)
        {
            StartCoroutine(Warp(collision.gameObject, false));
        }
    }
}
