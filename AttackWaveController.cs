using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackWaveController : MonoBehaviour {

    PlayerController playerController;

    [System.NonSerialized]
    public GameObject parentObject;

    [System.NonSerialized]
    public MeleeAttacker meleeAttacker;

    [System.NonSerialized]
    public UnarmedAttacker unarmedAttacker;

    public bool meleeRange = true;

    public bool destroysRegularProjectiles = true;

    public bool destroysMassiveProjectiles = false;

    [System.NonSerialized]
    public float burnChance = 0f;

    [System.NonSerialized]
    public float chillChance = 0f;

    [System.NonSerialized]
    public float bleedChance = 0f;

    PolygonCollider2D polygonCollider2D;

    [System.NonSerialized]
    public List<int> spawnedIDsToIgnore = new List<int>();

    [System.NonSerialized]
    public float spawnTime;

    [System.NonSerialized]
    public float damage = 0f;

    [System.NonSerialized]
    public float maxDuration = 0f;

    private void Awake()
    {
        spawnTime = Time.time;
        playerController = FindObjectOfType<PlayerController>();
        polygonCollider2D = GetComponent<PolygonCollider2D>();
        if(meleeRange)
        {
            float distAtWvToPlayer = -Vector2.Distance(playerController.transform.position, transform.position);
            Vector2[] pathToPlayer = new Vector2[] { polygonCollider2D.points[0],
                new Vector2(distAtWvToPlayer, polygonCollider2D.points[1].y),
                new Vector2(distAtWvToPlayer, polygonCollider2D.points[2].y),
                polygonCollider2D.points[3],
                polygonCollider2D.points[4],
                polygonCollider2D.points[5] };

            polygonCollider2D.SetPath(0, pathToPlayer);
        }

        parentObject = transform.parent.gameObject;
        transform.parent = null;


        meleeAttacker = parentObject.GetComponent<MeleeAttacker>();
        if (meleeAttacker != null)
        {
            maxDuration = meleeAttacker.attackWaveDuration;
            damage = meleeAttacker.damage;
        }
        else
        {
            unarmedAttacker = parentObject.GetComponent<UnarmedAttacker>();
            if (unarmedAttacker != null)
            {
                maxDuration = unarmedAttacker.attackWaveDuration;
                damage = unarmedAttacker.damage;
            }
        }


        playerController.UpdateAttackWaveEffects(gameObject);
    }

    private void Update()
    {
        if (Time.time >= spawnTime + maxDuration)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D otherCollider)
    {
        Vector2 contactPoint = otherCollider.ClosestPoint(transform.position);
        float totalDamage = damage;

        if(otherCollider.CompareTag("Enemy") && !spawnedIDsToIgnore.Contains(otherCollider.GetComponent<EnemyController>().spawnedID))
        {
            GameObject enemyObject = otherCollider.gameObject;
            EnemyController enemyController = enemyObject.GetComponent<EnemyController>();

            if ((meleeAttacker != null && meleeAttacker.allowMultipleHitsOnSameEnemy == false) || (unarmedAttacker != null && unarmedAttacker.allowMultipleHitsOnSameEnemy == false))
                spawnedIDsToIgnore.Add(enemyController.spawnedID);

            playerController.UpdateDamageAttackWave(gameObject, ref totalDamage);
            playerController.UpdateTotalDamage(gameObject, ref totalDamage, enemyObject);
            totalDamage -= enemyController.armor;
            if (totalDamage < 1)
                totalDamage = 1;

            playerController.UpdateOnMeleeHitEffects(hitterObj: gameObject, hitObj: enemyObject, contactPoint: contactPoint);
            playerController.UpdateOnHitEffects(weaponObj: parentObject, hitterObj: gameObject, hitObj: enemyObject, hitWall: false);

            enemyController.CurrentHealth -= totalDamage;

            if (meleeAttacker != null)
            {
                if (meleeAttacker.flipped)
                    enemyController.rb.AddForce(-transform.right * 10 * meleeAttacker.knockbackMultiplier * playerController.knockbackMultiplier * playerController.knockbackDirectionMultiplier, ForceMode2D.Impulse);
                else
                    enemyController.rb.AddForce(transform.right * 10 * meleeAttacker.knockbackMultiplier * playerController.knockbackMultiplier * playerController.knockbackDirectionMultiplier, ForceMode2D.Impulse);
            }
            else if(unarmedAttacker != null)
            {
                if (unarmedAttacker.flipped)
                    enemyController.rb.AddForce(-transform.right * 10 * unarmedAttacker.knockbackMultiplier * playerController.knockbackMultiplier * playerController.knockbackDirectionMultiplier, ForceMode2D.Impulse);
                else
                    enemyController.rb.AddForce(transform.right * 10 * unarmedAttacker.knockbackMultiplier * playerController.knockbackMultiplier * playerController.knockbackDirectionMultiplier, ForceMode2D.Impulse);
            }

            if (enemyController.CurrentHealth <= 0)
            {
                playerController.UpdateOnMeleeKillEffects(attackWaveObj: gameObject, enemyObj: enemyObject);
                playerController.UpdateOnKillEffects();
            }
        }

        if (otherCollider.CompareTag("Obstacle") && !spawnedIDsToIgnore.Contains(otherCollider.GetComponent<ObstacleController>().spawnedID))
        {
            GameObject obstacleObject = otherCollider.gameObject;
            ObstacleController obstacleController = obstacleObject.GetComponent<ObstacleController>();

            if ((meleeAttacker != null && meleeAttacker.allowMultipleHitsOnSameEnemy == false) || (unarmedAttacker != null && unarmedAttacker.allowMultipleHitsOnSameEnemy == false))
                spawnedIDsToIgnore.Add(obstacleController.spawnedID);

            playerController.UpdateDamageAttackWave(gameObject, ref totalDamage);
            playerController.UpdateTotalDamage(gameObject, ref totalDamage, obstacleObject);
            totalDamage -= obstacleController.armor;
            if (totalDamage < 1)
                totalDamage = 1;

            playerController.UpdateOnMeleeHitEffects(hitterObj: gameObject, hitObj: obstacleObject, contactPoint: contactPoint);
            playerController.UpdateOnHitEffects(weaponObj: parentObject, hitterObj: gameObject, hitObj: obstacleObject, hitWall: false);

            obstacleController.currentHealth -= totalDamage;
        }

        if(otherCollider.CompareTag("EnemyShot"))
        {
            GameObject enemyShotObject = otherCollider.gameObject;
            EnemyShotMover enemyShotMover = enemyShotObject.GetComponent<EnemyShotMover>();

            if (destroysRegularProjectiles && !enemyShotMover.isMassive)
            {
                Destroy(enemyShotObject);
                return;
            }
            if (destroysMassiveProjectiles && enemyShotMover.isMassive)
                Destroy(enemyShotObject);
        }
    }

}
