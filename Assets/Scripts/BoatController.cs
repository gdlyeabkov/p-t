using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

public class BoatController : MonoBehaviour
{

    public GameManager gameManager;
    public int number;

    void OnTriggerEnter(Collider other)
    {
        if (gameManager.treasureInst != null)
        {
            GameObject detectedObject = other.gameObject;
            /*
            PirateController pirateController = detectedObject.GetComponent<PirateController>();
            bool isPlayer = pirateController != null;
            NavMeshAgent botController = detectedObject.GetComponent<NavMeshAgent>();
            bool isBot = botController != null;
            bool isPirate = isPlayer || isBot;
            if (isPirate)
            {
                if (isPlayer)
                {
                    *//*
                    int localIndex = pirateController.localIndex;
                    int networkIndex = pirateController.networkIndex;
                    gameManager.ShowWin(localIndex, networkIndex);
                    pirateController.gameObject.GetComponent<Animator>().Play("Victory");
                    *//*
                }
                else if (isBot)
                {
                    pirateController = botController.gameObject.transform.GetChild(0).gameObject.GetComponent<PirateController>();
                    *//*
                    int localIndex = pirateController.localIndex;
                    int networkIndex = pirateController.networkIndex;
                    gameManager.ShowWin(localIndex, networkIndex);
                    pirateController.gameObject.GetComponent<Animator>().Play("Victory");
                    *//*
                }
                int localIndex = pirateController.localIndex;
                int networkIndex = pirateController.networkIndex;
                gameManager.ShowWin(localIndex, networkIndex);
                pirateController.gameObject.GetComponent<Animator>().Play("Victory");

                pirateController.miniGameCursor = 0;
                pirateController.isMiniGame = false;
                pirateController.isStopped = false;
                pirateController.isShovelFound = false;
                bool isShovelExists = pirateController.foundedShovel != null;
                bool isOnNavMesh = botController.isOnNavMesh;
                bool isSwitchTarget = isShovelExists && isOnNavMesh;
                if (isSwitchTarget)
                {
                    gameManager.GiveOrders();
                    pirateController.isHaveShovel = true;
                    GameObject rawFoundedShovel = pirateController.foundedShovel.gameObject;
                    Destroy(rawFoundedShovel);
                    pirateController.destination = gameManager.cross.transform.position;
                    pirateController.agentTarget = gameManager.cross.transform;
                }
                Vector3 origin = Vector3.zero;
                GameObject handController = pirateController.leftHandController.gameObject;
                Rig rig = handController.GetComponent<Rig>();
                rig.weight = 0.0f;
                Transform ik = pirateController.leftHandController.GetChild(0);
                Transform target = ik.GetChild(0); ;
                target.localPosition = origin;
                handController = pirateController.rightHandController.gameObject;
                rig = handController.GetComponent<Rig>();
                rig.weight = 0.0f;
                ik = pirateController.rightHandController.GetChild(0);
                target = ik.GetChild(0); ;
                target.localPosition = origin;
                PirateController[] pirates = GameObject.FindObjectsOfType<PirateController>();
                CapsuleCollider pirateCollider = GetComponent<CapsuleCollider>();
                foreach (PirateController pirate in pirates)
                {
                    GameObject rawPirate = pirate.gameObject;
                    CapsuleCollider localCollider = null;
                    CapsuleCollider somePirateCollider = null;
                    if (isBot)
                    {
                        localCollider = transform.parent.gameObject.GetComponent<CapsuleCollider>();
                    }
                    else
                    {
                        localCollider = GetComponent<CapsuleCollider>();
                    }
                    isBot = rawPirate.transform.parent != null;
                    if (isBot)
                    {
                        somePirateCollider = rawPirate.transform.parent.gameObject.GetComponent<CapsuleCollider>();
                    }
                    else
                    {
                        somePirateCollider = rawPirate.GetComponent<CapsuleCollider>();
                    }
                    try
                    {
                        Physics.IgnoreCollision(localCollider, somePirateCollider, false);
                    }
                    catch
                    {

                    }
                }
                gameManager.localPirate.GetComponent<Animator>().Play("Loose");
                gameManager.localPirate.isStopped = true;
                List<GameObject> bots = gameManager.bots;
                foreach (GameObject localBot in bots)
                {
                    Transform localBotTransform = localBot.transform;
                    Transform localPirateTransform = localBotTransform.GetChild(0);
                    GameObject localPirate = localPirateTransform.gameObject;
                    PirateController localPirateController = localPirate.GetComponent<PirateController>();
                    int localPirateIndex = localPirateController.localIndex;
                    bool isLooser = localPirateIndex != localIndex;
                    if (isLooser)
                    {
                        Animator localPirateAnimator = localPirate.GetComponent<Animator>();
                        localPirateAnimator.Play("Loose");
                        isOnNavMesh = botController.isOnNavMesh;
                        if (isOnNavMesh)
                        {
                            botController.isStopped = true;
                        }
                        gameManager.viewCamera.Follow = null;
                    }
                }
                AudioSource botAudio = pirateController.GetComponent<AudioSource>();
                botAudio.Stop();
            }
            */

            SpringJoint joint = detectedObject.GetComponent<SpringJoint>();
            bool isTreasure = joint != null;
            if (isTreasure)
            {
                Rigidbody rb = joint.connectedBody;
                GameObject rbOwner = rb.gameObject;
                NavMeshAgent botController = rbOwner.GetComponent<NavMeshAgent>();
                bool isBot = botController != null;
                PirateController pirateController = null;
                if (isBot)
                {
                    pirateController = botController.gameObject.transform.GetChild(0).gameObject.GetComponent<PirateController>();
                }
                else
                {
                    pirateController = rbOwner.GetComponent<PirateController>();
                }
                int localIndex = pirateController.localIndex;
                int networkIndex = pirateController.networkIndex;
                gameManager.ShowWin(localIndex, networkIndex);
                pirateController.gameObject.GetComponent<Animator>().Play("Victory");
                pirateController.miniGameCursor = 0;
                pirateController.isMiniGame = false;
                pirateController.isStopped = false;
                pirateController.isShovelFound = false;
                bool isShovelExists = pirateController.foundedShovel != null;
                bool isOnNavMesh = botController.isOnNavMesh;
                bool isSwitchTarget = isShovelExists && isOnNavMesh;
                if (isSwitchTarget)
                {
                    gameManager.GiveOrders();
                    pirateController.isHaveShovel = true;
                    GameObject rawFoundedShovel = pirateController.foundedShovel.gameObject;
                    Destroy(rawFoundedShovel);
                    pirateController.destination = gameManager.cross.transform.position;
                    pirateController.agentTarget = gameManager.cross.transform;
                }
                Vector3 origin = Vector3.zero;
                GameObject handController = pirateController.leftHandController.gameObject;
                Rig rig = handController.GetComponent<Rig>();
                rig.weight = 0.0f;
                Transform ik = pirateController.leftHandController.GetChild(0);
                Transform target = ik.GetChild(0); ;
                target.localPosition = origin;
                handController = pirateController.rightHandController.gameObject;
                rig = handController.GetComponent<Rig>();
                rig.weight = 0.0f;
                ik = pirateController.rightHandController.GetChild(0);
                target = ik.GetChild(0); ;
                target.localPosition = origin;
                PirateController[] pirates = GameObject.FindObjectsOfType<PirateController>();
                CapsuleCollider pirateCollider = GetComponent<CapsuleCollider>();
                foreach (PirateController pirate in pirates)
                {
                    GameObject rawPirate = pirate.gameObject;
                    CapsuleCollider localCollider = null;
                    CapsuleCollider somePirateCollider = null;
                    if (isBot)
                    {
                        localCollider = transform.parent.gameObject.GetComponent<CapsuleCollider>();
                    }
                    else
                    {
                        localCollider = GetComponent<CapsuleCollider>();
                    }
                    isBot = rawPirate.transform.parent != null;
                    if (isBot)
                    {
                        somePirateCollider = rawPirate.transform.parent.gameObject.GetComponent<CapsuleCollider>();
                    }
                    else
                    {
                        somePirateCollider = rawPirate.GetComponent<CapsuleCollider>();
                    }
                    try
                    {
                        Physics.IgnoreCollision(localCollider, somePirateCollider, false);
                    }
                    catch
                    {

                    }
                }
                gameManager.localPirate.GetComponent<Animator>().Play("Loose");
                gameManager.localPirate.isStopped = true;
                List<GameObject> bots = gameManager.bots;
                foreach (GameObject localBot in bots)
                {
                    Transform localBotTransform = localBot.transform;
                    Transform localPirateTransform = localBotTransform.GetChild(0);
                    GameObject localPirate = localPirateTransform.gameObject;
                    PirateController localPirateController = localPirate.GetComponent<PirateController>();
                    int localPirateIndex = localPirateController.localIndex;
                    bool isLooser = localPirateIndex != localIndex;
                    if (isLooser)
                    {
                        Animator localPirateAnimator = localPirate.GetComponent<Animator>();
                        localPirateAnimator.Play("Loose");
                        isOnNavMesh = botController.isOnNavMesh;
                        if (isOnNavMesh)
                        {
                            botController.isStopped = true;
                        }
                        gameManager.viewCamera.Follow = null;
                    }
                }
                AudioSource botAudio = pirateController.GetComponent<AudioSource>();
                botAudio.Stop();
            }

        }
    }

}
