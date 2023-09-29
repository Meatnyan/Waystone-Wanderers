using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningUp : MonoBehaviour
{
    public float maxSpinAttackSpeedMultiplier;

    public float spinSpeedUpMultiplier;

    public float spinSlowDownMultiplier;

    Shooter shooter;

    private void Awake()
    {
        shooter = GetComponent<Shooter>();

        shooter.onButtonHoldEffects += SpeedUpSpinning;

        shooter.onWeaponHeldEffects += SlowDownSpinning;
    }

    void SpeedUpSpinning()
    {
        shooter.charge += Time.deltaTime * spinSpeedUpMultiplier;

        if (shooter.charge > 1f)
            shooter.charge = 1f;

        UpdateFireDelayAndTimeOfNextAllowedShot();
    }

    void SlowDownSpinning()
    {
        shooter.charge -= Time.deltaTime * spinSlowDownMultiplier;

        if (shooter.charge < 0f)
            shooter.charge = 0f;

        UpdateFireDelayAndTimeOfNextAllowedShot();
    }

    void UpdateFireDelayAndTimeOfNextAllowedShot()
    {
        shooter.fireDelay /= 1f + shooter.charge * maxSpinAttackSpeedMultiplier;

        shooter.timeOfNextAllowedShot = shooter.timeOfLastShot + shooter.fireDelay;
    }
}
