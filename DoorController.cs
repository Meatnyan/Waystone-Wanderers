using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Sprite[] openingSprites;

    public float timeBetweenLowering;

    public Sprite[] flashingSprites;

    public float singleFlashFrameTime;

    public float timeBetweenFullFlashes;

    AudioSource openingSound;

    SpriteRenderer spriteRenderer;

    BoxCollider2D boxCollider2D;

    PlayerController playerController;

    [HideInInspector]
    public bool opening = false;

    [HideInInspector]
    public bool isOpen = false;

    Coroutine flasherCoroutine = null;

    Sprite lockedIdleSprite;

    private void Awake()
    {
        openingSound = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        lockedIdleSprite = spriteRenderer.sprite;
        playerController = FindObjectOfType<PlayerController>();

        if (flashingSprites.Length > 0)
            flasherCoroutine = StartCoroutine(Flasher());
    }

    IEnumerator Flasher()
    {
        while (true)
        {
            for (int i = 0; i < flashingSprites.Length; ++i)
            {
                spriteRenderer.sprite = flashingSprites[i];
                yield return new WaitForSeconds(singleFlashFrameTime);
            }

            spriteRenderer.sprite = lockedIdleSprite;

            yield return new WaitForSeconds(timeBetweenFullFlashes);
        }
    }

    public void StartOpener()
    {
        StartCoroutine(Opener());
    }

    IEnumerator Opener()
    {
        opening = true;

        GenericExtensions.StopCoroutineAndMakeItNullIfItExists(this, ref flasherCoroutine);

        openingSound.Play();

        for(int i = 0; i < openingSprites.Length; i++)
        {
            yield return new WaitForSeconds(timeBetweenLowering);
            spriteRenderer.sprite = openingSprites[i];
        }

        boxCollider2D.enabled = false;

        opening = false;

        isOpen = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && playerController.heldKeys > 0 && !opening)
        {
            playerController.heldKeys--;
            StartOpener();
        }
    }
}
