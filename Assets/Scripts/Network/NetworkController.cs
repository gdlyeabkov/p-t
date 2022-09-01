using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Photon;

public class NetworkController : PunBehaviour
{

	public string gameVersion = "1";
	public int maxCountPlayers = 2;
	public GameObject playerRoom;
	public Coroutine checkLobby;
	public GameObject players;
	public Text playersCount;
	public int playerCursor;
	public GameObject playerRoomPrefab;
	public Button buttonJoinedArena;
	public string playerName;
	public string roomName;
	public GameObject rooms;
	public GameObject emptyPrefab;
	public InputField roomNameField;
	public int countPlayers = 0;
	public InputField nickNameField;

	public void Start()
	{
		// PlayerPrefs.DeleteAll();
		PlayerPrefs.DeleteKey("ShowAd");
		PlayerPrefs.DeleteKey("Mode");
		ConnectToPhoton();
		LoadNickName();
		PhotonNetwork.OnEventCall += OnEvent;
	}

	void ConnectToPhoton()
	{
		PhotonNetwork.gameVersion = gameVersion;
		PhotonNetwork.ConnectUsingSettings(gameVersion);
	}

	public void LoadArena()
	{
		// PhotonNetwork.LoadLevel("SampleScene");
		PhotonNetwork.LoadLevel("Main");
	}
	public override void OnConnectedToMaster()
	{
		checkLobby = StartCoroutine(CheckLobby());
		PhotonNetwork.JoinLobby();
	}

	public IEnumerator CheckLobby()
	{
		while (true)
		{
			if (PhotonNetwork.insideLobby)
			{
				OnReceivedRoomListUpdate();
				StopCoroutine(checkLobby);
				yield return null;
			}
			else if (!PhotonNetwork.insideLobby && PhotonNetwork.connectionStateDetailed == ClientState.ConnectedToGameserver)
			{
				PhotonNetwork.JoinLobby();
			}
			yield return new WaitForSeconds(1f);
		}
	}

	public void JoinRoom()
	{
		try
		{
			Debug.Log("PhotonNetwork.connectionStateDetailed: " + PhotonNetwork.connectionStateDetailed.ToString());
			if (PhotonNetwork.connected && PhotonNetwork.connectionStateDetailed == ClientState.JoinedLobby)
			{
				RoomInfo roomInfo = PhotonNetwork.room;
				bool isRoomExist = roomInfo != null;
				if (isRoomExist)
				{
					int countPlayersOnline = roomInfo.playerCount + 1;
					string parsedCountPlayersOnline = countPlayersOnline.ToString();
				}

				/*
				 *  лучше здесь этого не делать а делать в OnJoinedRoom() так как JoinRoom() вызывается только при создании комнаты но не при подключении к ней
				 *	string playerName = SystemInfo.deviceName;
				 *	SetPlayerName(playerName);
				 *	PhotonNetwork.player.NickName = playerName;
				 */
				Debug.Log("PhotonNetwork.IsConnected! | Trying to Create/Join Room " + roomNameField.text);

				List<RoomInfo> roomList = PhotonNetwork.GetRoomList().ToList();
				RoomInfo room = roomList.FirstOrDefault(r => r.Name == roomName);
				bool isRoomNotExists = room == null;
				if (isRoomNotExists)
                {
					RoomOptions roomOptions = new RoomOptions();
					byte maxPlayers = ((byte)(maxCountPlayers));
					roomOptions.maxPlayers = maxPlayers;
					countPlayers = roomOptions.maxPlayers;
					TypedLobby typedLobby = new TypedLobby(roomName, LobbyType.Default);
					SetRoomName(roomNameField.text);
					PhotonNetwork.CreateRoom(roomName, roomOptions, typedLobby);
				}
			}
		}
		catch
		{
			Debug.Log("ошибка создание комнаты");
		}
	}

	public override void OnReceivedRoomListUpdate()
	{
		for (int roomIndex = 0; roomIndex < rooms.transform.childCount; roomIndex++)
		{
			Destroy(rooms.transform.GetChild(roomIndex).gameObject);

			rooms.GetComponent<RectTransform>().sizeDelta = new Vector2(rooms.GetComponent<RectTransform>().sizeDelta.x, rooms.GetComponent<RectTransform>().sizeDelta.y - 30);

		}
		// if (PhotonNetwork.GetRoomList().Length >= 1)
		bool isHaveRooms = PhotonNetwork.GetRoomList().Count((RoomInfo someRoom) =>
		{
			int someRoomCountPlayers = someRoom.playerCount;
			bool isRoomNotFull = someRoomCountPlayers < maxCountPlayers;
			return isRoomNotFull;
		}) >= 1;
		if (isHaveRooms)
		{
			for (int roomIndex = 0; roomIndex < PhotonNetwork.countOfRooms; roomIndex++)
			{
				GameObject roomInst = Instantiate(emptyPrefab, new Vector2(0f, 0f), Quaternion.identity);
				try
				{
					roomInst.GetComponent<Button>().enabled = true;
					string parsedMaxCountPlayers = maxCountPlayers.ToString();
					RoomInfo room = PhotonNetwork.GetRoomList()[roomIndex];
					string nameOfRoom = room.Name;
					string roomName = room.Name;
					int roomCountPlayersOnline = room.PlayerCount;
					string parsedRoomCountPlayersOnline = roomCountPlayersOnline.ToString();
					roomInst.GetComponent<Text>().text = nameOfRoom + ": " + parsedRoomCountPlayersOnline + "/" + parsedMaxCountPlayers;
					roomInst.transform.parent = rooms.transform;
					roomInst.transform.localScale = new Vector2(1f, 1f);
					roomInst.transform.localPosition = new Vector3(0f, 0f, 0f);

					bool isFirstRoom = roomIndex == 0;
					bool isNotFirstRoom = !isFirstRoom;
					if (isNotFirstRoom)
                    {
						rooms.GetComponent<RectTransform>().sizeDelta = new Vector2(rooms.GetComponent<RectTransform>().sizeDelta.x, rooms.GetComponent<RectTransform>().sizeDelta.y + 30);
					}

				}
				catch (System.Exception e)
				{
					Debug.Log("ошибка при обновлении комнат " + e.Message);
				}
				roomInst.GetComponent<RectTransform>().sizeDelta = new Vector2(rooms.GetComponent<RectTransform>().sizeDelta.x / 2, roomInst.GetComponent<RectTransform>().sizeDelta.y);
			}
		}
		// else if (PhotonNetwork.GetRoomList().Length <= 0)
		else
		{
			GameObject roomInst = Instantiate(emptyPrefab, new Vector2(0f, 0f), Quaternion.identity);
			roomInst.GetComponent<Text>().text = "Room list is empty...";
			roomInst.transform.parent = rooms.transform;
			roomInst.GetComponent<RectTransform>().sizeDelta = new Vector2(rooms.GetComponent<RectTransform>().sizeDelta.x / 2, roomInst.GetComponent<RectTransform>().sizeDelta.y);
			roomInst.transform.localScale = new Vector2(1f, 1f);
			roomInst.transform.localPosition = new Vector3(0f, 0f, 0f);
		
			roomInst.GetComponent<Button>().interactable = false;

		}
	}

	public void SetPlayerName(string name)
	{
		playerName = name;
	}

	public void SetRoomName(string name)
	{
		roomName = name;
	}

	public override void OnJoinedRoom()
	{

		string nickName = nickNameField.text;
		int nickNameLength = nickName.Length;
		bool isNickNameExists = nickNameLength <= 0;
		if (isNickNameExists)
        {
			string generatedName = SystemInfo.deviceName;
			SetPlayerName(generatedName);
			PhotonNetwork.player.NickName = generatedName;
		}

		playerRoom.SetActive(true);
		int countPlayersOnline = PhotonNetwork.room.playerCount;
		string parsedCountPlayersOnline = countPlayersOnline.ToString();
		string parsedMaxCountPlayers = maxCountPlayers.ToString();
		string playersCountContent = parsedCountPlayersOnline + "/" + parsedMaxCountPlayers;
		playersCount.text = playersCountContent;
		int switchedPlayerIndex = -1;
		for (int playerIndex = PhotonNetwork.room.playerCount - 1; playerIndex >= 0; playerIndex--)
		{
			switchedPlayerIndex++;
			GameObject playerInst = Instantiate(playerRoomPrefab, new Vector2(0f, 0f), Quaternion.identity);
			Transform playerInstContainer = playerInst.transform;
			GameObject playerInstLabel = playerInstContainer.GetChild(0).gameObject;
			playerInstLabel.GetComponent<Text>().text = PhotonNetwork.playerList[playerIndex].name;
			string switchedNickname = PhotonNetwork.playerList[playerIndex].name;
			PhotonPlayer currentPlayer = PhotonNetwork.player;
			string currentPlayerName = currentPlayer.NickName;
			Text textedPlayerInstLabel = playerInstLabel.GetComponent<Text>();
			string textedPlayerInstLabelContent = textedPlayerInstLabel.text;
			playerInstLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(228f, playerInst.GetComponent<RectTransform>().sizeDelta.y);
			playerInst.transform.parent = players.transform;
			playerInst.transform.localScale = new Vector2(1f, 1f);
			playerInst.transform.localPosition = new Vector3(0f, 0f, 0f);

			// players.GetComponent<RectTransform>().sizeDelta = new Vector2(players.GetComponent<RectTransform>().sizeDelta.x, players.GetComponent<RectTransform>().sizeDelta.y + 35);

		}
		PlayerPrefs.SetInt("PlayerIndex", PhotonNetwork.room.playerCount);
		playerCursor = PhotonNetwork.room.playerCount - 1;

		ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();
		customProperties.Add("index", playerCursor);
		PhotonNetwork.player.SetCustomProperties(customProperties);

		// buttonJoinedArena.GetComponent<Button>().interactable = false;

	}

	public override void OnLeftRoom()
	{
		for (int playerIndex = 1; playerIndex < players.transform.childCount; playerIndex++)
		{
			Transform playersTransform = players.transform;
			Transform playersTransformChild = playersTransform.GetChild(playerIndex);
			GameObject rawPlayersTransformChild = playersTransformChild.gameObject;
			Destroy(rawPlayersTransformChild);
		}
		string parsedMaxCountPlayers = maxCountPlayers.ToString();
		Text textedPlayersCount = playersCount.GetComponent<Text>();
		string textedPlayersCountMessage = "1/" + parsedMaxCountPlayers;
		textedPlayersCount.text = textedPlayersCountMessage;
		checkLobby = StartCoroutine(CheckLobby());
	}

	public void ExitFromRoom()
	{
		PhotonNetwork.LeaveRoom();
		playerRoom.SetActive(false);
		checkLobby = StartCoroutine(CheckLobby());
	}

	public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
	{
			playerRoom.SetActive(true);
		
		int countPlayersOnline = PhotonNetwork.room.playerCount;
		string parsedCountPlayersOnline = countPlayersOnline.ToString();
		string parsedMaxCountPlayers = maxCountPlayers.ToString();
		string playersCountContent = parsedCountPlayersOnline + "/" + parsedMaxCountPlayers;
		playersCount.text = playersCountContent;

		if (PhotonNetwork.room.playerCount >= countPlayers)
		{
			bool isRoot = PhotonNetwork.isMasterClient;
			if (isRoot)
			{
				buttonJoinedArena.interactable = true;
			}
		}

		for (int playerIndex = 1; playerIndex < players.transform.childCount; playerIndex++)
		{
			Destroy(players.transform.GetChild(playerIndex).gameObject);
		}
		int switchedPlayerIndex = -1;
		for (int playerIndex = PhotonNetwork.room.playerCount - 1; playerIndex >= 0; playerIndex--)
		{
			switchedPlayerIndex++;
			GameObject playerInst = Instantiate(playerRoomPrefab, new Vector2(0f, 0f), Quaternion.identity);
			Transform playerInstContainer = playerInst.transform;
			GameObject playerInstLabel = playerInstContainer.GetChild(0).gameObject;
			playerInstLabel.GetComponent<Text>().text = PhotonNetwork.playerList[playerIndex].name;
			string switchedNickname = PhotonNetwork.playerList[playerIndex].name;
			playerInstLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(228f, playerInst.GetComponent<RectTransform>().sizeDelta.y);
			playerInst.transform.parent = players.transform;
			playerInst.transform.localScale = new Vector2(1f, 1f);
			playerInst.transform.localPosition = new Vector3(0f, 0f, 0f);
		
			// players.GetComponent<RectTransform>().sizeDelta = new Vector2(players.GetComponent<RectTransform>().sizeDelta.x, players.GetComponent<RectTransform>().sizeDelta.y + 35);

		}

		players.GetComponent<RectTransform>().sizeDelta = new Vector2(players.GetComponent<RectTransform>().sizeDelta.x, players.GetComponent<RectTransform>().sizeDelta.y + 35);

	}

	void Awake()
	{
		PhotonNetwork.automaticallySyncScene = true;
	}

	public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
	{
		int countPlayersOnline = PhotonNetwork.room.playerCount;
		string parsedCountPlayersOnline = countPlayersOnline.ToString();
		bool isRoomFull = countPlayersOnline >= countPlayers;
		bool isRoomAlonePlayer = PhotonNetwork.room.playerCount <= 1;
		string parsedMaxCountPlayers = maxCountPlayers.ToString();
		Text playersCountLabel = playersCount.GetComponent<Text>();
		string playersCountLabelMessage = "";
		if (isRoomFull)
		{
			playersCountLabelMessage = parsedCountPlayersOnline + "/" + parsedMaxCountPlayers;
			playersCountLabel.text = playersCountLabelMessage;
		}
		else if (isRoomAlonePlayer)
		{
			playersCountLabelMessage = "1/" + parsedMaxCountPlayers;
			playersCountLabel.text = playersCountLabelMessage;
		}
		int allPlayers = players.transform.childCount;
		for (int playerIndex = 1; playerIndex < allPlayers; playerIndex++)
		{
			Transform playersTransform = players.transform;
			Transform playersTransformChildTransform = playersTransform.GetChild(playerIndex);
			GameObject playersTransformChild = playersTransformChildTransform.gameObject;
			Transform sameRawPlayersTransformChildTransform = playersTransformChild.transform;
			Transform rawPlayersTransformChildTransformLabel = sameRawPlayersTransformChildTransform.GetChild(0);
			GameObject rawPlayersTransformChildTransform = rawPlayersTransformChildTransformLabel.gameObject;
			Text playersTransformChildLabel = rawPlayersTransformChildTransform.GetComponent<Text>();
			string playersTransformChildLabelContent = playersTransformChildLabel.text;
			string otherPlayerName = otherPlayer.name;
			bool isNamesMatch = playersTransformChildLabelContent.Contains(otherPlayerName);
			if (isNamesMatch)
			{
				Destroy(playersTransformChild);
				break;
			}
		}

		players.GetComponent<RectTransform>().sizeDelta = new Vector2(players.GetComponent<RectTransform>().sizeDelta.x, players.GetComponent<RectTransform>().sizeDelta.y - 35);

		buttonJoinedArena.GetComponent<Button>().interactable = false;

	}

	public void OnEvent(byte eventCode, object content, int senderId)
	{
		
	}

	public void RefreshRooms()
	{
		PhotonNetwork.JoinLobby();
	}

	public void DoTrain()
    {
		PlayerPrefs.SetString("Mode", "Train");
		// PhotonNetwork.LoadLevel("SampleScene");
		PhotonNetwork.LoadLevel("Main");
	}

	public void SetNickName()
    {
		string nickName = nickNameField.text;
		SetPlayerName(nickName);
		PhotonNetwork.player.NickName = nickName;
		PlayerPrefs.SetString("nickName", nickName);
	}

	public void LoadNickName()
	{
		bool isNickNameExists = PlayerPrefs.HasKey("nickName");
		if (isNickNameExists)
        {
			string nickName = PlayerPrefs.GetString("nickName");
			SetPlayerName(nickName);
			PhotonNetwork.player.NickName = nickName;
			nickNameField.text = nickName;
		}
	}

}