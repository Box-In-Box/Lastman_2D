using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.EventSystems;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("-----LoginPanel-----")]
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
    public Text playerListText;
    public Transform chatContent;
    public GameObject chatText;
    public InputField chatInput;

    [Header("-----ETC-----")]
    public Text serverStatusText;
    private PhotonView PV;

    List<RoomInfo> myRoomList = new List<RoomInfo>();
    int currentRoomPage = 1, maxRoomPage, multiple;

    void Awake() => Screen.SetResolution(1920, 1080, false);

    void Start()
    {
        PV = GetComponent<PhotonView>();
        lobbyPanel.SetActive(false);
        roomPanel.SetActive(false);
    }

    void Update()
    {
        serverStatusText.text = PhotonNetwork.NetworkClientState.ToString();
        lobbyInfoText.text = (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + "로비 / " + PhotonNetwork.CountOfPlayers + "접속";

        if(roomPanel.activeSelf == true) {
                if (chatInput.text != "" && Input.GetKeyDown(KeyCode.Return)) {
                    MsgSend();
                    chatInput.ActivateInputField();
                    chatInput.Select();
            }
        }
    }

    #region Server Setting
    public void Connect() => PhotonNetwork.ConnectUsingSettings();

    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();

    public override void OnJoinedLobby()
    {
        lobbyPanel.SetActive(true);
        roomPanel.SetActive(false);
        PhotonNetwork.LocalPlayer.NickName = nickNameInput.text;
        wellcomText.text = PhotonNetwork.LocalPlayer.NickName + "님 환영합니다.";
        myRoomList.Clear();
    }

    public void Disconnect() => PhotonNetwork.Disconnect();

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
        PhotonNetwork.CreateRoom(RoomNameInput.text == "" ? "Room" + Random.Range(0, 100) : RoomNameInput.text, new RoomOptions { MaxPlayers = 2});
        RoomNameInput.text = "";
    }

    public void JoinRoom()
    {
        string roomName = EventSystem.current.currentSelectedGameObject.transform.GetChild(0).GetComponent<Text>().text;
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        roomPanel.SetActive(true);
        RoomRenewal();
        chatInput.text = "";
    }

    public override void OnCreateRoomFailed(short returnCode, string message) { RoomNameInput.text = ""; CreateRoom(); }

    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();
    public override void OnJoinRandomFailed(short returnCode, string message) { RoomNameInput.text = ""; CreateRoom(); }

    public void LeaveRoom() => PhotonNetwork.LeaveRoom();

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RoomRenewal();
        PV.RPC("ChatRPC", RpcTarget.All, "<color=yellow>" + newPlayer.NickName + "님이 참가하셨습니다</color>");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RoomRenewal();
        PV.RPC("ChatRPC", RpcTarget.All, "<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");
    }

    void RoomRenewal()
    {
        playerListText.text = "";
        for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            playerListText.text += PhotonNetwork.PlayerList[i].NickName + ((i + 1 == PhotonNetwork.PlayerList.Length) ? "" : ", ");
            roomInfoText.text = PhotonNetwork.CurrentRoom.Name + " / " + PhotonNetwork.CurrentRoom.PlayerCount + "명 / " + PhotonNetwork.CurrentRoom.MaxPlayers + "최대";
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
