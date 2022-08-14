using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon;

public class RoomController : UnityEngine.MonoBehaviour
{

    string playerName;

    public void SetPlayerName(string name)
    {
        playerName = name;
    }

    public void JoinToRoom()
    {
        if (PhotonNetwork.connected)
        {
            /*
            SetPlayerName(SystemInfo.deviceName);
            PhotonNetwork.player.NickName = playerName;
            */
            
            SetPlayerName(PlayerPrefs.GetString("nickName"));
            PhotonNetwork.player.NickName = playerName;

            string roomName = GetComponent<Text>().text.Split(new string[] { ":" }, StringSplitOptions.None)[0];
            
            try
            {
                PhotonNetwork.JoinRoom(roomName);

                NetworkController networkController = GameObject.FindObjectOfType<NetworkController>();
                networkController.buttonJoinedArena.GetComponent<Button>().interactable = false;

            }
            catch
            {
                Debug.Log("Комната заполнена");
            }


        }
    }

    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn.enabled)
        {
            btn.onClick.AddListener(JoinToRoom);
        }
    }

}