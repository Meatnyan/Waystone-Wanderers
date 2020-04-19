using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CollectionCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [System.Serializable]
    struct Rarity
    {
        [SerializeField]
        string rarityName;
        [SerializeField]
        Color rarityColor;
    }

    [SerializeField]
    Rarity[] rarities;

    [Space(30f)]
    public GameObject tooltipObj;

    public RectTransform tooltipRectTransform;

    [Space(20f)]
    public Text nameText;

    [Space(10f)]
    public Text flavorText;

    [Space(10f)]
    public Text rarityText;

    [Space(10f)]
    public Text unlockDescriptionText;

    public void UpdateTooltip(GameObject collectibleObj)
    {
        Shooter shooter = collectibleObj.GetComponent<Shooter>();


    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }
}
