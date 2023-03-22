using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using static Singleton;
using ExitGames.Client.Photon;


public class MultiManager : MonoBehaviourPunCallbacks
{
    public PhotonView PV;
    public List<PlayerInfo> playerInfos;
    public bool isStart, isEnd;

    IEnumerator Start()
    {
        yield return Loading();

        if (singleton.Master()) {
            MasterInitPlayerInfo();
            yield return new WaitForSeconds(3f);
            PV.RPC("StartSyncRPC", RpcTarget.AllViaServer);
        }
    }

    IEnumerator Loading()
    {
        singleton.SetTag("loadScene", true);
        //씬 동기화
        while (!singleton.AllhasTag("loadScene")) yield return null;

        PhotonNetwork.Instantiate("Player", new Vector2(Random.Range(-12f, 0f), 3f), QI);
        //씬의 모든 플레이어 동기화
        while (!singleton.AllhasTag("loadPlayer")) yield return null;
    }

    void MasterInitPlayerInfo()
    {
        for (int i = 0; i <PhotonNetwork.PlayerList.Length; i++) {
            Player player = PhotonNetwork.PlayerList[i];
            playerInfos.Add(new PlayerInfo(player.NickName, player.ActorNumber, 0, PhotonNetwork.Time + 3.0, false));
        }
        MasterSendPlayerInfo(INIT);
    }


    [PunRPC]
    public void MasterReceiveRPC(byte code, int actorNum, int colActorNum)
    {
        PlayerInfo playerinfo = playerInfos.Find(x => x.actorNum == actorNum);
        double lifeTime = PhotonNetwork.Time - playerinfo.lifeTime;
        lifeTime = System.Math.Truncate(lifeTime * 100) * 0.01;
        playerinfo.lifeTime = lifeTime;
        playerinfo.isDie = true;

        if (code == DIE) {
            playerinfo = null;
            playerinfo = playerInfos.Find(x => x.actorNum == colActorNum);
            ++playerinfo.killDeath;
        }

        MasterSendPlayerInfo(code);
    }

    void MasterSendPlayerInfo(byte code)
    {
        playerInfos.Sort((p1, p2) => p2.lifeTime.CompareTo(p1.lifeTime));
        
        string jdata = JsonUtility.ToJson(new Serialization<PlayerInfo>(playerInfos));
        PV.RPC("OtherReceivePlayerInfoRPC", RpcTarget.Others, code, jdata);
    }

    [PunRPC]
    void OtherReceivePlayerInfoRPC(byte code, string jdata)
    {
        playerInfos = JsonUtility.FromJson<Serialization<PlayerInfo>>(jdata).target;
    }


    [PunRPC]
    void StartSyncRPC()
    {
        isStart = true;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (singleton.Master()) {
            MasterRemovePlayerInfo(otherPlayer.ActorNumber);
        }
    }

    void MasterRemovePlayerInfo(int actorNumber)
    {
        PlayerInfo playerInfo = playerInfos.Find(x => x.actorNum == actorNumber);
        playerInfos.Remove(playerInfo);
        MasterSendPlayerInfo(REMOVE);
    }
}