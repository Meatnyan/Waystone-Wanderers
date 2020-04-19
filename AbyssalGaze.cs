using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbyssalGaze : MonoBehaviour {

    public float speedMultiplier;

    GameObject playerObject;

    PlayerController playerController;

    [System.NonSerialized]
    public bool donePickingUp = false;

    private void Awake()
    {
        playerObject = GameObject.FindWithTag("Player");
        playerController = playerObject.GetComponent<PlayerController>();
    }

    void Update ()
    {
		if(donePickingUp == false && transform.root == playerObject.transform)
        {
            playerController.abyssalGaze = this;
            donePickingUp = true;
        }
	}
}
