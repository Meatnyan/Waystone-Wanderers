using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPedestal : MonoBehaviour
{
    public GameObject particleSystemObject;

    public float floatStepDistance;

    public float floatStepDuration;

    public float maxFloatSteps;

    [System.NonSerialized]
    public bool currentlyHoldingAnItem = false;

    PlayerController playerController;

    [HideInInspector]
    public StatModifier heldStatModifier;

    LootManager lootManager;

    public Coroutine itemFloaterCoroutine = null;

    [HideInInspector]
    public bool spawnedNaturally = true;

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        lootManager = playerController.lootManager;

        if(spawnedNaturally)
            lootManager.GenerateItem(objItemWillAttachTo: gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentlyHoldingAnItem && collision.gameObject.CompareTag("Player") && playerController.allowControl)
            playerController.PickUpItem(gameObject);
    }

    public void StartItemFloater()
    {
        if (itemFloaterCoroutine == null)
            itemFloaterCoroutine = StartCoroutine(ItemFloater());
    }

    IEnumerator ItemFloater()
    {
        while(true)
        {
            for (int i = 0; i < maxFloatSteps; i++)
            {
                heldStatModifier.transform.localPosition = new Vector2(heldStatModifier.transform.localPosition.x, heldStatModifier.transform.localPosition.y + floatStepDistance);
                yield return new WaitForSeconds(floatStepDuration);
            }

            for(int i = 0; i < maxFloatSteps; i++)
            {
                heldStatModifier.transform.localPosition = new Vector2(heldStatModifier.transform.localPosition.x, heldStatModifier.transform.localPosition.y - floatStepDistance);
                yield return new WaitForSeconds(floatStepDuration);
            }
        }
    }

    public void StopItemFloater()
    {
        if (itemFloaterCoroutine != null)
        {
            StopCoroutine(itemFloaterCoroutine);
            itemFloaterCoroutine = null;
        }
    }
}
