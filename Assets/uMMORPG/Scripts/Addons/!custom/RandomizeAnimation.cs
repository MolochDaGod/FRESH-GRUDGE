
using UnityEngine;

public class RandomizeAnimation : StateMachineBehaviour {

    [Tooltip("Set this string to the Paramater in Animator we want to change value for")]
    public string parameterName;
    [Tooltip("Set these integers to the value in Animator we want to randomize between. Remember to add +1 to the highest.")]
    public int range1;
    public int range2;
	 
	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.SetInteger(parameterName,(Random.Range(range1,range2)));

    }
}
