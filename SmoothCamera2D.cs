using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothCamera2D : MonoBehaviour {

    public float dampTime = 0.15f;

    Vector3 velocity = Vector3.zero;

    Camera cam;

    [HideInInspector]
    public PlayerController playerController = null;

    [System.NonSerialized]
    public EdgeCollider2D edgeCollider2D;

    void Awake()
    {
        cam = GetComponent<Camera>();
        edgeCollider2D = GetComponent<EdgeCollider2D>();
        playerController = FindObjectOfType<PlayerController>();
    }

    private void FixedUpdate()
    {
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();

        if (playerController != null && playerController.worldManager.levelIsLoaded)
        {
            Vector3 delta = playerController.transform.position - cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 1f));
            Vector3 destination = transform.position + delta;
            transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);

        }
    }
}
