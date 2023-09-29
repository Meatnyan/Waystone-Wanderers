using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShotMover : MonoBehaviour {

    public bool isAttached;

    public bool isMassive;

    [HideInInspector]
    public bool isFriendly = false;

    [HideInInspector]
    public float damage;

    [HideInInspector]
    public float shotSpeed;

    [HideInInspector]
    public float maxDuration;

    [HideInInspector]
    public float detachTime = Mathf.Infinity;

    [HideInInspector]
    public Rigidbody2D rb;

    EnemyController enemyController;

    public void DetachFromParent()
    {
        transform.SetParent(null);

        detachTime = Time.time;
    }

    public void SetStats(float newDamage, float newShotSpeed, float newMaxDuration)
    {
        damage = newDamage;
        shotSpeed = newShotSpeed;
        maxDuration = newMaxDuration;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (transform.parent)
            enemyController = transform.root.GetComponent<EnemyController>();


        if (!isAttached)
            DetachFromParent();


        // SetStats() is called from Instantiate()ing object (EnemyShooter or OnDeathEvent)
        // and as such it triggers right after Awake(), before Start()
    }

    private void Start()
    {
        // velocity is set in Start() because shotSpeed is determined between Awake() and Start()
        if (!isAttached)
            rb.velocity = shotSpeed * enemyController.shotSpeedMultiplier * (-transform.right);
    }

    private void Update()
    {
        if (Time.time > detachTime + maxDuration)
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnTriggerEnter2D(Collider2D hitCollider)
    {
        if (hitCollider.CompareTag("Wall") || hitCollider.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
            return;
        }

        if(hitCollider.CompareTag("Player"))
        {
            PlayerController playerController = hitCollider.GetComponent<PlayerController>();

            if (playerController.invincibilityLayers < 1)
            {
                damage *= enemyController.damageMultiplier;
                damage -= playerController.Armor;

                if (damage < 1)
                    damage = 1;

                playerController.currentHealth -= damage;

                playerController.UpdateAfterGettingHitEffects();
            }

            Destroy(gameObject);
            return;
        }

        if(isFriendly && hitCollider.CompareTag("Enemy"))
        {
            EnemyController otherEnemyController = hitCollider.GetComponent<EnemyController>();

            damage *= enemyController.damageMultiplier;
            damage -= otherEnemyController.armor;

            if (damage < 1)
                damage = 1;

            otherEnemyController.CurrentHealth -= damage;


            Destroy(gameObject);
            return;
        }
    }
}
