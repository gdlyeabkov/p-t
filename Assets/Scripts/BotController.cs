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

    /*
    Работает, но можно через GameManager
    public void Update()
    {
        Camera mainCamera = Camera.main;
        GameObject rawMainCamera = mainCamera.gameObject;
        CameraTracker cameraTracker = rawMainCamera.GetComponent<CameraTracker>();
        GameManager gameManager = cameraTracker.gameManager;
        GameObject treasureInst = gameManager.treasureInst;
        bool isTreasureExists = treasureInst != null;
        if (isTreasureExists)
        {
            SpringJoint joint = treasureInst.GetComponent<SpringJoint>();
            Rigidbody treasureBody = joint.connectedBody;
            bool isTreasureFree = treasureBody == null;
            if (isTreasureFree)
            {
                Debug.LogWarning("isTreasureFree: " + isTreasureFree);
                PirateController pirateController = transform.GetChild(0).GetComponent<PirateController>();
                Transform agentTarget = pirateController.agentTarget;
                bool isPickTreasure = agentTarget == treasureInst.transform;
                if (isPickTreasure)
                {
                    Debug.LogWarning("isPickTreasure: " + isPickTreasure);
                    Collider[] subjects = Physics.OverlapSphere(transform.position, 0.7f);
                    bool isTreasureDetected = false;
                    // bool isTreasureDetected = GetComponent<NavMeshAgent>().remainingDistance <= 0.7f;
                    foreach (Collider subject in subjects)
                    {
                        GameObject someSubject = subject.gameObject;
                        string someSubjectTag = someSubject.tag;
                        bool isTreasure = someSubjectTag == "Treasure";
                        if (isTreasure)
                        {
                            isTreasureDetected = true;
                            break;
                        }
                    }
                    if (isTreasureDetected)
                    {
                        Debug.LogWarning("isTreasureDetected: " + isTreasureDetected);
                        Rigidbody pirateBody = GetComponent<Rigidbody>();
                        treasureInst.GetComponent<SpringJoint>().connectedBody = pirateBody;
                        pirateController.GetComponent<Animator>().SetBool("isGrab", true);
                        Transform pirateTransform = pirateController.transform;
                        Transform armature = pirateTransform.GetChild(0);
                        Transform hips = armature.GetChild(0);
                        Transform spine = hips.GetChild(2);
                        Transform spine1 = spine.GetChild(0);
                        Transform spine2 = spine1.GetChild(0);
                        Transform rightSholder = spine2.GetChild(2);
                        Transform rightArm = rightSholder.GetChild(0);
                        Transform rightForeArm = rightArm.GetChild(0);
                        Transform rightHand = rightForeArm.GetChild(0);
                        Transform treasureTransform = rightHand.GetChild(2);
                        GameObject treasure = treasureTransform.gameObject;
                        treasure.SetActive(true);
                        treasureInst.transform.position = pirateBody.position;
                        treasureInst.SetActive(false);
                        treasureInst.GetComponent<MeshRenderer>().enabled = false;
                        pirateController.GetComponent<Animator>().Play("Grab_Idle");
                        List<GameObject> boats = gameManager.boats;
                        pirateController.agentTarget = boats[pirateController.localIndex].transform;
                        pirateController.destination = boats[pirateController.localIndex].transform.position;
                    }
                }
            }
        }
    }*/

}