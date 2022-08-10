using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BotController : MonoBehaviour
{
    public void OnCollisionEnter(Collision other)
    {
        GameObject detectedObject = other.gameObject;
        bool isPirate = detectedObject.GetComponent<PirateController>() != null;
        if (isPirate)
        {
            Transform pirateTransform = transform.GetChild(0);
            GameObject pirate = pirateTransform.gameObject;
            PirateController pirateController = pirate.GetComponent<PirateController>();
            bool isEnemy = detectedObject.GetComponent<PirateController>().localIndex != pirateController.localIndex;
            if (isEnemy)
            {
                NavMeshAgent agent = GetComponent<NavMeshAgent>();
                bool isBot = agent != null;
                Transform detectedObjectTransform = detectedObject.transform;
                bool isMissionComplete = pirateController.agentTarget == detectedObjectTransform;
                bool isStop = isBot && isMissionComplete;
                if (isStop)
                {
                    pirateController.DoAttack();
                    pirateController.gameManager.GiveOrder(gameObject);
                }
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        GameObject detectedObject = other.gameObject;
        string detectedObjectTag = detectedObject.tag;
        bool isCross = detectedObjectTag == "Cross";
        bool isShovel = detectedObjectTag == "Shovel";
        if (isCross)
        {
            Transform detectedObjectTransform = detectedObject.transform;
            Transform rawPirateTransform = transform.GetChild(0);
            GameObject rawPirate = rawPirateTransform.gameObject;
            PirateController pirateController = rawPirate.GetComponent<PirateController>();
            CrossController crossController = detectedObject.GetComponent<CrossController>();
            Transform agentTarget = pirateController.agentTarget;
            bool isHaveTarget = agentTarget != null;
            if (isHaveTarget)
            {
                bool isMissionComplete = agentTarget == detectedObjectTransform;
                if (isMissionComplete)
                {
                    NavMeshAgent agent = GetComponent<NavMeshAgent>();
                    // agent.isStopped = true;
                    pirateController.gameManager.GiveOrder(gameObject);
                    pirateController.isCrossFound = true;
                    pirateController.foundedCross = crossController;
                    pirateController.DoAction();
                }
            }
        }
        else if (isShovel)
        {
            Transform detectedObjectTransform = detectedObject.transform;
            Transform rawPirateTransform = transform.GetChild(0);
            GameObject rawPirate = rawPirateTransform.gameObject;
            PirateController pirateController = rawPirate.GetComponent<PirateController>();
            Transform agentTarget = pirateController.agentTarget;
            bool isHaveTarget = agentTarget != null;
            if (isHaveTarget)
            {
                bool isMissionComplete = agentTarget == detectedObjectTransform;
                if (isMissionComplete)
                {
                    NavMeshAgent agent = GetComponent<NavMeshAgent>();
                    // agent.isStopped = true;
                    pirateController.gameManager.GiveOrder(gameObject);
                    pirateController.isShovelFound = true;
                    pirateController.foundedShovel = detectedObject.transform;
                    pirateController.DoAction();
                }
            }
        }
    }

    public void Update()
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        bool isOnNavMesh = agent.isOnNavMesh;
        if (isOnNavMesh)
        {
            if (agent.remainingDistance <= 3f)
            // if (false)
            {
                Transform pirateTransform = transform.GetChild(0);
                GameObject pirate = pirateTransform.gameObject;
                Animator pirateAnimator = pirate.GetComponent<Animator>();
                AnimatorStateInfo animatorStateInfo = pirateAnimator.GetCurrentAnimatorStateInfo(0);
                bool isAttack = animatorStateInfo.IsName("Attack");
                bool isNotAttack = !isAttack;
                if (isNotAttack)
                {
                    PirateController pirateController = pirate.GetComponent<PirateController>();
                    pirateController.DoAttack();
                    pirateController.gameManager.GiveOrder(gameObject);
                }
            }
        }
    }

}