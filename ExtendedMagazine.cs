using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtendedMagazine : MonoBehaviour
{
    PlayerController playerController;

    [System.NonSerialized]
    public bool donePickingUp = false;

    [System.NonSerialized]
    public List<int> spawnedIDsOfShootersWithExtendedMagApplied = new List<int>();

    void Awake()
    {
        playerController = FindObjectOfType<PlayerController>();
    }

    void Update()
    {
        if (donePickingUp == false && transform.root == playerController.transform)
        {
            playerController.extendedMagazine = this;
            donePickingUp = true;

            if (playerController.heldShooter != null)
                playerController.UpdateOnGlobalWeaponChangingEffects(playerController.heldShooter);
        }
    }
}
