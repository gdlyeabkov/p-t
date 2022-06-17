using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon;
using UnityEngine.AI;

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
                cross.transform.RotateAround(islandSphereTransformPosition, new Vector3(1f, 0f, 1f), randomRotation);
                Ray ray = new Ray(cross.transform.position, Vector3.down);
                RaycastHit hit = new RaycastHit();
                bool isDetectIsland = Physics.Raycast(ray, out hit, Mathf.Infinity);
                if (isDetectIsland)
                {
                    Vector3 hitPoint = hit.point;
                    cross.transform.position = new Vector3(hitPoint.x, hitPoint.y + 0.1f, hitPoint.z);
                }
                GenerateShovel();
            }
            int countPaints = Random.Range(0, 5);
            for (int i = 0; i < countPaints; i++)
            {
                StartCoroutine(GeneratePaint());
            }
            if (PhotonNetwork.isMasterClient)
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
                    GameObject pirate = PhotonNetwork.Instantiate("pirate", randomPosition, baseRotation, 0);
                }
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
            cross.transform.RotateAround(islandSphereTransformPosition, new Vector3(1f, 0f, 1f), randomRotation);
            Ray ray = new Ray(cross.transform.position, Vector3.down);
            RaycastHit hit = new RaycastHit();
            bool isDetectIsland = Physics.Raycast(ray, out hit, Mathf.Infinity);
            if (isDetectIsland)
            {
                Vector3 hitPoint = hit.point;
                cross.transform.position = new Vector3(hitPoint.x, hitPoint.y + 0.1f, hitPoint.z);
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
            GameObject pirate = Instantiate(piratePrefab, randomPosition, baseRotation);
            randomCoordX = Random.Range(-45, 45);
            crossTransform = cross.transform;
            crossTransformPosition = crossTransform.position;
            coordY = 6f;
            randomCoordZ = Random.Range(-45, 45);
            randomPosition = new Vector3(randomCoordX, coordY, randomCoordZ);
            respawnPoint = respawnPoints[1];
            randomPosition = respawnPoint.position;
            baseRotation = Quaternion.identity;
            pirate = Instantiate(pirateEnemyPrefab, randomPosition, baseRotation);
            bots.Add(pirate);
            Animator pirateAnimator = pirate.GetComponent<Animator>();
            pirateAnimator.Play("Walk");
            randomCoordX = Random.Range(-45, 45);
            crossTransform = cross.transform;
            crossTransformPosition = crossTransform.position;
            coordY = 6f;
            randomCoordZ = Random.Range(-45, 45);
            randomPosition = new Vector3(randomCoordX, coordY, randomCoordZ);
            respawnPoint = respawnPoints[2];
            randomPosition = respawnPoint.position;
            baseRotation = Quaternion.identity;
            pirate = Instantiate(pirateEnemyPrefab, randomPosition, baseRotation);
            bots.Add(pirate);
            pirateAnimator = pirate.GetComponent<Animator>();
            pirateAnimator.Play("Walk");
            randomCoordX = Random.Range(-45, 45);
            crossTransform = cross.transform;
            crossTransformPosition = crossTransform.position;
            coordY = 6f;
            randomCoordZ = Random.Range(-45, 45);
            randomPosition = new Vector3(randomCoordX, coordY, randomCoordZ);
            respawnPoint = respawnPoints[3];
            randomPosition = respawnPoint.position;
            baseRotation = Quaternion.identity;
            pirate = Instantiate(pirateEnemyPrefab, randomPosition, baseRotation);
            bots.Add(pirate);
            pirateAnimator = pirate.GetComponent<Animator>();
            pirateAnimator.Play("Walk");
            StartCoroutine(GiveOrder());
        }
    }

    public void ShowWin(int localIndex, int networkIndex)
    {
        bool isNotWin = !isWin;
        if (isNotWin)
        {
            isWin = true;
            mainCameraAudio.clip = winSound;
            mainCameraAudio.Play();
            object[] networkData = new object[] { localIndex, networkIndex };
            PhotonNetwork.RaiseEvent(196, networkData, true, new RaiseEventOptions
            {
                Receivers = ReceiverGroup.All
            });
        }

    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
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
        // float coordY = 4.10f;
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
        paintGo.transform.RotateAround(islandSphereTransformPosition, new Vector3(1f, 0f, 1f), randomRotation);
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
                else
                {
                    StartCoroutine(ResetGame());
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

    public IEnumerator ResetGame()
    {
        yield return new WaitForSeconds(30f);
        LeaveLobby();
    }

    public static char GetRandomCharacter(string text = "ABCDEFGHJKLMNOPRSTUVWXYZ")
    {
        System.Random rng = new System.Random();
        int index = rng.Next(text.Length);
        return text[index];
    }

    /*
    public void Update()
    {
        if (PhotonNetwork.isMasterClient)
        {
            PhotonPlayer currentPlayer = PhotonNetwork.player;
            int networkId = currentPlayer.ID;
            PhotonView localPhotonView = shovel.GetComponent<PhotonView>();
            localPhotonView.TransferOwnership(networkId);
        }
    }
    */

    public void GenerateShovel()
    {
        Vector3 shovelPosition = new Vector3(0, -0.9f, 0);
        Quaternion shovelRotation = Quaternion.Euler(90, 0, 0);
        if (isStandardMode)
        {
            shovel = PhotonNetwork.Instantiate("shovel", shovelPosition, shovelRotation, 0);
        }
        else
        {
            shovel = Instantiate(shovelPrefab, shovelPosition, shovelRotation);
        }
        float randomRotation = Random.Range(-5f, 5f);
        Vector3 islandSphereTransformPosition = islandSphereTransform.position;
        shovel.transform.RotateAround(islandSphereTransformPosition, new Vector3(1f, 1f, 0f), randomRotation);
    }

    void IncreaseMiniGameCursor()
    {
        localPirate.IncreaseMiniGameCursor();
    }

    public IEnumerator GiveOrder()
    {
        yield return new WaitForSeconds(3f);
        foreach (GameObject pirate in bots)
        {
            PirateController pirateController = pirate.GetComponent<PirateController>();
            NavMeshAgent agent = pirate.GetComponent<NavMeshAgent>();
            float target = Random.Range(0, 3);
            Vector3 destination = Vector3.zero;
            bool isCaptureCross = target == 0;
            bool isCapturePaint = target == 1;
            bool isAttack = target == 2;
            if (isCaptureCross)
            {
                if (pirateController.isHaveShovel)
                {
                    destination = cross.transform.position;
                    pirateController.agentTarget = cross.transform;
                }
                else
                {
                    destination = shovel.transform.position;
                    pirateController.agentTarget = shovel.transform;
                }
            }
            else if (isCapturePaint)
            {
                int countPaints = paints.Count;
                bool isHavePaints = countPaints >= 1;
                if (isHavePaints)
                {
                    GameObject somePaint = paints[0];
                    destination = somePaint.transform.position;
                    pirateController.agentTarget = somePaint.transform;
                }
            }
            else if (isAttack)
            {
                destination = localPirate.transform.position;
                pirateController.agentTarget = localPirate.transform;
            }
            agent.Warp(destination);
            agent.SetDestination(destination);
            pirateController.destination = destination;
        }
    }

    public void Update()
    {
        foreach (GameObject pirate in bots)
        {
            NavMeshAgent agent = pirate.GetComponent<NavMeshAgent>();
            PirateController pirateController = pirate.GetComponent<PirateController>();
            Vector3 destination = pirateController.destination;
        }
    }

}