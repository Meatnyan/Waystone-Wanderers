using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnstableConcoction : MonoBehaviour {

    GameObject playerObject;

    PlayerController playerController;

    public float minMultiplier;

    public float maxMultiplier;

    [System.NonSerialized]
    public bool donePickingUp = false;

	void Start () {
        playerObject = GameObject.FindWithTag("Player");
        playerController = playerObject.GetComponent<PlayerController>();
    }
	
	void Update () {
        if (donePickingUp == false && transform.root == playerObject.transform)
        {
            playerController.unstableConcoction = GetComponent<UnstableConcoction>();
            donePickingUp = true;
        }
    }
}
