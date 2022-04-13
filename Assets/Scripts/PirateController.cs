using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public float cameraRotationSpeed = 2f;

    void Start ()
    {

        Camera mainCamera = Camera.main;
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
                    KeyCode eKey = KeyCode.E;
                    bool isEKeyDown = Input.GetKeyDown(eKey);
                    KeyCode qKey = KeyCode.Q;
                    bool isQKeyDown = Input.GetKeyDown(qKey);
                    bool isEKeyUp = Input.GetKeyUp(eKey);
                    bool isEKey = Input.GetKey(eKey);
                    if (isEKeyDown)
                    {
                        if (isCrossFound)
                        {
                            bool isCrossTrap = foundedCross.isTrap;
                            if (isCrossTrap)
                            {
                                GameObject rawFoundedCross = foundedCross.gameObject;
                                AudioSource foundedCrossAudio = rawFoundedCross.GetComponent<AudioSource>();
                                foundedCrossAudio.Play();
                                object[]  networkData = new object[] { };
                                PhotonNetwork.RaiseEvent(198, networkData, true, new RaiseEventOptions
                                {
                                    Receivers = ReceiverGroup.All
                                });
                                isCrossFound = false;
                            }
                            else
                            {
                                progressCoroutine = StartCoroutine(AddProgressCoroutine());
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
                    }
                    else if (isEKey)
                    {
                        if (isCrossFound)
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
                    yaw += cameraRotationSpeed * Input.GetAxis("Mouse X");
                    pitch -= cameraRotationSpeed * Input.GetAxis("Mouse Y");
                    Vector3 cameraRotation = new Vector3(pitch, yaw, transform.eulerAngles.z);
                    Camera.main.transform.eulerAngles = cameraRotation;
                    rb.MoveRotation(Quaternion.Euler(cameraRotation.x, cameraRotation.y, cameraRotation.z));
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
                float horizontalDelta = Input.GetAxis("Horizontal");
                float verticalDelta = Input.GetAxis("Vertical");
                bool isHorizontalMotion = horizontalDelta != 0;
                bool isVerticalMotion = verticalDelta != 0;
                bool isMotion = isHorizontalMotion || isVerticalMotion;
                if (isMotion)
                {
                    int networkId = currentPlayer.ID;
                    photonView.TransferOwnership(networkId);
                }
                Vector3 m_Input = new Vector3(horizontalDelta, 0, verticalDelta);
                Vector3 currentPosition = rb.position;
                Vector3 boostMotion = m_Input * speed;
                Vector3 updatedPosition = currentPosition + boostMotion;
                rb.MovePosition(updatedPosition);
            }
        }
    }

    public void OnTriggerEnter (Collider other)
    {
        GameObject detectedObject = other.gameObject;
        string detectedObjectTag = detectedObject.tag;
        bool isCross = detectedObjectTag == "Cross";
        bool isPaint = detectedObjectTag == "Paint";
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

}