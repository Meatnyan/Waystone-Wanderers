using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChilledChilli : MonoBehaviour {

    PlayerController playerController;

    [System.NonSerialized]
    public bool donePickingUp = false;

    void Awake()
    {
        playerController = FindObjectOfType<PlayerController>();
    }

    void Update()
    {
        if (donePickingUp == false && transform.root == playerController.transform)
        {
            playerController.chilledChilli = this;
            donePickingUp = true;
        }
    }
}
