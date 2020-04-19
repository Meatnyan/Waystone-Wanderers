using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PrimaryStat
{
    GlobalDamage,
    AttackSpeed,
    Range,
    ReloadSpeed,
    Accuracy,
    ShotSpeed,
    MoveSpeed
}

public enum StatChangeType
{
    DontDisplay,
    AddFlat,
    Multiply
}

public enum StatRetrieveType
{
    Total,
    Flat,
    Multiplier
}

public class PrimaryStatDisplayer : MonoBehaviour
{
    public Color positiveStatChangeColor;

    public Color negativeStatChangeColor;

    [Space(10f)]
    public float displayTime;

    [System.Serializable]
    public struct StatIconAndText
    {
        public RectTransform statIconRectTransform;
        public Text postModificationValueText;
        public Text modificationAmountText;
    }

    [Space(20f)]
    public StatIconAndText[] statIconAndTexts;

    float yPosOffsetPerStat;

    int primaryStatsOnScreen = 0;

    Coroutine[] displayPrimaryStatChangeCoroutines = new Coroutine[System.Enum.GetNames(typeof(PrimaryStat)).Length];

    int latestFrameCount = -1;

    WorldManager worldManager;

    //private void OnValidate() // could be useful again if the lineup of primary stats is changed
    //{
    //    for (int i = 0; i < statIconAndTexts.Length; i++)
    //    {
    //        RectTransform iconRectTransform;
    //        iconRectTransform = transform.Find(System.Enum.GetName(typeof(PrimaryStat), i) + "Icon").GetComponent<RectTransform>();
    //        statIconAndTexts[i].statIconRectTransform = iconRectTransform;

    //        statIconAndTexts[i].postModificationValueText = iconRectTransform.Find("PostModificationValueText").GetComponent<Text>();
    //        statIconAndTexts[i].modificationAmountText = iconRectTransform.Find("ModificationAmountText").GetComponent<Text>();
    //    }
    //}

    private void Awake()
    {
        worldManager = FindObjectOfType<WorldManager>();

        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(false);

        yPosOffsetPerStat = transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition.y;
    }

    public void StartDisplayPrimaryStatChange(PrimaryStat stat, float valueBefore, float valueAfter, bool isFlat)
    {
        if (latestFrameCount == -1)
            latestFrameCount = Time.frameCount;

        if (Time.frameCount != latestFrameCount) // if done on a different frame, clear all current stat displays
        {
            latestFrameCount = Time.frameCount;

            for (int i = 0; i < primaryStatsOnScreen; i++)
                GenericExtensions.StopCoroutineAndMakeItNullIfItExists(this, ref displayPrimaryStatChangeCoroutines[i]);

            for(int i = 0; i < statIconAndTexts.Length; i++)
                statIconAndTexts[i].statIconRectTransform.gameObject.SetActive(false);

            primaryStatsOnScreen = 0;
        }

        displayPrimaryStatChangeCoroutines[primaryStatsOnScreen] = StartCoroutine(DisplayPrimaryStatChange(stat, valueBefore, valueAfter, isFlat));
    }

    IEnumerator DisplayPrimaryStatChange(PrimaryStat stat, float valueBefore, float valueAfter, bool isFlat)
    {
        valueBefore *= 10f;
        valueAfter *= 10f;

        statIconAndTexts[(int)stat].statIconRectTransform.gameObject.SetActive(true);

        statIconAndTexts[(int)stat].statIconRectTransform.anchoredPosition = new Vector2(0f, yPosOffsetPerStat * primaryStatsOnScreen);
        primaryStatsOnScreen++;

        statIconAndTexts[(int)stat].postModificationValueText.text = GenericExtensions.ToStringWithTwoDecimals(GenericExtensions.RoundToTwoDecimals(valueAfter));

        if(isFlat)
        {
            float valueChange = GenericExtensions.RoundToTwoDecimals(valueAfter - valueBefore);
            char prefixChar = valueChange >= 0f ? '+' : '-';

            statIconAndTexts[(int)stat].modificationAmountText.text = prefixChar + GenericExtensions.ToStringWithTwoDecimals(valueChange);

            statIconAndTexts[(int)stat].modificationAmountText.color = valueChange >= 0f ? positiveStatChangeColor : negativeStatChangeColor;
            statIconAndTexts[(int)stat].postModificationValueText.color = valueChange >= 0f ? positiveStatChangeColor : negativeStatChangeColor;
        }
        else
        {
            float valueChange = GenericExtensions.RoundToTwoDecimals(valueAfter / valueBefore);

            statIconAndTexts[(int)stat].modificationAmountText.text = $"x{GenericExtensions.ToStringWithTwoDecimals(valueChange)}";

            statIconAndTexts[(int)stat].modificationAmountText.color = valueChange >= 1f? positiveStatChangeColor : negativeStatChangeColor;
            statIconAndTexts[(int)stat].postModificationValueText.color = valueChange >= 1f? positiveStatChangeColor : negativeStatChangeColor;
        }

        yield return new WaitForSeconds(displayTime);

        primaryStatsOnScreen--;

        statIconAndTexts[(int)stat].statIconRectTransform.gameObject.SetActive(false);
    }
}
