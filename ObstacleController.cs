using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleController : MonoBehaviour {

    public float maxHealth;

    public float armor;

    [System.NonSerialized]
    public float currentHealth;

    public float contactDamage;

    public float timeBetweenContactDmgTicks = 0.25f;

    [System.NonSerialized]
    public float latestContactDmgTick = 0f;

    public Sprite[] phaseSprites;

    SpriteRenderer spriteRenderer;

    Collider2D[] colliders;

    [System.NonSerialized]
    public int currentPhaseIndex = 0;

    PlayerController playerController;

    [HideInInspector]
    public int spawnedID;

    private void Awake()
    {
        playerController = FindObjectOfType<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        colliders = GetComponents<Collider2D>();

        playerController.totalSpawnedID++;
        spawnedID = playerController.totalSpawnedID;

        spriteRenderer.sprite = phaseSprites[0];
        for (int i = colliders.Length - 1; i > 0; i--)
            colliders[i].enabled = false;
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if(currentHealth <= 0 && currentPhaseIndex < phaseSprites.Length - 1)
        {
            currentPhaseIndex++;
            currentHealth = maxHealth;
            spriteRenderer.sprite = phaseSprites[currentPhaseIndex];
            colliders[currentPhaseIndex - 1].enabled = false;
            if (currentPhaseIndex <= colliders.Length - 1)
                colliders[currentPhaseIndex].enabled = true;
        }
    }

    void TryApplyDamageToPlayer()
    {
        if (Time.time > latestContactDmgTick + timeBetweenContactDmgTicks)
        {
            if (playerController.invincibilityLayers < 1)
            {
                playerController.currentHealth -= contactDamage;

                latestContactDmgTick = Time.time;

                playerController.UpdateAfterGettingHitEffects();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            TryApplyDamageToPlayer();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            TryApplyDamageToPlayer();
    }
}
