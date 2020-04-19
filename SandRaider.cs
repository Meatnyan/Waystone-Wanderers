using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandRaider : MonoBehaviour {

    public float minIdleTimeMulti;

    public float maxIdleTimeMulti;

    [Space(20f)]
    public float minMoveTimeMulti;

    public float maxMoveTimeMulti;

    [Space(20f)]
    [Tooltip("Maximum angle difference (in degrees) between a direction that would go directly away from the player and the random direction picked. In effect only when aggrod.")]
    public float maxAngleDiffWhenAggrod;

    EnemyController enemyController;

    float moveRotation;

    private void Start()
    {
        enemyController = GetComponent<EnemyController>();
        StartCoroutine(Mover());
    }

    IEnumerator Mover()
    {
        while (true)
        {
            enemyController.rb.velocity = Vector2.zero;
            enemyController.animator.SetBool(enemyController.movingHash, false);

            yield return new WaitForSeconds(enemyController.idleTime * Random.Range(minIdleTimeMulti, maxIdleTimeMulti) * (enemyController.IsAggrod ? enemyController.seenIdleTimeMultiplier : 1f));

            if (enemyController.IsAggrod)   // if isAggrod, proper rotation towards player is handled automatically in EnemyController's Update()
            {
                // + 90f because transform.up starts at "90 degrees"
                float rotAgainstPlayer = GenericExtensions.GetRotationToFaceTargetPosition(transform.position, enemyController.playerController.transform.position) + 90f;
                moveRotation = Random.Range(rotAgainstPlayer - maxAngleDiffWhenAggrod, rotAgainstPlayer + maxAngleDiffWhenAggrod);
            }
            else
            {
                moveRotation = Random.Range(-180f, 180f);
                if (moveRotation > 0f)
                {
                    enemyController.flipped = true;
                    transform.localScale = new Vector2(-Mathf.Abs(transform.localScale.x), transform.localScale.y);
                }
                else
                {
                    enemyController.flipped = false;
                    transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
                }
            }

            enemyController.rotator.transform.rotation = Quaternion.Euler(0f, 0f, moveRotation);

            enemyController.rb.velocity = enemyController.rotator.transform.up * enemyController.moveSpeed * (enemyController.IsAggrod ? enemyController.seenMoveSpeedMultiplier : 1f);

            enemyController.animator.SetBool(enemyController.movingHash, true);

            yield return new WaitForSeconds(enemyController.moveTime * Random.Range(minMoveTimeMulti, maxMoveTimeMulti) * (enemyController.IsAggrod ? enemyController.seenMoveTimeMultiplier : 1f));
        }
    }
}
