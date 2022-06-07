﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    private float cameraRotationSpeed = 2f;
    public Vector3 offset = Vector3.zero;
    private Camera mainCamera;
    public bool isHaveShovel = false;
    public bool isMiniGame = false;
    public int miniGameCursor = 0;

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
            gameManager.localPirate = gameObject;
        }

    }

    void Update()
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
                        KeyCode spaceKey = KeyCode.Space;
                        bool isSpaceKeyDown = Input.GetKeyDown(spaceKey);
                        if (isSpaceKeyDown)
                        {
                            miniGameCursor++;
                            bool isMiniGameFinish = miniGameCursor >= 5;
                            gameManager.miniGameLabel.text = GameManager.GetRandomCharacter().ToString();
                            if (isMiniGameFinish)
                            {
                                miniGameCursor = 0;
                                isMiniGame = false;
                                gameManager.miniGame.SetActive(false);
                                if (isCrossFound)
                                {
                                    progressCoroutine = StartCoroutine(AddProgressCoroutine());
                                    AnimatorStateInfo animatorStateInfo = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
                                    bool isAlreadyDig = animatorStateInfo.IsName("Dig");
                                    bool isDoDig = !isAlreadyDig;
                                    if (isDoDig)
                                    {
                                        GetComponent<Animator>().Play("Dig");
                                    }
                                }
                                else
                                {
                                    isHaveShovel = true;
                                    object[] networkData = new object[] { };
                                    PhotonNetwork.RaiseEvent(195, networkData, true, new RaiseEventOptions
                                    {
                                        Receivers = ReceiverGroup.All
                                    });
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
                                    /*
                                    progressCoroutine = StartCoroutine(AddProgressCoroutine());

                                    AnimatorStateInfo animatorStateInfo = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
                                    bool isAlreadyDig = animatorStateInfo.IsName("Dig");
                                    bool isDoDig = !isAlreadyDig;
                                    if (isDoDig)
                                    {
                                        GetComponent<Animator>().Play("Dig");
                                    }
                                    */
                                    
                                    isMiniGame = true;
                                    gameManager.miniGame.SetActive(true);
                                    gameManager.miniGameLabel.text = GameManager.GetRandomCharacter().ToString();

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
                                float coordY = 0.02f;
                                float coordZ = currentPiratePosition.z;
                                Vector3 crossTrapPosition = new Vector3(coordX, coordY, coordZ);
                                GameObject crossTrapInst = PhotonNetwork.Instantiate("cross_trap", crossTrapPosition, baseRotation, 0);
                                CrossController crossController = crossTrapInst.GetComponent<CrossController>();
                                crossController.isOwner = true;

                                GetComponent<Animator>().Play("Paint");

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
                            Vector3 cameraRotation = new Vector3(pitch, yaw, currentCameraZRotation);
                            Transform mainCameraTransform = mainCamera.transform;
                            mainCameraTransform.eulerAngles = cameraRotation;
                            float cameraXRotation = cameraRotation.x;
                            float cameraYRotation = cameraRotation.y;
                            float cameraZRotation = cameraRotation.z;
                            rb.MoveRotation(Quaternion.Euler(cameraXRotation, cameraYRotation, cameraZRotation));
                            Vector3 forwardDirection = Vector3.up;
                            Quaternion aroundRotation = Quaternion.AngleAxis(yawDelta, forwardDirection);
                            offset = aroundRotation * offset;
                            Vector3 piratePosition = transform.position;
                            Vector3 offsetPosition = piratePosition + offset;
                            Vector3 currentMainCameraTransformPosition = mainCameraTransform.position;
                            mainCameraTransform.position = Vector3.Lerp(currentMainCameraTransformPosition, offsetPosition, 0.25f);
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

                if (isMiniGame)
                {
                    /*
                    KeyCode spaceKey = KeyCode.Space;
                    bool isSpaceKeyDown = Input.GetKeyDown(spaceKey);
                    if (isSpaceKeyDown)
                    {
                        miniGameCursor++;
                        bool isMiniGameFinish = miniGameCursor >= 5;
                        if (isMiniGameFinish)
                        {
                            miniGameCursor = 0;
                            isMiniGame = false;
                        }
                    }
                    */
                }
                else
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

                            bool isHaveMotion = verticalDelta > 0 || horizontalDelta > 0;
                            if (isHaveMotion)
                            {
                                animatorStateInfo = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
                                bool isAlreadyWalk = animatorStateInfo.IsName("Walk");
                                isDoWalk = !isAlreadyWalk;
                                if (isDoWalk)
                                {
                                    GetComponent<Animator>().Play("Walk");
                                }
                            }

                        }

                    }

                    else
                    {
                        animatorStateInfo = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
                        bool isAlreadyIdle = animatorStateInfo.IsName("Idle");
                        bool isAttack = animatorStateInfo.IsName("Attack");
                        bool isDoIdle = !isAlreadyIdle && !isAttack;
                        if (isDoIdle)
                        {
                            GetComponent<Animator>().Play("Idle");
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
            /*
            isHaveShovel = true;
            object[] networkData = new object[] {  };
            PhotonNetwork.RaiseEvent(195, networkData, true, new RaiseEventOptions
            {
                Receivers = ReceiverGroup.All
            });
            */

            isMiniGame = true;
            gameManager.miniGame.SetActive(true);

        }
    }

    public void OnTriggerExit(Collider other)
    {
        GameObject detectedObject = other.gameObject;
        string detectedObjectTag = detectedObject.tag;
        bool isCross = detectedObjectTag == "Cross";
        if (isCross)
        {
            isCrossFound = false;
            foundedCross = null;
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

                    GetComponent<Animator>().Play("Walk");

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
    }

    public IEnumerator InitOffset()
    {
        yield return new WaitForSeconds(2f);
        Transform mainCameraTransform = mainCamera.transform;
        Vector3 mainCameraTransformPosition = mainCameraTransform.position;
        Vector3 piratePosition = transform.position;
        offset = mainCameraTransformPosition - piratePosition;
    }

}