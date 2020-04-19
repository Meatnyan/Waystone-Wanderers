using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatedAbilitySandstorm : MonoBehaviour {

    public GameObject sandstormObject;

    public float duration;

    public float cooldown;

    [System.NonSerialized]
    public float timeOfAbilityTrigger = Mathf.NegativeInfinity;

    [System.NonSerialized]
    public float timeOfAbilityEnd = Mathf.Infinity;

    [System.NonSerialized]
    public bool allowTrigger = true;

    PlayerController playerController;

    float startingDuration;

    float startingCooldown;

    GameObject cooldownBgObject;

    void Start () {
        playerController = gameObject.GetComponent<PlayerController>();
        cooldownBgObject = GameObject.FindWithTag("CooldownBg");
        startingDuration = duration;
        startingCooldown = cooldown;
	}
	
	void Update () {
        duration = startingDuration * playerController.abilityDurationMultiplier;
        cooldown = startingCooldown * playerController.abilityCooldownMultiplier;

		if(playerController.allowControl && (Input.GetKeyDown(KeyCode.LeftShift) && allowTrigger))
        {
            timeOfAbilityTrigger = Time.time;
            cooldownBgObject.SetActive(true);
            Instantiate(sandstormObject, transform);
        }

	}

}
