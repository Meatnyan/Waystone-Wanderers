using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeldAmmo : MonoBehaviour {

    Text heldAmmoText;

    PlayerController playerController;

    public GameObject ammoIconObject;

    Image ammoIcon;

    public Sprite usamIcon;

    public Sprite shellsIcon;

    public Sprite energyIcon;

    public Sprite meleeIcon;

    public Sprite unarmedIcon;

    private void Start()
    {
        heldAmmoText = GetComponent<Text>();
        playerController = FindObjectOfType<PlayerController>();
        ammoIcon = ammoIconObject.GetComponent<Image>();
    }

    private void Update()
    {
        if (playerController.heldShooter)
        {
            switch (playerController.heldShooter.ammoType)
            {
                case AmmoType.USAM:
                    ammoIcon.sprite = usamIcon;
                    heldAmmoText.text = "" + playerController.heldAmmoUSAM;
                    break;
                case AmmoType.Shells:
                    ammoIcon.sprite = shellsIcon;
                    heldAmmoText.text = "" + playerController.heldAmmoShells;
                    break;
                case AmmoType.Energy:
                    ammoIcon.sprite = energyIcon;
                    heldAmmoText.text = "" + playerController.heldAmmoEnergy;
                    break;
                default:
                    Debug.Log("Unrecognized ammo type on " + playerController.heldShooter);
                    break;
            }
        }
        else if(playerController.heldMeleeAttacker)
        {
            ammoIcon.sprite = meleeIcon;
            heldAmmoText.text = "";
        }
        else if(playerController.unarmedObject.activeSelf)
        {
            ammoIcon.sprite = unarmedIcon;
            heldAmmoText.text = "";
        }
    }

}
