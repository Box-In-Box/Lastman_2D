using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.EventSystems;
using static Singleton;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public const byte LOGIN = 0, LOBBY = 1, ROOM = 2;

    [Header("-----LoginPanel-----")]
    public GameObject logInPanel;
    public InputField nickNameInput;

    [Header("-----LobbyPanel-----")]
    public GameObject lobbyPanel;
    public Text nickNameText;
    public Text lobbyInfoText;
    public Button[] roomBtn;
    public Button previousBtn;
    public Button nextBtn;
    public InputField RoomNameInput;
    public GameObject LobbyPlayer;
    public GameObject LobbyBlock;

    [Header("-----RoomPanel-----")]
    public GameObject roomPanel;
    public Text roomInfoText;
    public GameObject[] playerSlot;
    public Transform[] playerInstantiatePosition;
    public List<TopDown.PlayerController> players = new List<TopDown.PlayerController>();
    public Transform chatContent;
    public InputField chatInput;
    public Button gameStartBtn;

    [Header("-----ETC-----")]
    public bool inGame = false;
    public PhotonView PV;

    List<RoomInfo> myRoomList = new List<RoomInfo>();
    int currentRoomPage = 1, maxRoomPage, multiple;

    void Start()
    {
        Setting();
        
        //게임 씬에서 돌아왔을 때
        if (PhotonNetwork.IsConnected) 
            EnterRoom();
    }

    void Setting()
    {
        SetPanel(LOGIN);
        gameStartBtn.onClick.AddListener(()=> singleton.GameStart());
    }

    void Update()
    {
        if (!inGame) {
            lobbyInfoText.text = "로비 " + "<color=yellow>" + (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + "</color>" + "  /  " 
                +  " 게임접속중 " + "<color=red>" + PhotonNetwork.CountOfPlayers + "</color>";

            if(roomPanel.activeSelf == true) {
                    if (chatInput.text != "" && Input.GetKeyDown(KeyCode.Return)) {
                        MsgSend();
                        chatInput.ActivateInputField();
                        chatInput.Select();
                }
            }
        }
    }

    #region (Login <-> Lobby)
    public void Connect()
    {
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.LocalPlayer.NickName = nickNameInput.text;
    }

    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();

    public override void OnJoinedLobby()
    {
        SetPanel(LOBBY);

        LobbyPlayer.SetActive(true);
        LobbyBlock.SetActive(true);
        
        nickNameInput.text = "";
        nickNameText.text = "[ " + "<color=white>" + PhotonNetwork.LocalPlayer.NickName + "</color>" + " ]";
        myRoomList.Clear();
    }

    public void Disconnect()
    {
        SetPanel(LOGIN);

        LobbyPlayer.SetActive(false);
        LobbyBlock.SetActive(false);

        PhotonNetwork.Disconnect();
    } 

    public override void OnDisconnected(DisconnectCause cause)
    {
        SetPanel(LOGIN);

        LobbyPlayer.SetActive(false);
        LobbyBlock.SetActive(false);
    }
    #endregion

    #region RoomList
    public void RoomListClick(int num)
    {
        if (num == -2)
            --currentRoomPage;
        else if (num == -1)
            ++currentRoomPage;
        else 
            PhotonNetwork.JoinRoom(myRoomList[multiple + num].Name);
        MyRommListRenewal();
    }

    void MyRommListRenewal()
    {
        //최대페이지 설정
        maxRoomPage = (myRoomList.Count % roomBtn.Length == 0) ? myRoomList.Count / roomBtn.Length : myRoomList.Count / roomBtn.Length + 1;

        //이전, 다음버튼
        previousBtn.interactable = (currentRoomPage <= 1) ? false : true;
        nextBtn.interactable = (currentRoomPage >= maxRoomPage) ? false : true;

        multiple = (currentRoomPage - 1) * roomBtn.Length;
        for (int i = 0; i < roomBtn.Length; i++) {
            roomBtn[i].interactable = (multiple + i < myRoomList.Count) ? true : false;
            roomBtn[i].transform.GetChild(0).GetComponent<Text>().text = (multiple + i < myRoomList.Count) ? myRoomList[multiple + i].Name : "";
            roomBtn[i].transform.GetChild(1).GetComponent<Text>().text = (multiple + i < myRoomList.Count) ? myRoomList[multiple + i].PlayerCount + " / " + myRoomList[multiple + i].MaxPlayers : "";
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        int roomCount = roomList.Count;
        for (int i = 0; i < roomCount; i++) {
            if(!roomList[i].RemovedFromList) {
                if (!myRoomList.Contains(roomList[i]))
                    myRoomList.Add(roomList[i]);
                else
                    myRoomList[myRoomList.IndexOf(roomList[i])] = roomList[i];
            }
            else if (myRoomList.IndexOf(roomList[i]) != -1)
                myRoomList.RemoveAt(myRoomList.IndexOf(roomList[i]));
        }
        MyRommListRenewal();
    }
    #endregion

    #region (Lobby <-> Room)
    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();
    public override void OnJoinRandomFailed(short returnCode, string message) { RoomNameInput.text = ""; CreateRoom(); }

    public void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = true;
		roomOptions.IsOpen = true;
        roomOptions.MaxPlayers = 4;
        roomOptions.CustomRoomProperties = new Hashtable() { {"Slot_0", ""}, {"Slot_1", ""}, {"Slot_2", ""}, {"Slot_3", ""} };
        PhotonNetwork.JoinOrCreateRoom(RoomNameInput.text == "" ? "Room" + Random.Range(0, 100) : RoomNameInput.text, roomOptions, null);

        RoomNameInput.text = "";
    }
    
    public override void OnCreateRoomFailed(short returnCode, string message) { RoomNameInput.text = ""; CreateRoom(); }

    public void JoinRoom() => PhotonNetwork.JoinRoom(EventSystem.current.currentSelectedGameObject.transform.GetChild(0).GetComponent<Text>().text);

    public override void OnJoinedRoom()
    {   
        LobbyPlayer.SetActive(false);
        LobbyBlock.SetActive(false);

        singleton.SetPlayerSlot();
        PV.RPC("PrintPlayerSlot", RpcTarget.All);
        EnterRoom();
        PhotonNetwork.Instantiate("PFPlayer", new Vector2(0, 0), QI).GetComponent<TopDown.PlayerController>();
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();

        //챗 로그 삭제
        var child = chatContent.GetComponentsInChildren<Transform>();
        if (child != null) {
            for (int i = 1; i < child.Length; i++) {
                if (child[i] != transform)
                    Destroy(child[i].gameObject);
            }
        }
        
        //플레이어 닉네임 삭제
        for (int i = 0; i < playerSlot.Length; i++) {
            playerSlot[i].transform.GetChild(0).GetComponent<Text>().text = "";
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RoomRenewal();

        if (singleton.Master())
            PV.RPC("ChatRPC", RpcTarget.All, "<color=yellow>" + newPlayer.NickName + "님이 참가하셨습니다</color>");

        if (newPlayer != PhotonNetwork.LocalPlayer)
            ((GameObject)PhotonNetwork.LocalPlayer.TagObject).GetComponent<TopDown.PlayerController>().InvokeProperties();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        for (int i = 0; i < playerSlot.Length; i++)
            if (playerSlot[i].transform.GetChild(0).GetComponent<Text>().text == otherPlayer.NickName)
                playerSlot[i].transform.GetChild(0).GetComponent<Text>().text = "";

        if (singleton.Master()) {
            singleton.RemovePlayerSlot(otherPlayer);
            PV.RPC("PrintPlayerSlot", RpcTarget.All);
            PV.RPC("ChatRPC", RpcTarget.All, "<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");
        }
    }

    void EnterRoom()
    {
        SetPanel(ROOM);
        chatInput.text = ""; 
        RoomRenewal();
    }

    public void RoomRenewal()
    {
        SortPlayers();
        roomInfoText.text = PhotonNetwork.CurrentRoom.Name + " / " + "현재" + PhotonNetwork.CurrentRoom.PlayerCount + "명 / " + "최대" +PhotonNetwork.CurrentRoom.MaxPlayers + "명";
        Hashtable CP = PhotonNetwork.CurrentRoom.CustomProperties;
        Vector3 playerPosition;

        for(int i = 0; i < players.Count; i++) {
            if (CP["Slot_0"].Equals(players[i].nick)) {
                playerSlot[0].transform.GetChild(0).GetComponent<Text>().text = players[i].nick;
                playerPosition = playerInstantiatePosition[0].position;
                playerPosition.z = 0;
                players[i].gameObject.transform.position = playerPosition;
                continue;
            }
            else if (CP["Slot_1"].Equals(players[i].nick)) {
                playerSlot[1].transform.GetChild(0).GetComponent<Text>().text = players[i].nick;
                playerPosition = playerInstantiatePosition[1].position;
                playerPosition.z = 0;
                players[i].gameObject.transform.position = playerPosition;
                continue;
            }
            else if (CP["Slot_2"].Equals(players[i].nick)) {
                playerSlot[2].transform.GetChild(0).GetComponent<Text>().text = players[i].nick;
                playerPosition = playerInstantiatePosition[2].position;
                playerPosition.z = 0;
                players[i].gameObject.transform.position = playerPosition;
                continue;
            }
            else if (CP["Slot_3"].Equals(players[i].nick)) {
                playerSlot[3].transform.GetChild(0).GetComponent<Text>().text = players[i].nick;
                playerPosition = playerInstantiatePosition[3].position;
                playerPosition.z = 0;
                players[i].gameObject.transform.position = playerPosition;
                continue;
            }
        }
    }

    [PunRPC] void PrintPlayerSlot() => singleton.PrintPlayerSlot();

    #endregion

    #region Chat
    public void MsgSend()
    {
        string msg = PhotonNetwork.NickName + " : " + chatInput.text;
        PV.RPC("ChatRPC", RpcTarget.All, msg);
        chatInput.text = "";
    }

    [PunRPC]
    void ChatRPC(string msg)
    {
        GameObject chatTextGo = Instantiate(Resources.Load("chatText")) as GameObject;
        chatTextGo.transform.SetParent(chatContent, false);
        chatTextGo.GetComponent<Text>().text = msg;
    }
    #endregion

    #region ETC - 플레이어 정렬, 활성화 패널
    public void SortPlayers() => players.Sort((p1, p2) => p1.actor.CompareTo(p2.actor));

    void SetPanel(byte value)   //한 개의 패널만 활성화
    {
        switch (value) {
            case LOGIN :
                logInPanel.SetActive(true);
                lobbyPanel.SetActive(false);
                roomPanel.SetActive(false);
                break;
            case LOBBY :
                logInPanel.SetActive(false);
                lobbyPanel.SetActive(true);
                roomPanel.SetActive(false);
                break;
            case ROOM :
                logInPanel.SetActive(false);
                lobbyPanel.SetActive(false);
                roomPanel.SetActive(true);
                break;
        }
    }

    public void SettingPanel(bool isActive)
    {
        //LobbyPlayer 설정
        if (lobbyPanel.activeSelf) {
            LobbyPlayer.SetActive(isActive);
        }
        //Player 설정
        if (roomPanel.activeSelf) {
            for (int i = 0; i < players.Count; i++) {
                players[i].gameObject.SetActive(isActive);
            }
        }
    }
    #endregion
}
