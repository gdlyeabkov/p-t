using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShovelController : MonoBehaviour
{

    void Start()
    {
        PhotonNetwork.OnEventCall += OnEvent;
    }

    public void OnEvent(byte eventCode, object content, int senderId)
    {
        bool isRemoveShovelEvent = eventCode == 195;
        if (isRemoveShovelEvent)
        {
            try
            {
                object[] data = (object[])content;
                
                PhotonPlayer currentPlayer = PhotonNetwork.player;
                PhotonNetwork.SetMasterClient(currentPlayer);
                
                PhotonNetwork.Destroy(gameObject);
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
