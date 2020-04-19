using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReloadingBar : MonoBehaviour
{
    Image reloadingBarImg;

    PlayerController playerController;

    float timeSinceReload = 0f;

    float reloadProgress = 0f;

    float startingWidth;

    private void Awake()
    {
        playerController = FindObjectOfType<PlayerController>();
        reloadingBarImg = GetComponent<Image>();
        reloadingBarImg.enabled = false;

        startingWidth = reloadingBarImg.rectTransform.sizeDelta.x;
    }

    private void Update()
    {
        if (playerController.heldShooter != null && playerController.heldShooter.reloading  // might wanna implement isReloading on PlayerController later for non-Shooters that reload
            && (!playerController.heldShooter.multiStepReloading || playerController.heldShooter.reloadingOneShot))
        {
            reloadingBarImg.enabled = true;

            timeSinceReload = Time.time - playerController.heldShooter.shotReloadStartTime;

            if (playerController.heldShooter.multiStepReloading)
                reloadProgress = timeSinceReload / playerController.heldShooter.reloadOneShotTime;
            else
                reloadProgress = timeSinceReload / playerController.heldShooter.reloadTime;

            reloadingBarImg.rectTransform.sizeDelta = new Vector2(startingWidth * reloadProgress, reloadingBarImg.rectTransform.sizeDelta.y);
        }
        else
            reloadingBarImg.enabled = false;
    }
}
