using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChillController : MonoBehaviour
{
    [HideInInspector]
    public PlayerController playerController;

    [HideInInspector]
    public EnemyController enemyController;

    [HideInInspector]
    public float beginTime;

    float startingChillSlowdown;

    private void Awake()
    {
        beginTime = Time.time;
    }

    private void Start()
    {
        startingChillSlowdown = playerController.chillSlowdown;

        enemyController.moveSpeedMultiplier *= startingChillSlowdown;
        enemyController.shotSpeedMultiplier *= startingChillSlowdown;
        enemyController.attackSpeedMultiplier *= startingChillSlowdown;

        enemyController.spriteRenderer.color = new Color(enemyController.spriteRenderer.color.r * startingChillSlowdown, enemyController.spriteRenderer.color.g * startingChillSlowdown,
            enemyController.spriteRenderer.color.b);

        ParticleSystem particleSystem = GetComponent<ParticleSystem>();
        var particleSystemShape = particleSystem.shape;
        particleSystemShape.scale = new Vector2(enemyController.bloodExplosionXSize * 0.15f, enemyController.bloodExplosionYSize * 0.15f);
    }

    private void Update()
    {
        if (transform.parent == null)
        {
            Destroy(gameObject);
            return;
        }

        if (Time.time >= beginTime + playerController.chillDuration)
        {
            enemyController.chillStacks--;
            enemyController.chillControllers.Remove(this);

            enemyController.moveSpeedMultiplier /= startingChillSlowdown;
            enemyController.shotSpeedMultiplier /= startingChillSlowdown;
            enemyController.attackSpeedMultiplier /= startingChillSlowdown;

            enemyController.spriteRenderer.color = new Color(enemyController.spriteRenderer.color.r / startingChillSlowdown, enemyController.spriteRenderer.color.g / startingChillSlowdown,
                enemyController.spriteRenderer.color.b);

            Destroy(gameObject);
        }
    }
}
