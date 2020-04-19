using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSystemController : MonoBehaviour
{
    RestartManager restartManager;

    private void Awake()
    {
        restartManager = FindObjectOfType<RestartManager>();
        restartManager.DontDestroyOnLoadButDestroyWhenRestarting(gameObject);
    }
}
