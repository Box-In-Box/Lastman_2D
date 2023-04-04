using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using static Singleton;
using UnityEngine.SceneManagement;

namespace TopDown
{
    public class PlayerController : MonoBehaviourPun, IPunObservable, IPunInstantiateMagicCallback
    {
        [Header("-----Component-----")]
        [SerializeField] LobbyManager LM;
        [SerializeField] MultiManager MM;
        [SerializeField] PhotonView PV;
        Vector3 curPos;
        Rigidbody2D rigid;
        Animator anim;

        [Header("-----Player Info-----")]
        public string nick;
        public int actor;
        public bool isDie;
        float speed = 3;
        
        void Awake()
        {
            DontDestroyOnLoad(this);
        }

        void Start()
        {
            Init();
            LobbyPlayerSetting();
        }

        void Init() //Lobby
        {
            rigid = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();

            singleton.SetTag("loadPlayer", true);

            PV = photonView;
            nick = PV.Owner.NickName;
            actor = PV.OwnerActorNr;
        }

        void LobbyPlayerSetting()
        {
            LM = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();
            LM.players.Add(this);
            LM.RoomRenewal();
        }

        void GamePlayerSetting()
        {
            MM = GameObject.Find("MultiManager").GetComponent<MultiManager>();
        }

        void Update()
        {
            if (!singleton.isStart) {
                if (LM == null && SceneManager.GetActiveScene().name == "Lobby")
                    LobbyPlayerSetting();
                else if (MM == null && SceneManager.GetActiveScene().name == "Game")
                    GamePlayerSetting();
                return;
            }

            if (!PV.IsMine) OtherMove();

            if (Forbidden())
                return;
                
            Move();
        }

        void OtherMove()
        {
            //Move
            if ((transform.position - curPos).sqrMagnitude >= 100)
                transform.position = curPos;
            else
                transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
            
            //Animation
            anim.SetInteger("Direction", Direction);
        }

        /* 플레이어 프로퍼티*/
        [SerializeField] int direction;
        public int Direction { get => direction; set => ActionRPC(nameof(SetDirectionRPC), value); }
        [PunRPC] void SetDirectionRPC(int value) => direction = value;

        void ActionRPC(string functionName, object value)
        {
            photonView.RPC(functionName, RpcTarget.All, value);
        }

        public void InvokeProperties()
        {
            Direction = Direction;
        }
        /**********/

        bool Forbidden() { return !PV.IsMine || !singleton.isStart || isDie; }
        
        public bool isMinePlayer() { return PV.IsMine; }

        void Move()
        {
            curPos = Vector3.zero;
            if (Input.GetKey(KeyCode.A)) {
                curPos.x = -1;
                Direction = 3;
            }
            else if (Input.GetKey(KeyCode.D)) {
                curPos.x = 1;
                Direction = 2;
            }

            if (Input.GetKey(KeyCode.W)) {
                curPos.y = 1;
                Direction = 1;
            }
            else if (Input.GetKey(KeyCode.S)) {
                curPos.y = -1;
                Direction = 0;
                
            }
            curPos.Normalize();
            rigid.velocity = speed * curPos;

            anim.SetInteger("Direction", Direction);
            anim.SetBool("IsMoving", curPos.magnitude > 0);
        }

        public void OnTriggerStay2D(Collider2D col) {
            if (Forbidden())
                return;

            //OhterSendMaster(col.GetComponent<PhotonView>());
            //singleton.SetPos(transform, new Vector3(0, 100, 0));
        }

        public void OhterSendMaster(PhotonView colPV)
        {
            if (colPV != null && singleton.ActorNum() != colPV.Owner.ActorNumber)
                MM.PV.RPC("MasterReceiveRPC", RpcTarget.MasterClient, DIE, singleton.ActorNum(), colPV.Owner.ActorNumber);

            //else MM.PV.RPC("MasterReceiveRPC", RpcTarget.MasterClient, DIEWALL, singleton.ActorNum(), 0);
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

        void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info)
        {
            info.Sender.TagObject = gameObject;
        }
    }
}