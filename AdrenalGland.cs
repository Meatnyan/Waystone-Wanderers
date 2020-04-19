using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdrenalGland : MonoBehaviour {

    [HideInInspector]
    public GameObject adrenalGlandCountdownObject;

    public float baseCountdownDuration;

    [HideInInspector]
    public float countdownDuration;

    [HideInInspector]
    public float countdownStartTime = Mathf.NegativeInfinity;

    [HideInInspector]
    public float remainingTime = Mathf.NegativeInfinity;

    GameObject playerObject;

    PlayerController playerController;

    [System.NonSerialized]
    public bool donePickingUp = false;

    [System.NonSerialized]
    public bool countingDown = false;

    Text countdownText;

    float highlighterTransitionTime = 0.05f;

    Coroutine countdownHighlighterCoroutine = null;

    private void Awake()
    {
        playerObject = GameObject.FindWithTag("Player");
        playerController = playerObject.GetComponent<PlayerController>();
        adrenalGlandCountdownObject = FindObjectOfType<UICanvasController>().adrenalGlandCountdownObj;
        countdownText = adrenalGlandCountdownObject.GetComponent<Text>();
        countdownDuration = baseCountdownDuration;
    }

    void Update()
    {
        if (donePickingUp == false && transform.root == playerObject.transform)
        {
            playerController.adrenalGland = this;
            donePickingUp = true;
        }
        CountdownUpdate();
    }


    public void CountdownUpdate()
    {
        if (countingDown)
        {
            adrenalGlandCountdownObject.SetActive(true);
            if (countdownStartTime < 0f)
                countdownStartTime = Time.time;
            remainingTime = countdownStartTime - Time.time + countdownDuration;
            if (remainingTime <= 0f)
            {
                playerController.preventDeathLayers--;
                countingDown = false;
                countdownText.text = "0";
            }
            else
                countdownText.text = "" + Mathf.FloorToInt(remainingTime);
        }
    }

    public void StartCountdownHighlighter()
    {
        if(countdownHighlighterCoroutine == null)
            countdownHighlighterCoroutine = StartCoroutine(CountdownHighlighter());
    }

    public void StopCountdownHighlighter()
    {
        if(countdownHighlighterCoroutine != null)
        {
            StopCoroutine(countdownHighlighterCoroutine);
            countdownText.color = new Color(1f, 0f, 0f);
            countdownHighlighterCoroutine = null;
        }
    }

    IEnumerator CountdownHighlighter()
    {
        countdownText.color = new Color(1f, 1f, 1f);
        yield return new WaitForSeconds(highlighterTransitionTime);
        countdownText.color = new Color(1f, 0.5f, 0.5f);
        yield return new WaitForSeconds(highlighterTransitionTime);
        countdownText.color = new Color(1f, 0f, 0f);
        countdownHighlighterCoroutine = null;
    }
}
