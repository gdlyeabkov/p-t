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
            // SetPlayerName(SystemInfo.deviceName);
			string nickName = PlayerPrefs.GetString("nickName");
            SetPlayerName(nickName);
            PhotonNetwork.player.NickName = playerName;
            string roomName = GetComponent<Text>().text.Split(new string[] { ":" }, StringSplitOptions.None)[0];
            PhotonNetwork.JoinRoom(roomName);
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