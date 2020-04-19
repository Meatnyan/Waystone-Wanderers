using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GodRay : MonoBehaviour
{
    public float duration;

    public float durLeftToStartDisappearing;

    [Space(10f)]
    public float baseDamage;

    public float timeBetweenContactDmgTicks;

    [HideInInspector]
    public Dictionary<int, float> latestDmgTicks = new Dictionary<int, float>();

    [HideInInspector]
    public float damageMultiplier = 1f;

    float damage;

    Animator animator;

    float spawnTime;

    [HideInInspector]
    public bool isDisappearing = false;

    [HideInInspector]
    public GodRaySpawner parentSpawner;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spawnTime = Time.time;
    }

    private void Start()
    {
        damage = baseDamage * damageMultiplier; // damageMultiplier is set in SpawnGodRay() in GodRaySpawner
    }

    private void Update()
    {
        if(Time.time >= spawnTime + duration)
        {
            Destroy(gameObject);
            if(parentSpawner != null)
                parentSpawner.currentAmountSpawned--;
            return;
        }

        if (!isDisappearing && Time.time >= spawnTime + duration - durLeftToStartDisappearing)
        {
            animator.Play("GodRay_Start_Reverse");
            isDisappearing = true;
        }
    }

    void CheckAndApplyCollisionDamage(Collider2D collider2D)
    {
        if (collider2D.gameObject.CompareTag("Enemy"))
        {
            EnemyController enemyController = collider2D.gameObject.GetComponent<EnemyController>();
            if (latestDmgTicks.ContainsKey(enemyController.spawnedID) == false || Time.time >= latestDmgTicks[enemyController.spawnedID] + timeBetweenContactDmgTicks)
            {
                float totalDamage = damage - enemyController.armor;
                if (totalDamage < 1f)
                    totalDamage = 1f;
                enemyController.CurrentHealth -= totalDamage;

                if (latestDmgTicks.ContainsKey(enemyController.spawnedID) == false)
                    latestDmgTicks.Add(enemyController.spawnedID, Time.time);
                else
                    latestDmgTicks[enemyController.spawnedID] = Time.time;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collider2D)
    {
        CheckAndApplyCollisionDamage(collider2D);
    }

    private void OnTriggerStay2D(Collider2D collider2D)
    {
        CheckAndApplyCollisionDamage(collider2D);
    }
}
