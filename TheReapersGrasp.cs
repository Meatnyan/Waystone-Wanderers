using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheReapersGrasp : MonoBehaviour {

    [Header("On-Kill Bonus")]
    public float regularEnemyDamageBonus;

    public float bossDamageBonus;

    [Header("Soul Steal Particles")]
#pragma warning disable 0649
    [SerializeField]
    GameObject soulStealParticlesObject;
#pragma warning restore 0649

    public int maxParticleSystems;

    public float particleHomingStrength;

    public float particleStartingSpeed;

    public float particleAcceleration;

    public float particleDuration;

    public float particleMinSqrDist;

    ParticleSystem[] soulStealParticleSystems;

    int curParticleSystemID = -1;

    Coroutine[] soulStealCoroutines;

    private void Awake()
    {
        soulStealParticleSystems = new ParticleSystem[maxParticleSystems];
        for (int i = 0; i < maxParticleSystems; ++i)
        {
            soulStealParticleSystems[i] = Instantiate(soulStealParticlesObject, transform.position, Quaternion.identity, null).GetComponent<ParticleSystem>();
            soulStealParticleSystems[i].gameObject.SetActive(false);
        }
        soulStealCoroutines = new Coroutine[maxParticleSystems];
    }

    public void StartConnectSoulStealParticlesWithPlayer(Vector2 fromPos)
    {
        if (curParticleSystemID == maxParticleSystems - 1)
            curParticleSystemID = 0;
        else
            curParticleSystemID++;

        GenericExtensions.StopCoroutineAndMakeItNullIfItExists(this, ref soulStealCoroutines[curParticleSystemID]);

        soulStealCoroutines[curParticleSystemID] = StartCoroutine(ConnectSoulStealParticlesWithPlayer(fromPos));
    }

    IEnumerator ConnectSoulStealParticlesWithPlayer(Vector2 fromPos)
    {
        ParticleSystem curSoulStealParticleSystem = soulStealParticleSystems[curParticleSystemID];
        Rigidbody2D curRb = curSoulStealParticleSystem.GetComponent<Rigidbody2D>();

        float particleStartTime = Time.time;
        curSoulStealParticleSystem.gameObject.SetActive(true);
        curSoulStealParticleSystem.transform.position = fromPos;
        curRb.velocity = particleStartingSpeed * curRb.transform.right;
        curSoulStealParticleSystem.Play();

        while (Time.time <= particleStartTime + particleDuration)
        {
            if (Vector2.SqrMagnitude(curSoulStealParticleSystem.transform.position - transform.root.position) < particleMinSqrDist)
                curSoulStealParticleSystem.Stop();
            else if(curSoulStealParticleSystem.isEmitting)
            {
                float angleDifference = GenericExtensions.GetRotationToFaceTargetPosition(curSoulStealParticleSystem.transform.position, transform.root.position)
                    - curSoulStealParticleSystem.transform.rotation.eulerAngles.z;

                if (angleDifference < -180)
                    angleDifference += 360;
                else if (angleDifference > 180)
                    angleDifference -= 360;

                curRb.angularVelocity = angleDifference * particleHomingStrength;

                curRb.velocity = curRb.velocity.magnitude * particleAcceleration * curRb.transform.right;
            }

            yield return null;
        }

        curSoulStealParticleSystem.gameObject.SetActive(false);
    }
}
