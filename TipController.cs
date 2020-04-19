using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipController : MonoBehaviour
{
    public float timeBetweenTips;

    public List<string> tipsList;

    Text tipText;

    [HideInInspector]
    public float timeOfLatestTip = Mathf.NegativeInfinity;

    private void Awake()
    {
        tipText = GetComponent<Text>();
    }

    public void DisplayTip()
    {
        if (Time.fixedTime >= timeOfLatestTip + timeBetweenTips)    // doesn't really work due to everything being mainthread
        {
            tipText.text = tipsList[Random.Range(0, tipsList.Count - 1)];
            timeOfLatestTip = Time.fixedTime;
        }
    }
}
