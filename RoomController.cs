using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    public GameObject roomEntranceObject;

    public bool unlocked = true;

    public float spawnWeight = 10f;

    [HideInInspector]
    public float minRangeToBePicked;

    [HideInInspector]
    public float maxRangeToBePicked;

    [HideInInspector]
    public RoomEntranceController roomEntranceController;

    private void Awake()
    {
        roomEntranceController = roomEntranceObject.GetComponent<RoomEntranceController>();
    }
}
