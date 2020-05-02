using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GenericExtensions
{
    public static GameObject GetClosestGameObject(Vector2 fromPosition, List<GameObject> objectsToCheck)
    {
        float shortestSqrMagToTarget = Mathf.Infinity;
        int closestObjID = -1;
        for (int objID = 0; objID < objectsToCheck.Count; ++objID)
        {
            float curSqrMag = (fromPosition - (Vector2)objectsToCheck[objID].transform.position).sqrMagnitude;

            if (curSqrMag < shortestSqrMagToTarget)
            {
                shortestSqrMagToTarget = curSqrMag;
                closestObjID = objID;
            }
        }
        return closestObjID == -1 ? null : objectsToCheck[closestObjID];
    }

    public static float GetClosestSqrMagnitude(Vector2 fromPosition, List<GameObject> objectsToCheck)
    {
        float shortestSqrMagToTarget = Mathf.Infinity;
        for (int objID = 0; objID < objectsToCheck.Count; ++objID)
        {
            float curSqrMag = (fromPosition - (Vector2)objectsToCheck[objID].transform.position).sqrMagnitude;

            if (curSqrMag < shortestSqrMagToTarget)
                shortestSqrMagToTarget = curSqrMag;
        }
        return shortestSqrMagToTarget;
    }

    public static bool SqrMagIsInDistance(float sqrMag, float distance)
    {
        return sqrMag <= distance * distance ? true : false;
    }

    public static void LogWarningForUnrecognizedValue(object unrecognizedValue)
    {
        Debug.LogWarning($"Unrecognized { unrecognizedValue.GetType() } \"{ unrecognizedValue }\"");
    }

    public static float GetRotationToFaceTargetPosition(Vector2 originPosition, Vector2 targetPosition)
    {
        return Vector2.SignedAngle(Vector2.right, targetPosition - originPosition);
    }

    public static Vector2 GetMousePositionInWorld()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public static void StopCoroutineAndMakeItNullIfItExists(MonoBehaviour monoBehaviour, ref Coroutine coroutine)
    {
        if (coroutine != null)
        {
            monoBehaviour.StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    public static float RollRandom0To100()
    {
        return Random.Range(ExtConst.justAbove0, ExtConst.justUnder100);
    }

    public static bool DetermineIfPercentChancePasses(float chance)
    {
        return RollRandom0To100() <= chance ? true : false;
    }

    public static float RoundToTwoDecimals(float number)
    {
        return Mathf.Round(number * 100f) / 100f;
    }

    public static string ToStringWithTwoDecimals(float number)
    {
        string convertedNumber = number.ToString();

        bool foundDot = false;
        int dotIndex = 0;
        int amountOfCharsAfterDot = 0;
        for (int i = 0; i < convertedNumber.Length; i++)
        {
            if (foundDot)
                amountOfCharsAfterDot++;
            else
            {
                if (convertedNumber[i] == '.')
                    foundDot = true;
                else
                    dotIndex++;
            }
        }
        if (!foundDot)
            return convertedNumber + ".00";
        if (amountOfCharsAfterDot == 1)
            return convertedNumber + "0";
        if (amountOfCharsAfterDot == 2)
            return convertedNumber;

        return convertedNumber.Remove(startIndex: dotIndex + 2);
    }

    public static Sprite GetWeaponSprite(GameObject weaponObj)
    {
        Shooter shooter = weaponObj.GetComponent<Shooter>();
        if (shooter)
            return shooter.GetComponent<SpriteRenderer>().sprite;

        MeleeAttacker meleeAttacker = weaponObj.GetComponentInChildren<MeleeAttacker>();
        if (meleeAttacker)
            return meleeAttacker.GetComponent<SpriteRenderer>().sprite;

        return null;
    }
}
