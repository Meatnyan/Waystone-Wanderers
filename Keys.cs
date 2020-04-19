using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Keys : MonoBehaviour {

    Text keys;

    GameObject playerObject;

    PlayerController playerController;

    private void Start()
    {
        keys = GetComponent<Text>();
        playerObject = GameObject.FindWithTag("Player");
        playerController = playerObject.GetComponent<PlayerController>();
    }

    private void Update()
    {
        keys.text = "x" + playerController.heldKeys;
    }
}
