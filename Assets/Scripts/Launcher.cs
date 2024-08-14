using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;
    private void Awake()
    {
        if (Instance)
        {
            Destroy(Instance);
            return;
        }
        Instance = this;
    }

    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] Transform roomListContent;
    [SerializeField] GameObject roomListItemPrefab;
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject playerListItemPrefab;
    [SerializeField] GameObject startGameButton;
    private static Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    private void Update()
    {
        if (!Cursor.visible)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    #region Setup
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Connected using setting file");
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
        Debug.Log("Connected to Master");
    }

    public override void OnJoinedLobby()
    {
        MenuManager.Instance.OpenMenu("Title");
        Debug.Log("Joined Lobby");
    }
    #endregion

    #region CreateRoom
    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInputField.text)) return;

        PhotonNetwork.CreateRoom(roomNameInputField.text);
        MenuManager.Instance.OpenMenu("Loading");
    }

    public override void OnJoinedRoom()
    {
        MenuManager.Instance.OpenMenu("Room");
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        foreach (Transform each in playerListContent) Destroy(each.gameObject);

        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().Setup(players[i]);
        }

        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        MenuManager.Instance.OpenMenu("Error");
        errorText.text = "CreateRoom Failed: " + message;
    }
    #endregion

    #region LeaveRoom
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.Instance.OpenMenu("Loading");
    }

    public override void OnLeftRoom()
    {
        MenuManager.Instance.OpenMenu("Title");
    }
    #endregion

    #region JoinRoom
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    // This callback only sends the changing of room list
    {
        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }

        for (int i = 0; i < roomList.Count; i++)
        {
            RoomInfo info = roomList[i];
            if (info.RemovedFromList)
            {
                cachedRoomList.Remove(info.Name);
            }
            else
            {
                cachedRoomList[info.Name] = info;
            }
        }

        foreach (KeyValuePair<string, RoomInfo> entry in cachedRoomList)
        {
            Debug.Log("name: "+entry.Value.Name+"   num of player: " + entry.Value.PlayerCount + "   status: " + entry.Value.IsOpen);
            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().Setup(cachedRoomList[entry.Key]);
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        MenuManager.Instance.OpenMenu("Error");
        errorText.text = "JoinRoom Failed: " + message;
    }

    public void JoinRoom(RoomInfo roomInfo)
    {
        PhotonNetwork.JoinRoom(roomInfo.Name);
        MenuManager.Instance.OpenMenu("Loading");

        // After loading, the callback OnJoinedRoom() will run
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().Setup(newPlayer);
    }

    public void StartGame()
    {
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel(1);
    }

        #endregion

    }