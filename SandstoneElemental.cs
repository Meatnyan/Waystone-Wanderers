using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandstoneElemental : MonoBehaviour
{
    public bool enragesOnLosingChildren;

    public float angleToApproachPlayer;

    [Space(20f)]
    public float orbitingSpeed;

    public float seenOrbitingSpeedMultiplier;

    [Space(20f)]
    public float radius;

    public float distanceBetweenOrbiters;

    [Space(20f)]
    public float timeBetweenLaunches;

    public LayerMask launchRaycastHitMask;

    [Space(20f)]
    public float floatStepDistance;

    public int maxFloatSteps;

    [Space(20f)]
    public float orbitExpansionStep;

    [HideInInspector]
    public float timeCounter;

    float defaultOrbitingSpeed;

    EnemyController enemyController;

    PlayerController playerController;

    Animator animator;

    int idleStateHash;

    int enragedStateHash;

    int scaredStateHash;

    bool isEnraged = false;

    [HideInInspector]
    public List<LivingSandstone> livingSandstones;

    Coroutine livingSandstoneLauncherCoroutine;

    Coroutine orbiterRadiusExpanderCoroutine;

    Coroutine orbiterRadiusRetracterCoroutine;

    private void Start()
    {
        enemyController = GetComponent<EnemyController>();
        playerController = enemyController.playerController;
        animator = enemyController.animator;

        livingSandstones = new List<LivingSandstone>(GetComponentsInChildren<LivingSandstone>());

        for (int i = 0; i < livingSandstones.Count; ++i)
            livingSandstones[i].orbiting = true;

        defaultOrbitingSpeed = orbitingSpeed;

        if(enemyController.animator.runtimeAnimatorController.name == "SandstoneElemental")
        {
            idleStateHash = Animator.StringToHash("SandstoneElemental_IdleAnim1");
            enragedStateHash = Animator.StringToHash("SandstoneElemental_EnragedAnim1");
            scaredStateHash = Animator.StringToHash("SandstoneElemental_ScaredAnim1");
        }
        else
        {
            idleStateHash = Animator.StringToHash("SandstoneElementalSpiked_Idle_1");
            enragedStateHash = Animator.StringToHash("SandstoneElementalSpiked_Enraged_1");
        }

        StartCoroutine(UpAndDownMover());
    }

    IEnumerator UpAndDownMover()
    {
        while(true)
        {
            for (int i = 0; i < maxFloatSteps; i++)
            {
                transform.position = new Vector2(transform.position.x, transform.position.y + floatStepDistance);
                yield return null;
            }
            for(int i = 0; i < maxFloatSteps; i++)
            {
                transform.position = new Vector2(transform.position.x, transform.position.y - floatStepDistance);
                yield return null;
            }
        }
    }

    IEnumerator OrbiterRadiusExpander()
    {
        while(orbitingSpeed < defaultOrbitingSpeed * seenOrbitingSpeedMultiplier)
        {
            orbitingSpeed *= orbitExpansionStep;
            radius *= orbitExpansionStep;
            yield return null;
        }
        orbiterRadiusExpanderCoroutine = null;
    }

    IEnumerator OrbiterRadiusRetracter()
    {
        while(orbitingSpeed > defaultOrbitingSpeed)
        {
            orbitingSpeed /= orbitExpansionStep;
            radius /= orbitExpansionStep;
            yield return null;
        }
        orbiterRadiusRetracterCoroutine = null;
    }

    IEnumerator LivingSandstoneLauncher()
    {
        while(livingSandstones.Count > 0)
        {
            float shortestSqrDistToTarget = Mathf.Infinity;
            int closestObjID = -1;

            for (int objID = 0; objID < livingSandstones.Count; ++objID)    // get closest LivingSandstone
            {
                float curSqrDistance = ((Vector2)playerController.transform.position - (Vector2)livingSandstones[objID].transform.position).sqrMagnitude;

                if (curSqrDistance < shortestSqrDistToTarget)
                {
                    shortestSqrDistToTarget = curSqrDistance;
                    closestObjID = objID;
                }
            }

            livingSandstones[closestObjID].orbiting = false;
            livingSandstones[closestObjID].forceEnrage = true;

            if(Physics2D.Raycast(livingSandstones[closestObjID].transform.position, playerController.transform.position - livingSandstones[closestObjID].transform.position,
                Mathf.Infinity, launchRaycastHitMask).transform.CompareTag("Player"))   // if able to raycast between living sandstone and player, set launch time for launch velocity boost
                livingSandstones[closestObjID].launchTime = Time.time;

            livingSandstones.RemoveAt(closestObjID);

            yield return new WaitForSeconds(timeBetweenLaunches);
        }
    }

    private void Update()
    {
        if (!enemyController.IsAggrod)
        {
            if (!enemyController.isScared)
            {
                animator.Play(idleStateHash);
                isEnraged = false;

                if (orbitingSpeed > defaultOrbitingSpeed && orbiterRadiusRetracterCoroutine == null && orbiterRadiusExpanderCoroutine == null)
                    orbiterRadiusRetracterCoroutine = StartCoroutine(OrbiterRadiusRetracter());
            }

            GenericExtensions.StopCoroutineAndMakeItNullIfItExists(this, ref livingSandstoneLauncherCoroutine);
        }
        else if (!enemyController.isScared)
        {
            if (!isEnraged)
            {
                animator.Play(enragedStateHash);
                isEnraged = true;
            }

            if (orbitingSpeed < defaultOrbitingSpeed * seenOrbitingSpeedMultiplier && orbiterRadiusRetracterCoroutine == null && orbiterRadiusExpanderCoroutine == null)
                orbiterRadiusExpanderCoroutine = StartCoroutine(OrbiterRadiusExpander());

            if (livingSandstoneLauncherCoroutine == null)
                livingSandstoneLauncherCoroutine = StartCoroutine(LivingSandstoneLauncher());
        }

        if (livingSandstones.Count > 0)
        {
            timeCounter += Time.deltaTime * orbitingSpeed;

            for (int i = 0; i < livingSandstones.Count; ++i)
                livingSandstones[i].transform.localPosition = new Vector2(Mathf.Cos(timeCounter + i * distanceBetweenOrbiters) * radius,
                    Mathf.Sin(timeCounter + i * distanceBetweenOrbiters) * radius);
        }
        else if(enemyController.IsAggrod)
        {
            if (!enragesOnLosingChildren)   // moves away from player
            {
                if (!enemyController.isScared)
                {
                    animator.Play(scaredStateHash);
                    enemyController.isScared = true;
                    isEnraged = false;
                }

                enemyController.rotator.transform.rotation = Quaternion.Euler(0f, 0f, GenericExtensions.GetRotationToFaceTargetPosition(transform.position, playerController.transform.position));
                enemyController.rb.velocity = -enemyController.rotator.transform.right * enemyController.moveSpeed;
            }
            else    // moves towards player
            {
                enemyController.rotator.transform.rotation = Quaternion.Euler(0f, 0f, GenericExtensions.GetRotationToFaceTargetPosition(transform.position, playerController.transform.position)
                    + angleToApproachPlayer);
                enemyController.rb.velocity = enemyController.rotator.transform.right * enemyController.moveSpeed;
            }
        }
    }

    private void OnDestroy()
    {
        for(int i = 0; i < livingSandstones.Count; ++i)
        {
            livingSandstones[i].orbiting = false;
            livingSandstones[i].forceEnrage = true;
        }
    }
}
