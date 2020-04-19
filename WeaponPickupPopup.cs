using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponPickupPopup : MonoBehaviour {

    public float popupTransitionWait;

    public float popupDuration;

    RectTransform bgRectTransform;

    Text weaponNameText;

    Image popupBackgroundImage;

    AudioSource weaponPickupSound;

    Vector2 startingAnchoredPosition;

    Coroutine popupMoverCoroutine;

    PlayerController playerController;

	void Start () {
        playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        playerController.weaponPickupPopup = GetComponent<WeaponPickupPopup>();
        bgRectTransform = GetComponent<RectTransform>();
        popupBackgroundImage = GetComponent<Image>();
        weaponPickupSound = GetComponent<AudioSource>();
        weaponNameText = GetComponentInChildren<Text>();
        startingAnchoredPosition = bgRectTransform.anchoredPosition;
        weaponNameText.enabled = false;
        popupBackgroundImage.enabled = false;
	}

    public void StartPopup(Shooter shooter)
    {
        if (popupMoverCoroutine != null)
            StopCoroutine(popupMoverCoroutine);
        popupMoverCoroutine = StartCoroutine(PopupMover(shooter.displayedName));
    }

    public void StartPopup(MeleeAttacker meleeAttacker)
    {
        if (popupMoverCoroutine != null)
            StopCoroutine(popupMoverCoroutine);
        popupMoverCoroutine = StartCoroutine(PopupMover(meleeAttacker.displayedName));
    }

    IEnumerator PopupMover(string displayedName)
    {
        weaponPickupSound.Play();
        popupBackgroundImage.enabled = true;
        bgRectTransform.anchoredPosition = new Vector2(startingAnchoredPosition.x * -0.5f, startingAnchoredPosition.y);
        yield return new WaitForSeconds(popupTransitionWait);
        bgRectTransform.anchoredPosition = new Vector2(0, startingAnchoredPosition.y);
        yield return new WaitForSeconds(popupTransitionWait);
        bgRectTransform.anchoredPosition = new Vector2(startingAnchoredPosition.x * 0.5f, startingAnchoredPosition.y);
        yield return new WaitForSeconds(popupTransitionWait);
        bgRectTransform.anchoredPosition = startingAnchoredPosition;

        weaponNameText.enabled = true;
        weaponNameText.text = displayedName;

        yield return new WaitForSeconds(popupDuration);
        weaponNameText.enabled = false;
        bgRectTransform.anchoredPosition = new Vector2(startingAnchoredPosition.x * 0.5f, startingAnchoredPosition.y);
        yield return new WaitForSeconds(popupTransitionWait);
        bgRectTransform.anchoredPosition = new Vector2(0, startingAnchoredPosition.y);
        yield return new WaitForSeconds(popupTransitionWait);
        bgRectTransform.anchoredPosition = new Vector2(startingAnchoredPosition.x * -0.5f, startingAnchoredPosition.y);
        yield return new WaitForSeconds(popupTransitionWait);
        popupBackgroundImage.enabled = false;
    }

}
