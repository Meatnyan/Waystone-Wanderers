using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShotMover : MonoBehaviour {

    public Orientation orientation;

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
    public float spawnTime = Mathf.Infinity;

    [HideInInspector]
    public Rigidbody2D rb;

    EnemyShooter enemyShooter;

    EnemyController enemyController;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (transform.parent)
        {
            enemyController = transform.root.GetComponent<EnemyController>();
            enemyShooter = transform.parent.GetComponent<EnemyShooter>();
        }

        if (enemyShooter)
        {
            damage = enemyShooter.damage;
            shotSpeed = enemyShooter.shotSpeed;
            maxDuration = enemyShooter.shotMaxDuration;
        }

        if (!isAttached)
        {
            transform.SetParent(null);
            spawnTime = Time.time;
        }
    }

    private void Start()    // velocity needs to be set in Start so that shotSpeed can be modified after instantiating
    {
        if (!isAttached)
        {
            rb.velocity = shotSpeed * enemyController.shotSpeedMultiplier * (orientation switch
            {
                Orientation.left => -transform.right,
                Orientation.right => transform.right,
                Orientation.up => transform.up,
                Orientation.down => -transform.up,
                _ => Vector3.zero
            });            
        }
    }

    private void Update()
    {
        if (Time.time > spawnTime + maxDuration)
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
            return;
        }

        if(other.CompareTag("Player"))
        {
            GameObject playerObject = other.gameObject;
            PlayerController playerController = playerObject.GetComponent<PlayerController>();
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

        if(isFriendly && other.CompareTag("Enemy"))
        {
            GameObject otherEnemyObject = other.gameObject;
            EnemyController otherEnemyController = otherEnemyObject.GetComponent<EnemyController>();
            damage *= enemyController.damageMultiplier;
            damage -= otherEnemyController.armor;
            if (damage < 1)
                damage = 1;
            otherEnemyController.CurrentHealth -= damage;
            Destroy(gameObject);
        }
    }
}
