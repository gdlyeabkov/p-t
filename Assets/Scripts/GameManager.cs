﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using Photon;
using Cinemachine;

public class GameManager : PunBehaviour
{

    public GameObject cross;
    public int digProgress = 0;
    public AudioClip winSound;
    public AudioClip looseSound;
    public GameObject treasure;
    public bool isWin = false;
    public GameObject paint;
    public Material paintMaterial;
    public int piratesCursor = -1;
    public int paintCursor = -1;
    public int globalNetworkIndex = 0;
    private Camera mainCamera;
    private AudioSource mainCameraAudio;
    public PirateController localPirate;
    public Transform islandSphereTransform;
    public List<Transform> respawnPoints;
    public GameObject miniGame;
    public Text miniGameLabel;
    public GameObject shovel = null;
    public LayerMask islandLayer;
    public List<Material> playerMaterials;
    public AudioClip dieSound;
    public AudioClip diggSound;
    public Joystick movementJoystick;
    public Joystick rotationJoystick;
    public AdController adController;
    public AudioClip aSound;
    public AudioClip bSound;
    public AudioClip cSound;
    public AudioClip wrongSound;
    public AudioClip successSound;
    public GameObject pirateCrossPrefab;
    public GameObject piratePrefab;
    public GameObject shovelPrefab;
    public bool isStandardMode = true;
    public GameObject paintPrefab;
    public GameObject pirateCrossTrapPrefab;
    public GameObject pirateEnemyPrefab;
    public List<GameObject> bots;
    public List<GameObject> paints;
    public CinemachineVirtualCamera viewCamera;
    public float maxSpeed = 0f;
    public GameObject treasureInst;
    public List<GameObject> boats;
    public bool isInit = false;

    void Start()
    {
        mainCamera = Camera.main;
        mainCameraAudio = mainCamera.GetComponent<AudioSource>();
        bool isNotStandardMode = PlayerPrefs.HasKey("Mode");
        isStandardMode = !isNotStandardMode;
        if (isStandardMode)
        {
            PhotonNetwork.OnEventCall += OnEvent;
            PhotonPlayer currentPlayer = PhotonNetwork.player;
            ExitGames.Client.Photon.Hashtable customProperties = currentPlayer.CustomProperties;
            object rawCustomPropertiesIndex = customProperties["index"];
            globalNetworkIndex = ((int)(rawCustomPropertiesIndex));
            bool isHost = PhotonNetwork.isMasterClient;
            if (isHost)
            {
                Vector3 crossBasePosition = new Vector3(0f, -0.9f, 0f);
                Quaternion baseRotation = Quaternion.identity;
                cross = PhotonNetwork.Instantiate("pirateCross", crossBasePosition, baseRotation, 0);
                float randomCoordX = 0f;
                Transform crossTransform = cross.transform;
                Vector3 crossTransformPosition = crossTransform.position;
                float coordY = -0.9f;
                float randomCoordZ = 0f;
                Vector3 crossPosition = new Vector3(randomCoordX, coordY, randomCoordZ);
                float randomRotation = Random.Range(-5, 5);
                Vector3 islandSphereTransformPosition = islandSphereTransform.position;
                Vector3 crossRotationAxes = new Vector3(1f, 0f, 1f);
                cross.transform.RotateAround(islandSphereTransformPosition, crossRotationAxes, randomRotation);
                Vector3 updatedCrossPosition = cross.transform.position;
                Vector3 downDirection = Vector3.down;
                Ray ray = new Ray(updatedCrossPosition, downDirection);
                RaycastHit hit = new RaycastHit();
                float infinityLength = Mathf.Infinity;
                bool isDetectIsland = Physics.Raycast(ray, out hit, infinityLength);
                if (isDetectIsland)
                {
                    Vector3 hitPoint = hit.point;
                    float hitPointX = hitPoint.x;
                    float hitPointY = hitPoint.y;
                    float neededHitPointY = hitPointY + 0.1f;
                    float hitPointZ = hitPoint.z;
                    cross.transform.position = new Vector3(hitPointX, neededHitPointY, hitPointZ);
                }
                GenerateShovel();
            }
            int countPaints = Random.Range(0, 5);
            for (int i = 0; i < countPaints; i++)
            {
                StartCoroutine(GeneratePaint());
            }
            if (isHost)
            {
                /*
                    * кеширование в переменную ниже приводит к тому, что пираты вообще не создаются
                    * RoomInfo room = PhotonNetwork.room;
                    * int countPlayers = room.playerCount;
                */
                for (int i = 0; i < PhotonNetwork.room.playerCount; i++)
                {
                    float randomCoordX = Random.Range(-45, 45);
                    Transform crossTransform = cross.transform;
                    Vector3 crossTransformPosition = crossTransform.position;
                    float coordY = 6f;
                    float randomCoordZ = Random.Range(-45, 45);
                    Vector3 randomPosition = new Vector3(randomCoordX, coordY, randomCoordZ);
                    Transform respawnPoint = respawnPoints[i];
                    randomPosition = respawnPoint.position;
                    Quaternion baseRotation = Quaternion.identity;
                    GameObject pirate = PhotonNetwork.Instantiate("fixed_pirate", randomPosition, baseRotation, 0);
                }
                
                CreateNetworkBots();

            }
        }
        else
        {
            bots = new List<GameObject>();
            globalNetworkIndex = 0;
            Vector3 crossBasePosition = new Vector3(0f, -0.9f, 0f);
            Quaternion baseRotation = Quaternion.identity;
            cross = Instantiate(pirateCrossPrefab, crossBasePosition, baseRotation);
            float randomCoordX = 0f;
            Transform crossTransform = cross.transform;
            Vector3 crossTransformPosition = crossTransform.position;
            float coordY = -0.9f;
            float randomCoordZ = 0f;
            Vector3 crossPosition = new Vector3(randomCoordX, coordY, randomCoordZ);
            float randomRotation = Random.Range(-5, 5);
            Vector3 islandSphereTransformPosition = islandSphereTransform.position;
            Vector3 crossRotationAxes = new Vector3(1f, 0f, 1f);
            cross.transform.RotateAround(islandSphereTransformPosition, crossRotationAxes, randomRotation);
            Vector3 updatedCrossPosition = cross.transform.position;
            Vector3 downDirection = Vector3.down;
            Ray ray = new Ray(updatedCrossPosition, downDirection);
            RaycastHit hit = new RaycastHit();
            float infinityLength = Mathf.Infinity;
            bool isDetectIsland = Physics.Raycast(ray, out hit, infinityLength);
            if (isDetectIsland)
            {
                Vector3 hitPoint = hit.point;
                float hitPointX = hitPoint.x;
                float hitPointY = hitPoint.y;
                float neededHitPointY = hitPointY + 0.1f;
                float hitPointZ = hitPoint.z;
                cross.transform.position = new Vector3(hitPointX, neededHitPointY, hitPointZ);
            }
            GenerateShovel();
            int countPaints = Random.Range(0, 5);
            for (int i = 0; i < countPaints; i++)
            {
                StartCoroutine(GeneratePaint());
            }
            randomCoordX = Random.Range(-45, 45);
            crossTransform = cross.transform;
            crossTransformPosition = crossTransform.position;
            coordY = 6f;
            randomCoordZ = Random.Range(-45, 45);
            Vector3 randomPosition = new Vector3(randomCoordX, coordY, randomCoordZ);
            Transform respawnPoint = respawnPoints[0];
            randomPosition = respawnPoint.position;
            baseRotation = Quaternion.identity;
            Instantiate(piratePrefab, randomPosition, baseRotation);
        }
    }
    
    public void ShowWin(int localIndex, int networkIndex)
    {
        bool isNotWin = !isWin;
        if (isNotWin)
        {
            isWin = true;

            /*
            bool isCrossFound = localPirate.isCrossFound;
            if (isCrossFound)
            {
                mainCameraAudio.clip = winSound;
                mainCameraAudio.Play();
            }
            else
            {
                mainCameraAudio.clip = looseSound;
                mainCameraAudio.Play();
            }
            object[] networkData = new object[] { localIndex, networkIndex };
            PhotonNetwork.RaiseEvent(196, networkData, true, new RaiseEventOptions
            {
                Receivers = ReceiverGroup.All
            });
            
            GameObject rawObject = null;
            if (isStandardMode)
            {
                rawObject = localPirate.gameObject;
            }
            else
            {
                // int botIndex = localIndex - 1;
                int botIndex = localIndex;
                GameObject bot = bots[botIndex];
                rawObject = bot;
            }

            Transform rawObjectTransform = rawObject.transform;
            Vector3 currentPosition = rawObjectTransform.position;
            float coordX = currentPosition.x;
            float coordY = currentPosition.y;
            float verticalOffset = 1f;
            float coordZ = currentPosition.z;
            coordY += verticalOffset;
            Vector3 treasurePosition = new Vector3(coordX, coordY, coordZ);
            Quaternion baseRotation = Quaternion.identity;
            StartCoroutine(ResetGame());
            */

            mainCameraAudio.clip = winSound;
            mainCameraAudio.Play();
            if (isStandardMode)
            {
                object[] networkData = new object[] { localIndex, networkIndex };
                PhotonNetwork.RaiseEvent(196, networkData, true, new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.All
                });
            }
            else
            {
                bool isLooser = networkIndex != localIndex;
                if (isLooser)
                {
                    mainCameraAudio.clip = looseSound;
                    mainCameraAudio.Play();
                    localPirate.GetComponent<Animator>().Play("Loose");
                }
                StartCoroutine(ResetGame());
            }

        }
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        adController.ScheduleAd();
        LeaveLobby();
    }

    public void LeaveLobby()
    {
        if (isStandardMode)
        {
            PhotonNetwork.LeaveRoom();
        }
        Application.LoadLevel("Lobby");
    }

    public IEnumerator GeneratePaint()
    {
        yield return new WaitForSeconds(5f);
        float randomCoordX = 0f;
        float coordY = -0.9f;
        float randomCoordZ = 0f;
        Vector3 randomPosition = new Vector3(randomCoordX, coordY, randomCoordZ);
        Quaternion paintRotation = Quaternion.Euler(270f, 0f, 0f);
        GameObject paintGo = null;
        if (isStandardMode)
        {
            paintGo = PhotonNetwork.Instantiate("paint", randomPosition, paintRotation, 0);
        }
        else
        {
            paintGo = Instantiate(paintPrefab, randomPosition, paintRotation);
        }
        PaintController paintController = paintGo.GetComponent<PaintController>();
        paintController.isOwner = true;
        float randomRotation = Random.Range(-5f, 5f);
        Vector3 islandSphereTransformPosition = islandSphereTransform.position;
        Vector3 paintRotationAxes = new Vector3(1f, 0f, 1f);
        paintGo.transform.RotateAround(islandSphereTransformPosition, paintRotationAxes, randomRotation);

        paints.Add(paintGo);

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
                bool isLooser = globalNetworkIndex != localNetworkIndex;
                if (isLooser)
                {
                    mainCameraAudio.clip = looseSound;
                    mainCameraAudio.Play();
                    localPirate.GetComponent<Animator>().Play("Loose");
                    int localPirateIndex = localPirate.localIndex;
                    object[] networkData = new object[] { localPirateIndex, "Loose" };
                    PhotonNetwork.RaiseEvent(194, networkData, true, new RaiseEventOptions
                    {
                        Receivers = ReceiverGroup.Others
                    });

                }
                /*
                else
                {
                    StartCoroutine(ResetGame());
                }
                */

                foreach (GameObject localBot in bots)
                {
                    Transform localBotTransform = localBot.transform;
                    Transform localPirateTransform = localBotTransform.GetChild(0);
                    GameObject localPirate = localPirateTransform.gameObject;
                    PirateController localPirateController = localPirate.GetComponent<PirateController>();
                    int localPirateIndex = localPirateController.localIndex;
                    bool isBotLooser = localPirateIndex != index;
                    if (isBotLooser)
                    {
                        Animator localPirateAnimator = localPirate.GetComponent<Animator>();
                        localPirateAnimator.Play("Loose");
                        NavMeshAgent botController = localBot.GetComponent<NavMeshAgent>();
                        bool isOnNavMesh = botController.isOnNavMesh;
                        if (isOnNavMesh)
                        {
                            botController.isStopped = true;
                        }
                        viewCamera.Follow = null;
                    }
                }
                isWin = true;

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

    public IEnumerator ResetGame()
    {
        yield return new WaitForSeconds(30f);
        adController.ScheduleAd();
        LeaveLobby();
    }

    public static char GetRandomCharacter(string text = "ABCDEFGHJKLMNOPRSTUVWXYZ")
    {
        System.Random rng = new System.Random();
        int index = rng.Next(text.Length);
        return text[index];
    }
    
    public void GenerateShovel()
    {
        Vector3 shovelPosition = new Vector3(0, -0.9f, 0);
        Quaternion shovelRotation = Quaternion.Euler(90, 0, 0);

        if (isStandardMode)
        {
            shovel = PhotonNetwork.Instantiate("fixed_shovel", shovelPosition, shovelRotation, 0);
        }
        else
        {
            shovel = Instantiate(shovelPrefab, shovelPosition, shovelRotation);
        }

        float randomRotation = Random.Range(-5f, 5f);
        Vector3 islandSphereTransformPosition = islandSphereTransform.position;
        Vector3 shovelRotationAxes = new Vector3(1f, 1f, 0f);
        shovel.transform.RotateAround(islandSphereTransformPosition, shovelRotationAxes, randomRotation);

        if (isStandardMode)
        {
            StartCoroutine(SyncShovelPlacement(randomRotation));
            /*object[] networkData = new object[] { randomRotation };
            PhotonNetwork.RaiseEvent(190, networkData, true, new RaiseEventOptions
            {
                Receivers = ReceiverGroup.All
            });*/
        }

        shovel.GetComponent<ShovelController>().gameManager = this;

    }

    public void DoAction()
    {
        localPirate.DoAction();
    }

    public void DoPaint()
    {
        localPirate.DoPaint();
    }

    public void DoAttack()
    {
        localPirate.DoAttack();
    }

    public void IncreaseMiniGameCursor()
    {
        localPirate.IncreaseMiniGameCursor();
    }

    public IEnumerator GiveOrders()
    {
        yield return new WaitForSeconds(3f);
        foreach (GameObject pirateWrap in bots)
        {
            GiveOrder(pirateWrap);
        }
    }

    public void GiveOrder (GameObject pirateWrap)
    {
        Transform pirateWrapTransform = pirateWrap.transform;
        Transform pirateTransform = pirateWrapTransform.GetChild(0);
        GameObject pirate = pirateTransform.gameObject;
        PirateController pirateController = pirate.GetComponent<PirateController>();
        NavMeshAgent agent = pirateWrap.GetComponent<NavMeshAgent>();
        float target = Random.Range(0, 3);
        Vector3 destination = Vector3.zero;
        bool isCaptureCross = target == 0;
        bool isCapturePaint = target == 1;
        bool isAttack = target == 2;
        if (isCaptureCross)
        {
            bool isHaveShovel = pirateController.isHaveShovel;
            bool isShovelExists = shovel != null;
            if (isHaveShovel)
            {
                Transform agentTarget = cross.transform;
                destination = agentTarget.position;
                pirateController.agentTarget = agentTarget;
            }
            else if (isShovelExists)
            {
                Transform agentTarget = shovel.transform;
                destination = agentTarget.position;
                pirateController.agentTarget = agentTarget;
            }
        }
        else if (isCapturePaint)
        {
            int countPaints = paints.Count;
            bool isHavePaints = countPaints >= 1;
            if (isHavePaints)
            {
                GameObject somePaint = paints[0];
                bool isPaintExists = somePaint != null;
                if (isPaintExists)
                {
                    Transform agentTarget = somePaint.transform;
                    destination = agentTarget.position;
                    pirateController.agentTarget = agentTarget;
                }
            }
            else
            {
                GiveOrder(pirateWrap);
            }
        }
        else if (isAttack)
        {
            Transform agentTarget = localPirate.transform;
            destination = agentTarget.position;
            pirateController.agentTarget = agentTarget;
        }
        if (!isInit)
        {
            agent.Warp(destination);
        }
        pirateController.destination = destination;
    }

    public void Update()
    {
        foreach (GameObject pirateWrap in bots)
        {
            Transform pirateWrapTransform = pirateWrap.transform;
            Transform pirateTransform = pirateWrapTransform.GetChild(0);
            GameObject pirate = pirateTransform.gameObject;
            NavMeshAgent agent = pirateWrap.GetComponent<NavMeshAgent>();
            Animator pirateAnimator = pirate.GetComponent<Animator>();
            AnimatorStateInfo animatorStateInfo = pirateAnimator.GetCurrentAnimatorStateInfo(0);
            bool isPull = animatorStateInfo.IsName("Pull");
            bool isDig = animatorStateInfo.IsName("Dig");
            bool isAttack = animatorStateInfo.IsName("Attack");
            // bool isStop = isWin || isDig || isPull;
            bool isStop = isWin || isDig || isPull || isAttack;
            if (isStop)
            {
                agent.speed = 0;
                agent.angularSpeed = 0;
                agent.acceleration = 0;
                agent.updatePosition = false;
                PirateController pirateController = pirate.GetComponent<PirateController>();
                Transform foundedShovel = pirateController.foundedShovel;
                bool isShovelExists = foundedShovel != null;
                bool isSyncPullPosition = isPull && isShovelExists;
                if (isSyncPullPosition)
                {
                    Vector3 foundedShovelPosition = foundedShovel.position;
                    agent.Warp(foundedShovelPosition);
                }
                else if (isDig)
                {
                    bool isCrossExists = cross != null;
                    if (isCrossExists)
                    {
                        Transform crossTransform = cross.transform;
                        Vector3 crossPosition = crossTransform.position;
                        agent.Warp(crossPosition);
                    }
                }
            }
            else
            {
                agent.updatePosition = true;
                PirateController pirateController = pirate.GetComponent<PirateController>();
                Transform agentTarget = pirateController.agentTarget;
                bool isTargetExists = agentTarget != null;
                bool isOnNavMesh = agent.isOnNavMesh;
                // bool isUpdateBot = isTargetExists && isOnNavMesh;
                bool isUpdateBot = (isTargetExists && isOnNavMesh) || (pirateController.networkIndex != 0);
                if (isUpdateBot)
                {
                    agent.speed = 1;
                    agent.angularSpeed = 30;
                    agent.acceleration = 30;
                    NavMeshPath path = new NavMeshPath();
                    if (isOnNavMesh)
                    {
                        if (agentTarget != null)
                        {
                            Vector3 destination = agentTarget.position;
                            agent.CalculatePath(destination, path);
                            agent.ResetPath();
                            agent.SetPath(path);
                            Vector3 yAxis = Vector3.up;
                            Rigidbody pirateWrapRB = pirateWrap.GetComponent<Rigidbody>();
                            Vector3 velocity = agent.velocity;
                            Quaternion lookRotation = Quaternion.LookRotation(velocity, yAxis);
                            pirate.transform.rotation = lookRotation;
                        }
                    }

                    // pirate.GetComponent<Animator>().Play("Walk");
                    if (!animatorStateInfo.IsName("Pull") && !animatorStateInfo.IsName("Attack") && !animatorStateInfo.IsName("Dig"))
                    {
                        if (agent.speed > 0f)
                        {
                            pirateAnimator.Play("Walk");
                        }
                    }
                    else if (pirateController.agentTarget == null && globalNetworkIndex == 0)
                    {
                        pirateAnimator.Play("Idle");
                    }

                }
            }

            pirateWrapTransform.GetChild(0).localPosition = Vector3.zero;

        }
    }

    public IEnumerator ResetConstraints(GameObject someTreasure)
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            someTreasure.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        }
    }

    public void CreateNetworkBots()
    {
        bots = new List<GameObject>();
        Vector3 crossBasePosition = new Vector3(0f, -0.9f, 0f);
        Quaternion baseRotation = Quaternion.identity;
        float randomCoordX = 0f;
        Transform crossTransform = cross.transform;
        Vector3 crossTransformPosition = crossTransform.position;
        float coordY = -0.9f;
        float randomCoordZ = 0f;
        Vector3 crossPosition = new Vector3(randomCoordX, coordY, randomCoordZ);
        float randomRotation = Random.Range(-5, 5);
        Vector3 islandSphereTransformPosition = islandSphereTransform.position;
        cross.transform.RotateAround(islandSphereTransformPosition, new Vector3(1f, 0f, 1f), randomRotation);
        Ray ray = new Ray(cross.transform.position, Vector3.down);
        RaycastHit hit = new RaycastHit();
        bool isDetectIsland = Physics.Raycast(ray, out hit, Mathf.Infinity);
        if (isDetectIsland)
        {
            Vector3 hitPoint = hit.point;
            cross.transform.position = new Vector3(hitPoint.x, hitPoint.y + 0.1f, hitPoint.z);
        }
        int countPaints = Random.Range(0, 5);
        for (int i = 0; i < countPaints; i++)
        {
            StartCoroutine(GeneratePaint());
        }

        for (int i = PhotonNetwork.room.playerCount; i < 4; i++)
        {
            baseRotation = Quaternion.identity;
            randomCoordX = Random.Range(-45, 45);
            crossTransform = cross.transform;
            crossTransformPosition = crossTransform.position;
            coordY = 6f;
            randomCoordZ = Random.Range(-45, 45);
            Vector3 randomPosition = new Vector3(randomCoordX, coordY, randomCoordZ);
            Transform respawnPoint = respawnPoints[1];
            randomPosition = respawnPoint.position;
            baseRotation = Quaternion.identity;
            GameObject pirateWrap = PhotonNetwork.Instantiate("fixedatePirateEnemyWrapResourse", randomPosition, baseRotation, 0);
            NavMeshAgent agent = pirateWrap.GetComponent<NavMeshAgent>();
            agent.speed = 0.1f;
            bots.Add(pirateWrap);
        }
        StartCoroutine(GiveOrders());
    }

    public IEnumerator SyncShovelPlacement (float randomRotation)
    {
        yield return new WaitForSeconds(10f);
        object[] networkData = new object[] { randomRotation };
        PhotonNetwork.RaiseEvent(190, networkData, true, new RaiseEventOptions
        {
            // Receivers = ReceiverGroup.All
            Receivers = ReceiverGroup.Others
        });
    }

}
