using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectralBlade : MonoBehaviour {

    public GameObject spectralCopy;

    public float copySpeedMultiplier;

    AttackWaveController attackWaveController;

    Rigidbody2D rb;

    SpriteRenderer spriteRenderer;

    SpriteRenderer spectralCopySpriteRenderer;

    float spawnTime;

    float attackWaveDuration;

    [HideInInspector]
    public float partOfTimeLeft;

    private void Start()
    {
        attackWaveController = GetComponent<AttackWaveController>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spectralCopySpriteRenderer = spectralCopy.GetComponent<SpriteRenderer>();

        if (attackWaveController.meleeAttacker.flipped)
            rb.velocity = -transform.right * copySpeedMultiplier;
        else
            rb.velocity = transform.right * copySpeedMultiplier;

        attackWaveDuration = attackWaveController.meleeAttacker.attackWaveDuration;

        spawnTime = attackWaveController.spawnTime;
    }

    private void Update()
    {
        partOfTimeLeft = 1 - (Time.time - spawnTime) / attackWaveDuration;

        spriteRenderer.color = new Color(1f, 1f, 1f, partOfTimeLeft);

        spectralCopySpriteRenderer.color = new Color(1f, 1f, 1f, partOfTimeLeft * 0.5f);
    }
}
