using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Impalmer : MonoBehaviour {

    public float attackAngleChangeTime;

    public float timeBetweenSpikes;

    public float spikeDistanceIncrement;

    public float initialSpikeDistance;

    public GameObject spikeObject;

    EnemyController enemyController;

    [System.NonSerialized]
    public int lickingStateHash = Animator.StringToHash("Impalmer_IdleLickingAnim1");

    [System.NonSerialized]
    public int burrowStateHash = Animator.StringToHash("Impalmer_BurrowAnim1");

    [System.NonSerialized]
    public int unburrowStateHash = Animator.StringToHash("Impalmer_UnburrowAnim1");

    float randomMove;

    [System.NonSerialized]
    public bool burrowing = false;

    [System.NonSerialized]
    public bool burrowed = false;

    [System.NonSerialized]
    public bool unburrowing = false;

    float attackStartTime;

    int numberOfSpikesSpawnedThisCycle = 0;

    void Awake () {
        enemyController = GetComponent<EnemyController>();
	}

    private void Start()
    {
        StartIdler();
    }

    public void StartIdler()
    {
        StartCoroutine(Idler());
    }

    public void StartAttacker()
    {
        StartCoroutine(Attacker());
    }

    IEnumerator Idler()
    {
        while (true)
        {
            enemyController.isIdle = true;
            enemyController.rb.velocity = Vector2.zero;
            enemyController.animator.SetBool(enemyController.movingHash, false);
            yield return new WaitForSeconds(enemyController.idleTime * Random.Range(0.7f, 1.3f));
            if (!enemyController.isIdle)
                break;
            if (Random.Range(0, 4) == 0)
            {
                enemyController.animator.Play(lickingStateHash);
                continue;
            }
            enemyController.isIdle = false;
            StartCoroutine(Mover());
            break;
        }
    }

    IEnumerator Mover()
    {
        randomMove = Random.Range(0f, 360f);
        enemyController.rotator.transform.rotation = Quaternion.Euler(0f, 0f, randomMove);
        enemyController.rb.velocity = enemyController.rotator.transform.up * enemyController.moveSpeed;
        enemyController.animator.SetBool(enemyController.movingHash, true);
        yield return new WaitForSeconds(enemyController.moveTime * Random.Range(0.7f, 1.3f));
        if (!burrowing && !burrowed && !unburrowing && !enemyController.isIdle)
            StartCoroutine(Idler());
    }

    IEnumerator Attacker()
    {
        while (burrowed)
        {
            attackStartTime = Time.time;

            Vector2 diff = enemyController.playerObject.transform.position - transform.position;
            diff.Normalize();
            float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            enemyController.rotator.transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90f);

            numberOfSpikesSpawnedThisCycle = 0;
            while (Time.time < attackStartTime + attackAngleChangeTime && burrowed)
            {
                Instantiate(spikeObject, transform.position + enemyController.rotator.transform.up * spikeDistanceIncrement * (numberOfSpikesSpawnedThisCycle + initialSpikeDistance), Quaternion.identity, transform).GetComponent<EnemySpikeController>();
                numberOfSpikesSpawnedThisCycle++;
                yield return new WaitForSeconds(timeBetweenSpikes / enemyController.attackSpeedMultiplier);
            }
        }
    }

    private void Update()
    {
        if (enemyController.noticesPlayer && !burrowing && !burrowed && !unburrowing)
        {
            enemyController.isIdle = false;
            enemyController.rb.velocity = Vector2.zero;
            enemyController.animator.SetBool(enemyController.movingHash, false);
            burrowing = true;
            enemyController.animator.Play(burrowStateHash);
        }

        if (burrowed && enemyController.hasNoticedPlayer && enemyController.noticesPlayer == false && enemyController.stillCares == false)
        {
            burrowed = false;
            unburrowing = true;
            enemyController.animator.Play(unburrowStateHash);
        }
    }
}
