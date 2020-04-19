using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomEntranceController : MonoBehaviour
{
    public float fadeOutDuration;

    public float betweenTransitionsDuration;

    [Space(20f)]
    public float gracePeriod;

    [Space(20f)]
    public GameObject fakeWall;

    public int fakeWallSquareSize;

    List<GameObject> treasureRooms;

    GameObject fadeOutOverlayObject;

    Image fadeOutOverlayImage;

    DoorController doorController;

    EdgeCollider2D edgeCollider2D;

    [System.NonSerialized]
    public bool entranceIsEnabled = false;

    Vector2 startingPos;

    [HideInInspector]
    public Vector2 destinationPos;

    float blockSize = 0.64f;

    [HideInInspector]
    public static Vector2 latestRoomPosition = new Vector2(100f, 100f);

    [HideInInspector]
    public RoomController destinationRoomController = null;

    GameObject playerObject;

    PlayerController playerController;

    [HideInInspector]
    public static Coroutine transitionToOtherRoomCoroutine = null;

    AudioSource transitionSound;

    [HideInInspector]
    public bool isInside = false;

    WorldManager worldManager;

    SmoothCamera2D smoothCamera2D;

    private void Awake()
    {
        worldManager = FindObjectOfType<WorldManager>();
        edgeCollider2D = GetComponent<EdgeCollider2D>();
        playerObject = GameObject.FindWithTag("Player");
        playerController = playerObject.GetComponent<PlayerController>();
        smoothCamera2D = FindObjectOfType<SmoothCamera2D>();
        fadeOutOverlayObject = FindObjectOfType<UICanvasController>().fadeOutOverlayObj;
        fadeOutOverlayImage = fadeOutOverlayObject.GetComponent<Image>();
        transitionSound = GetComponent<AudioSource>();
        treasureRooms = FindObjectOfType<RoomManager>().wastelandTreasureRooms;

        doorController = GetComponentInChildren<DoorController>();
        if (doorController == null)
        {
            entranceIsEnabled = true;

            edgeCollider2D.enabled = true;
        }
        else
            edgeCollider2D.enabled = false;

        if (Mathf.Approximately(transform.rotation.eulerAngles.z, 0f))
            startingPos = new Vector2(transform.position.x, transform.position.y - blockSize);

        if (Mathf.Approximately(transform.rotation.eulerAngles.z, 90f))
            startingPos = new Vector2(transform.position.x + blockSize, transform.position.y);

        if (Mathf.Approximately(transform.rotation.eulerAngles.z, 180f))
            startingPos = new Vector2(transform.position.x, transform.position.y + blockSize);

        if (Mathf.Approximately(transform.rotation.eulerAngles.z, 270f))
            startingPos = new Vector2(transform.position.x - blockSize, transform.position.y);


        if (transform.root.GetComponent<RoomController>() != null)
            isInside = true;
    }

    private void Update()
    {
        if(!entranceIsEnabled && doorController.isOpen)
        {
            worldManager.closedTreasureRooms.Remove(gameObject);

            entranceIsEnabled = true;

            edgeCollider2D.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (playerController.isInRoom)  // if player is currently in a room, go outside
            {
                if(transitionToOtherRoomCoroutine == null)
                    transitionToOtherRoomCoroutine = StartCoroutine(TransitionToOtherRoom());

                return;
            }


            if (destinationRoomController == null)  // if player is currently outside, look for a destination room
            {
                destinationRoomController = Instantiate(treasureRooms[worldManager.GenerateRandomRoomID(treasureRooms)], latestRoomPosition, Quaternion.identity, null).GetComponent<RoomController>();

                latestRoomPosition = new Vector2(latestRoomPosition.x + 100f, latestRoomPosition.y + 100f);


                destinationPos = destinationRoomController.roomEntranceController.startingPos;

                destinationRoomController.roomEntranceController.destinationPos = startingPos;


                Vector2 startingFakeWallPos = new Vector2(destinationPos.x - ((fakeWallSquareSize / 2) * blockSize), destinationPos.y - ((fakeWallSquareSize / 2) * blockSize));
                for(int curY = 0; curY < fakeWallSquareSize; curY++)
                    for(int curX = 0; curX < fakeWallSquareSize; curX++)
                        Instantiate(fakeWall, new Vector2(startingFakeWallPos.x + curX * blockSize, startingFakeWallPos.y + curY * blockSize), Quaternion.identity, null);
            }


            if (transitionToOtherRoomCoroutine == null)     // no matter what, try going to destinationPos
                transitionToOtherRoomCoroutine = StartCoroutine(TransitionToOtherRoom());
        }
    }

    IEnumerator TransitionToOtherRoom()
    {
        playerController.polygonCollider2D.enabled = false;
        playerController.allowControl = false;

        fadeOutOverlayObject.SetActive(true);
        fadeOutOverlayImage.color = new Color(1f, 1f, 1f, 0f);
        fadeOutOverlayImage.CrossFadeColor(new Color(1f, 1f, 1f, 1f), fadeOutDuration, true, true);

        yield return new WaitForSeconds(fadeOutDuration);

        fadeOutOverlayImage.color = new Color(1f, 1f, 1f, 1f);

        transitionSound.Play();

        yield return new WaitForSeconds(betweenTransitionsDuration / 2);

        playerObject.transform.position = destinationPos;

        smoothCamera2D.transform.position = destinationPos;

        yield return new WaitForSeconds(betweenTransitionsDuration / 2);

        fadeOutOverlayImage.CrossFadeColor(new Color(1f, 1f, 1f, 0f), fadeOutDuration, true, true);

        yield return new WaitForSeconds(fadeOutDuration);

        fadeOutOverlayObject.SetActive(false);

        if (isInside)    // if current RoomEntranceController is inside, that means that after fadeout, player will be outside
            playerController.isInRoom = false;
        else
            playerController.isInRoom = true;

        playerController.polygonCollider2D.enabled = true;
        playerController.allowControl = true;

        playerController.invincibilityLayers++;
        worldManager.StartRemoveInvincibilityLayer(gracePeriod);

        transitionToOtherRoomCoroutine = null;
    }
}
