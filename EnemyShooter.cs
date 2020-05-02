using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenericExtensions;

public class EnemyShooter : MonoBehaviour {

    public GameObject shot;

    public Transform shotSpawn;

    public float damage;

    public int shotsPerWave = 1;

    public int wavesPerBurst = 1;

    public int minExtraBurstWaves = 0;

    public int maxExtraBurstWaves = 0;

    public float timeBetweenBurstWaves = 0;

    public float fireRate;

    public float shotSpeed;

    public float shotSpread;

    public float shotMaxDuration;

    [System.NonSerialized]
    public float finalShotSpread;

    float timeOfNextAllowedShot = Mathf.NegativeInfinity;

    private AudioSource shotSound;

    GameObject enemyObject;

    GameObject playerObject;

    EnemyController enemyController;

    [System.NonSerialized]
    public bool isBurstShooting = false;

    float angleToPlayer = 0f;

    private void Awake()
    {
        enemyObject = transform.root.gameObject;
        enemyController = enemyObject.GetComponent<EnemyController>();
        playerObject = GameObject.FindWithTag("Player");
        shotSound = GetComponent<AudioSource>();

        enemyController.enemyShooter = this;
    }

    private void Update()
    {
        if (enemyController.IsAggrod)
        {
            angleToPlayer = GetRotationToFaceTargetPosition(transform.position, playerObject.transform.position);

            transform.rotation = Quaternion.Euler(0f, 0f, angleToPlayer - (enemyController.flipped ? 180f : 0f));

            if (Time.time > timeOfNextAllowedShot && !isBurstShooting)
                StartCoroutine(ShotBurst());
        }
        else
            transform.rotation = Quaternion.identity;
    }

    IEnumerator ShotBurst()
    {
        isBurstShooting = true;

        // bursts = multiple single shots or shot waves one after another
        // waves = multiple shots in various directions all at once

        int amountOfWavesInBurst = wavesPerBurst + Random.Range(minExtraBurstWaves, maxExtraBurstWaves + 1);

        for (int waveID = 0; waveID < amountOfWavesInBurst; waveID++)
        {
            for (int shotInWaveID = 0; shotInWaveID < shotsPerWave; shotInWaveID++)
            {
                finalShotSpread = shotSpread / enemyController.accuracyMultiplier * Random.Range(-0.5f, 0.5f);
                if (enemyController.isScared)
                    finalShotSpread /= enemyController.scaredAccuracyMultiplier;    // getting divided by a number in the range (0, 1) increases shot spread

                Quaternion shotRotation = Quaternion.Euler(0f, 0f, angleToPlayer - 180 + finalShotSpread);

                Instantiate(shot, shotSpawn.position, shotRotation, transform).
                    GetComponent<EnemyShotMover>().SetStats(
                    newDamage: damage,
                    newShotSpeed: shotSpeed,
                    newMaxDuration: shotMaxDuration);
            }
            shotSound.Play();

            yield return new WaitForSeconds(timeBetweenBurstWaves * Random.Range(0.7f, 1.3f));
        }

        timeOfNextAllowedShot = Time.time + (fireRate * Random.Range(0.9f, 1.1f) / enemyController.attackSpeedMultiplier);

        isBurstShooting = false;
    }
}
