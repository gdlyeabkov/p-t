using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossController : MonoBehaviour
{

    public bool isTrap = false;
    public bool isOwner = false;

    void Start ()
    {
        PhotonNetwork.OnEventCall += OnEvent;
    }

    public void OnEvent(byte eventCode, object content, int senderId)
    {
        bool isRemoveCrossTrapEvent = eventCode == 198;
        if (isRemoveCrossTrapEvent)
        {
            try
            {
                object[] data = (object[])content;
                if (isOwner)
                {
                    PhotonNetwork.Destroy(gameObject);
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
