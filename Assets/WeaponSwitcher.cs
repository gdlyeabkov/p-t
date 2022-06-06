using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitcher : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        bool isDig = stateInfo.IsName("Dig");
        GameObject pirate = animator.gameObject;
        Transform pirateTransform = pirate.transform;
        Transform armature = pirateTransform.GetChild(0);
        Transform shovelTransform = armature.GetChild(0);
        GameObject shovel = shovelTransform.gameObject;
        if (isDig)
        {
            Debug.Log("Это анимация лопаты");
        }
        else
        {
            Debug.Log("Это не анимация лопаты");
        }
        shovel.SetActive(isDig);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
