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
    public bool isShovelFound = false;
    public Transform leftHandController;
    public Transform rightHandController;
    public Transform foundedShovel;
    public Transform cameraTarget;
    public List<int> answers;
    public bool isStandardMode = true;
    public Vector3 destination = Vector3.zero;
    public Transform agentTarget;
    public TextMesh numberLabel;
    public Transform numberLabelWrap;
    private Coroutine answersCoroutine;
    public float ratio = 1f;

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

            Transform armature = transform.GetChild(0);
            Transform hips = armature.GetChild(0);
            Transform spine = hips.GetChild(2);
            Transform spine1 = spine.GetChild(0);
            Transform spine2 = spine1.GetChild(0);
            Transform neck = spine2.GetChild(1);
            Transform head = neck.GetChild(0);
            StartCoroutine(SetPlayerCamera());

        }

        Transform pirateTransform = gameObject.transform;
        Transform bodyTransform = pirateTransform.GetChild(2);
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
        else if (localIndex != networkIndex)
        {
            bool isBot = localIndex >= PhotonNetwork.playerList.Length;
            if (isBot)
            {
                /*
                numberLabel.text = "Бот";
                numberLabelWrap.transform.position = new Vector3(transform.position.x, numberLabelWrap.position.y, numberLabelWrap.position.z);
                */
            }
            else
            {
                numberLabel.text = PhotonNetwork.playerList[localIndex].name;
                numberLabelWrap.transform.position = new Vector3(transform.position.x, numberLabelWrap.position.y, numberLabelWrap.position.z);
            }
        }
        
        bool isLocalBot = transform.parent != null;
        bool isHost = PhotonNetwork.isMasterClient;
        bool isNotHost = !isHost;
        bool isAddBot = isLocalBot && isNotHost;
        if (isAddBot)
        {
            gameManager.bots.Add(transform.parent.gameObject);
        }

    }

    void Update()
    {
        bool isLocalPirate = localIndex == networkIndex;
        if ((isLocalPirate && isStandardMode) || (isLocalPirate && !isStandardMode && localIndex == 0))
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

                        if (isStandardMode)
                        {
                            int networkId = currentPlayer.ID;
                            photonView.TransferOwnership(networkId);
                        }

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
                    Transform botTransform = transform.parent;
                    bool isBot = botTransform != null;
                    bool isNotBot = !isBot;
                    if (isMotion || isBot)
                    {
                        bool isPaint = animatorStateInfo.IsName("Paint");
                        bool isAttack = animatorStateInfo.IsName("Attack");
                        bool isPull = animatorStateInfo.IsName("Pull");
                        bool isIdle = animatorStateInfo.IsName("Idle");
                        bool isWalkAnim = animatorStateInfo.IsName("Walk");
                        bool isGrabIdle = animatorStateInfo.IsName("Grab_Idle");
                        bool isGrabWalk = animatorStateInfo.IsName("Grab_Walk");
                        // bool isDoWalk = !isPaint && !isAttack && !isPull && !isIdle && !isWalkAnim && !isGrabIdle && !isGrabWalk;
                        bool isDoWalk = !isPaint && !isAttack && !isPull;
                        Transform parent = transform.parent;
                        GameObject rawParent = null;
                        NavMeshAgent botController = null;
                        Vector3 botVelocity = Vector3.zero;
                        if (parent != null)
                        {
                            rawParent = parent.gameObject;
                            botController = rawParent.GetComponent<NavMeshAgent>();
                            botVelocity = botController.velocity;
                        }
                        float botVelocityZ = botVelocity.z;
                        bool isHaveVelocity = botVelocityZ != 0f;
                        bool isNotWin = !gameManager.isWin;
                        bool isPlayerWalk = isDoWalk && isNotBot && isNotWin;
                        bool isBotWalk = isDoWalk && isBot && isHaveVelocity && isNotWin;
                        bool isWalk = isPlayerWalk || isBotWalk;
                        if (isWalk)
                        {
                            if (isStandardMode)
                            {
                                int networkId = currentPlayer.ID;
                                photonView.TransferOwnership(networkId);
                            }
                            if (isBot)
                            {
                                GameObject bot = botTransform.gameObject;
                                rb = bot.GetComponent<Rigidbody>();
                            }
                            Vector3 currentPosition = rb.position;
                            Vector3 forwardDirection = Vector3.forward;
                            Animator animator = GetComponent<Animator>();
                            bool isGrab = animator.GetBool("isGrab");
                            ratio = 1f;
                            if (isGrab)
                            {
                                ratio = 0.5f;
                            }
                            float speedRatio = speed * ratio;
                            Vector3 speedforwardDirection = forwardDirection * speedRatio;
                            // Vector3 speedforwardDirection = forwardDirection * speed;
                            Vector3 boostMotion = speedforwardDirection * Time.fixedDeltaTime;
                            Vector3 localOffset = transform.TransformDirection(boostMotion);
                            Vector3 updatedPosition = currentPosition + localOffset;
                            if (isStandardMode)
                            {
                                /*object[] networkData = new object[] { localIndex, "Walk" };
                                PhotonNetwork.RaiseEvent(194, networkData, true, new RaiseEventOptions
                                {
                                    Receivers = ReceiverGroup.Others
                                });*/
                            }
                            if (isNotBot)
                            {
                                rb.MovePosition(updatedPosition);
                                if (gameManager.treasureInst != null)
                                {
                                    if (gameManager.treasureInst.GetComponent<SpringJoint>().connectedBody == GetComponent<Rigidbody>())
                                    {
                                        GetComponent<Animator>().Play("Grab_Walk");
                                        if (isStandardMode)
                                        {
                                            object[] networkData = new object[] { localIndex, "Grab_Walk" };
                                            PhotonNetwork.RaiseEvent(194, networkData, true, new RaiseEventOptions
                                            {
                                                Receivers = ReceiverGroup.Others
                                            });
                                        }
                                    }
                                    else
                                    {
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
                                else
                                {
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
                        }
                    }
                    else if (isNotBot)
                    {
                        animatorStateInfo = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
                        bool isAlreadyIdle = animatorStateInfo.IsName("Idle");
                        bool isAttack = animatorStateInfo.IsName("Attack");
                        bool isPaint = animatorStateInfo.IsName("Paint");
                        bool isLoose = animatorStateInfo.IsName("Loose");
                        bool isVictory = animatorStateInfo.IsName("Victory");
                        bool isDoIdle = !isAlreadyIdle && !isAttack && !isPaint && !isLoose && !isVictory;
                        if (isDoIdle)
                        {
                            if (gameManager.treasureInst != null)
                            {
                                if (gameManager.treasureInst.GetComponent<SpringJoint>().connectedBody == GetComponent<Rigidbody>())
                                {
                                    GetComponent<Animator>().Play("Grab_Idle");
                                    object[] networkData = new object[] { localIndex, "Grab_Idle" };
                                    PhotonNetwork.RaiseEvent(194, networkData, true, new RaiseEventOptions
                                    {
                                        Receivers = ReceiverGroup.Others
                                    });
                                }
                                else
                                {
                                    GetComponent<Animator>().Play("Idle");
                                    object[] networkData = new object[] { localIndex, "Idle" };
                                    PhotonNetwork.RaiseEvent(194, networkData, true, new RaiseEventOptions
                                    {
                                        Receivers = ReceiverGroup.Others
                                    });
                                }
                            }
                            else
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
            Transform botTransform = transform.parent;
            bool isBot = botTransform != null;
            Transform detectedObjectTransform = detectedObject.transform;
            bool isMissionComplete = agentTarget == detectedObjectTransform;
            bool isStop = isBot && isMissionComplete;
            if (isStop)
            {
                NavMeshAgent agent = transform.parent.gameObject.GetComponent<NavMeshAgent>();
                DoPaint();
                GameObject bot = botTransform.gameObject;
                gameManager.GiveOrder(bot);
            }

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
        bool isTreasureFree = eventCode == 189;
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
                    StartCoroutine(gameManager.ResetConstraints(gameManager.treasureInst));
                    agentTarget = gameManager.boats[localIndex].transform;
                    destination = gameManager.boats[localIndex].transform.position;
                    StopMiniGame();
                    isShovelFound = false;
                    Transform botTransform = transform.parent;
                    GameObject bot = botTransform.gameObject;
                    NavMeshAgent agent = bot.GetComponent<NavMeshAgent>();
                    SetIKController();
                    PirateController[] pirates = GameObject.FindObjectsOfType<PirateController>();
                    CapsuleCollider pirateCollider = GetComponent<CapsuleCollider>();
                    foreach (PirateController pirate in pirates)
                    {
                        GameObject rawPirate = pirate.gameObject;
                        CapsuleCollider localCollider = null;
                        CapsuleCollider somePirateCollider = null;
                        bool isBot = botTransform != null;
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
                    }
                    List<GameObject> bots = gameManager.bots;
                    foreach (GameObject localBot in bots)
                    {
                        int botIndex = localBot.transform.GetChild(0).gameObject.GetComponent<PirateController>().localIndex;
                        bool isOtherBot = localIndex != botIndex;
                        if (isOtherBot)
                        {
                            gameManager.GiveOrder(localBot);
                        }
                    }
                    gameManager.treasureInst.GetComponent<SpringJoint>().connectedBody = transform.parent.gameObject.GetComponent<Rigidbody>();
                    GetComponent<AudioSource>().Stop();

                }

                if (isLocalPirate)
                {
                    Transform armature = transform.GetChild(0);
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
                    // показывает сундук бота при достижении лодки - так быть не должно
                    // treasure.SetActive(true);
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
                    // gameObject.GetComponent<Animator>().Play(name);

                    if (!gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName(name))
                    {
                        gameObject.GetComponent<Animator>().Play(name);
                    }

                    GameObject handController = leftHandController.gameObject;
                    Rig rig = handController.GetComponent<Rig>();
                    rig.weight = 0.0f;
                    handController = rightHandController.gameObject;
                    rig = handController.GetComponent<Rig>();
                    rig.weight = 0.0f;

                    bool isIdle = name == "Idle";
                    if(isIdle)
                    {
                        GetComponent<AudioSource>().Stop();
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
        else if (isDieEvent)
        {
            try
            {
                object[] data = (object[])content;
                int index = (int)data[0];
                int killerIndex = (int)data[1];
                bool isLocalPirate = index == localIndex;
                bool isKiller = killerIndex == localIndex;
                if (isLocalPirate)
                {
                    StopMiniGame();
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
                    SetIKController();
                    foreach (PirateController pirate in GameObject.FindObjectsOfType<PirateController>())
                    {
                        GameObject rawPirate = pirate.gameObject;
                    }
                    AudioSource audio = GetComponent<AudioSource>();
                    AudioClip dieSound = gameManager.dieSound;
                    audio.clip = dieSound;
                    audio.loop = false;
                    audio.Play();

                    Transform armature = transform.GetChild(0);
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
                    treasure.SetActive(false);

                }
                else if (isKiller)
                {
                    if (gameManager.treasureInst != null)
                    {
                        if (transform.parent != null)
                        {
                            if (gameManager.treasureInst.GetComponent<SpringJoint>().connectedBody == transform.parent.gameObject.GetComponent<Rigidbody>())
                            {
                                Transform armature = transform.GetChild(0);
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
                            }
                        }
                        else
                        {
                            if (gameManager.treasureInst.GetComponent<SpringJoint>().connectedBody == GetComponent<Rigidbody>())
                            {
                                Transform armature = transform.GetChild(0);
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
                            }
                        }
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
        else if (isTreasureFree)
        {
            try
            {
                object[] data = (object[])content;
                int index = (int)data[0];
                bool isLocalPirate = index == localIndex;
                if (isLocalPirate)
                {
                    GetComponent<Animator>().SetBool("isGrab", true);
                    Transform armature = transform.GetChild(0);
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

                    GetComponent<AudioSource>().Stop();
                    Transform shivelTransform = rightHand.GetChild(1);
                    GameObject shovel = shivelTransform.gameObject;
                    shovel.SetActive(false);

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
        if (isLocalPirate || (!isLocalPirate && transform.parent != null))
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
                        StopMiniGame();
                        GameObject miniGame = gameManager.miniGame;
                        miniGame.SetActive(false);
                        
                        SetIKController();
                        foreach (PirateController pirate in GameObject.FindObjectsOfType<PirateController>())
                        {
                            GameObject rawPirate = pirate.gameObject.transform.gameObject;
                            Transform botTransform = transform.parent;
                            bool isBot = botTransform != null;
                            if (isBot)
                            {
                                CapsuleCollider rawPirateCollider = null;
                                Transform localBotTransform = rawPirate.transform.parent;
                                bool isLocalBot = localBotTransform != null;
                                if (isLocalBot)
                                {
                                    rawPirateCollider = localBotTransform.gameObject.GetComponent<CapsuleCollider>();
                                }
                                else
                                {
                                    rawPirateCollider = rawPirate.GetComponent<CapsuleCollider>();
                                }
                            }
                        }

                        if (answersCoroutine != null)
                        {
                            StopCoroutine(answersCoroutine);
                        }

                        GetComponent<AudioSource>().Stop();

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
                                Transform botTransform = transform.parent;
                                bool isBot = botTransform != null;
                                bool isNotBot = !isBot;
                                if (isNotBot)
                                {
                                    miniGame.SetActive(true);
                                    answersCoroutine = StartCoroutine(PlayAnswers());
                                }
                                else
                                {
                                    NavMeshAgent agent = transform.parent.gameObject.GetComponent<NavMeshAgent>();
                                    agent.Warp(cross.transform.position);
                                    StartCoroutine(GetCrossForBot());
                                }
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
                                        GameObject rawPirate = pirate.gameObject.transform.gameObject;
                                        if (isBot)
                                        {
                                            CapsuleCollider rawPirateCollider = null;
                                            Transform localBotTransform = rawPirate.transform.parent;
                                            bool isLocalBot = localBotTransform != null;
                                            if (isLocalBot)
                                            {
                                                rawPirateCollider = localBotTransform.gameObject.GetComponent<CapsuleCollider>();
                                            }
                                            else
                                            {
                                                rawPirateCollider = rawPirate.GetComponent<CapsuleCollider>();
                                            }
                                        }
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
                                isStopped = true;
                            }
                        }
                        else if (isShovelFound)
                        {
                            Transform botTransform = transform.parent;
                            bool isBot = botTransform != null;
                            isMiniGame = true;
                            GameObject miniGame = gameManager.miniGame;
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
                                    GameObject rawPirate = pirate.gameObject.transform.gameObject;
                                    if (isBot)
                                    {
                                        CapsuleCollider rawPirateCollider = null;
                                        Transform localBotTransform = rawPirate.transform.parent;
                                        bool isLocalBot = localBotTransform != null;
                                        if (isLocalBot)
                                        {
                                            rawPirateCollider = localBotTransform.gameObject.GetComponent<CapsuleCollider>();
                                        }
                                        else
                                        {
                                            rawPirateCollider = rawPirate.GetComponent<CapsuleCollider>();
                                        }
                                    }
                                }
                            }
                            isStopped = true;
                            bool isNotBot = !isBot;
                            if (isNotBot)
                            {
                                miniGame.SetActive(true);
                                answersCoroutine = StartCoroutine(PlayAnswers());
                            }
                            else
                            {
                                NavMeshAgent agent = transform.parent.gameObject.GetComponent<NavMeshAgent>();
                                agent.updatePosition = false;
                                agent.Warp(foundedShovel.position);
                                agent.updatePosition = true;
                                
                                if (gameObject.activeSelf)
                                {
                                    StartCoroutine(GetShovelForBot());
                                }

                            }
                        }
                    }
                }
            }
        }
    }

    public void DoPaint()
    {
        bool isLocalPirate = localIndex == networkIndex;
        if (isLocalPirate || (!isLocalPirate && transform.parent != null))
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
        if (isLocalPirate || (!isLocalPirate && transform.parent != null))
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

                        AnimatorStateInfo animatorStateInfo = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
                        bool isGrabWalk = animatorStateInfo.IsName("Grab_Walk");
                        bool isGrabIdle = animatorStateInfo.IsName("Grab_Idle");
                        bool isGrab = isGrabWalk || isGrabIdle;
                        bool isNotGrab = !isGrab;
                        if (isNotGrab)
                        {
                            GetComponent<Animator>().Play("Attack");

                            if (isStandardMode)
                            {
                                object[] networkData = new object[] { localIndex, "Attack" };
                                PhotonNetwork.RaiseEvent(194, networkData, true, new RaiseEventOptions
                                {
                                    Receivers = ReceiverGroup.Others
                                });
                            }
                        }

                    }
                    else if (transform.parent == null)
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
                if (gameManager.treasureInst == null)
                {
                    Vector3 treasurePosition = Vector3.zero;
                    if (gameManager.cross != null)
                    {
                        treasurePosition = gameManager.cross.transform.position;
                    }
                    else
                    {
                        treasurePosition = transform.position;
                    }
                    Quaternion baseRotation = Quaternion.identity;
                    if (isStandardMode)
                    {
                        gameManager.treasureInst = PhotonNetwork.Instantiate("treasure", treasurePosition, baseRotation, 0);
                    }
                    else
                    {
                        gameManager.treasureInst = Instantiate(gameManager.treasure, treasurePosition, baseRotation);
                        
                        Physics.IgnoreCollision(gameManager.treasureInst.GetComponent<BoxCollider>(), gameManager.localPirate.GetComponent<CapsuleCollider>(), true);

                    }
                    StartCoroutine(gameManager.ResetConstraints(gameManager.treasureInst));
                    agentTarget = gameManager.boats[localIndex].transform;
                    destination = gameManager.boats[localIndex].transform.position;
                    GetComponent<Animator>().Play("Walk");
                    StopMiniGame();
                    isShovelFound = false;
                    Transform botTransform = transform.parent;
                    GameObject bot = null;
                    NavMeshAgent agent = null;
                    if (botTransform)
                    {
                        bot = botTransform.gameObject;
                        agent = bot.GetComponent<NavMeshAgent>();
                    }
                    SetIKController();
                    PirateController[] pirates = GameObject.FindObjectsOfType<PirateController>();
                    CapsuleCollider pirateCollider = GetComponent<CapsuleCollider>();
                    foreach (PirateController pirate in pirates)
                    {
                        GameObject rawPirate = pirate.gameObject;
                        CapsuleCollider localCollider = null;
                        CapsuleCollider somePirateCollider = null;
                        bool isBot = botTransform != null;
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
                        Physics.IgnoreCollision(localCollider, somePirateCollider, false);
                    }
                    List<GameObject> bots = gameManager.bots;
                    foreach (GameObject localBot in bots)
                    {
                        int botIndex = localBot.transform.GetChild(0).gameObject.GetComponent<PirateController>().localIndex;
                        bool isOtherBot = localIndex != botIndex;
                        if (isOtherBot)
                        {
                            gameManager.GiveOrder(localBot);
                        }
                    }
                    gameManager.treasureInst.GetComponent<SpringJoint>().connectedBody = GetComponent<Rigidbody>();
                    GetComponent<AudioSource>().Stop();

                    Transform localArmature = transform.GetChild(0);
                    Transform localHips = localArmature.GetChild(0);
                    Transform localSpine = localHips.GetChild(2);
                    Transform localSpine1 = localSpine.GetChild(0);
                    Transform localSpine2 = localSpine1.GetChild(0);
                    Transform localRightSholder = localSpine2.GetChild(2);
                    Transform localRightArm = localRightSholder.GetChild(0);
                    Transform localRightForeArm = localRightArm.GetChild(0);
                    Transform localRightHand = localRightForeArm.GetChild(0);
                    Transform treasureTransform = localRightHand.GetChild(2);
                    GameObject treasure = treasureTransform.gameObject;
                    treasure.SetActive(true);

                }
                else if (transform.parent != null)
                {
                    gameObject.GetComponent<Animator>().Play("Idle");
                }
                
                object[] networkData = new object[] { localIndex };
                PhotonNetwork.RaiseEvent(189, networkData, true, new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.All
                });

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
                        Receivers = ReceiverGroup.MasterClient
                    });
                }
                else
                {
                    GameObject rawFoundedShovel = foundedShovel.gameObject;
                    Destroy(rawFoundedShovel);
                }
                SetIKController();
                PirateController[] pirates = GameObject.FindObjectsOfType<PirateController>();
                CapsuleCollider pirateCollider = GetComponent<CapsuleCollider>();
                foreach (PirateController pirate in pirates)
                {
                    GameObject rawPirate = pirate.gameObject;
                    CapsuleCollider localCollider = null;
                    CapsuleCollider somePirateCollider = null;
                    if (transform.parent != null)
                    {
                        localCollider = transform.parent.gameObject.GetComponent<CapsuleCollider>();
                    }
                    else
                    {
                        localCollider = GetComponent<CapsuleCollider>();
                    }
                    if (rawPirate.transform.parent != null)
                    {
                        somePirateCollider = rawPirate.transform.parent.gameObject.GetComponent<CapsuleCollider>();
                    }
                    else
                    {
                        somePirateCollider = rawPirate.GetComponent<CapsuleCollider>();
                    }
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

    public IEnumerator GetShovelForBot()
    {
        yield return new WaitForSeconds(10f);
        StopMiniGame();
        isShovelFound = false;
        GetComponent<Animator>().Play("Idle");

        object[] networkData = new object[] { localIndex, "Idle" };
        PhotonNetwork.RaiseEvent(194, networkData, true, new RaiseEventOptions
        {
            Receivers = ReceiverGroup.Others
        });

        Transform botTransform = transform.parent;
        GameObject bot = botTransform.gameObject;
        NavMeshAgent agent = bot.GetComponent<NavMeshAgent>();
        if (foundedShovel != null && agent.isOnNavMesh)
        {
            isHaveShovel = true;
            GameObject rawFoundedShovel = foundedShovel.gameObject;
            
            PhotonNetwork.RaiseEvent(195, networkData, true, new RaiseEventOptions
            {
                Receivers = ReceiverGroup.MasterClient
            });

        }
        SetIKController();
        PirateController[] pirates = GameObject.FindObjectsOfType<PirateController>();
        CapsuleCollider pirateCollider = GetComponent<CapsuleCollider>();
        foreach (PirateController pirate in pirates)
        {
            GameObject rawPirate = pirate.gameObject;
            CapsuleCollider localCollider = null;
            CapsuleCollider somePirateCollider = null;
            if (transform.parent != null)
            {
                localCollider = transform.parent.gameObject.GetComponent<CapsuleCollider>();
            }
            else
            {
                localCollider = GetComponent<CapsuleCollider>();
            }
            if (rawPirate.transform.parent != null)
            {
                somePirateCollider = rawPirate.transform.parent.gameObject.GetComponent<CapsuleCollider>();
            }
            else
            {
                somePirateCollider = rawPirate.GetComponent<CapsuleCollider>();
            }
        }
        // gameManager.GiveOrder(transform.parent.gameObject);
        // gameManager.GiveOrders();
    }

    public IEnumerator GetCrossForBot()
    {
        yield return new WaitForSeconds(10f);
        if (isStandardMode)
        {

            object[] networkData = null;

            if (gameManager.treasureInst == null)
            {
                Vector3 treasurePosition = gameManager.cross.transform.position;
                Quaternion baseRotation = Quaternion.identity;
                bool isHost = PhotonNetwork.isMasterClient;
                if (isHost)
                {
                    gameManager.treasureInst = PhotonNetwork.Instantiate("treasure", treasurePosition, baseRotation, 0);
                }
                StartCoroutine(gameManager.ResetConstraints(gameManager.treasureInst));
                agentTarget = gameManager.boats[localIndex].transform;
                destination = gameManager.boats[localIndex].transform.position;

                if (gameManager.treasureInst.GetComponent<SpringJoint>().connectedBody == GetComponent<Rigidbody>())
                {
                    GetComponent<Animator>().Play("Grab_Walk");
                    networkData = new object[] { localIndex, "Grab_Walk" };
                    PhotonNetwork.RaiseEvent(194, networkData, true, new RaiseEventOptions
                    {
                        Receivers = ReceiverGroup.Others
                    });
                }
                else
                {
                    GetComponent<Animator>().Play("Walk");
                    networkData = new object[] { localIndex, "Walk" };
                    PhotonNetwork.RaiseEvent(194, networkData, true, new RaiseEventOptions
                    {
                        Receivers = ReceiverGroup.Others
                    });
                }
                StopMiniGame();
                isShovelFound = false;
                Transform botTransform = transform.parent;
                GameObject bot = botTransform.gameObject;
                NavMeshAgent agent = bot.GetComponent<NavMeshAgent>();
                SetIKController();
                PirateController[] pirates = GameObject.FindObjectsOfType<PirateController>();
                CapsuleCollider pirateCollider = GetComponent<CapsuleCollider>();
                foreach (PirateController pirate in pirates)
                {
                    GameObject rawPirate = pirate.gameObject;
                    CapsuleCollider localCollider = null;
                    CapsuleCollider somePirateCollider = null;
                    bool isBot = botTransform != null;
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
                    Physics.IgnoreCollision(localCollider, somePirateCollider, false);
                }
                List<GameObject> bots = gameManager.bots;
                foreach (GameObject localBot in bots)
                {
                    int botIndex = localBot.transform.GetChild(0).gameObject.GetComponent<PirateController>().localIndex;
                    bool isOtherBot = localIndex != botIndex;
                    if (isOtherBot)
                    {
                        gameManager.GiveOrder(localBot);
                    }
                }
                gameManager.treasureInst.GetComponent<SpringJoint>().connectedBody = transform.parent.gameObject.GetComponent<Rigidbody>();
                GetComponent<AudioSource>().Stop();
            }
            
            networkData = new object[] { localIndex };
            PhotonNetwork.RaiseEvent(189, networkData, true, new RaiseEventOptions
            {
                Receivers = ReceiverGroup.All
            });

        }
        else if (gameManager.treasureInst == null)
        {
            Vector3 treasurePosition = gameManager.cross.transform.position;
            Quaternion baseRotation = Quaternion.identity;
            gameManager.treasureInst = Instantiate(gameManager.treasure, treasurePosition, baseRotation);
            StartCoroutine(gameManager.ResetConstraints(gameManager.treasureInst));
            agentTarget = gameManager.boats[localIndex].transform;
            destination = gameManager.boats[localIndex].transform.position;
            GetComponent<Animator>().Play("Walk");
            StopMiniGame();
            isShovelFound = false;
            Transform botTransform = transform.parent;
            GameObject bot = botTransform.gameObject;
            NavMeshAgent agent = bot.GetComponent<NavMeshAgent>();
            SetIKController();
            PirateController[] pirates = GameObject.FindObjectsOfType<PirateController>();
            CapsuleCollider pirateCollider = GetComponent<CapsuleCollider>();
            foreach (PirateController pirate in pirates)
            {
                GameObject rawPirate = pirate.gameObject;
                CapsuleCollider localCollider = null;
                CapsuleCollider somePirateCollider = null;
                bool isBot = botTransform != null;
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
            }
            List<GameObject> bots = gameManager.bots;
            foreach (GameObject localBot in bots)
            {
                int botIndex = localBot.transform.GetChild(0).gameObject.GetComponent<PirateController>().localIndex;
                bool isOtherBot = localIndex != botIndex;
                if (isOtherBot)
                {
                    gameManager.GiveOrder(localBot);
                }
            }
            gameManager.treasureInst.GetComponent<SpringJoint>().connectedBody = transform.parent.gameObject.GetComponent<Rigidbody>();
            GetComponent<AudioSource>().Stop();
        }
        else
        {
            gameManager.GiveOrder(transform.parent.gameObject);
            transform.parent.GetChild(0).gameObject.GetComponent<Animator>().Play("Idle");
        }
    }

    public void ComputeDamage()
    {
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
            PirateController pirate = null;
            if (gameObject.activeSelf && colliderObject.activeSelf)
            {
                bool isBot = colliderObject.GetComponent<NavMeshAgent>();
                if (isBot)
                {
                    Transform botTransform = colliderObject.transform;
                    Transform pirateTransform = botTransform.GetChild(0);
                    GameObject rawPirate = pirateTransform.gameObject;
                    pirate = rawPirate.GetComponent<PirateController>();
                }
                else
                {
                    pirate = colliderObject.GetComponent<PirateController>();
                }
            }
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

                    NavMeshAgent agent = colliderObject.GetComponent<NavMeshAgent>();
                    bool isAgentExists = agent != null;
                    bool isObjectActive = colliderObject.activeSelf;
                    if (gameManager.treasureInst != null)
                    {
                        Rigidbody body = null;
                        if (transform.parent != null)
                        {
                            body = transform.parent.gameObject.GetComponent<Rigidbody>();
                        }
                        else
                        {
                            body = GetComponent<Rigidbody>();
                        }
                        bool isEnemyTreasureOwner = gameManager.treasureInst.GetComponent<SpringJoint>().connectedBody == body;
                        if (isEnemyTreasureOwner)
                        {
                            if (isAgentExists && isObjectActive)
                            {
                                colliderObject.transform.GetChild(0).gameObject.GetComponent<Animator>().SetBool("isGrab", false);
                            }
                            else if (isObjectActive)
                            {
                                colliderObject.GetComponent<Animator>().SetBool("isGrab", false);
                            }
                        }
                    }
                    if (isAgentExists && isObjectActive)
                    {
                        colliderObject.transform.GetChild(0).gameObject.GetComponent<PirateController>().StartCoroutine(RespawnPirate(colliderObject.transform.GetChild(0).gameObject, randomPosition));
                    }
                    else if (isObjectActive)
                    {
                        colliderObject.GetComponent<PirateController>().StartCoroutine(RespawnPirate(colliderObject, randomPosition));
                    }

                    if (isStandardMode)
                    {
                        object[] networkData = new object[] { pirateLocalIndex, localIndex };
                        PhotonNetwork.RaiseEvent(193, networkData, true, new RaiseEventOptions
                        {
                            Receivers = ReceiverGroup.All
                        });

                        if (gameManager.treasureInst != null)
                        {
                            Rigidbody body = null;
                            if (transform.parent != null)
                            {
                                body = transform.parent.gameObject.GetComponent<Rigidbody>();
                            }
                            else
                            {
                                body = GetComponent<Rigidbody>();
                            }
                            bool isEnemyTreasureOwner = gameManager.treasureInst.GetComponent<SpringJoint>().connectedBody == body;
                            if (isEnemyTreasureOwner)
                            {
                                if (transform.parent != null)
                                {
                                    gameManager.treasureInst.GetComponent<SpringJoint>().connectedBody = transform.parent.gameObject.GetComponent<Rigidbody>();
                                }
                                else
                                {
                                    gameManager.treasureInst.GetComponent<SpringJoint>().connectedBody = GetComponent<Rigidbody>();
                                }
                                destination = gameManager.boats[localIndex].transform.position;
                                agentTarget = gameManager.boats[localIndex].transform;

                                Transform localArmature = transform.GetChild(0);
                                Transform localHips = localArmature.GetChild(0);
                                Transform localSpine = localHips.GetChild(2);
                                Transform localSpine1 = localSpine.GetChild(0);
                                Transform localSpine2 = localSpine1.GetChild(0);
                                Transform localRightSholder = localSpine2.GetChild(2);
                                Transform localRightArm = localRightSholder.GetChild(0);
                                Transform localRightForeArm = localRightArm.GetChild(0);
                                Transform localRightHand = localRightForeArm.GetChild(0);
                                Transform treasureTransform = localRightHand.GetChild(2);
                                GameObject treasure = treasureTransform.gameObject;
                                treasure.SetActive(false);

                            }
                        }

                    }
                    else
                    {
                        StopMiniGame();
                        pirate.isShovelFound = false;
                        GameObject miniGame = pirate.gameManager.miniGame;
                        miniGame.SetActive(false);
                        bool isHaveShovel = pirate.isHaveShovel;
                        if (isHaveShovel && gameManager.treasureInst == null)
                        {
                            gameManager.GenerateShovel();
                        }
                        pirate.isHaveShovel = false;
                        SetIKController();
                        PirateController[] pirates = GameObject.FindObjectsOfType<PirateController>();
                        foreach (PirateController localPirate in pirates)
                        {
                            GameObject rawPirate = localPirate.gameObject;
                            CapsuleCollider localCollider = null;
                            CapsuleCollider pirateCollider = null;
                            bool isPirateBot = transform.parent != null;
                            if (isPirateBot)
                            {
                                localCollider = transform.parent.gameObject.GetComponent<CapsuleCollider>();
                            }
                            else
                            {
                                localCollider = GetComponent<CapsuleCollider>();
                            }
                            if (rawPirate.transform.parent != null)
                            {
                                pirateCollider = rawPirate.transform.parent.gameObject.GetComponent<CapsuleCollider>();
                            }
                            else
                            {
                                pirateCollider = rawPirate.GetComponent<CapsuleCollider>();
                            }
                        }
                        AudioSource audio = pirate.GetComponent<AudioSource>();
                        AudioClip dieSound = pirate.gameManager.dieSound;
                        audio.clip = dieSound;
                        audio.loop = false;
                        audio.Play();

                        GetComponent<Animator>().Play("Idle");
                        gameManager.GiveOrder(colliderObject);
                        if (gameManager.treasureInst != null)
                        {
                            if (transform.parent != null)
                            {
                                gameManager.treasureInst.GetComponent<SpringJoint>().connectedBody = transform.parent.gameObject.GetComponent<Rigidbody>();
                            }
                            else
                            {
                                gameManager.treasureInst.GetComponent<SpringJoint>().connectedBody = GetComponent<Rigidbody>();
                            }
                        }

                    }
                }
            }
        }
        /*
        if (isStandardMode)
        {
            object[] networkData = new object[] { localIndex, "Attack" };
            PhotonNetwork.RaiseEvent(194, networkData, true, new RaiseEventOptions
            {
                Receivers = ReceiverGroup.Others
            });
        }
        */

        Transform pirateArmature = transform.GetChild(0);
        Transform pirateHips = pirateArmature.GetChild(0);
        Transform pirateSpine = pirateHips.GetChild(2);
        Transform pirateSpine1 = pirateSpine.GetChild(0);
        Transform pirateSpine2 = pirateSpine1.GetChild(0);
        Transform saberTransform = pirateSpine2.GetChild(0);
        GameObject saber = saberTransform.gameObject;
        saber.SetActive(false);

    }

    public IEnumerator RespawnPirate (GameObject colliderObject, Vector3 randomPosition)
    {
        GameObject pirate = colliderObject;
        if (colliderObject.transform.parent != null)
        {
            pirate = colliderObject.transform.parent.gameObject;
            pirate.GetComponent<NavMeshAgent>().enabled = false;
        }
        pirate.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        for (int i = 0; i < colliderObject.transform.childCount; i++)
        {
            colliderObject.transform.GetChild(i).gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(10f);
        colliderObject.GetComponent<PirateController>().StopAllCoroutines();
        if (colliderObject.transform.parent != null)
        {
            pirate.GetComponent<NavMeshAgent>().enabled = true;
            pirate.GetComponent<NavMeshAgent>().nextPosition = randomPosition;
        }
        else
        {
            colliderObject.transform.position = randomPosition;
        }
        pirate.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        for (int i = 0; i < colliderObject.transform.childCount; i++)
        {
            colliderObject.transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    public IEnumerator SetPlayerCamera()
    {
        yield return new WaitForSeconds(5f);
        Transform armature = transform.GetChild(0);
        Transform hips = armature.GetChild(0);
        Transform spine = hips.GetChild(2);
        Transform spine1 = spine.GetChild(0);
        Transform spine2 = spine1.GetChild(0);
        Transform neck = spine2.GetChild(1);
        Transform head = neck.GetChild(0);
        gameManager.viewCamera.Follow = head;
        gameManager.viewCamera.LookAt = head;
        gameManager.isInit = true;

        // gameManager.shovel.GetComponent<BoxCollider>().isTrigger = false;

        List<GameObject> bots = gameManager.bots;
        foreach (GameObject localBot in bots)
        {
            List<Transform> respawnPoints = gameManager.respawnPoints;
            GameObject pirate = localBot.transform.GetChild(0).gameObject;
            int pirateLocalIndex = pirate.GetComponent<PirateController>().localIndex;
            Transform respawnPoint = respawnPoints[pirateLocalIndex];
            Vector3 randomPosition = respawnPoint.position;
            localBot.GetComponent<NavMeshAgent>().nextPosition = randomPosition;
        }

    }

    public void SetIKController()
    {
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
    }

    public void StopMiniGame ()
    {
        miniGameCursor = 0;
        isMiniGame = false;
        isStopped = false;
    }

}