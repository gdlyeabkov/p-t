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
                    if (GetComponent<NavMeshAgent>().enabled && gameObject.activeSelf && detectedObject.activeSelf && GetComponent<Rigidbody>().constraints != RigidbodyConstraints.FreezeAll)
                    {
                        pirateController.DoAttack();
                        pirateController.gameManager.GiveOrder(gameObject);
                    }
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
        bool isPaint = detectedObjectTag == "Paint";
        bool isPistol = detectedObjectTag == "Pistol";
        Camera mainCamera = Camera.main;
        GameObject rawMainCamera = mainCamera.gameObject;
        CameraTracker cameraTracker = rawMainCamera.GetComponent<CameraTracker>();
        GameManager gameManager = cameraTracker.gameManager;
        bool isInit = gameManager.isInit;
        if (isInit)
        {
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
                        // pirateController.gameManager.GiveOrder(gameObject);
                        pirateController.isShovelFound = true;
                        pirateController.foundedShovel = detectedObject.transform;
                        pirateController.DoAction();
                    }
                }
                else if (pirateController.networkIndex != 0)
                {
                    pirateController.gameManager.GiveOrder(gameObject);
                    pirateController.isShovelFound = true;
                    pirateController.foundedShovel = detectedObject.transform;
                    pirateController.DoAction();
                }
            }
            else if (isPaint)
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
                        // pirateController.DoPaint();
                        pirateController.isHavePaint = true;
                        PaintController paintController = detectedObject.GetComponent<PaintController>();
                        int paintIndex = paintController.localIndex;
                        if (pirateController.isStandardMode)
                        {
                            object[] networkData = new object[] { paintIndex };
                            PhotonNetwork.RaiseEvent(197, networkData, true, new RaiseEventOptions
                            {
                                Receivers = ReceiverGroup.All
                            });
                        }
                        else
                        {
                            Destroy(detectedObject);
                        }
                        pirateController.DoPaint();
                        // gameManager.GiveOrder(gameObject);
                        StartCoroutine(gameManager.GiveOrders());
                    }
                }
                else if (pirateController.networkIndex != 0)
                {
                    pirateController.DoPaint();
                }
            }
            else if (isPistol)
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
                        pirateController.isHavePistol = true;
                        PistolController pistolController = detectedObject.GetComponent<PistolController>();
                        int pistolIndex = pistolController.localIndex;
                        if (pirateController.isStandardMode)
                        {
                            object[] networkData = new object[] { pistolIndex };
                            PhotonNetwork.RaiseEvent(185, networkData, true, new RaiseEventOptions
                            {
                                Receivers = ReceiverGroup.All
                            });
                        }
                        else
                        {
                            Destroy(detectedObject);
                        }
                        Transform newAgentTarget = gameManager.localPirate.transform;
                        Vector3 destination = newAgentTarget.position;
                        pirateController.agentTarget = newAgentTarget;
                        pirateController.destination = destination;
                        pirateController.DoAttack();
                        StartCoroutine(gameManager.GiveOrders());
                    }
                }
                else if (pirateController.networkIndex != 0)
                {
                    pirateController.DoAttack();
                }
            }
        }
    }

}