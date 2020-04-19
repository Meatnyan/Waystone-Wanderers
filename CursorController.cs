using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour {

    public Texture2D cursorRegularTexture;

    public Texture2D cursorClickTexture;

    Sprite cursorRegularSprite;

    Sprite cursorClickSprite;

    public LayerMask arrowKeysHitLayers;

    Vector2 cursorHotspot;

    [System.NonSerialized]
    public bool arrowKeyAiming = false;

    [HideInInspector]
    public RaycastHit2D rayHit;

    [HideInInspector]
    public PlayerController playerController;

    SpriteRenderer spriteRenderer;

    Vector2 mousePos;

    Vector2 latestMousePos;

    bool fetchLockedMousePosition;

    float targetingHorizontal;

    float targetingVertical;

    [System.NonSerialized]
    public Vector2 targetingMove;

    [HideInInspector]
    public bool allowArrowKeyAiming = false;

    RestartManager restartManager;

	void Awake () {
        restartManager = FindObjectOfType<RestartManager>();
        restartManager.DontDestroyOnLoadButDestroyWhenRestarting(gameObject);
        spriteRenderer = GetComponent<SpriteRenderer>();
        latestMousePos = Input.mousePosition;
        cursorRegularSprite = Sprite.Create(cursorRegularTexture, new Rect(0, 0, cursorRegularTexture.width, cursorRegularTexture.height), new Vector2(0.5f, 0.5f));
        cursorClickSprite = Sprite.Create(cursorClickTexture, new Rect(0, 0, cursorClickTexture.width, cursorClickTexture.height), new Vector2(0.5f, 0.5f));
        cursorHotspot = new Vector2(cursorRegularTexture.width / 2, cursorRegularTexture.height / 2);
        Cursor.SetCursor(cursorRegularTexture, cursorHotspot, CursorMode.Auto);
	}
	
	void Update () {
        if (Input.GetKeyDown(KeyCode.Mouse0))
            Cursor.SetCursor(cursorClickTexture, cursorHotspot, CursorMode.Auto);
        else if (Input.GetKeyUp(KeyCode.Mouse0))
            Cursor.SetCursor(cursorRegularTexture, cursorHotspot, CursorMode.Auto);


        if (fetchLockedMousePosition)
        {
            latestMousePos = Input.mousePosition;
            fetchLockedMousePosition = false;
        }

        if (allowArrowKeyAiming)
        {
            mousePos = Input.mousePosition;
            if (arrowKeyAiming && mousePos != latestMousePos)
            {
                arrowKeyAiming = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                spriteRenderer.sprite = null;
                latestMousePos = mousePos;
            }

            if (playerController.allowControl && !playerController.pauseMenu.paused)
            {
                targetingHorizontal = Input.GetAxisRaw("TargetingHorizontal");
                targetingVertical = Input.GetAxisRaw("TargetingVertical");
                targetingMove = new Vector2(targetingHorizontal, targetingVertical);

                if (targetingMove != Vector2.zero)
                {
                    arrowKeyAiming = true;
                    Cursor.lockState = CursorLockMode.Locked;
                    fetchLockedMousePosition = true;
                    Cursor.visible = false;
                    spriteRenderer.sprite = cursorClickSprite;
                    rayHit = Physics2D.Raycast(playerController.transform.position, targetingMove, Mathf.Infinity, arrowKeysHitLayers);
                    if (rayHit)
                        transform.position = rayHit.point;
                }

                if (arrowKeyAiming && targetingMove == Vector2.zero)
                {
                    spriteRenderer.sprite = cursorRegularSprite;
                }
            }
        }
    }
}
