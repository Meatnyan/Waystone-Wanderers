using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmoredDillo : MonoBehaviour {

    public float angleToApproachPlayer;

    public float idleArmor;

    public float rollingArmor;

    public float shockedArmor;

    public float shockDurBeforeJump;

    public float jumpStepHeight;

    [Tooltip("Amount of frames to spend jumping up, equal to amount of frames to spend falling down after jumping")]
    public int jumpStepsOneWay;

    public float shockDurAfterJump;

    public float defaultRollTime;

    public float seenRollTime;

    bool allowShock = true;

    EnemyController enemyController;

    float randomMove;

    Coroutine shockerCoroutine;

    Coroutine rollerCoroutine;

    Coroutine moverCoroutine;

    Sprite idleSprite;

    public Sprite moveSprite;

    Collider2D[] colliders;

    Collider2D standingCollider;

    Collider2D rollingCollider;

    int jumpStepsBothWays;

	void Start () {
        enemyController = GetComponent<EnemyController>();
        colliders = GetComponents<Collider2D>();
        idleSprite = enemyController.spriteRenderer.sprite;
        standingCollider = colliders[0];
        rollingCollider = colliders[1];

        jumpStepsBothWays = jumpStepsOneWay * 2;

        moverCoroutine = StartCoroutine(Mover());
	}
	
    IEnumerator Mover()
    {
        while(true)
        {
            GenericExtensions.StopCoroutineAndMakeItNullIfItExists(this, ref rollerCoroutine);

            enemyController.rb.velocity = Vector2.zero;
            transform.rotation = Quaternion.identity;
            enemyController.FixShadowPosition();

            enemyController.animator.enabled = true;
            enemyController.spriteRenderer.sprite = idleSprite;

            standingCollider.enabled = true;
            rollingCollider.enabled = false;

            enemyController.armor = idleArmor;

            yield return new WaitForSeconds(enemyController.idleTime * Random.Range(0.7f, 1.3f));
            randomMove = Random.Range(0f, 360f);

            if (randomMove < 180f)
            {
                transform.localScale = new Vector2(-Mathf.Abs(transform.localScale.x), transform.localScale.y);
                enemyController.flipped = true;
            }
            else
            {
                transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
                enemyController.flipped = false;
            }

            enemyController.rotator.transform.rotation = Quaternion.Euler(0f, 0f, randomMove);
            enemyController.rb.velocity = enemyController.rotator.transform.up * enemyController.moveSpeed;

            if(rollerCoroutine == null)
                rollerCoroutine = StartCoroutine(Roller(defaultRollTime));
            yield return new WaitForSeconds(enemyController.moveTime * Random.Range(0.7f, 1.3f));
        }
    }

    IEnumerator Roller(float rollTime)
    {
        enemyController.animator.enabled = false;
        enemyController.spriteRenderer.sprite = moveSprite;

        standingCollider.enabled = false;
        rollingCollider.enabled = true;

        enemyController.armor = rollingArmor;

        while (true)
        {
            for (int i = 0; i < 4; ++i)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 90f * i);
                enemyController.FixShadowPosition();
                yield return new WaitForSeconds(rollTime / enemyController.attackSpeedMultiplier);
            }
        }
    }

    IEnumerator Shocker()
    {
        enemyController.rb.velocity = Vector2.zero;
        transform.rotation = Quaternion.identity;
        enemyController.FixShadowPosition();

        enemyController.armor = shockedArmor;

        enemyController.animator.enabled = false;
        enemyController.spriteRenderer.sprite = idleSprite;

        standingCollider.enabled = true;
        rollingCollider.enabled = false;

        yield return new WaitForSeconds(shockDurBeforeJump / enemyController.attackSpeedMultiplier);

        for(int i = 0; i < jumpStepsBothWays; ++i)
        {
            if(i < jumpStepsOneWay)   // first half of steps - move up
                enemyController.transform.position = new Vector2(enemyController.transform.position.x, enemyController.transform.position.y + jumpStepHeight);
            else   // second half of steps - move down
                enemyController.transform.position = new Vector2(enemyController.transform.position.x, enemyController.transform.position.y - jumpStepHeight);
            yield return null;
        }

        yield return new WaitForSeconds(shockDurAfterJump / enemyController.attackSpeedMultiplier);

        if(rollerCoroutine == null)
            rollerCoroutine = StartCoroutine(Roller(seenRollTime));

        shockerCoroutine = null;
    }

	void Update () {
        if (!enemyController.IsAggrod)
        {
            allowShock = true;

            if (moverCoroutine == null)
            {
                GenericExtensions.StopCoroutineAndMakeItNullIfItExists(this, ref rollerCoroutine);

                moverCoroutine = StartCoroutine(Mover());
            }
        }

		if(enemyController.IsAggrod && allowShock)  // jump in shock
        {
            allowShock = false;

            GenericExtensions.StopCoroutineAndMakeItNullIfItExists(this, ref moverCoroutine);
            GenericExtensions.StopCoroutineAndMakeItNullIfItExists(this, ref rollerCoroutine);

            shockerCoroutine = StartCoroutine(Shocker());
        }

        if(enemyController.IsAggrod && shockerCoroutine == null)
        {
            enemyController.rotator.transform.rotation = Quaternion.Euler(0f, 0f,
                GenericExtensions.GetRotationToFaceTargetPosition(transform.position, enemyController.playerObject.transform.position) - angleToApproachPlayer);

            enemyController.rb.velocity = enemyController.rotator.transform.right * enemyController.moveSpeed * enemyController.seenMoveSpeedMultiplier;
        }
	}
}
