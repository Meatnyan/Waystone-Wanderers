using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActivatedAbilityCooldownBar : MonoBehaviour {

    GameObject playerObject;

    PlayerController playerController;

    RectTransform rectTransform;

    public Image sandstormImage;

    Image cooldownBarImage;

    GameObject cooldownBgObject;

    ActivatedAbilitySandstorm activatedAbilitySandstorm;

    Vector2 startingLocalScale;

    Vector2 startingAnchoredPosition;

    [System.NonSerialized]
    public bool animationRunning = false;

	void Start () {
        playerObject = GameObject.FindWithTag("Player");
        activatedAbilitySandstorm = playerObject.GetComponent<ActivatedAbilitySandstorm>();
        rectTransform = GetComponent<RectTransform>();
        startingLocalScale = rectTransform.localScale;
        startingAnchoredPosition = rectTransform.anchoredPosition;
        cooldownBarImage = GetComponent<Image>();
        cooldownBgObject = GameObject.FindWithTag("CooldownBg");
        cooldownBarImage.enabled = false;
        cooldownBgObject.SetActive(false);
	}

    IEnumerator CooldownBarAnimation()
    {
        animationRunning = true;
        cooldownBarImage.enabled = true;
        cooldownBgObject.SetActive(true);
        while (true)
        {
            rectTransform.localScale = new Vector2(startingLocalScale.x,
            startingLocalScale.y * (((activatedAbilitySandstorm.timeOfAbilityEnd + activatedAbilitySandstorm.cooldown) - Time.time) / activatedAbilitySandstorm.cooldown));
            rectTransform.anchoredPosition = new Vector2(startingAnchoredPosition.x,
            startingAnchoredPosition.y - (rectTransform.sizeDelta.y / 2f - rectTransform.sizeDelta.y * (((activatedAbilitySandstorm.timeOfAbilityEnd + activatedAbilitySandstorm.cooldown) - Time.time) / activatedAbilitySandstorm.cooldown) / 2f));
            if(rectTransform.localScale.y <= 0f)
            {
                animationRunning = false;
                activatedAbilitySandstorm.allowTrigger = true;
                cooldownBarImage.enabled = false;
                cooldownBgObject.SetActive(false);
                break;
            }
            yield return new WaitForSeconds(0.15f);
        }
    }
	
	void Update ()
    {
        if (activatedAbilitySandstorm != null)
        {
            var activatedAbility = activatedAbilitySandstorm;
            if (animationRunning == false && Time.time > (activatedAbility.timeOfAbilityEnd) && Time.time < activatedAbility.timeOfAbilityEnd + activatedAbility.cooldown)
                StartCoroutine(CooldownBarAnimation());
        }
        if (cooldownBgObject.activeSelf)
            cooldownBarImage.enabled = true;
	}
}
