using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Singleton : MonoBehaviourPun
{
    public const byte INIT = 0, REMOVE = 1, DIE = 2;
    public static readonly Quaternion QI = Quaternion.identity;
    public bool isStart;

    public static Singleton singleton;
    
    #region Singleton
    void Awake()
	{
		if (null == singleton)
		{
			singleton = this;
			DontDestroyOnLoad(this);
		}
		else Destroy(gameObject);
	}
    #endregion

    void Start()
    {
        Setting();
    }

    void Setting()
    {
        Screen.SetResolution(1280, 720, false);
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.SendRate = 40;
        PhotonNetwork.SerializationRate = 20;
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    #region Set/Get Tag
    public bool Master() => PhotonNetwork.LocalPlayer.IsMasterClient;
    
    public int ActorNum(Player player = null)
	{
		if (player == null) player = PhotonNetwork.LocalPlayer;
		return player.ActorNumber;
	}

	public void Destroy(List<GameObject> GO)
	{
		for (int i = 0; i < GO.Count; i++) PhotonNetwork.Destroy(GO[i]);
	}

	public void SetPos(Transform Tr, Vector3 target) 
	{
		Tr.position = target;
	}

	public void SetTag(string key, object value, Player player = null)
	{
		if (player == null) player = PhotonNetwork.LocalPlayer;
		player.SetCustomProperties(new Hashtable { { key, value } });
	}

	public object GetTag(Player player, string key)
	{
		if (player.CustomProperties[key] == null) return null;
		return player.CustomProperties[key].ToString();
	}

	public bool AllhasTag(string key)
	{
		for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
			if (PhotonNetwork.PlayerList[i].CustomProperties[key] == null) return false;
		return true;
	}
    #endregion

    public void GameStart()
    {
        if (Master()) {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel("Game");
        }
    }

    public void GameEnd()
    {
        if (Master()) {
            PhotonNetwork.CurrentRoom.IsOpen = true;
            PhotonNetwork.LoadLevel("Lobby");
        }
    }

    #region Room Slot Properties
    public void SetPlayerSlot()
    {
        Hashtable CP = PhotonNetwork.CurrentRoom.CustomProperties;

        if (CP["Slot_0"].Equals(""))
            CP["Slot_0"] = PhotonNetwork.NickName;
        else if (CP["Slot_1"].Equals(""))
            CP["Slot_1"] = PhotonNetwork.NickName;
        else if (CP["Slot_2"].Equals(""))
            CP["Slot_2"] = PhotonNetwork.NickName;
        else if (CP["Slot_3"].Equals(""))
            CP["Slot_3"] = PhotonNetwork.NickName;
        
        PhotonNetwork.CurrentRoom.SetCustomProperties(CP);
    }

    public void RemovePlayerSlot(Player player)
    {
        Hashtable CP = PhotonNetwork.CurrentRoom.CustomProperties;

        if (CP["Slot_0"].Equals(player.NickName))
            CP["Slot_0"] = "";
        else if (CP["Slot_1"].Equals(player.NickName))
            CP["Slot_1"] = "";
        else if (CP["Slot_2"].Equals(player.NickName))
            CP["Slot_2"] = "";
        else if (CP["Slot_3"].Equals(player.NickName))
            CP["Slot_3"] = "";
        
        PhotonNetwork.CurrentRoom.SetCustomProperties(CP);
    }

    public void PrintPlayerSlot()
    {
        Hashtable CP = PhotonNetwork.CurrentRoom.CustomProperties;

        print("[Slot_0] : " + ( CP["Slot_0"].Equals("") ? "Null": CP["Slot_0"] ) + ",   "
            + "[Slot_1] : " + ( CP["Slot_1"].Equals("") ? "Null": CP["Slot_1"] ) + ",   "
            + "[Slot_2] : " + ( CP["Slot_2"].Equals("") ? "Null": CP["Slot_2"] ) + ",   "
            + "[Slot_3] : " + ( CP["Slot_3"].Equals("") ? "Null": CP["Slot_3"] ) );
    }
    #endregion

    void OnGUI()
    {
        /*
        GUI.skin.label.fontSize = 10;
		GUI.skin.button.fontSize = 10;
		GUILayout.BeginVertical("Box", GUILayout.Width(200), GUILayout.MinHeight(100));

        GUILayout.Label("서버시간 : " + PhotonNetwork.Time);
		GUILayout.Label("상태 : " + PhotonNetwork.NetworkClientState);
        GUILayout.Label("닉네임 : " + PhotonNetwork.NickName);
		GUILayout.Label("씬 : " + SceneManager.GetActiveScene().name);

        GUILayout.EndVertical();
        */
    }
}
