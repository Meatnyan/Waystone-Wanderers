using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpalmerBurrowBehavior : StateMachineBehaviour {

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        EnemyController enemyController = animator.GetComponent<EnemyController>();
        Impalmer impalmer = animator.GetComponent<Impalmer>();

        impalmer.burrowing = false;
        impalmer.burrowed = true;

        enemyController.rb.constraints = RigidbodyConstraints2D.FreezeAll;
        impalmer.StartAttacker();
	}

}
