using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PistolController : MonoBehaviour
{

    public bool isOwner = false;
    public int localIndex = 0;
    public GameManager gameManager;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        GameObject rawMainCamera = mainCamera.gameObject;
        CameraTracker cameraTracker = rawMainCamera.GetComponent<CameraTracker>();
        gameManager = cameraTracker.gameManager;
        gameManager.pistolCursor++;
        int updatedPistolCursor = gameManager.pistolCursor;
        localIndex = gameManager.pistolCursor;
        PhotonNetwork.OnEventCall += OnEvent;
    }


    public void OnEvent(byte eventCode, object content, int senderId)
    {
        bool isRemovePistolEvent = eventCode == 185;
        if (isRemovePistolEvent)
        {
            try
            {
                object[] data = (object[])content;
                int index = (int)data[0];
                if (isOwner)
                {
                    bool isPistolIndexesMatches = index == localIndex;
                    if (isPistolIndexesMatches)
                    {
                        PhotonNetwork.Destroy(gameObject);
                    }
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

    void LateUpdate()
    {
        transform.LookAt(mainCamera.transform);
        transform.rotation = Quaternion.Euler(-270f, transform.eulerAngles.y, transform.eulerAngles.z);
    }

}
