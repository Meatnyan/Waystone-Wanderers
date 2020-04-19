using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunNut : MonoBehaviour
{
    [Header("Flat Bonuses (in percentage points)")]
    public float rangedWeaponChance;

    public float woodenChestWeaponDropChance;

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
            playerController.rangedWeaponChance += rangedWeaponChance; // probably a good idea to balance weapon chance and ranged chance so that melee weapon chance is unaffected
            playerController.woodenChestWeaponDropChance += woodenChestWeaponDropChance;

            playerController.gunNut = this;
            donePickingUp = true;
        }
    }
}
