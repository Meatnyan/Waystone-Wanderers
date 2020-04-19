using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathsBeckoning : MonoBehaviour
{
    public float maxDmgMultiplierBonus;

    public float distancePenaltyMultiplier;

    GameObject playerObject;

    PlayerController playerController;

    [System.NonSerialized]
    public bool donePickingUp = false;

    private void Awake()
    {
        playerObject = GameObject.FindWithTag("Player");
        playerController = playerObject.GetComponent<PlayerController>();
    }

    void Update()
    {
        if (donePickingUp == false && transform.root == playerObject.transform)
        {
            playerController.deathsBeckoning = this;
            playerController.knockbackDirectionMultiplier = -1;
            donePickingUp = true;
        }
    }
}
