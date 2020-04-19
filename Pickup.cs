using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour {

    public string internalName;

    public float dropWeight;

    public enum SoundName
    {
        GenericPickup,
        KeyPickup,
        HealthPickup
    };

    public SoundName soundName;

    public float healthValue = 0f;

    public int usamValue = 0;

    public int shotgunAmmoValue = 0;

    public int energyAmmoValue = 0;

    public int keysValue = 0;

    GameObject playerObject;

    PlayerController playerController;

    [System.NonSerialized]
    public float minRangeToBePicked;

    [System.NonSerialized]
    public float maxRangeToBePicked;

    private void OnValidate()
    {
        internalName = internalName.ToLowerInvariant();

        //if (GetComponent<GenericItem>() == null)    // move GenericItem just below sprite renderer, as first script
        //{
        //    GenericItem genericItem = gameObject.AddComponent<GenericItem>();
        //    for (int i = 0; i < 20; i++)
        //        UnityEditorInternal.ComponentUtility.MoveComponentUp(genericItem);
        //    for (int i = 0; i < 1; i++)
        //        UnityEditorInternal.ComponentUtility.MoveComponentDown(genericItem);
        //}
    }

    private void Start()
    {
        playerObject = GameObject.FindWithTag("Player");
        playerController = playerObject.GetComponent<PlayerController>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (healthValue > 0f && usamValue == 0 && shotgunAmmoValue == 0 && energyAmmoValue == 0 && keysValue == 0 && playerController.currentHealth >= playerController.maxHealth)
                return;     // if it's a purely healing pickup and the player is already at or above max health, don't pick it up

            playerController.currentHealth += Mathf.RoundToInt(healthValue);
            if (playerController.currentHealth > playerController.maxHealth)
                playerController.currentHealth = playerController.maxHealth;
            else
                playerController.currentHealth = Mathf.Round(playerController.currentHealth);


            playerController.totalHealthRestored += Mathf.RoundToInt(healthValue);


            playerController.heldAmmoUSAM += usamValue;
            playerController.heldAmmoShells += shotgunAmmoValue;
            playerController.heldAmmoEnergy += energyAmmoValue;
            playerController.heldKeys += keysValue;

            playerController.audioManager.Play(soundName.ToString());

            Destroy(gameObject);
        }
    }

}
