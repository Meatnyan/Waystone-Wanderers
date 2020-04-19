using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VampireFang : MonoBehaviour
{
    public int amountToHealFromRegularEnemies;

    public int amountToHealFromBosses;

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
            playerController.vampireFang = this;
            donePickingUp = true;
        }
    }
}
