using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartManager : MonoBehaviour
{
    [HideInInspector]
    public List<GameObject> objectsToDestroyWhenRestarting = new List<GameObject>();

    [HideInInspector]
    public bool restartPending = false;

    public static bool alreadyExists = false;

    private void Awake()
    {
        if(alreadyExists)
        {
            Destroy(gameObject);
            return;
        }

        alreadyExists = true;
        DontDestroyOnLoad(gameObject);  // CameraShaker and RestartManager are the only objects persistent throughout all scenes
    }

    public void DontDestroyOnLoadButDestroyWhenRestarting(GameObject chosenObject)
    {
        DontDestroyOnLoad(chosenObject);
        objectsToDestroyWhenRestarting.Add(chosenObject);
    }

    public void DestroyAllListedDDOL()
    {
        foreach (GameObject objToDestroy in objectsToDestroyWhenRestarting)
            Destroy(objToDestroy);


        objectsToDestroyWhenRestarting = new List<GameObject>();
    }

    public void DestroyAllUnownedWeapons(PlayerController playerController)
    {
        if (playerController != null)
        {
            foreach (GameObject weaponObj in playerController.worldManager.allWeaponsOnTheGround)
                Destroy(weaponObj);
        }
        else
            Debug.LogWarning("DestroyAllUnownedWeapons() can't continue - playerController is null");
    }

    public void LoadMainMenuSceneAndRestart()
    {
        Time.timeScale = 1;
        restartPending = true;
        SceneManager.LoadScene(0);  // 0 = MainMenu
    }

    public void LoadMainMenuSceneWithoutRestarting()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);  // 0 = MainMenu
    }
}