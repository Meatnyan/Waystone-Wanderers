using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningUp : MonoBehaviour
{
    public float bonusFireRateMultiplier;

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

        UpdateFireRateAndTimeOfNextAllowedShot();
    }

    void SlowDownSpinning()
    {
        shooter.charge -= Time.deltaTime * spinSlowDownMultiplier;

        if (shooter.charge < 0f)
            shooter.charge = 0f;

        UpdateFireRateAndTimeOfNextAllowedShot();
    }

    void UpdateFireRateAndTimeOfNextAllowedShot()
    {
        shooter.fireRate /= 1f + shooter.charge * bonusFireRateMultiplier;

        shooter.timeOfNextAllowedShot = shooter.timeOfLastShot + shooter.fireRate;
    }
}
