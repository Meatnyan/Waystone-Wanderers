using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperGuildBadge : MonoBehaviour
{
    public float distanceBonusDivider;

    public float maxDmgMultiplierBonus;

    GameObject playerObject;

    PlayerController playerController;

    [System.NonSerialized]
    public bool donePickingUp = false;

    void Awake()
    {
        playerObject = GameObject.FindWithTag("Player");
        playerController = playerObject.GetComponent<PlayerController>();
    }

    void Update()
    {
        if (donePickingUp == false && transform.root == playerObject.transform)
        {
            playerController.sniperGuildBadge = this;
            donePickingUp = true;
        }
    }
}
