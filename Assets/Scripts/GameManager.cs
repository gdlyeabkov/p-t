using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon;

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

    void Start()
    {

        mainCamera = Camera.main;
        mainCameraAudio = mainCamera.GetComponent<AudioSource>();
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
            
            // cross = PhotonNetwork.Instantiate("cross", crossBasePosition, baseRotation, 0);
            cross = PhotonNetwork.Instantiate("pirateCross", crossBasePosition, baseRotation, 0);
            
            float randomCoordX = 0f;
            Transform crossTransform = cross.transform;
            Vector3 crossTransformPosition = crossTransform.position;
            // float coordY = 4.107f;
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
                GameObject pirate = PhotonNetwork.Instantiate("color_pirate_all_anims_4 Variant", randomPosition, baseRotation, 0);
            }
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
        PhotonNetwork.LeaveRoom();
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
        GameObject paintGo = PhotonNetwork.Instantiate("paint", randomPosition, paintRotation, 0);
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
        shovel = PhotonNetwork.Instantiate("shovel", shovelPosition, shovelRotation, 0);
        float randomRotation = Random.Range(-5f, 5f);
        Vector3 islandSphereTransformPosition = islandSphereTransform.position;
        shovel.transform.RotateAround(islandSphereTransformPosition, new Vector3(1f, 1f, 0f), randomRotation);
    }

}
