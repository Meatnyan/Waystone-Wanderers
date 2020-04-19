using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionScreen : MonoBehaviour
{
    public GameObject grayedOutBgObj;

    public float crossfadeAlphaDur;

    [Space(20f)]
    public GameObject weaponsButtonObj;

    public GameObject itemsButtonObj;

    [Space(20f)]
    public GameObject weaponsScrollViewObj;

    public GameObject weaponsContentObj;

    [Space(10f)]
    public GameObject itemsScrollViewObj;

    public GameObject itemsContentObj;

    [Space(20f)]
    public GameObject cellObj;

    public Sprite lockedCellSprite;

    public Sprite unlockedCellSprite;

    [Space(20f)]
    public int cellsPerRow;

    [Space(10f)]
    public float cellXOffset;

    public float cellYOffset;

    AchievementManager achievementManager;

    LootManager lootManager;

    Image grayedOutBgImg;

    private void Awake()
    {
        achievementManager = FindObjectOfType<AchievementManager>();
        lootManager = FindObjectOfType<LootManager>();
        grayedOutBgImg = grayedOutBgObj.GetComponent<Image>();
    }
    
    public void DisplayCollectionCategoryChoices()
    {
        weaponsScrollViewObj.SetActive(false);

        grayedOutBgObj.SetActive(true);
        weaponsButtonObj.SetActive(true);
        itemsButtonObj.SetActive(true);

        grayedOutBgImg.CrossFadeAlpha(1f, crossfadeAlphaDur, true);
    }

    void StopDisplayingCollectionCategoryChoices()
    {
        grayedOutBgObj.SetActive(false);
        weaponsButtonObj.SetActive(false);
        itemsButtonObj.SetActive(false);

        grayedOutBgImg.CrossFadeAlpha(0f, crossfadeAlphaDur, true);
    }

    public void DisplayWeaponsCollection()
    {
        StopDisplayingCollectionCategoryChoices();

        weaponsScrollViewObj.SetActive(true);

        for(int i = 0; i < lootManager.currentWoodenChestWeaponPool.Count; i++)
        {
            GameObject curCellObj = Instantiate(cellObj, weaponsContentObj.transform);
            curCellObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(cellXOffset / 2 + cellXOffset * (i % cellsPerRow), cellYOffset / 2 + cellYOffset * (i / cellsPerRow));

            Image curImage = curCellObj.transform.GetChild(0).GetComponent<Image>();
            Shooter curShooter = lootManager.currentWoodenChestWeaponPool[i].GetComponent<Shooter>();
            MeleeAttacker curMeleeAttacker = lootManager.currentWoodenChestWeaponPool[i].GetComponentInChildren<MeleeAttacker>();

            if ((curShooter && curShooter.unlocked) || (curMeleeAttacker && curMeleeAttacker.unlocked))
            {
                curImage.sprite = curShooter ? curShooter.GetComponent<SpriteRenderer>().sprite : curMeleeAttacker.GetComponent<SpriteRenderer>().sprite;
                curImage.SetNativeSize();
            }
            else
            {
                curImage.enabled = false;
                curCellObj.GetComponent<Image>().sprite = lockedCellSprite;
            }
        }
    }

    public void DisplayItemsCollection()
    {
        StopDisplayingCollectionCategoryChoices();

        itemsScrollViewObj.SetActive(true);
    }
}
