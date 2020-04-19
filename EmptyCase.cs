using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EmptyCase : MonoBehaviour
{
    public GameObject caseObject;

    public float totalSpreadAngle;

    public float speed;

    public float minRotation;

    public float maxRotation;

    public float duration;

    public float dmgMultiplier;

    PlayerController playerController;

    [System.NonSerialized]
    public bool donePickingUp = false;

    private void Awake()
    {
        playerController = FindObjectOfType<PlayerController>();
    }

    void Update()
    {
        if (donePickingUp == false && transform.root == playerController.transform)
        {
            playerController.emptyCase = this;
            donePickingUp = true;
        }
    }

    public void ShootCase(float originalShotRotEulerZ, float originalShotDamage, int originalAmountOfShotsInWave)
    {
        ShotMover caseShotMover = Instantiate(caseObject, playerController.transform.position,
            Quaternion.Euler(0f, 0f, Random.Range(originalShotRotEulerZ - totalSpreadAngle / 2 + 180, originalShotRotEulerZ + totalSpreadAngle / 2 + 180)), null).GetComponent<ShotMover>();

        caseShotMover.GetComponent<Rigidbody2D>().velocity = caseShotMover.transform.right * speed;
        caseShotMover.damage = originalShotDamage * originalAmountOfShotsInWave * dmgMultiplier;
        caseShotMover.maxDuration = duration;
        caseShotMover.transform.DORotate(new Vector3(0f, 0f, Random.Range(minRotation, maxRotation)), duration - 0.1f, RotateMode.FastBeyond360);
    }
}
