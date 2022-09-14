using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShovelController : MonoBehaviour
{

    public GameManager gameManager;

    void Start()
    {
        PhotonNetwork.OnEventCall += OnEvent;
        Camera mainCamera = Camera.main;
        GameObject rawMainCamera = mainCamera.gameObject;
        CameraTracker cameraTracker = rawMainCamera.GetComponent<CameraTracker>();
        gameManager = cameraTracker.gameManager;
    }

    public void OnEvent(byte eventCode, object content, int senderId)
    {
        bool isRemoveShovelEvent = eventCode == 195;
        bool isShovelPlacementEvent = eventCode == 190;
        if (isRemoveShovelEvent)
        {
            try
            {
                object[] data = (object[])content;

                PhotonPlayer currentPlayer = PhotonNetwork.player;
                PhotonNetwork.SetMasterClient(currentPlayer);

                gameManager.DestroyShovel(gameObject);

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

            Debug.LogWarning("isRemoveShovelEvent");

        }
        else if (isShovelPlacementEvent)
        {
            try
            {
                object[] data = (object[])content;
                float randomRotation = (float)data[0];
                Vector3 islandSphereTransformPosition = Vector3.zero;
                if (gameManager != null)
                {
                    Transform islandSphereTransform = gameManager.islandSphereTransform;
                    islandSphereTransformPosition = islandSphereTransform.position;
                }
                Vector3 shovelRotationAxes = new Vector3(1f, 1f, 0f);
                gameObject.GetComponent<BoxCollider>().enabled = false;
                transform.RotateAround(islandSphereTransformPosition, shovelRotationAxes, randomRotation);
                gameObject.GetComponent<BoxCollider>().enabled = true;
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
            Debug.LogWarning("isShovelPlacementEvent");
        }
    }

}
