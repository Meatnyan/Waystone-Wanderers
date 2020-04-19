using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooter : MonoBehaviour {

    public GameObject shot;

    public Transform shotSpawn;

    public float damage;

    public int shotsPerWave = 1;

    public int shotsPerBurst = 1;

    public int minExtraBurstShots = 0;

    public int maxExtraBurstShots = 0;

    public float timeBetweenBurstShots = 0;

    public float fireRate;

    public float shotSpeed;

    public float shotSpread;

    public float shotMaxDuration;

    [System.NonSerialized]
    public float trueShotSpread;

    float timeOfNextAllowedShot = Mathf.NegativeInfinity;

    private AudioSource shotSound;

    GameObject enemyObject;

    GameObject playerObject;

    EnemyController enemyController;

    [System.NonSerialized]
    public bool isBurstShooting = false;

    float rot_z;

    private void Awake()
    {
        enemyObject = transform.root.gameObject;
        enemyController = enemyObject.GetComponent<EnemyController>();
        enemyController.enemyShooter = GetComponent<EnemyShooter>();
        playerObject = GameObject.FindWithTag("Player");
        shotSound = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (enemyController.IsAggrod)
        {
            Vector2 diff = playerObject.transform.position - transform.position;
            diff.Normalize();
            rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            if (enemyController.flipped)
                transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 180f);
            else
                transform.rotation = Quaternion.Euler(0f, 0f, rot_z);

            if (Time.time > timeOfNextAllowedShot && !isBurstShooting)
                StartCoroutine(ShotBurst());
        }
        else
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    IEnumerator ShotBurst()
    {
        isBurstShooting = true;
        for(int burstShotID = 0; burstShotID < shotsPerBurst + Random.Range(minExtraBurstShots, maxExtraBurstShots + 1); burstShotID++)
        {
            for (int shotInWaveID = 0; shotInWaveID < shotsPerWave; shotInWaveID++)
            {
                trueShotSpread = shotSpread / enemyController.accuracyMultiplier * Random.Range(-0.5f, 0.5f);
                if (enemyController.isScared)
                    trueShotSpread /= enemyController.scaredAccuracyMultiplier;

                Quaternion shotRotation = Quaternion.Euler(0f, 0f, rot_z - 180 + trueShotSpread);
                Instantiate(shot, shotSpawn.position, shotRotation, transform);
            }
            shotSound.Play();
            yield return new WaitForSeconds(timeBetweenBurstShots * Random.Range(0.7f, 1.3f));
        }
        timeOfNextAllowedShot = Time.time + (fireRate * Random.Range(0.9f, 1.1f) / enemyController.attackSpeedMultiplier);
        isBurstShooting = false;
    }
}
