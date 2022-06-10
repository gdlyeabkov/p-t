using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitcher : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        bool isDig = stateInfo.IsName("Dig");
        bool isAttack = stateInfo.IsName("Attack");
        GameObject pirate = animator.gameObject;
        Transform pirateTransform = pirate.transform;
        Transform armature = pirateTransform.GetChild(0);
        Transform hips = armature.GetChild(0);
        Transform spine = hips.GetChild(2);
        Transform spine1 = spine.GetChild(0);
        Transform spine2 = spine1.GetChild(0);
        /*
        Transform saberTransform = armature.GetChild(0);
        Transform shovelTransform = armature.GetChild(1);
        */
        Transform saberTransform = spine2.GetChild(0);
        Transform shovelTransform = spine2.GetChild(2);
        GameObject shovel = shovelTransform.gameObject;
        GameObject saber = saberTransform.gameObject;
        if (isDig)
        {
            Debug.Log("Это анимация лопаты");
        }
        else
        {
            Debug.Log("Это не анимация лопаты");
        }
        shovel.SetActive(isDig);
        saber.SetActive(isAttack);
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
