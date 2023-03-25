using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.EventSystems;
using static Singleton;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public const byte LOGIN = 0, LOBBY = 1, ROOM = 2;

    [Header("-----LoginPanel-----")]
    public GameObject logInPanel;
    public InputField nickNameInput;

    [Header("-----LobbyPanel-----")]
    public GameObject lobbyPanel;
    public Text wellcomText;
    public Text lobbyInfoText;
    public Button[] roomBtn;
    public Button previousBtn;
    public Button nextBtn;
    public InputField RoomNameInput;

    [Header("-----RoomPanel-----")]
    public GameObject roomPanel;
    public Text roomInfoText;
    public GameObject[] playerSlot;
    public List<PlayerManager> players = new List<PlayerManager>();
    public PlayerManager myPlayer;
    public Transform chatContent;
    public GameObject chatText;
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
        
        if (PhotonNetwork.IsConnected) //게임 씬에서 돌아왔을 때
            RoomIn();
    }

    void SetPanel(byte value)   //한 개 패널만 활성화
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

    void Setting()
    {
        SetPanel(LOGIN);
        gameStartBtn.onClick.AddListener(()=> singleton.GameStartBtn());
    }

    void RoomIn()
    {
        SetPanel(ROOM);
        myPlayer = PhotonNetwork.Instantiate("Player", new Vector2(0, 0), QI).GetComponent<PlayerManager>();
        RoomRenewal();
    }

    void Update()
    {
        if (!inGame) {
            lobbyInfoText.text = (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + "로비 / " + PhotonNetwork.CountOfPlayers + "접속";

            if(roomPanel.activeSelf == true) {
                    if (chatInput.text != "" && Input.GetKeyDown(KeyCode.Return)) {
                        MsgSend();
                        chatInput.ActivateInputField();
                        chatInput.Select();
                }
            }
        }
    }

    #region Server Setting
    public void Connect()
    {
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.LocalPlayer.NickName = nickNameInput.text;
    }

    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();

    public override void OnJoinedLobby()
    {
        SetPanel(LOBBY);
        
        nickNameInput.text = "";
        wellcomText.text = PhotonNetwork.LocalPlayer.NickName + "님 환영합니다.";
        myRoomList.Clear();
    }

    public void Disconnect()
    {
        SetPanel(LOGIN);
        PhotonNetwork.Disconnect();
    } 

    public override void OnDisconnected(DisconnectCause cause)
    {
        lobbyPanel.SetActive(false);
        roomPanel.SetActive(false);
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

    #region Room
    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(RoomNameInput.text == "" ? "Room" + Random.Range(0, 100) : RoomNameInput.text, new RoomOptions { MaxPlayers = 4});
        RoomNameInput.text = "";
        playerSlot[0].transform.GetChild(0).GetComponent<Text>().text = PhotonNetwork.NickName;
    }

    public void JoinRoom()
    {
        string roomName = EventSystem.current.currentSelectedGameObject.transform.GetChild(0).GetComponent<Text>().text;
        PhotonNetwork.JoinRoom(roomName);
    }

    public void SortPlayers() => players.Sort((p1, p2) => p1.actor.CompareTo(p2.actor));

    public override void OnJoinedRoom()
    {
        RoomIn();
        chatInput.text = "";
    }

    public override void OnCreateRoomFailed(short returnCode, string message) { RoomNameInput.text = ""; CreateRoom(); }

    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();
    public override void OnJoinRandomFailed(short returnCode, string message) { RoomNameInput.text = ""; CreateRoom(); }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        RemoveRoomLog();
    }

    void RemoveRoomLog()
    {
        //챗 로그 삭제
        var child = chatContent.GetComponentsInChildren<Transform>();
        if (child != null) {
            for (int i = 1; i < child.Length; i++) {
                if (child[i] != transform)
                    Destroy(child[i].gameObject);
            }
        }

        //플레이어 닉네임 삭제
        for(int i = 0; i < playerSlot.Length; i++) {
            playerSlot[i].transform.GetChild(0).GetComponent<Text>().text = "";
        }
    } 
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RoomRenewal();
        if (singleton.Master())
            PV.RPC("ChatRPC", RpcTarget.All, "<color=yellow>" + newPlayer.NickName + "님이 참가하셨습니다</color>");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RoomRenewal();
        if (singleton.Master())
            PV.RPC("ChatRPC", RpcTarget.All, "<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");
    }

    public void RoomRenewal()
    {
        roomInfoText.text = PhotonNetwork.CurrentRoom.Name + " / " + "현재" + PhotonNetwork.CurrentRoom.PlayerCount + "명 / " + "최대" +PhotonNetwork.CurrentRoom.MaxPlayers + "명";

        //플레이어 리셋
        for(int i = 0; i < playerSlot.Length; i++) {
            playerSlot[i].transform.GetChild(0).GetComponent<Text>().text = "";
        }

        //플레이어 추가
        for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++) {
            playerSlot[i].transform.GetChild(0).GetComponent<Text>().text = PhotonNetwork.PlayerList[i].NickName;
        }

        for (int i = 0; i < players.Count; i++) {
            players[i].gameObject.transform.SetParent(playerSlot[i].transform);
            players[i].gameObject.transform.localPosition = new Vector3(0, 0, 0);
        }
    }

    public void SetPlayerColorBtn(int value)
    {
        switch(value) {
            case 0 : myPlayer.PlayerColor = 0;
                myPlayer.SR.color = new Color(1, 0, 0, 1);
                break;
            case 1: myPlayer.PlayerColor = 1;
                myPlayer.SR.color = new Color(0, 1, 0, 1);
                break;
            case 2 : myPlayer.PlayerColor = 2;
                myPlayer.SR.color = new Color(0, 0, 1, 1);
                break;
        }
    }
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
        GameObject chatTextGo = Instantiate(chatText) as GameObject;
        chatTextGo.transform.SetParent(chatContent, false);
        chatTextGo.GetComponent<Text>().text = msg;
    }
    #endregion
}
