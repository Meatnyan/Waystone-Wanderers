using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TunnellingSandcrawler : MonoBehaviour
{
    public GameObject dustParticlesObject;

    public float burrowedSpeedMultiplier;

    public float altIdleChance;

    public float angleToApproachPlayer;

    public float distFromPlayerToAmbush;

    public float distFromPlayerToChase;

    public float maxChaseDuration;

    public float distFromPlayerToAttack;

    public float timeBetweenAttacks;

    EnemyController enemyController;

    float randomMove;

    bool startedApplyingAggro = false;

    int idleAnimHash = Animator.StringToHash("TunnellingSandcrawler_Idle_1");

    int altIdleAnimHash = Animator.StringToHash("TunnellingSandcrawler_Idle_2");

    int unaggrodMoveAnimHash = Animator.StringToHash("TunnellingSandcrawler_Move_Unaggrod");

    int aggroAnimHash = Animator.StringToHash("TunnellingSandcrawler_Aggro");

    int aggrodMoveAnimHash = Animator.StringToHash("TunnellingSandcrawler_Move_Aggrod");

    int burrowAnimHash = Animator.StringToHash("TunnellingSandcrawler_Burrow");

    int unburrowAnimHash = Animator.StringToHash("TunnellingSandcrawler_Unburrow");

    int attackAnimHash = Animator.StringToHash("TunnellingSandcrawler_Attack");

    PolygonCollider2D[] colliders;

    int currentColliderID = 0;

    bool isAttacking = false;

    private void Start()
    {
        colliders = GetComponents<PolygonCollider2D>();
        enemyController = GetComponent<EnemyController>();
        StartCoroutine(UnaggrodMover());
    }

    private void Update()
    {
        if (enemyController.IsAggrod && !startedApplyingAggro)  // stop moving and start aggro
        {
            enemyController.rb.velocity = Vector2.zero;
            enemyController.isMoving = false;

            enemyController.animator.Play(aggroAnimHash);   // once aggroing is done, this anim calls AggrodMover()
            ChangeCollider(1);  // aggrod collider

            startedApplyingAggro = true;
        }

        if(!enemyController.IsAggrod && startedApplyingAggro)   // stop aggro
        {
            ChangeCollider(0);  // unaggrod collider
            enemyController.spriteRenderer.enabled = true;
            dustParticlesObject.SetActive(false);
            isAttacking = false;

            StartCoroutine(UnaggrodMover());

            startedApplyingAggro = false;
        }
    }

    public void DisableShadow()
    {
        enemyController.shadowObject.SetActive(false);
    }

    public void EnableShadow()
    {
        enemyController.shadowObject.SetActive(true);
    }

    public void ChangeIsAttackingToFalse()
    {
        isAttacking = false;
    }

    public void ChangeCollider(int newColliderID)   // 0 = unaggrod, 1 = aggrod, 2 = starting attack, 3 = fully extended attack, 4-9 = burrowing, 10-13 = unburrowing
    {
        colliders[currentColliderID].enabled = false;

        colliders[newColliderID].enabled = true;
        currentColliderID = newColliderID;
    }

    void StartBurrowing()
    {
        enemyController.rb.velocity = Vector2.zero;
        enemyController.isMoving = false;

        enemyController.animator.Play(burrowAnimHash);  // once burrowing is done, this anim calls BurrowedMover()
    }

    IEnumerator BurrowedMover()
    {
        if (!GenericExtensions.SqrMagIsInDistance(enemyController.sqrMagPlayerEnemy, distFromPlayerToAmbush))
        {
            colliders[currentColliderID].enabled = false;   // ChangeCollider() will disable it again once changing collider, but that's fine
            enemyController.spriteRenderer.enabled = false;
            enemyController.OverrideAllowNoticingPlayer(true);
            dustParticlesObject.SetActive(true);

            enemyController.isMoving = true;

            do
            {
                enemyController.rotator.transform.rotation = Quaternion.Euler(0f, 0f,
                    GenericExtensions.GetRotationToFaceTargetPosition(transform.position, enemyController.playerController.transform.position) - angleToApproachPlayer);
                enemyController.rb.velocity = enemyController.rotator.transform.up * enemyController.moveSpeed * burrowedSpeedMultiplier;

                yield return null;
            } while (!GenericExtensions.SqrMagIsInDistance(enemyController.sqrMagPlayerEnemy, distFromPlayerToAmbush)   // once close enough to player, unburrow
                    && enemyController.IsAggrod);
        }

        enemyController.spriteRenderer.enabled = true;
        enemyController.StopOverrideAllowNoticingPlayer();
        dustParticlesObject.SetActive(false);

        enemyController.rb.velocity = Vector2.zero;
        enemyController.isMoving = false;

        enemyController.animator.Play(unburrowAnimHash);    // once unburrowing is done, this anim calls AggrodMover()
    }

    IEnumerator AggrodMover()
    {
        if (enemyController.IsAggrod)
        {
            if (GenericExtensions.SqrMagIsInDistance(enemyController.sqrMagPlayerEnemy, distFromPlayerToChase))
            {
                enemyController.animator.Play(aggrodMoveAnimHash);  // play looping anim, this anim also resumes after attacking
                float chaseStartTime = Time.time;

                do
                {
                    enemyController.rotator.transform.rotation = Quaternion.Euler(0f, 0f,
                        GenericExtensions.GetRotationToFaceTargetPosition(transform.position, enemyController.playerController.transform.position) - angleToApproachPlayer);
                    enemyController.rb.velocity = enemyController.rotator.transform.up * enemyController.moveSpeed * enemyController.seenMoveSpeedMultiplier;

                    if (!isAttacking && GenericExtensions.SqrMagIsInDistance(enemyController.sqrMagPlayerEnemy, distFromPlayerToAttack)) // attack if close enough to player
                    {
                        enemyController.animator.Play(attackAnimHash);
                        isAttacking = true;
                    }

                    if (isAttacking)
                    {
                        yield return isAttacking == false;
                        yield return new WaitForSeconds(timeBetweenAttacks);
                    }
                    else
                        yield return null;
                } while (GenericExtensions.SqrMagIsInDistance(enemyController.sqrMagPlayerEnemy, distFromPlayerToChase)
                        && Time.time <= chaseStartTime + maxChaseDuration // once too far from player or chased too long, begin to burrow
                        && enemyController.IsAggrod);
            }
        }
        else
            yield break;

        isAttacking = false;
        StartBurrowing();
    }

    IEnumerator UnaggrodMover()
    {
        while (!enemyController.IsAggrod)
        {
            enemyController.rb.velocity = Vector2.zero;
            enemyController.isMoving = false;

            if (GenericExtensions.DetermineIfPercentChancePasses(altIdleChance))
                enemyController.animator.Play(altIdleAnimHash); // jaws anim
            else
                enemyController.animator.Play(idleAnimHash);    // regular idle anim

            yield return new WaitForSeconds(enemyController.idleTime * Random.Range(0.7f, 1.3f));

            if (!enemyController.IsAggrod)
            {
                randomMove = Random.Range(-180f, 180f);
                if (randomMove > 0f)
                {
                    enemyController.flipped = true;
                    transform.localScale = new Vector2(-Mathf.Abs(transform.localScale.x), transform.localScale.y);
                }
                else
                {
                    enemyController.flipped = false;
                    transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
                }

                enemyController.rotator.transform.rotation = Quaternion.Euler(0f, 0f, randomMove);

                enemyController.rb.velocity = enemyController.rotator.transform.up * enemyController.moveSpeed;

                enemyController.isMoving = true;

                enemyController.animator.Play(unaggrodMoveAnimHash);

                yield return new WaitForSeconds(enemyController.moveTime * Random.Range(0.7f, 1.3f));
            }
            else
                break;
        }
    }
}
