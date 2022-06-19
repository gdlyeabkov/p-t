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
                    // agent.isStopped = true;
                    // GetComponent<Animator>().Play("Idle");
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
        bool isShovel = detectedObjectTag == "Shovel";
        if (isShovel)
        {
            /*
            Transform botTransform = transform.parent;
            bool isBot = botTransform != null;
            if (isBot)
            */
            {
                Transform detectedObjectTransform = detectedObject.transform;
                // GameObject bot = botTransform.gameObject;
                Transform rawPirateTransform = transform.GetChild(0);
                GameObject rawPirate = rawPirateTransform.gameObject;
                PirateController pirateController = rawPirate.GetComponent<PirateController>();
                Transform agentTarget = pirateController.agentTarget;
                bool isHaveTarget = agentTarget != null;
                if (isHaveTarget)
                {
                    bool isMissionComplete = agentTarget == detectedObjectTransform;
                    Debug.Log("isMissionComplete: " + isMissionComplete.ToString());
                    if (isMissionComplete)
                    {
                        // NavMeshAgent agent = bot.GetComponent<NavMeshAgent>();
                        NavMeshAgent agent = GetComponent<NavMeshAgent>();
                        agent.isStopped = true;
                        // GetComponent<Animator>().Play("Idle");
                        // pirateController.gameManager.GiveOrder(bot);
                        pirateController.gameManager.GiveOrder(gameObject);
                        pirateController.isShovelFound = true;
                        pirateController.foundedShovel = detectedObject.transform;
                        pirateController.DoAction();
                    }
                }
            }
        }

    }
}