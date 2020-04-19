using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GodParticle : MonoBehaviour
{
    public int extraParticles;

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
            playerController.godParticle = this;
            donePickingUp = true;
        }
    }
}
