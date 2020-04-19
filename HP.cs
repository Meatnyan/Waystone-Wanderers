using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HP : MonoBehaviour {

    Text hpText;

    PlayerController playerController;

    public Image HPBarFillImg;

    float startingWidth;

    private void Start()
    {
        hpText = GetComponent<Text>();
        playerController = FindObjectOfType<PlayerController>();

        startingWidth = HPBarFillImg.rectTransform.sizeDelta.x;     // called Width in RectTransform in inspector, Height is .y
    }

    private void Update()
    {
        hpText.text = "HP: " + playerController.currentHealth + "/" + playerController.maxHealth;
        if (playerController.maxHealth >= 1000)
            hpText.fontSize = 24;
        else
            hpText.fontSize = 32;

        HPBarFillImg.rectTransform.sizeDelta = new Vector2 (startingWidth * playerController.currentHealth / playerController.maxHealth, HPBarFillImg.rectTransform.sizeDelta.y);
    }
}
