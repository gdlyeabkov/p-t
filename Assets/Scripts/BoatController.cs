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
        GameObject treasureInst = gameManager.treasureInst;
        bool isTreasureExists = treasureInst != null;
        if (isTreasureExists)
        {
            GameObject detectedObject = other.gameObject;
            // SpringJoint joint = detectedObject.GetComponent<SpringJoint>();
            // bool isTreasure = joint != null;
            SpringJoint joint = gameManager.treasureInst.GetComponent<SpringJoint>();
            string detectedObjectTag = detectedObject.tag;
            bool isTreasure = detectedObjectTag == "Treasure";
            if (isTreasure)
            {
                Rigidbody rb = joint.connectedBody;
                bool isBodyExists = rb != null;
                if (isBodyExists)
                {
                    GameObject rbOwner = rb.gameObject;
                    NavMeshAgent botController = rbOwner.GetComponent<NavMeshAgent>();
                    bool isBot = botController != null;
                    PirateController pirateController = null;
                    if (isBot)
                    {
                        GameObject rawBot = botController.gameObject;
                        Transform botTransform = rawBot.transform;
                        Transform pirateBotTransform = botTransform.GetChild(0);
                        GameObject rawPirateBot = pirateBotTransform.gameObject;
                        pirateController = rawPirateBot.GetComponent<PirateController>();
                    }
                    else
                    {
                        pirateController = rbOwner.GetComponent<PirateController>();
                    }
                    int localIndex = pirateController.localIndex;
                    int networkIndex = pirateController.networkIndex;
                    bool isPirateBoat = localIndex == number;
                    if (isPirateBoat)
                    {

                        gameManager.ShowWin(localIndex, networkIndex);
                        pirateController.gameObject.GetComponent<Animator>().Play("Victory");

                        StartCoroutine(gameManager.ResetGame());

                        /*bool isLooser = networkIndex != localIndex;
                        if (!isLooser)
                        {
                            gameManager.mainCameraAudio.clip = gameManager.winSound;
                            gameManager.mainCameraAudio.Play();
                        }*/

                        object[] networkData = new object[] { pirateController.localIndex, "Victory" };
                        PhotonNetwork.RaiseEvent(194, networkData, true, new RaiseEventOptions
                        {
                            Receivers = ReceiverGroup.All
                        });

                        pirateController.miniGameCursor = 0;
                        pirateController.isMiniGame = false;
                        pirateController.isStopped = false;
                        pirateController.isShovelFound = false;
                        bool isShovelExists = pirateController.foundedShovel != null;
                        bool isOnNavMesh = true;
                        if (isBot)
                        {
                            isOnNavMesh = botController.isOnNavMesh;
                        }
                        bool isSwitchTarget = isShovelExists && isOnNavMesh;
                        if (isSwitchTarget)
                        {
                            gameManager.GiveOrders();
                            pirateController.isHaveShovel = true;
                            Transform foundedShovel = pirateController.foundedShovel;
                            GameObject rawFoundedShovel = foundedShovel.gameObject;
                            Destroy(rawFoundedShovel);
                            GameObject cross = gameManager.cross;
                            Transform agentTarget = cross.transform;
                            pirateController.destination = agentTarget.position;
                            pirateController.agentTarget = agentTarget;
                        }
                        Vector3 origin = Vector3.zero;
                        Transform leftHandController = pirateController.leftHandController;
                        GameObject handController = leftHandController.gameObject;
                        Rig rig = handController.GetComponent<Rig>();
                        rig.weight = 0.0f;
                        Transform ik = leftHandController.GetChild(0);
                        Transform target = ik.GetChild(0); ;
                        target.localPosition = origin;
                        Transform rightHandController = pirateController.rightHandController;
                        handController = rightHandController.gameObject;
                        rig = handController.GetComponent<Rig>();
                        rig.weight = 0.0f;
                        ik = rightHandController.GetChild(0);
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
                                Transform parent = transform.parent;
                                GameObject rawParent = parent.gameObject;
                                localCollider = rawParent.GetComponent<CapsuleCollider>();
                            }
                            else
                            {
                                localCollider = GetComponent<CapsuleCollider>();
                            }
                            Transform rawPirateTransform = rawPirate.transform;
                            Transform botTransform = rawPirateTransform.parent;
                            isBot = botTransform != null;
                            if (isBot)
                            {
                                GameObject bot = botTransform.gameObject;
                                somePirateCollider = bot.GetComponent<CapsuleCollider>();
                            }
                            else
                            {
                                somePirateCollider = rawPirate.GetComponent<CapsuleCollider>();

                                if (localIndex != pirate.localIndex)
                                {
                                    pirate.GetComponent<Animator>().Play("Loose");
                                    networkData = new object[] { pirate.localIndex, "Loose" };
                                    PhotonNetwork.RaiseEvent(194, networkData, true, new RaiseEventOptions
                                    {
                                        Receivers = ReceiverGroup.Others
                                    });
                                }

                            }
                            try
                            {
                                Physics.IgnoreCollision(localCollider, somePirateCollider, false);
                            }
                            catch
                            {

                            }
                        }
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
                                botController = localBot.GetComponent<NavMeshAgent>();
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

                        /*Transform pirateControllerTransform = pirateController.transform;
                        Transform armature = pirateControllerTransform.GetChild(0);
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
                        treasure.SetActive(false);*/

                    }
                }
            }

        }
    }

}
