using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPackController : MonoBehaviour
{
    public bool unlocked = true;

    public float spawnWeight = 10f;

    public bool isFlying = false;

    [System.NonSerialized]
    public float minRangeToBePicked;

    [System.NonSerialized]
    public float maxRangeToBePicked;

    private void Awake()
    {
        transform.DetachChildren();

        Destroy(gameObject);
    }
}
