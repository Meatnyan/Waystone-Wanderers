using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverController : MonoBehaviour
{
    public float timeBeforeFlavorText;

    public float timeAfterFlavorText;

    public float timeBetweenTheRest;

    public float timeBeforeTotalScore;

    public float timeBeforeNewHighscore;

    public float timeBeforePreviousBest;

    public float crossfadeTime;

    public GameObject flavorTextObject;

    public GameObject levelsBeatenTextObject;

    public GameObject itemsObtainedTextObject;

    public GameObject enemiesKilledTextObject;

    public GameObject scoreTextObject;

    public GameObject newHighscoreObject;

    public GameObject tryAgainButton;

    public GameObject exitButton;

    public List<string> flavorTexts;

    PlayerController playerController;

    WorldManager worldManager;

    [HideInInspector]
    public MainMenu mainMenu;

    Coroutine displayGameOverScreenCoroutine = null;

    Coroutine newHighscoreFlasherCoroutine = null;

    int levelsBeaten = 0;

    int amountOfItemsObtained = 0;

    int totalKillCount = 0;

    RestartManager restartManager;

    int score = 0;

    AchievementManager achievementManager;

    bool skipWaiting = false;

    private void Awake()
    {
        playerController = FindObjectOfType<PlayerController>();
        worldManager = FindObjectOfType<WorldManager>();
        restartManager = FindObjectOfType<RestartManager>();
        achievementManager = FindObjectOfType<AchievementManager>();
    }

    public void StartDisplayGameOverScreen()
    {
        displayGameOverScreenCoroutine = StartCoroutine(DisplayGameOverScreen());
    }

    IEnumerator DisplayGameOverScreen()
    {
        flavorTextObject.SetActive(false);
        levelsBeatenTextObject.SetActive(false);
        itemsObtainedTextObject.SetActive(false);
        enemiesKilledTextObject.SetActive(false);
        scoreTextObject.SetActive(false);
        newHighscoreObject.transform.GetChild(0).GetComponent<Text>().text = "(previous best: ";
        newHighscoreObject.transform.GetChild(0).gameObject.SetActive(false);
        newHighscoreObject.SetActive(false);

        levelsBeaten = worldManager.levelsBeaten;
        amountOfItemsObtained = playerController.amountOfItemsObtained;
        totalKillCount = playerController.totalKillCount;

        score = levelsBeaten * 1000 + amountOfItemsObtained * 100 + totalKillCount * 10;

        bool displayNewHighscore = false;
        int previousHighscore = achievementManager.highscore;
        if (score > achievementManager.highscore)
        {
            achievementManager.highscore = score;

            displayNewHighscore = true;
        }

        achievementManager.UpdateSaveFile();



        if (!skipWaiting)
            yield return new WaitForSecondsRealtime(timeBeforeFlavorText);

        flavorTextObject.SetActive(true);
        flavorTextObject.GetComponent<Text>().text = flavorTexts[Random.Range(0, flavorTexts.Count)];

        if(!skipWaiting)
            yield return new WaitForSecondsRealtime(timeAfterFlavorText);


        levelsBeatenTextObject.SetActive(true);
        levelsBeatenTextObject.transform.GetChild(0).GetComponent<Text>().text = "" + levelsBeaten;

        if(!skipWaiting)
            yield return new WaitForSecondsRealtime(timeBetweenTheRest);

        itemsObtainedTextObject.SetActive(true);
        itemsObtainedTextObject.transform.GetChild(0).GetComponent<Text>().text = "" + amountOfItemsObtained;

        if(!skipWaiting)
            yield return new WaitForSecondsRealtime(timeBetweenTheRest);

        enemiesKilledTextObject.SetActive(true);
        enemiesKilledTextObject.transform.GetChild(0).GetComponent<Text>().text = "" + totalKillCount;

        if(!skipWaiting)
            yield return new WaitForSecondsRealtime(timeBeforeTotalScore);

        scoreTextObject.SetActive(true);
        scoreTextObject.transform.GetChild(0).GetComponent<Text>().text = "" + score;

        if (displayNewHighscore)
        {
            if(!skipWaiting)
                yield return new WaitForSecondsRealtime(timeBeforeNewHighscore);

            newHighscoreObject.SetActive(true);
            newHighscoreFlasherCoroutine = StartCoroutine(NewHighscoreFlasher());

            if (!skipWaiting && timeBeforePreviousBest > 0f)
                yield return new WaitForSecondsRealtime(timeBeforePreviousBest);

            newHighscoreObject.transform.GetChild(0).gameObject.SetActive(true);
            newHighscoreObject.transform.GetChild(0).GetComponent<Text>().text += previousHighscore + ")";
        }

        displayGameOverScreenCoroutine = null;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R) && displayGameOverScreenCoroutine != null)
        {
            GenericExtensions.StopCoroutineAndMakeItNullIfItExists(this, ref displayGameOverScreenCoroutine);

            TryAgain();
            return;
        }

        if (displayGameOverScreenCoroutine == null)     // if done displaying everything, press enter or space (or R) to Try Again or escape to Exit
        {
            if (Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.R))
            {
                TryAgain();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Exit();
                return;
            }
        }

        if ((Input.GetKeyDown(KeyCode.Mouse0) || Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.Escape)) && displayGameOverScreenCoroutine != null)
            skipWaiting = true;
    }

    public void TryAgain()
    {
        achievementManager.UpdateSaveFile();

        GenericExtensions.StopCoroutineAndMakeItNullIfItExists(this, ref newHighscoreFlasherCoroutine);

        worldManager.levelIsLoaded = false;
        restartManager.DestroyAllListedDDOL();
        restartManager.LoadMainMenuSceneAndRestart();
    }

    public void Exit()
    {
        achievementManager.UpdateSaveFile();

        GenericExtensions.StopCoroutineAndMakeItNullIfItExists(this, ref newHighscoreFlasherCoroutine);

        worldManager.levelIsLoaded = false;
        restartManager.DestroyAllListedDDOL();
        restartManager.LoadMainMenuSceneWithoutRestarting();
    }

    IEnumerator NewHighscoreFlasher()
    {
        Text newHighscoreText = newHighscoreObject.GetComponent<Text>();

        while (true)
        {
            newHighscoreText.CrossFadeColor(Color.white, crossfadeTime, true, false);
            yield return new WaitForSecondsRealtime(crossfadeTime);
            newHighscoreText.CrossFadeColor(Color.yellow, crossfadeTime, true, false);
            yield return new WaitForSecondsRealtime(crossfadeTime);
        }
    }
}