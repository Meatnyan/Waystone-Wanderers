using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    StatModifier,
    Shooter,
    MeleeAttacker,
    Pickup
}

public class GenericItem : MonoBehaviour
{
    public ItemType itemType;

    [Space(10f)]
    public bool isUnlocked = true;

    [Space(10f)]
    public int dropWeight = 100;

    [Space(20f)]
    public string internalName;

    [Space(10f)]
    public string displayedName;

    [Space(10f)]
    public string flavorText;

    [Space(20f)]
    public SpriteRenderer baseSpriteRenderer;

    [Space(10f)]
    public Sprite baseSprite;

    [System.NonSerialized]
    public int spawnedID = -1;

    [System.NonSerialized]
    public int minRangeToBePicked = -1;

    [System.NonSerialized]
    public int maxRangeToBePicked = -1;
}
