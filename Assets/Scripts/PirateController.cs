using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations.Rigging;
using UnityEngine.AI;

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
    public List<int> answers;
    public bool isStandardMode = true;
    public Vector3 destination = Vector3.zero;
    public Transform agentTarget;

    void Start()
    {
        mainCamera = Camera.main;
        GameObject rawMainCamera = mainCamera.gameObject;
        CameraTracker cameraTracker = rawMainCamera.GetComponent<CameraTracker>();
        rb = GetComponent<Rigidbody>();
        
        bool isNotStandardMode = PlayerPrefs.HasKey("Mode");
        isStandardMode = !isNotStandardMode;
        if (isStandardMode)
        {
            photonView = GetComponent<PhotonView>();
            currentPlayer = PhotonNetwork.player;
            ExitGames.Client.Photon.Hashtable customProperties = currentPlayer.CustomProperties;
            object rawCustomPropertiesIndex = customProperties["index"];
            networkIndex = ((int)(rawCustomPropertiesIndex));
            PhotonNetwork.OnEventCall += OnEvent;
        }
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
        Material playerMaterial = playerMaterials[localIndex];
        materials[2] = playerMaterial;
        bodyRenderer.materials = materials;

        if (isNotStandardMode)
        {
            networkIndex = updatedPiratesCursor;
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
                    Joystick rotationJoystick = gameManager.rotationJoystick;
                    float joystickVertical = rotationJoystick.Vertical;
                    bool isVerticalRotation = joystickVertical != 0f;
                    float joystickHorizontal = rotationJoystick.Vertical;
                    bool isHorizontalRotation = joystickHorizontal != 0f;
                    bool isRotation = isVerticalRotation || isHorizontalRotation;
                    if (isRotation)
                    {
                        float mouseXDelta = Input.GetAxis("Mouse X");
                        float yawDelta = cameraRotationSpeed * mouseXDelta;
                        float mouseYDelta = Input.GetAxis("Mouse Y");
                        float pitchDelta = cameraRotationSpeed * mouseYDelta;
                        Vector3 currentCameraRotation = transform.eulerAngles;
                        float currentCameraZRotation = currentCameraRotation.z;
                        Transform mainCameraTransform = mainCamera.transform;
                        yaw += yawDelta;
                        pitch -= pitchDelta;
                        float currentCameraXRotation = currentCameraRotation.x;
                        Vector3 cameraRotation = new Vector3(currentCameraXRotation, yaw, currentCameraZRotation);
                        mainCameraTransform.eulerAngles = cameraRotation;
                        float cameraYRotation = cameraRotation.y;
                        bool isNotMiniGame = !isMiniGame;
                        if (isNotMiniGame)
                        {
                            rb.MoveRotation(Quaternion.Euler(0f, cameraYRotation, 0f));
                        }
                        Vector3 forwardDirection = Vector3.up;
                        Quaternion aroundRotation = Quaternion.AngleAxis(yawDelta, forwardDirection);
                        offset = aroundRotation * offset;
                        Vector3 piratePosition = transform.position;
                        Vector3 offsetPosition = piratePosition + offset;
                        Vector3 currentMainCameraTransformPosition = mainCamera.transform.position;
                        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, offsetPosition, 0.25f);
                    }
                    mainCamera.transform.Translate(0, 0.08f, -0.15f, transform);
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
                    Joystick movementJoystick = gameManager.movementJoystick;
                    float joystickVertical = movementJoystick.Vertical;
                    bool isMotion = joystickVertical > 0f;
                    AnimatorStateInfo animatorStateInfo = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
                    if (isMotion)
                    {
                        bool isPaint = animatorStateInfo.IsName("Paint");
                        bool isDoWalk = !isPaint;
                        if (isDoWalk)
                        {
                            if (isStandardMode)
                            {
                                int networkId = currentPlayer.ID;
                                photonView.TransferOwnership(networkId);
                            }
                            Vector3 currentPosition = rb.position;
                            Vector3 forwardDirection = Vector3.forward;
                            Vector3 speedforwardDirection = forwardDirection * speed;
                            Vector3 boostMotion = speedforwardDirection * Time.fixedDeltaTime;
                            Vector3 localOffset = transform.TransformDirection(boostMotion);
                            Vector3 updatedPosition = currentPosition + localOffset;
                            rb.MovePosition(updatedPosition);
                            GetComponent<Animator>().Play("Walk");
                            if (isStandardMode)
                            {
                                object[] networkData = new object[] { localIndex, "Walk" };
                                PhotonNetwork.RaiseEvent(194, networkData, true, new RaiseEventOptions
                                {
                                    Receivers = ReceiverGroup.Others
                                });
                            }
                        }
                    }
                    else if (GetComponent<NavMeshAgent>() == null)
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

    public void OnCollisionEnter(Collision other)
    {
        GameObject detectedObject = other.gameObject;
        bool isPirate = detectedObject.GetComponent<PirateController>() != null;
        if (isPirate)
        {
            bool isEnemy = detectedObject.GetComponent<PirateController>().localIndex != localIndex;
            if (isEnemy)
            {
                NavMeshAgent agent = GetComponent<NavMeshAgent>();
                bool isBot = agent != null;
                Transform detectedObjectTransform = detectedObject.transform;
                bool isMissionComplete = agentTarget == detectedObjectTransform;
                bool isStop = isBot && isMissionComplete;
                if (isStop)
                {
                    agent.isStopped = true;
                    // GetComponent<Animator>().Play("Idle");
                    DoAttack();
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
            if (isStandardMode)
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

            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            bool isBot = agent != null;
            Transform detectedObjectTransform = detectedObject.transform;
            bool isMissionComplete = agentTarget == detectedObjectTransform;
            bool isStop = isBot && isMissionComplete;
            if (isStop)
            {
                agent.isStopped = true;
                // GetComponent<Animator>().Play("Idle");
                DoPaint();
            }

        }
        else if (isShovel)
        {
            isShovelFound = true;
            foundedShovel = detectedObject.transform;

            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            bool isBot = agent != null;
            Transform detectedObjectTransform = detectedObject.transform;
            bool isMissionComplete = agentTarget == detectedObjectTransform;
            bool isStop = isBot && isMissionComplete;
            if (isStop)
            {
                agent.isStopped = true;
                // GetComponent<Animator>().Play("Idle");
                DoAction();
            }

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
        bool isCrossEvent = eventCode == 191;
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

                    GameObject handController = leftHandController.gameObject;
                    Rig rig = handController.GetComponent<Rig>();
                    rig.weight = 0.0f;
                    handController = rightHandController.gameObject;
                    rig = handController.GetComponent<Rig>();
                    rig.weight = 0.0f;

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
                    AudioSource audio = GetComponent<AudioSource>();
                    AudioClip dieSound = gameManager.dieSound;
                    audio.clip = dieSound;
                    audio.loop = false;
                    audio.Play();
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
        else if (isCrossEvent)
        {
            try
            {
                object[] data = (object[])content;
                int index = (int)data[0];
                bool isLocalPirate = index == localIndex;
                if (isLocalPirate)
                {
                    GameObject handController = leftHandController.gameObject;
                    Rig rig = handController.GetComponent<Rig>();
                    rig.weight = 0.0f;
                    Transform ik = leftHandController.GetChild(0);
                    Transform target = ik.GetChild(0); ;
                    handController = rightHandController.gameObject;
                    rig = handController.GetComponent<Rig>();
                    rig.weight = 0.0f;
                    ik = rightHandController.GetChild(0);
                    target = ik.GetChild(0); ;
                    
                    foreach (PirateController pirate in GameObject.FindObjectsOfType<PirateController>())
                    {
                        GameObject rawPirate = pirate.gameObject;
                        Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), rawPirate.GetComponent<CapsuleCollider>(), false);
                    }

                    AudioSource audio = GetComponent<AudioSource>();
                    AudioClip diggSound = gameManager.diggSound;
                    audio.clip = diggSound;
                    audio.loop = true;
                    audio.Play();

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
        Transform mainCameraTransform = mainCamera.transform;
        Vector3 mainCameraTransformPosition = mainCameraTransform.position;
        Vector3 piratePosition = transform.position;
        offset = mainCameraTransformPosition - piratePosition;
    }

    public void DoAction()
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
                        miniGameCursor = 0;
                        isMiniGame = false;
                        isStopped = false;
                        GameObject miniGame = gameManager.miniGame;
                        miniGame.SetActive(false);
                        GetComponent<Animator>().Play("Idle");
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
                    else
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
                                    AudioSource audio = GetComponent<AudioSource>();
                                    AudioClip diggSound = gameManager.diggSound;
                                    audio.clip = diggSound;
                                    audio.loop = true;
                                    audio.Play();
                                    object[] localNetworkData = new object[] { localIndex };
                                    PhotonNetwork.RaiseEvent(191, localNetworkData, true, new RaiseEventOptions
                                    {
                                        Receivers = ReceiverGroup.Others
                                    });
                                    foreach (PirateController pirate in GameObject.FindObjectsOfType<PirateController>())
                                    {
                                        GameObject rawPirate = pirate.gameObject;
                                        Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), rawPirate.GetComponent<CapsuleCollider>());
                                    }
                                    GameObject handController = leftHandController.gameObject;
                                    Rig rig = handController.GetComponent<Rig>();
                                    rig.weight = 0.0f;
                                    Transform ik = leftHandController.GetChild(0);
                                    Transform target = ik.GetChild(0); ;
                                    handController = rightHandController.gameObject;
                                    rig = handController.GetComponent<Rig>();
                                    rig.weight = 0.0f;
                                    ik = rightHandController.GetChild(0);
                                    target = ik.GetChild(0);
                                }
                                StartCoroutine(PlayAnswers());
                            }
                            isStopped = true;
                        }
                        else if (isShovelFound)
                        {
                            isMiniGame = true;
                            GameObject miniGame = gameManager.miniGame;
                            miniGame.SetActive(true);
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
                                object[] localNetworkData = new object[] { localIndex, foundedShovelPosition.x, foundedShovelPosition.y, foundedShovelPosition.z };
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
                            isStopped = true;
                            StartCoroutine(PlayAnswers());
                        }
                    }
                }
            }
        }
    }

    public void DoPaint()
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
                    bool isNotMiniGame = !isMiniGame;
                    if (isNotMiniGame)
                    {
                        if (isHavePaint)
                        {
                            isHavePaint = false;
                            if (isStandardMode)
                            {
                                PhotonNetwork.SetMasterClient(currentPlayer);
                            }
                            Quaternion baseRotation = Quaternion.identity;
                            Vector3 currentPiratePosition = transform.position;
                            float coordX = currentPiratePosition.x;
                            float coordY = currentPiratePosition.y + 0.1f;
                            float coordZ = currentPiratePosition.z;
                            Vector3 crossTrapPosition = new Vector3(coordX, coordY, coordZ);
                            
                            GameObject crossTrapInst = null;
                            if (isStandardMode)
                            {
                                crossTrapInst = PhotonNetwork.Instantiate("pirate_cross_trap", crossTrapPosition, baseRotation, 0);
                            }
                            else
                            {
                                GameObject pirateCrossTrapPrefab = gameManager.pirateCrossTrapPrefab;
                                crossTrapInst = Instantiate(pirateCrossTrapPrefab, crossTrapPosition, baseRotation);
                            }

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

                            if (isStandardMode)
                            {
                                object[] networkData = new object[] { localIndex, "Paint" };
                                PhotonNetwork.RaiseEvent(194, networkData, true, new RaiseEventOptions
                                {
                                    Receivers = ReceiverGroup.Others
                                });
                            }

                        }
                    }
                    else
                    {
                        int activeAnswer = answers[miniGameCursor];
                        bool isRight = activeAnswer == 0;
                        AudioSource audio = gameManager.GetComponent<AudioSource>();
                        if (isRight)
                        {
                            IncreaseMiniGameCursor();
                            AudioClip successSound = gameManager.successSound;
                            audio.clip = successSound;
                        }
                        else
                        {
                            AudioClip wrongSound = gameManager.wrongSound;
                            audio.clip = wrongSound;
                        }
                        audio.Play();
                    }
                }
            }
        }
    }

    public void DoAttack()
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
                    bool isNotMiniGame = !isMiniGame;
                    if (isNotMiniGame)
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
                            PirateController pirate = colliderObject.GetComponent<PirateController>();
                            bool isPirate = pirate != null;
                            if (isPirate)
                            {
                                int pirateLocalIndex = pirate.localIndex;
                                bool isEnemy = pirateLocalIndex != localIndex;
                                if (isEnemy)
                                {
                                    if (isStandardMode)
                                    {
                                        int networkId = currentPlayer.ID;
                                        PhotonView localPhotonView = colliderObject.GetComponent<PhotonView>();
                                        localPhotonView.TransferOwnership(networkId);
                                    }
                                    float coordY = 4.107f;
                                    Vector3 randomPosition = new Vector3(0, coordY, 0);
                                    List<Transform> respawnPoints = gameManager.respawnPoints;
                                    Transform respawnPoint = respawnPoints[pirateLocalIndex];
                                    randomPosition = respawnPoint.position;
                                    colliderObject.transform.position = randomPosition;
                                    if (isStandardMode)
                                    {
                                        object[] networkData = new object[] { pirateLocalIndex };
                                        PhotonNetwork.RaiseEvent(193, networkData, true, new RaiseEventOptions
                                        {
                                            Receivers = ReceiverGroup.All
                                        });
                                    }
                                    else
                                    {
                                        pirate.miniGameCursor = 0;
                                        pirate.isMiniGame = false;
                                        pirate.isStopped = false;
                                        pirate.isShovelFound = false;
                                        GameObject miniGame = pirate.gameManager.miniGame;
                                        miniGame.SetActive(false);
                                        pirate.GetComponent<Animator>().Play("Idle");
                                        if (pirate.isHaveShovel)
                                        {
                                            gameManager.GenerateShovel();
                                        }
                                        pirate.isHaveShovel = false;
                                        Vector3 origin = Vector3.zero;
                                        GameObject handController = pirate.leftHandController.gameObject;
                                        Rig rig = handController.GetComponent<Rig>();
                                        rig.weight = 0.0f;
                                        Transform ik = pirate.leftHandController.GetChild(0);
                                        Transform target = ik.GetChild(0); ;
                                        target.localPosition = origin;
                                        handController = pirate.rightHandController.gameObject;
                                        rig = handController.GetComponent<Rig>();
                                        rig.weight = 0.0f;
                                        ik = pirate.rightHandController.GetChild(0);
                                        target = ik.GetChild(0); ;
                                        target.localPosition = origin;
                                        PirateController[] pirates = GameObject.FindObjectsOfType<PirateController>();
                                        foreach (PirateController localPirate in pirates)
                                        {
                                            GameObject rawPirate = localPirate.gameObject;
                                            Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), rawPirate.GetComponent<CapsuleCollider>(), false);
                                        }
                                        AudioSource audio = pirate.GetComponent<AudioSource>();
                                        AudioClip dieSound = pirate.gameManager.dieSound;
                                        audio.clip = dieSound;
                                        audio.loop = false;
                                        audio.Play();
                                    }
                                }
                            }
                        }
                        if (isStandardMode)
                        {
                            object[] networkData = new object[] { localIndex, "Attack" };
                            PhotonNetwork.RaiseEvent(194, networkData, true, new RaiseEventOptions
                            {
                                Receivers = ReceiverGroup.Others
                            });
                        }
                    }
                    else
                    {
                        int activeAnswer = answers[miniGameCursor];
                        bool isRight = activeAnswer == 1;
                        AudioSource audio = gameManager.GetComponent<AudioSource>();
                        if (isRight)
                        {
                            IncreaseMiniGameCursor();
                            AudioClip successSound = gameManager.successSound;
                            audio.clip = successSound;
                        }
                        else
                        {
                            AudioClip wrongSound = gameManager.wrongSound;
                            audio.clip = wrongSound;
                        }
                        audio.Play();
                    }
                }
            }
        }
    }

    public void IncreaseMiniGameCursor()
    {
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

                AudioSource audio = GetComponent<AudioSource>();
                audio.Stop();

            }
            else
            {
                isStopped = false;
                isShovelFound = false;
                isHaveShovel = true;
                if (isStandardMode)
                {
                    object[] networkData = new object[] { };
                    PhotonNetwork.RaiseEvent(195, networkData, true, new RaiseEventOptions
                    {
                        Receivers = ReceiverGroup.All
                    });
                }
                else
                {
                    GameObject rawFoundedShovel = foundedShovel.gameObject;
                    Destroy(rawFoundedShovel);
                }
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
                PirateController[] pirates = GameObject.FindObjectsOfType<PirateController>();
                CapsuleCollider pirateCollider = GetComponent<CapsuleCollider>();
                foreach (PirateController pirate in pirates)
                {
                    GameObject rawPirate = pirate.gameObject;
                    CapsuleCollider somePirateCollider = rawPirate.GetComponent<CapsuleCollider>();
                    Physics.IgnoreCollision(pirateCollider, somePirateCollider, false);
                }
            }
        }
    }

    public IEnumerator PlayAnswers()
    {
        answers = new List<int>();
        int answer = UnityEngine.Random.Range(0, 2);
        answers.Add(answer);
        answer = UnityEngine.Random.Range(0, 2);
        answers.Add(answer);
        answer = UnityEngine.Random.Range(0, 2);
        answers.Add(answer);
        answer = UnityEngine.Random.Range(0, 2);
        answers.Add(answer);
        answer = UnityEngine.Random.Range(0, 2);
        answers.Add(answer);
        AudioSource localAudio = gameManager.GetComponent<AudioSource>();
        answer = answers[0];
        bool isFirstAnswer = answer == 0;
        bool isSecondAnswer = answer == 1;
        if (isFirstAnswer)
        {
            localAudio.clip = gameManager.bSound;
        }
        else if (isSecondAnswer)
        {
            localAudio.clip = gameManager.cSound;
        }
        localAudio.Play();
        yield return new WaitForSeconds(2f);
        answer = answers[1];
        isFirstAnswer = answer == 0;
        isSecondAnswer = answer == 1;
        if (isFirstAnswer)
        {
            localAudio.clip = gameManager.bSound;
        }
        else if (isSecondAnswer)
        {
            localAudio.clip = gameManager.cSound;
        }
        localAudio.Play();
        yield return new WaitForSeconds(2f);
        answer = answers[2];
        isFirstAnswer = answer == 0;
        isSecondAnswer = answer == 1;
        if (isFirstAnswer)
        {
            localAudio.clip = gameManager.bSound;
        }
        else if (isSecondAnswer)
        {
            localAudio.clip = gameManager.cSound;
        }
        localAudio.Play();
        yield return new WaitForSeconds(2f);
        answer = answers[3];
        isFirstAnswer = answer == 0;
        isSecondAnswer = answer == 1;
        if (isFirstAnswer)
        {
            localAudio.clip = gameManager.bSound;
        }
        else if (isSecondAnswer)
        {
            localAudio.clip = gameManager.cSound;
        }
        localAudio.Play();
        yield return new WaitForSeconds(2f);
        answer = answers[4];
        isFirstAnswer = answer == 0;
        isSecondAnswer = answer == 1;
        if (isFirstAnswer)
        {
            localAudio.clip = gameManager.bSound;
        }
        else if (isSecondAnswer)
        {
            localAudio.clip = gameManager.cSound;
        }
        localAudio.Play();
    }

}