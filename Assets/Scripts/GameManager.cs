using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public GameObject localPirate;

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
            Vector3 crossBasePosition = new Vector3(0f, 0.01f, 0f);
            Quaternion baseRotation = Quaternion.identity;
            cross = PhotonNetwork.Instantiate("cross", crossBasePosition, baseRotation, 0);
            float randomCoordX = Random.Range(-45, 45);
            Transform crossTransform = cross.transform;
            Vector3 crossTransformPosition = crossTransform.position;
            float coordY = crossTransformPosition.y;
            float randomCoordZ = Random.Range(-45, 45);
            Vector3 crossPosition = new Vector3(randomCoordX, coordY, randomCoordZ);
            cross.transform.Translate(crossPosition);
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
                float coordY = crossTransformPosition.y;
                float randomCoordZ = Random.Range(-45, 45);
                Vector3 randomPosition = new Vector3(randomCoordX, coordY, randomCoordZ);
                Quaternion baseRotation = Quaternion.identity;
                PhotonNetwork.Instantiate("custom_pirate", randomPosition, baseRotation, 0);
            }
        }

    }

    public void ShowWin (int localIndex, int networkIndex)
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

    public void LeaveLobby ()
    {
        PhotonNetwork.LeaveRoom();
        Application.LoadLevel("Lobby");
    }

    public IEnumerator GeneratePaint ()
    {
        yield return new WaitForSeconds(5f);
        float randomCoordX = Random.Range(-45, 45);
        float coordY = 1f;
        float randomCoordZ = Random.Range(-45, 45);
        Vector3 randomPosition = new Vector3(randomCoordX, coordY, randomCoordZ);
        Quaternion paintRotation = Quaternion.Euler(270f, 0f, 0f);
        GameObject paintGo = PhotonNetwork.Instantiate("paint", randomPosition, paintRotation, 0);
        PaintController paintController = paintGo.GetComponent<PaintController>();
        paintController.isOwner = true;
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

    public IEnumerator ResetGame ()
    {
        yield return new WaitForSeconds(30f);
        LeaveLobby();
    }

}
