using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GodRaySpawner : MonoBehaviour
{
    public GameObject godRayObj;

    public int maxAmountToSpawn;

    public float damageMultiplier;

    [HideInInspector]
    public int currentAmountSpawned = 0;

    public void SpawnGodRay(Vector2 posToSpawnAt)
    {
        GodRay godRay = Instantiate(godRayObj, new Vector2(posToSpawnAt.x, posToSpawnAt.y + godRayObj.GetComponent<SpriteRenderer>().sprite.bounds.extents.y), Quaternion.identity, null)
            .GetComponent<GodRay>();
        godRay.damageMultiplier = damageMultiplier;
        godRay.parentSpawner = this;

        currentAmountSpawned++;
    }
}
