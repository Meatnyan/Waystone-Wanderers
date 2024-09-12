using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuAndLoadingCanvas;

    public GameObject loadingScreen;

    public GameObject gameOverTextObject;

    public GameObject comingSoonTextObject;

    public CollectionScreen collectionScreen;

    LoadingController loadingController;

    GameOverController gameOverController;

    RestartManager restartManager;

    private void Awake()
    {
        loadingController = loadingScreen.GetComponent<LoadingController>();
        gameOverController = gameOverTextObject.GetComponent<GameOverController>();
        gameOverController.mainMenu = this;

        restartManager = FindObjectOfType<RestartManager>();

        restartManager.DontDestroyOnLoadButDestroyWhenRestarting(mainMenuAndLoadingCanvas);

        if(restartManager.restartPending)
        {
            restartManager.restartPending = false;
            StartGame();
        }
    }

    public void StartGame()
    {
        loadingScreen.SetActive(true);
        loadingController.BeginLoadingLevel();
        gameObject.SetActive(false);
    }

    public void StartCollectionScreen()
    {
        comingSoonTextObject.SetActive(!comingSoonTextObject.activeSelf);
        //collectionScreen.gameObject.SetActive(true);
        //collectionScreen.DisplayCollectionCategoryChoices();    // enables gray bg, weapons button and items button. back button is always enabled as long as collection screen is enabled
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitGame();
            return;
        }

        if(Input.GetButtonDown("Submit"))
        {
            StartGame();
            return;
        }
    }
}
