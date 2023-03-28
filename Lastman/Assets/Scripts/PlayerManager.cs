using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using static Singleton;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviourPun, IPunObservable
{
    MultiManager MM;
    LobbyManager LM;
    PhotonView PV;
    Vector3 curPos;
    Rigidbody2D rigid;
    bool isDie;
    float speed = 10;

    public string nick;
    public int actor;
    
    void Start()
    {
        Init();
        if (SceneManager.GetActiveScene().name == "Lobby") LobbyPlayerSetting();
    }

    void Init()
    {
        MM = FindObjectOfType<MultiManager>();
        rigid = GetComponent<Rigidbody2D>();

        singleton.SetTag("loadPlayer", true);

        PV = photonView;
        nick = PV.Owner.NickName;
        actor = PV.OwnerActorNr;
    }

    void LobbyPlayerSetting()
    {
        rigid.bodyType = RigidbodyType2D.Static;

        LM = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();
        LM.players.Add(this);

        LM.RoomRenewal();
    }

    void Update()
    {
        if (!singleton.isStart)
            return;

        if (!PV.IsMine) OtherMove();

        if (Forbidden())
            return;

        Move();
    }

    void OtherMove()
    {
        if ((transform.position - curPos).sqrMagnitude >= 100)
            transform.position = curPos;
        else
            transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
    }

    bool Forbidden()
    {
        return !PV.IsMine || !singleton.isStart || isDie;
    }

    void Move()
    {
        transform.Translate(new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0) * Time.deltaTime * speed);
    }

    public void OnTriggerStay2D(Collider2D col) {
        if (Forbidden())
            return;
        isDie = true;

        OhterSendMaster(col.GetComponent<PhotonView>());
        singleton.SetPos(transform, new Vector3(0, 100, 0));
    }

    public void OhterSendMaster(PhotonView colPV)
    {
        if (colPV != null && singleton.ActorNum() != colPV.Owner.ActorNumber)
            MM.PV.RPC("MasterReceiveRPC", RpcTarget.MasterClient, DIE, singleton.ActorNum(), colPV.Owner.ActorNumber);

        else MM.PV.RPC("MasterReceiveRPC", RpcTarget.MasterClient, DIEWALL, singleton.ActorNum(), 0);
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
			stream.SendNext(transform.position);
		else 
			curPos = (Vector3)stream.ReceiveNext(); 
    }

    void OnDestroy()
    {
        if (SceneManager.GetActiveScene().name == "Lobby") {
            LM.players.Remove(this);
        }
    }
}
