using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillerCereal : MonoBehaviour {

    PlayerController playerController;

    public float dmgBonusPerRecentKill;

    public float maxDmgBonus;

    [System.NonSerialized]
    public bool donePickingUp = false;

	void Awake () {
        playerController = FindObjectOfType<PlayerController>();
	}
	
	void Update () {
		if(!donePickingUp && transform.root == playerController.transform)
        {
            playerController.killerCereal = this;
            donePickingUp = true;
        }

        if(donePickingUp && playerController.damagedTinterCoroutine == null)
            playerController.spriteRenderer.material.color = new Color(1f, 1f - (Mathf.Min(20f, playerController.recentKillCount) * 0.04f), 1f - (Mathf.Min(20f, playerController.recentKillCount) * 0.04f));
	}
}
