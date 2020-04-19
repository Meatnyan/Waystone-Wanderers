using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingController : MonoBehaviour
{
    public GameObject uiCanvasObject;

    public float timeBetweenLoadingDots;

    public Text loadingText;

    public GameObject tipTextObject;

    public GameObject mapGeneratorObject;

    public float minLoadingTime;

    [HideInInspector]
    public TipController tipController;

    [System.NonSerialized]
    public float timeOfBeginLoading = Mathf.NegativeInfinity;

    WorldManager worldManager;

    RestartManager restartManager;

    Coroutine loadingDotsAnimationCoroutine = null;

    private void Awake()
    {
        tipController = tipTextObject.GetComponent<TipController>();
        worldManager = FindObjectOfType<WorldManager>();
        restartManager = FindObjectOfType<RestartManager>();

        restartManager.DontDestroyOnLoadButDestroyWhenRestarting(uiCanvasObject);

        worldManager.loadingController = this;
    }

    IEnumerator LoadingDotsAnimation()
    {
        while(true)
        {
            loadingText.text = "Loading...";    // replace with "Loading." --> "Loading.." --> "Loading..." once the lag is fixed, although more realistically this would need to be interweaved into loading itself
            yield return new WaitForSecondsRealtime(timeBetweenLoadingDots);
            loadingText.text = "Loading...";
            yield return new WaitForSecondsRealtime(timeBetweenLoadingDots);
            loadingText.text = "Loading...";
            yield return new WaitForSecondsRealtime(timeBetweenLoadingDots);
        }
    }

    public void StopLoadingDotsAnimation()
    {
        if(loadingDotsAnimationCoroutine != null)
        {
            StopCoroutine(loadingDotsAnimationCoroutine);
            loadingDotsAnimationCoroutine = null;
        }
    }

    public void BeginLoadingLevel()
    {
        GameObject tempObject = null;
        BeginLoadingLevel(ref tempObject);
    }

    public void BeginLoadingLevel(ref GameObject playerObject)
    {
        worldManager.waystoneIsPresent = false;
        worldManager.allowWaystoneSpawning = false;

        MapGenerator mapGenerator = Instantiate(mapGeneratorObject, transform).GetComponent<MapGenerator>();
        mapGenerator.gameObject.SetActive(true);

        timeOfBeginLoading = Time.time;

        loadingDotsAnimationCoroutine = StartCoroutine(LoadingDotsAnimation());

        tipController.DisplayTip();

        mapGenerator.playerObject = playerObject;

        Debug.Log("playerObject before = " + playerObject + "; mapgenerator.playerObject before = " + mapGenerator.playerObject);

        if (playerObject != null)
            playerObject.SetActive(false);

        Debug.Log("playerObject after = " + playerObject + "; mapgenerator.playerObject after = " + mapGenerator.playerObject);

        mapGenerator.StartLoadBaseScene();
    }

    public void GenerateAnotherMap(GameObject currentMapGenToDestroy, ref GameObject playerObjectInstance)
    {
        Destroy(currentMapGenToDestroy);

        MapGenerator mapGenerator = Instantiate(mapGeneratorObject, transform).GetComponent<MapGenerator>();

        mapGenerator.playerObject = playerObjectInstance;

        mapGenerator.StartLoadBaseScene();
    }
}
