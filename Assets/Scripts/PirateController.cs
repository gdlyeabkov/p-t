﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations.Rigging;

public class PirateController : MonoBehaviour
{

    public float speed;
    public bool isCrossFound = false;
    public GameManager gameManager;
    public Coroutine progressCoroutine = null;
    public bool isHavePaint = false;
    public GameObject cross;
    public bool isStopped = false;
    public CrossController foundedCross;
    public int localIndex;
    public PhotonView photonView;
    public int networkIndex = 0;
    private PhotonPlayer currentPlayer;
    private Rigidbody rb;
    private float pitch = 0.0f;
    private float yaw = 0.0f;
    private float cameraRotationSpeed = 10f;
    public Vector3 offset = Vector3.zero;
    private Camera mainCamera;
    public bool isHaveShovel = false;
    public bool isMiniGame = false;
    public int miniGameCursor = 0;
    public string rawMiniGameKey = "";
    public bool isShovelFound = false;
    public Transform leftHandController;
    public Transform rightHandController;
    public Transform foundedShovel;
    public Transform cameraTarget;

    void Start()
    {

        mainCamera = Camera.main;
        GameObject rawMainCamera = mainCamera.gameObject;
        CameraTracker cameraTracker = rawMainCamera.GetComponent<CameraTracker>();
        photonView = GetComponent<PhotonView>();
        currentPlayer = PhotonNetwork.player;
        rb = GetComponent<Rigidbody>();
        ExitGames.Client.Photon.Hashtable customProperties = currentPlayer.CustomProperties;
        object rawCustomPropertiesIndex = customProperties["index"];
        networkIndex = ((int)(rawCustomPropertiesIndex));
        PhotonNetwork.OnEventCall += OnEvent;
        gameManager = cameraTracker.gameManager;
        gameManager.piratesCursor++;
        int updatedPiratesCursor = gameManager.piratesCursor;
        localIndex = updatedPiratesCursor;
        bool isLocalPirate = localIndex == networkIndex;
        if (isLocalPirate)
        {
            cameraTracker.Target = transform;
            StartCoroutine(InitOffset());
            gameManager.localPirate = this;
        }


        Transform pirateTransform = gameObject.transform;
        Transform bodyTransform = pirateTransform.GetChild(3);
        GameObject body = bodyTransform.gameObject;
        SkinnedMeshRenderer bodyRenderer = body.GetComponent<SkinnedMeshRenderer>();
        Material[] materials = bodyRenderer.materials;
        List<Material> playerMaterials = gameManager.playerMaterials;
        // int index = localIndex - 1;
        Material playerMaterial = playerMaterials[localIndex];
        materials[2] = playerMaterial;
        bodyRenderer.materials = materials;

    }

    void LateUpdate()
    {
        bool isLocalPirate = localIndex == networkIndex;
        if (isLocalPirate)
        {
            bool isGameManagerExists = gameManager != null;
            if (isGameManagerExists)
            {
                bool isWin = gameManager.isWin;
                bool isNotWin = !isWin;
                if (isNotWin)
                {
                    if (isMiniGame)
                    {
                        var possibleKeys = Enum.GetValues(typeof(KeyCode));
                        foreach (KeyCode kcode in possibleKeys)
                        {
                            bool isKeyDown = Input.GetKey(kcode);
                            if (isKeyDown)
                            {
                                string rawKeyCode = kcode.ToString();
                                bool isRightKeyDown = rawKeyCode == rawMiniGameKey;
                                if (isRightKeyDown)
                                {
                                    char generatedChar = GameManager.GetRandomCharacter();
                                    rawMiniGameKey = generatedChar.ToString();
                                    Text miniGameLabel = gameManager.miniGameLabel;
                                    miniGameLabel.text = rawMiniGameKey;
                                    miniGameCursor++;
                                    bool isMiniGameFinish = miniGameCursor >= 5;
                                    if (isMiniGameFinish)
                                    {
                                        miniGameCursor = 0;
                                        isMiniGame = false;
                                        GameObject miniGame = gameManager.miniGame;
                                        miniGame.SetActive(false);
                                        if (isCrossFound)
                                        {
                                            gameManager.ShowWin(localIndex, networkIndex);
                                            GetComponent<Animator>().Play("Victory");

                                            object[] networkData = new object[] { localIndex, "Victory" };
                                            PhotonNetwork.RaiseEvent(194, networkData, true, new RaiseEventOptions
                                            {
                                                Receivers = ReceiverGroup.Others
                                            });

                                        }
                                        else
                                        {
                                            isStopped = false;
                                            isShovelFound = false;
                                            isHaveShovel = true;
                                            object[] networkData = new object[] { };
                                            PhotonNetwork.RaiseEvent(195, networkData, true, new RaiseEventOptions
                                            {
                                                Receivers = ReceiverGroup.All
                                            });

                                            Vector3 origin = Vector3.zero;
                                            GameObject handController = leftHandController.gameObject;
                                            Rig rig = handController.GetComponent<Rig>();
                                            rig.weight = 0.0f;
                                            Transform ik = leftHandController.GetChild(0);
                                            Transform target = ik.GetChild(0); ;
                                            target.localPosition = origin;
                                            handController = rightHandController.gameObject;
                                            rig = handController.GetComponent<Rig>();
                                            rig.weight = 0.0f;
                                            ik = rightHandController.GetChild(0);
                                            target = ik.GetChild(0); ;
                                            target.localPosition = origin;

                                            foreach (PirateController pirate in GameObject.FindObjectsOfType<PirateController>())
                                            {
                                                GameObject rawPirate = pirate.gameObject;
                                                Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), rawPirate.GetComponent<CapsuleCollider>(), false);
                                            }

                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        KeyCode eKey = KeyCode.E;
                        bool isEKeyDown = Input.GetKeyDown(eKey);
                        KeyCode qKey = KeyCode.Q;
                        bool isQKeyDown = Input.GetKeyDown(qKey);
                        bool isEKeyUp = Input.GetKeyUp(eKey);
                        bool isEKey = Input.GetKey(eKey);
                        KeyCode spaceKey = KeyCode.Space;
                        bool isSpaceKeyDown = Input.GetKeyDown(spaceKey);
                        if (isEKeyDown)
                        {
                            if (isCrossFound && isHaveShovel)
                            {
                                bool isCrossTrap = foundedCross.isTrap;
                                if (isCrossTrap)
                                {
                                    GameObject rawFoundedCross = foundedCross.gameObject;
                                    AudioSource foundedCrossAudio = rawFoundedCross.GetComponent<AudioSource>();
                                    foundedCrossAudio.Play();
                                    object[] networkData = new object[] { };
                                    PhotonNetwork.RaiseEvent(198, networkData, true, new RaiseEventOptions
                                    {
                                        Receivers = ReceiverGroup.All
                                    });
                                    isCrossFound = false;
                                }
                                else
                                {
                                    isMiniGame = true;
                                    GameObject miniGame = gameManager.miniGame;
                                    miniGame.SetActive(true);
                                    char generatedChar = GameManager.GetRandomCharacter();
                                    rawMiniGameKey = generatedChar.ToString();
                                    Text miniGameLabel = gameManager.miniGameLabel;
                                    miniGameLabel.text = rawMiniGameKey;
                                    AnimatorStateInfo animatorStateInfo = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
                                    bool isAlreadyDig = animatorStateInfo.IsName("Dig");
                                    bool isDoDig = !isAlreadyDig;
                                    if (isDoDig)
                                    {
                                        GetComponent<Animator>().Play("Dig");
                                        
                                        object[] networkData = new object[] { localIndex, "Dig" };
                                        PhotonNetwork.RaiseEvent(194, networkData, true, new RaiseEventOptions
                                        {
                                            Receivers = ReceiverGroup.Others
                                        });

                                    }

                                }
                            }
                            else if (isShovelFound)
                            {
                                isMiniGame = true;
                                GameObject miniGame = gameManager.miniGame;
                                miniGame.SetActive(true);
                                char generatedChar = GameManager.GetRandomCharacter();
                                rawMiniGameKey = generatedChar.ToString();
                                Text miniGameLabel = gameManager.miniGameLabel;
                                miniGameLabel.text = rawMiniGameKey;
                                AnimatorStateInfo animatorStateInfo = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
                                bool isAlreadyPull = animatorStateInfo.IsName("Pull");
                                bool isDoPull = !isAlreadyPull;
                                if (isDoPull)
                                {
                                    GetComponent<Animator>().Play("Pull");
                                    object[] networkData = new object[] { localIndex, "Pull" };
                                    PhotonNetwork.RaiseEvent(194, networkData, true, new RaiseEventOptions
                                    {
                                        Receivers = ReceiverGroup.Others
                                    });

                                    Vector3 foundedShovelPosition = foundedShovel.position;
                                    GameObject handController = leftHandController.gameObject;
                                    Rig rig = handController.GetComponent<Rig>();
                                    rig.weight = 1.0f;
                                    Transform ik = leftHandController.GetChild(0);
                                    Transform target = ik.GetChild(0); ;
                                    target.position = foundedShovelPosition;
                                    handController = rightHandController.gameObject;
                                    rig = handController.GetComponent<Rig>();
                                    rig.weight = 1.0f;
                                    ik = rightHandController.GetChild(0);
                                    target = ik.GetChild(0); ;
                                    target.position = foundedShovelPosition;
                                    object[] localNetworkData = new object[] { localIndex, foundedShovelPosition.x, foundedShovelPosition.y, foundedShovelPosition.z};
                                    PhotonNetwork.RaiseEvent(192, localNetworkData, true, new RaiseEventOptions
                                    {
                                        Receivers = ReceiverGroup.Others
                                    });

                                    foreach (PirateController pirate in GameObject.FindObjectsOfType<PirateController>())
                                    {
                                        GameObject rawPirate = pirate.gameObject;
                                        Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), rawPirate.GetComponent<CapsuleCollider>());
                                    }
                                }
                            }
                            isStopped = true;
                        }
                        else if (isQKeyDown)
                        {
                            if (isHavePaint)
                            {
                                isHavePaint = false;
                                PhotonNetwork.SetMasterClient(currentPlayer);
                                Quaternion baseRotation = Quaternion.identity;
                                Vector3 currentPiratePosition = transform.position;
                                
                                float coordX = currentPiratePosition.x;
                                //float coordX = 0f;

                                // float coordY = currentPiratePosition.y;
                                float coordY = currentPiratePosition.y + 0.1f;

                                float coordZ = currentPiratePosition.z;
                                // float coordZ = 0f;

                                Vector3 crossTrapPosition = new Vector3(coordX, coordY, coordZ);
                                
                                // GameObject crossTrapInst = PhotonNetwork.Instantiate("cross_trap", crossTrapPosition, baseRotation, 0);
                                GameObject crossTrapInst = PhotonNetwork.Instantiate("pirate_cross_trap", crossTrapPosition, baseRotation, 0);

                                CrossController crossController = crossTrapInst.GetComponent<CrossController>();
                                crossController.isOwner = true;

                                Ray ray = new Ray(crossTrapInst.transform.position, Vector3.up);
                                RaycastHit hit = new RaycastHit();
                                bool isDetectIsland = Physics.Raycast(ray, out hit, Mathf.Infinity, gameManager.islandLayer);
                                if (isDetectIsland)
                                {
                                    Vector3 hitPoint = hit.point;
                                    crossTrapInst.transform.position = new Vector3(hitPoint.x, hitPoint.y + 0.1f, hitPoint.z);
                                }

                                GetComponent<Animator>().Play("Paint");

                                Transform islandSphereTransform = gameManager.islandSphereTransform;
                                Vector3 islandSphereTransformPosition = islandSphereTransform.position;
                                /*
                                crossTrapInst.transform.RotateAround(islandSphereTransformPosition, new Vector3(1f, 0f, 0f), currentPiratePosition.x);
                                crossTrapInst.transform.RotateAround(islandSphereTransformPosition, new Vector3(0f, 0f, 1f), currentPiratePosition.z);
                                */

                                /*
                                crossTrapInst.transform.RotateAround(islandSphereTransformPosition, new Vector3(1f, 0f, 0f), Quaternion.FromToRotation(Quaternion.identity.eulerAngles, transform.eulerAngles).eulerAngles.x);
                                crossTrapInst.transform.RotateAround(islandSphereTransformPosition, new Vector3(0f, 0f, 1f), Quaternion.FromToRotation(Quaternion.identity.eulerAngles, transform.eulerAngles).eulerAngles.z);
                                */

                                /*
                                crossTrapInst.transform.RotateAround(islandSphereTransformPosition, new Vector3(1f, 0f, 0f), Quaternion.FromToRotation(Vector3.left, transform.position).eulerAngles.x);
                                crossTrapInst.transform.RotateAround(islandSphereTransformPosition, new Vector3(0f, 0f, 1f), Quaternion.FromToRotation(Vector3.forward, transform.position).eulerAngles.z);
                                */
                                /*
                                crossTrapInst.transform.RotateAround(islandSphereTransformPosition, new Vector3(1f, 0f, 0f), Quaternion.FromToRotation(Vector3.up, transform.position).eulerAngles.x);
                                crossTrapInst.transform.RotateAround(islandSphereTransformPosition, new Vector3(0f, 0f, 1f), Quaternion.FromToRotation(Vector3.up, transform.position).eulerAngles.z);
                                */
                                /*
                                crossTrapInst.transform.RotateAround(islandSphereTransformPosition, new Vector3(1f, 0f, 0f), Vector3.Angle(islandSphereTransformPosition, transform.position));
                                crossTrapInst.transform.RotateAround(islandSphereTransformPosition, new Vector3(0f, 0f, 1f), Vector3.Angle(islandSphereTransformPosition, transform.position));
                                */
                                /*
                                crossTrapInst.transform.RotateAround(islandSphereTransformPosition, new Vector3(1f, 0f, 0f), Vector3.Angle(Vector3.zero, transform.eulerAngles));
                                crossTrapInst.transform.RotateAround(islandSphereTransformPosition, new Vector3(0f, 0f, 1f), Vector3.Angle(Vector3.zero, transform.eulerAngles));
                                */
                                /*
                                crossTrapInst.transform.RotateAround(islandSphereTransformPosition, new Vector3(1f, 0f, 0f), Vector3.Angle(transform.position, Vector3.left));
                                crossTrapInst.transform.RotateAround(islandSphereTransformPosition, new Vector3(0f, 0f, 1f), Vector3.Angle(transform.position, Vector3.forward));
                                */
                                /*
                                crossTrapInst.transform.RotateAround(islandSphereTransformPosition, new Vector3(1f, 0f, 0f), Quaternion.LookRotation(transform.position, Vector3.left).eulerAngles.x);
                                crossTrapInst.transform.RotateAround(islandSphereTransformPosition, new Vector3(0f, 0f, 1f), Quaternion.LookRotation(transform.position, Vector3.forward).eulerAngles.z);
                                */
                                /*
                                crossTrapInst.transform.RotateAround(islandSphereTransformPosition, new Vector3(1f, 0f, 0f), Quaternion.LookRotation(new Vector3(0f, 4.107f, 0f) - transform.position, Vector3.left).eulerAngles.x);
                                crossTrapInst.transform.RotateAround(islandSphereTransformPosition, new Vector3(0f, 0f, 1f), Quaternion.LookRotation(new Vector3(0f, 4.107f, 0f) - transform.position, Vector3.forward).eulerAngles.z);
                                */
                                /*
                                crossTrapInst.transform.RotateAround(islandSphereTransformPosition, new Vector3(1f, 0f, 0f), Vector3.Angle(new Vector3(0f, 4.107f, 0f) - transform.position, Vector3.left));
                                crossTrapInst.transform.RotateAround(islandSphereTransformPosition, new Vector3(0f, 0f, 1f), Vector3.Angle(new Vector3(0f, 4.107f, 0f) - transform.position, Vector3.forward));
                                */
                                /*
                                crossTrapInst.transform.RotateAround(islandSphereTransformPosition, new Vector3(1f, 0f, 0f), Vector3.Angle(new Vector3(0f, 4.107f, 0f), transform.eulerAngles));
                                crossTrapInst.transform.RotateAround(islandSphereTransformPosition, new Vector3(0f, 0f, 1f), Vector3.Angle(new Vector3(0f, 4.107f, 0f), transform.eulerAngles));
                                */

                            }
                        }
                        else if (isEKeyUp)
                        {
                            bool isProgressExists = progressCoroutine != null;
                            if (isProgressExists)
                            {
                                StopCoroutine(progressCoroutine);
                            }
                            isStopped = false;
                            GetComponent<Animator>().Play("Walk");
                        }
                        else if (isEKey)
                        {
                            if (isCrossFound && isHaveShovel)
                            {
                                AudioSource audio = GetComponent<AudioSource>();
                                bool isHaveSound = audio.isPlaying;
                                bool isSilence = !isHaveSound;
                                if (isSilence)
                                {
                                    audio.Play();
                                }
                            }
                        }
                        else if (isSpaceKeyDown)
                        {
                            GetComponent<Animator>().Play("Attack");
                            Transform armature = transform.GetChild(0);
                            Transform hips = armature.GetChild(0);
                            Transform spine = hips.GetChild(2);
                            Transform spine1 = spine.GetChild(0);
                            Transform spine2 = spine1.GetChild(0);
                            Transform shoulder = spine2.GetChild(0);
                            Transform arm = shoulder.GetChild(0);
                            Transform foreArm = arm.GetChild(0);
                            Transform hand = foreArm.GetChild(0);
                            Vector3 handPosition = hand.position;
                            Collider[] colliders = Physics.OverlapSphere(handPosition, 1f);
                            foreach (Collider collider in colliders)
                            {
                                GameObject colliderObject = collider.gameObject;
                                string name = colliderObject.name;
                                Debug.Log("collider: " + name);
                                PirateController pirate = colliderObject.GetComponent<PirateController>();
                                bool isPirate = pirate != null;
                                if (isPirate)
                                {
                                    int pirateLocalIndex = pirate.localIndex;
                                    bool isEnemy = pirateLocalIndex != localIndex;
                                    if (isEnemy)
                                    {
                                        Debug.Log("Враг был убит");
                                        int networkId = currentPlayer.ID;
                                        PhotonView localPhotonView = colliderObject.GetComponent<PhotonView>();
                                        localPhotonView.TransferOwnership(networkId);
                                        float coordY = 4.107f;
                                        Vector3 randomPosition = new Vector3(0, coordY, 0);
                                        List<Transform> respawnPoints = gameManager.respawnPoints;
                                        Transform respawnPoint = respawnPoints[pirateLocalIndex];
                                        randomPosition = respawnPoint.position;
                                        colliderObject.transform.position = randomPosition;

                                        object[] networkData = new object[] { pirateLocalIndex };
                                        PhotonNetwork.RaiseEvent(193, networkData, true, new RaiseEventOptions
                                        {
                                            Receivers = ReceiverGroup.All
                                        });
                                        
                                    }
                                }
                            }
                        }
                        float mouseXDelta = Input.GetAxis("Mouse X");
                        float yawDelta = cameraRotationSpeed * mouseXDelta;
                        float mouseYDelta = Input.GetAxis("Mouse Y");
                        float pitchDelta = cameraRotationSpeed * mouseYDelta;
                        Vector3 currentCameraRotation = transform.eulerAngles;
                        float currentCameraZRotation = currentCameraRotation.z;
                        bool isCanRotate = true;
                        if (isCanRotate)
                        {
                            
                            yaw += yawDelta;
                            pitch -= pitchDelta;
                            
                            // Vector3 cameraRotation = new Vector3(pitch, yaw, currentCameraZRotation);
                            float currentCameraXRotation = currentCameraRotation.x;
                            Vector3 cameraRotation = new Vector3(currentCameraXRotation, yaw, currentCameraZRotation);

                            Transform mainCameraTransform = mainCamera.transform;
                            mainCameraTransform.eulerAngles = cameraRotation;

                            float cameraXRotation = cameraRotation.x;
                            float cameraYRotation = cameraRotation.y;
                            float cameraZRotation = cameraRotation.z;
                            rb.MoveRotation(Quaternion.Euler(0f, cameraYRotation, 0f));

                            Vector3 forwardDirection = Vector3.up;
                            Quaternion aroundRotation = Quaternion.AngleAxis(yawDelta, forwardDirection);
                            offset = aroundRotation * offset;
                            
                            Vector3 piratePosition = transform.position;
                            
                            Vector3 offsetPosition = piratePosition + offset;
                            Vector3 currentMainCameraTransformPosition = mainCamera.transform.position;
                            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, offsetPosition, 0.25f);
                            
                            mainCameraTransform.transform.Translate(0, 0.2f, -0.5f, transform);

                        }
                    }
                }
            }
        }
    }

    void FixedUpdate()
    {
        bool isGo = !isStopped;
        if (isGo)
        {
            bool isIndexesMatches = networkIndex == localIndex;
            if (isIndexesMatches)
            {
                bool isNotMiniGame = !isMiniGame;
                if (isNotMiniGame)
                {
                    float horizontalDelta = Input.GetAxis("Horizontal");
                    float verticalDelta = Input.GetAxis("Vertical");
                    bool isHorizontalMotion = horizontalDelta != 0;
                    bool isVerticalMotion = verticalDelta != 0;
                    bool isMotion = isHorizontalMotion || isVerticalMotion;
                    AnimatorStateInfo animatorStateInfo = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
                    if (isMotion)
                    {
                        bool isPaint = animatorStateInfo.IsName("Paint");
                        bool isDoWalk = !isPaint;
                        if (isDoWalk)
                        {
                            int networkId = currentPlayer.ID;
                            photonView.TransferOwnership(networkId);
                            Vector3 m_Input = new Vector3(horizontalDelta, 0, verticalDelta);
                            Vector3 currentPosition = rb.position;
                            Vector3 forwardDirection = Vector3.forward;
                            Vector3 speedforwardDirection = forwardDirection * speed;
                            Vector3 boostMotion = speedforwardDirection * Time.fixedDeltaTime;
                            Vector3 localOffset = transform.TransformDirection(boostMotion);
                            Vector3 updatedPosition = currentPosition + localOffset;
                            rb.MovePosition(updatedPosition);
                            GetComponent<Animator>().Play("Walk");
                        }
                    }
                    else
                    {
                        animatorStateInfo = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
                        bool isAlreadyIdle = animatorStateInfo.IsName("Idle");
                        bool isAttack = animatorStateInfo.IsName("Attack");
                        bool isPaint = animatorStateInfo.IsName("Paint");
                        bool isDoIdle = !isAlreadyIdle && !isAttack && !isPaint;
                        if (isDoIdle)
                        {
                            GetComponent<Animator>().Play("Idle");

                            object[] networkData = new object[] { localIndex, "Idle" };
                            PhotonNetwork.RaiseEvent(194, networkData, true, new RaiseEventOptions
                            {
                                Receivers = ReceiverGroup.Others
                            });

                        }
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
        bool isPaint = detectedObjectTag == "Paint";
        bool isShovel = detectedObjectTag == "Shovel";
        if (isCross)
        {
            isCrossFound = true;
            CrossController crossController = detectedObject.GetComponent<CrossController>();
            foundedCross = crossController;
        }
        else if (isPaint)
        {
            isHavePaint = true;
            PaintController paintController = detectedObject.GetComponent<PaintController>();
            int paintIndex = paintController.localIndex;
            object[] networkData = new object[] { paintIndex };
            PhotonNetwork.RaiseEvent(197, networkData, true, new RaiseEventOptions
            {
                Receivers = ReceiverGroup.All
            });
        }
        else if (isShovel)
        {
            isShovelFound = true;
            foundedShovel = detectedObject.transform;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        GameObject detectedObject = other.gameObject;
        string detectedObjectTag = detectedObject.tag;
        bool isCross = detectedObjectTag == "Cross";
        bool isShovel = detectedObjectTag == "Shovel";
        if (isCross)
        {
            isCrossFound = false;
            foundedCross = null;
        }
        else if (isShovel)
        {
            isShovelFound = false;
            foundedShovel = null;
        }
    }

    public IEnumerator AddProgressCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.05f);
            gameManager.digProgress++;
            int updatedProgress = gameManager.digProgress;
            bool isGameOver = updatedProgress >= 100;
            if (isGameOver)
            {
                gameManager.ShowWin(localIndex, networkIndex);
                GetComponent<Animator>().Play("Victory");
            }
        }
    }

    public void OnEvent(byte eventCode, object content, int senderId)
    {
        bool isGameOverEvent = eventCode == 196;
        bool isPirateAnimationEvent = eventCode == 194;
        bool isDieEvent = eventCode == 193;
        bool isDigEvent = eventCode == 192;
        if (isGameOverEvent)
        {
            try
            {
                object[] data = (object[])content;
                int index = (int)data[0];
                int localNetworkIndex = (int)data[1];
                bool isLocalPlayer = networkIndex == localNetworkIndex;
                bool isLocalPirate = index == localIndex;
                bool isLocalPiratePlayer = isLocalPlayer && isLocalPirate;
                if (isLocalPiratePlayer)
                {
                    PhotonNetwork.SetMasterClient(currentPlayer);
                    Vector3 currentPosition = transform.position;
                    float coordX = currentPosition.x;
                    float coordY = currentPosition.y;
                    float verticalOffset = 1f;
                    float coordZ = currentPosition.z;
                    coordY += verticalOffset;
                    Vector3 treasurePosition = new Vector3(coordX, coordY, coordZ);
                    Quaternion baseRotation = Quaternion.identity;
                    PhotonNetwork.Instantiate("treasure", treasurePosition, baseRotation, 0);
                }
            }
            catch (System.InvalidCastException e)
            {
                string castError = "Ошибка с привидением типа photon. Не могу передать photon данные";
                Debug.Log(castError);
            }
            catch (System.Exception e)
            {
                string photonError = "Не могу передать photon данные";
                Debug.Log(photonError);
            }
        }
        else if (isPirateAnimationEvent)
        {
            try
            {
                object[] data = (object[])content;
                int index = (int)data[0];
                string name = (string)data[1];
                bool isLocalPirate = index == localIndex;
                if (isLocalPirate)
                {
                    gameObject.GetComponent<Animator>().Play(name);
                }
            }
            catch (System.InvalidCastException)
            {
                string castError = "Ошибка с привидением типа photon. Не могу передать photon данные";
                Debug.Log(castError);
            }
            catch (System.Exception)
            {
                string photonError = "Не могу передать photon данные";
                Debug.Log(photonError);
            }
        }
        else if (isDieEvent)
        {
            try
            {
                object[] data = (object[])content;
                int index = (int)data[0];
                bool isLocalPirate = index == localIndex;
                if (isLocalPirate)
                {
                    miniGameCursor = 0;
                    isMiniGame = false;
                    isStopped = false;
                    isShovelFound = false;
                    GameObject miniGame = gameManager.miniGame;
                    miniGame.SetActive(false);
                    GetComponent<Animator>().Play("Idle");
                
                    if (isHaveShovel)
                    {
                        bool isHost = PhotonNetwork.isMasterClient;
                        if (isHost)
                        {
                            gameManager.GenerateShovel();
                        }
                    }
                    isHaveShovel = false;

                    Vector3 origin = Vector3.zero;
                    GameObject handController = leftHandController.gameObject;
                    Rig rig = handController.GetComponent<Rig>();
                    rig.weight = 0.0f;
                    Transform ik = leftHandController.GetChild(0);
                    Transform target = ik.GetChild(0); ;
                    target.localPosition = origin;
                    handController = rightHandController.gameObject;
                    rig = handController.GetComponent<Rig>();
                    rig.weight = 0.0f;
                    ik = rightHandController.GetChild(0);
                    target = ik.GetChild(0); ;
                    target.localPosition = origin;

                    foreach (PirateController pirate in GameObject.FindObjectsOfType<PirateController>())
                    {
                        GameObject rawPirate = pirate.gameObject;
                        Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), rawPirate.GetComponent<CapsuleCollider>(), false);
                    }

                }
            }
            catch (System.InvalidCastException)
            {
                string castError = "Ошибка с привидением типа photon. Не могу передать photon данные";
                Debug.Log(castError);
            }
            catch (System.Exception)
            {
                string photonError = "Не могу передать photon данные";
                Debug.Log(photonError);
            }
        }
        else if (isDigEvent)
        {
            try
            {
                object[] data = (object[])content;
                int index = (int)data[0];
                float x = (float)data[1];
                float y = (float)data[2];
                float z = (float)data[3];
                bool isLocalPirate = index == localIndex;
                if (isLocalPirate)
                {
                    // Vector3 foundedShovelPosition = foundedShovel.position;
                    Vector3 foundedShovelPosition = new Vector3(x, y, z);
                    GameObject handController = leftHandController.gameObject;
                    Rig rig = handController.GetComponent<Rig>();
                    rig.weight = 1.0f;
                    Transform ik = leftHandController.GetChild(0);
                    Transform target = ik.GetChild(0); ;
                    target.position = foundedShovelPosition;
                    handController = rightHandController.gameObject;
                    rig = handController.GetComponent<Rig>();
                    rig.weight = 1.0f;
                    ik = rightHandController.GetChild(0);
                    target = ik.GetChild(0); ;
                    target.position = foundedShovelPosition;

                    foreach (PirateController pirate in GameObject.FindObjectsOfType<PirateController>())
                    {
                        GameObject rawPirate = pirate.gameObject;
                        Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), rawPirate.GetComponent<CapsuleCollider>(), false);
                    }

                }
            }
            catch (System.InvalidCastException)
            {
                string castError = "Ошибка с привидением типа photon. Не могу передать photon данные";
                Debug.Log(castError);
            }
            catch (System.Exception)
            {
                string photonError = "Не могу передать photon данные";
                Debug.Log(photonError);
            }
        }
    }

    public IEnumerator InitOffset()
    {

        mainCamera.transform.position = cameraTarget.position;

        yield return new WaitForSeconds(2f);

        // mainCamera.transform.position = cameraTarget.position;
        
        Transform mainCameraTransform = mainCamera.transform;
        Vector3 mainCameraTransformPosition = mainCameraTransform.position;
        
        Vector3 piratePosition = transform.position;
        // Vector3 piratePosition = cameraTarget.position;

        offset = mainCameraTransformPosition - piratePosition;

    }

}