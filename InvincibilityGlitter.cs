using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvincibilityGlitter : MonoBehaviour {

    public float invincibilityPeriod;

    GameObject playerObject;

    PlayerController playerController;

    [System.NonSerialized]
    public bool donePickingUp = false;

    void Awake()
    {
        playerObject = GameObject.FindWithTag("Player");
        playerController = playerObject.GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (donePickingUp == false && transform.root == playerObject.transform)
        {
            playerController.invincibilityGlitter = this;
            donePickingUp = true;
        }
    }
}
