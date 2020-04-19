using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrentAmmo : MonoBehaviour {

    Text currentAmmo;

    PlayerController playerController;

    private void Start()
    {
        currentAmmo = GetComponent<Text>();
        playerController = FindObjectOfType<PlayerController>();
    }

    private void Update()
    {
        if (playerController.heldShooter != null)
            currentAmmo.text = "" + playerController.heldShooter.currentAmmo;
        else
            currentAmmo.text = "";
    }

}
