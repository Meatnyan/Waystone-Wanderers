using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingSandstone : MonoBehaviour
{
    public Sprite enragedSprite;

    public float angleToApproachPlayer;

    public float launchSpeedBoost;

    public float launchBoostTime;

    [HideInInspector]
    public bool orbiting = false;

    [HideInInspector]
    public bool enraged = false;

    [HideInInspector]
    public bool forceEnrage = false;

    PlayerController playerController;

    EnemyController enemyController;

    Sprite asleepSprite;

    [HideInInspector]
    public Rigidbody2D rb;

    [HideInInspector]
    public float launchTime = Mathf.NegativeInfinity;

    private void Start()
    {
        enemyController = GetComponent<EnemyController>();
        playerController = enemyController.playerController;
        rb = enemyController.rb;

        asleepSprite = enemyController.spriteRenderer.sprite;
    }

    private void Update()
    {
        if (!orbiting && (enemyController.IsAggrod || forceEnrage))
        {
            enraged = true;
            enemyController.spriteRenderer.sprite = enragedSprite;
        }
        else
        {
            enraged = false;
            enemyController.spriteRenderer.sprite = asleepSprite;
        }

        if (enraged)
        {
            enemyController.rotator.transform.rotation = Quaternion.Euler(0f, 0f, GenericExtensions.GetRotationToFaceTargetPosition(transform.position, playerController.transform.position)
                - angleToApproachPlayer);

            enemyController.rb.velocity = enemyController.rotator.transform.up * enemyController.moveSpeed;
            if (Time.time < launchTime + launchBoostTime)
                enemyController.rb.velocity *= launchSpeedBoost;
        }
    }

    private void OnDestroy()
    {
        if (orbiting)
            transform.root.GetComponent<SandstoneElemental>().livingSandstones.Remove(this);
    }
}
