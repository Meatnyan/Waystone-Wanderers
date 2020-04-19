using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemPickupPopup : MonoBehaviour {

    public float popupTransitionWait;

    public float popupDuration;

    RectTransform bgRectTransform;

    Text[] childrenTexts;

    Text itemNameText;

    Text flavorText;

    Image popupBackgroundImage;

    AudioSource itemPickupSound;

    Vector2 startingAnchoredPosition;

    Coroutine popupMoverCoroutine;

    PlayerController playerController;

	void Start () {
        playerController = FindObjectOfType<PlayerController>();
        playerController.itemPickupPopup = GetComponent<ItemPickupPopup>();
        bgRectTransform = GetComponent<RectTransform>();
        popupBackgroundImage = GetComponent<Image>();
        itemPickupSound = GetComponent<AudioSource>();
        childrenTexts = GetComponentsInChildren<Text>();
        foreach(Text childText in childrenTexts)
        {
            if (childText.fontSize == 32)
                itemNameText = childText;
            else
                flavorText = childText;
        }
        startingAnchoredPosition = bgRectTransform.anchoredPosition;
        itemNameText.enabled = false;
        flavorText.enabled = false;
        popupBackgroundImage.enabled = false;
	}
	
    public void StartPopup(StatModifier statModifier)
    {
        if(popupMoverCoroutine != null)
            StopCoroutine(popupMoverCoroutine);
        popupMoverCoroutine = StartCoroutine(PopupMover(statModifier));
    }

    IEnumerator PopupMover(StatModifier statModifier)
    {
        itemPickupSound.Play();
        popupBackgroundImage.enabled = true;
        bgRectTransform.anchoredPosition = new Vector2(startingAnchoredPosition.x, startingAnchoredPosition.y * -0.5f);
        yield return new WaitForSeconds(popupTransitionWait);
        bgRectTransform.anchoredPosition = new Vector2(startingAnchoredPosition.x, 0);
        yield return new WaitForSeconds(popupTransitionWait);
        bgRectTransform.anchoredPosition = new Vector2(startingAnchoredPosition.x, startingAnchoredPosition.y * 0.5f);
        yield return new WaitForSeconds(popupTransitionWait);
        bgRectTransform.anchoredPosition = startingAnchoredPosition;

        itemNameText.enabled = true;
        flavorText.enabled = true;
        itemNameText.text = statModifier.displayedName;
        flavorText.text = statModifier.flavorText;

        yield return new WaitForSeconds(popupDuration);
        itemNameText.enabled = false;
        flavorText.enabled = false;
        bgRectTransform.anchoredPosition = new Vector2(startingAnchoredPosition.x, startingAnchoredPosition.y * 0.5f);
        yield return new WaitForSeconds(popupTransitionWait);
        bgRectTransform.anchoredPosition = new Vector2(startingAnchoredPosition.x, 0);
        yield return new WaitForSeconds(popupTransitionWait);
        bgRectTransform.anchoredPosition = new Vector2(startingAnchoredPosition.x, startingAnchoredPosition.y * -0.5f);
        yield return new WaitForSeconds(popupTransitionWait);
        popupBackgroundImage.enabled = false;
    }
}
