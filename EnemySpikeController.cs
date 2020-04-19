using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpikeController : MonoBehaviour {

    public float baseDamage;

    public float gracePeriod;

    public float phaseTransitionTime;

    public Sprite[] phaseSprites;

    SpriteRenderer spriteRenderer;

    PolygonCollider2D[] colliders;

    float latestPhaseChangeTime;

    int currentPhaseIndex = 0;

    [HideInInspector]
    public float damage;

    [HideInInspector]
    public float[] latestDmgToPlayersTimes = new float[4];

    [System.NonSerialized]
    public bool finishedRising = false;

    public EnemyController enemyController;

    void Awake () {
        enemyController = transform.parent.GetComponent<EnemyController>();
        transform.parent = null;
        for (int i = 0; i < latestDmgToPlayersTimes.Length; i++)
            latestDmgToPlayersTimes[i] = Mathf.NegativeInfinity;
        spriteRenderer = GetComponent<SpriteRenderer>();
        colliders = GetComponents<PolygonCollider2D>();
        spriteRenderer.sprite = phaseSprites[0];
        for (int i = colliders.Length - 1; i > 0; i--)
            colliders[i].enabled = false;
        latestPhaseChangeTime = Time.time;
        damage = baseDamage;
	}

    void Update () {
        if (Time.time >= latestPhaseChangeTime + (phaseTransitionTime / enemyController.attackSpeedMultiplier) && finishedRising == false)
        {
            currentPhaseIndex++;
            latestPhaseChangeTime = Time.time;
            spriteRenderer.sprite = phaseSprites[currentPhaseIndex];
            colliders[currentPhaseIndex - 1].enabled = false;
            colliders[currentPhaseIndex].enabled = true;
            if (currentPhaseIndex == phaseSprites.Length - 1)
                finishedRising = true;
        }
        if (Time.time >= latestPhaseChangeTime + (phaseTransitionTime / enemyController.attackSpeedMultiplier) && finishedRising == true)
        {
            currentPhaseIndex--;
            if (currentPhaseIndex < 0)
            {
                Destroy(gameObject);
                return;
            }
            latestPhaseChangeTime = Time.time;
            spriteRenderer.sprite = phaseSprites[currentPhaseIndex];
            colliders[currentPhaseIndex + 1].enabled = false;
            colliders[currentPhaseIndex].enabled = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameObject playerObject = other.gameObject;
            PlayerController playerController = playerObject.GetComponent<PlayerController>();
            if (Time.time > latestDmgToPlayersTimes[playerController.playerIndex] + gracePeriod)
            {
                if (playerController.invincibilityLayers < 1)
                {
                    damage *= enemyController.damageMultiplier;
                    damage -= playerController.Armor;
                    if (damage < 1)
                        damage = 1;
                    playerController.currentHealth -= damage;
                    latestDmgToPlayersTimes[playerController.playerIndex] = Time.time;  // for grace period between hits
                    playerController.UpdateAfterGettingHitEffects();
                }
            }
        }
    }
}
