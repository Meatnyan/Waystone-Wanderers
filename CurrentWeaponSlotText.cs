using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrentWeaponSlotText : MonoBehaviour {

    PlayerController playerController;

    Text currentWeaponSlotText;

    int latestWeaponSlot = 1;

	void Awake () {
        playerController = FindObjectOfType<PlayerController>();
        currentWeaponSlotText = GetComponent<Text>();
        currentWeaponSlotText.text = "";
	}
	
	void Update () {
        if(playerController.curWeaponSlot != latestWeaponSlot)
        {
            currentWeaponSlotText.text = "" + playerController.curWeaponSlot;
            currentWeaponSlotText.canvasRenderer.SetAlpha(1f);
            currentWeaponSlotText.CrossFadeAlpha(0f, 1f, false);
            latestWeaponSlot = playerController.curWeaponSlot;
        }
	}
}
