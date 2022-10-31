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
        bool isGrabIdle = stateInfo.IsName("Grab_Idle");
        bool isGrabWalk = stateInfo.IsName("Grab_Walk");
        bool isVictory = stateInfo.IsName("Victory");
        bool isShoot = stateInfo.IsName("Shoot");
        GameObject pirate = animator.gameObject;
        Transform pirateTransform = pirate.transform;
        Transform armature = pirateTransform.GetChild(0);
        Transform hips = armature.GetChild(0);
        Transform spine = hips.GetChild(2);
        Transform spine1 = spine.GetChild(0);
        Transform spine2 = spine1.GetChild(0);
        Transform rightSholder = spine2.GetChild(2);
        Transform rightArm = rightSholder.GetChild(0);
        Transform rightForeArm = rightArm.GetChild(0);
        Transform rightHand = rightForeArm.GetChild(0);
        Transform saberTransform = spine2.GetChild(0);
        Transform shovelTransform = rightHand.GetChild(1);
        Transform treasureTransform = rightHand.GetChild(2);
        Transform rightHand1 = rightHand.GetChild(0);
        Transform rightHand2 = rightHand1.GetChild(0);
        Transform rightHand3 = rightHand2.GetChild(0);
        Transform rightHand4 = rightHand3.GetChild(0);
        Transform rightHand4End = rightHand4.GetChild(0);
        Transform rightHand4EndEnd = rightHand4End.GetChild(0);
        Transform rightHand4EndEndEnd = rightHand4EndEnd.GetChild(0);
        Transform pistolGunTransform = rightHand4EndEndEnd.GetChild(0);
        Transform pistolHandleTransform = rightHand4EndEndEnd.GetChild(1);
        Transform pistolTriggerTransform = rightHand4EndEndEnd.GetChild(3);
        GameObject shovel = shovelTransform.gameObject;
        GameObject saber = saberTransform.gameObject;
        GameObject treasure = treasureTransform.gameObject;
        GameObject pistolGun = pistolGunTransform.gameObject;
        GameObject pistolHandle = pistolHandleTransform.gameObject;
        GameObject pistolTrigger = pistolTriggerTransform.gameObject;
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
        if (isVictory)
        {
            treasure.SetActive(false);
        }
        pistolGun.SetActive(isShoot);
        pistolHandle.SetActive(isShoot);
        pistolTrigger.SetActive(isShoot);

    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }

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
