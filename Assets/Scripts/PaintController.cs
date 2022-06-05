using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintController : MonoBehaviour
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
        gameManager.paintCursor++;
        int updatedPaintCursor = gameManager.paintCursor;
        localIndex = gameManager.paintCursor;
        PhotonNetwork.OnEventCall += OnEvent;
    }


    public void OnEvent(byte eventCode, object content, int senderId)
    {
        bool isRemovePaintEvent = eventCode == 197;
        if (isRemovePaintEvent)
        {
            try
            {
                object[] data = (object[])content;
                int index = (int)data[0];
                if (isOwner)
                {
                    bool isPaintIndexesMatches = index == localIndex;
                    if (isPaintIndexesMatches)
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
