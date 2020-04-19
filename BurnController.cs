using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnController : MonoBehaviour {

    [HideInInspector]
    public PlayerController playerController;

    [HideInInspector]
    public EnemyController enemyController;

    [HideInInspector]
    public float beginTime;

    float latestTickTime;

    private void Awake()
    {
        beginTime = Time.time;
        latestTickTime = beginTime;
    }

    private void Start()
    {
        ParticleSystem particleSystem = GetComponent<ParticleSystem>();
        var particleSystemShape = particleSystem.shape;
        particleSystemShape.scale = new Vector2(enemyController.bloodExplosionXSize * 0.15f, enemyController.bloodExplosionYSize * 0.15f);
    }

    private void Update()
    {
        if (Time.time >= latestTickTime + playerController.burnTickTime)
        {
            latestTickTime = Time.time;
            if (enemyController != null)
            {
                enemyController.CurrentHealth -= playerController.burnTickDmg;
                if (enemyController.CurrentHealth <= 0f)
                    playerController.UpdateOnKillWITHStatusEffectEffects(StatusEffect.bleed, enemyController); // this only handles kills DIRECTLY with this status effect, kills DURING status effects are handled in EnemyController
            }
        }

        if (Time.time >= beginTime + playerController.burnDuration)
        {
            enemyController.burnStacks--;
            enemyController.burnControllers.Remove(this);
            Destroy(gameObject);
            return;
        }

        if (transform.parent == null)
            Destroy(gameObject);
    }
}
