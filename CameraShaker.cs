using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraShaker : MonoBehaviour {

    [System.NonSerialized]
    public float shake = 0f;

    public float shakeThreshold = 0.2f;

    [System.NonSerialized]
    public bool constantShaking = false;

    public static bool alreadyExists = false;

    private void Awake()
    {
        if(alreadyExists)
        {
            Destroy(gameObject);
            return;
        }

        alreadyExists = true;
        DontDestroyOnLoad(gameObject);  // CameraShaker and RestartManager are the only objects persistent throughout all scenes
    }

    void Update () {
        if (shake >= shakeThreshold)
        {
            if(constantShaking)
                transform.DOShakePosition(Mathf.Min(shake * 1f, 1.5f), shake * Random.Range(0.13f, 0.17f), 30, 90f);
            else
                transform.DOShakePosition(Mathf.Min(shake * 0.3f, 1.5f), shake * Random.Range(0.23f, 0.27f), 30, 90f);

            shake = 0f;
        }
    }
}
