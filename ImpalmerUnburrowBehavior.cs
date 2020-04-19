using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpalmerUnburrowBehavior : StateMachineBehaviour {

	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        EnemyController enemyController = animator.GetComponent<EnemyController>();
        Impalmer impalmer = animator.GetComponent<Impalmer>();

        impalmer.unburrowing = false;

        if (enemyController.isIdle == false)
            impalmer.StartIdler();
	}

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        EnemyController enemyController = animator.GetComponent<EnemyController>();
        enemyController.rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
}
