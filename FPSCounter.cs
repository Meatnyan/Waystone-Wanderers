using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour {

    Text text;

	void Start () {
        text = GetComponent<Text>();
	}
	
	void Update () {
        text.text = "" + Mathf.Round(1f / Time.unscaledDeltaTime);
	}
}
