using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChargeBar : MonoBehaviour
{
    public Text maxText;

    public float crossFadeTime;

    public Color textFlashColorOne;

    public Color textFlashColorTwo;

    Image chargeBarImg;

    PlayerController playerController;

    float startingWidth;

    private void Awake()
    {
        playerController = FindObjectOfType<PlayerController>();
        chargeBarImg = GetComponent<Image>();
        chargeBarImg.enabled = false;
        maxText.enabled = false;

        startingWidth = chargeBarImg.rectTransform.sizeDelta.x;

        StartCoroutine(MaxTextFlasher());
    }

    private void Update()
    {
        if (playerController.heldShooter != null && playerController.heldShooter.charge > 0f)
        {
            chargeBarImg.enabled = true;

            chargeBarImg.rectTransform.sizeDelta = new Vector2(startingWidth * playerController.heldShooter.charge, chargeBarImg.rectTransform.sizeDelta.y);

            maxText.enabled = Mathf.Approximately(playerController.heldShooter.charge, 1f);
        }
        else
        {
            chargeBarImg.enabled = false;
            maxText.enabled = false;
        }
    }

    IEnumerator MaxTextFlasher()
    {
        while (true)
        {
            maxText.CrossFadeColor(textFlashColorOne, crossFadeTime, true, false);
            yield return new WaitForSecondsRealtime(crossFadeTime);
            maxText.CrossFadeColor(textFlashColorTwo, crossFadeTime, true, false);
            yield return new WaitForSecondsRealtime(crossFadeTime);
        }
    }
}
