using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using static Singleton;

public class PlayerManager : MonoBehaviourPun, IPunObservable
{
    MultiManager MM;
    PhotonView PV;
    Vector3 curPos;
    bool isDie;

    float speed = 10;

    void Start()
    {
        Init();
    }

    void Init()
    {
        MM = FindObjectOfType<MultiManager>();
        singleton.SetTag("loadPlayer", true);
        PV = photonView;
    }

    void Update()
    {
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
        return !PV.IsMine || !MM.isStart || isDie;
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
}
