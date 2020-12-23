using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AttackAnimEndEventTrigger
: StateMachineBehaviour
{
	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{

        if (animator.GetComponent<Character>() != null)
        {
            var evtHandler = animator.GetComponent<Character>().OnAttackAnimEnd;
            evtHandler.Invoke();
        }
    }
}
