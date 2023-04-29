using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using static Singleton;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TopDown
{
    public class PlayerController : MonoBehaviourPun, IPunObservable, IPunInstantiateMagicCallback
    {
        [Header("-----Component-----")]
        [SerializeField] LobbyManager LM;
        [SerializeField] MultiManager MM;
        [SerializeField] GameManager GM;
        [SerializeField] PhotonView PV;
        Vector3 curPos;
        Rigidbody2D rigid;
        SpriteRenderer renderer;
        Animator anim;
        public Canvas canvas;
        public Text nickNameText;
        public Image healthImage;

        [Header("-----Player Info-----")]
        public string nick;
        public int actor;
        public Transform attackPosition;
        public Transform defencePosition;

        [Header("-----Player State-----")]
        #region Player Properties
        //최대 체력
        [SerializeField] float maxHealth; //default == 100
        public float MaxHealth { get => maxHealth; set => ActionRPC(nameof(SetMaxHealthRPC), value); }
        [PunRPC] void SetMaxHealthRPC(float value) => maxHealth = value;

        //현제 체력
        [SerializeField] float health;
        public float Health { get => health; set => ActionRPC(nameof(SetHealthRPC), value); }
        [PunRPC] void SetHealthRPC(float value) { health = value; healthImage.fillAmount = Health / MaxHealth; }

        //스피드
        [SerializeField] float speed; //default = 2
        public float Speed { get => speed; set => ActionRPC(nameof(SetSpeedRPC), value); }
        [PunRPC] void SetSpeedRPC(float value) => speed = value;

        //데미지
        [SerializeField] float damage; //default = 2
        public float Damage { get => damage; set => ActionRPC(nameof(SetDamageRPC), value); }
        [PunRPC] void SetDamageRPC(float value) => damage = value;

        //바라보는 방향
        [SerializeField] int direction;
        public int Direction { get => direction; set => ActionRPC(nameof(SetDirectionRPC), value); }
        [PunRPC] void SetDirectionRPC(int value) => direction = value;

        //공격 딜레이
        [SerializeField] float attackDelay; //default = 0.5
        [SerializeField] float defenceDelay; //default = 2
        [SerializeField] bool attackable = true;
        [SerializeField] bool defensible = true;

        //Die
        [SerializeField] bool isDie;
        public bool IsDie { get => isDie; set => ActionRPC(nameof(SetIsDieRPC), value); }
        [PunRPC] void SetIsDieRPC(bool value) => isDie = value;

        void ActionRPC(string functionName, object value)
        {
            photonView.RPC(functionName, RpcTarget.All, value);
        }

        public void InvokeProperties()
        {
            Direction = Direction;
            Health = Health;
            IsDie = IsDie;
            Speed = Speed;
            Damage = Damage;
        }
        #endregion
        
        void Awake() => DontDestroyOnLoad(this);

        void Start()
        {
            Init();
            LobbyPlayerSetting();
        }

        void Init() //in Lobby
        {
            rigid = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();
            renderer = GetComponent<SpriteRenderer>();

            singleton.SetTag("loadPlayer", true);

            PV = photonView;
            nick = PV.Owner.NickName;
            actor = PV.OwnerActorNr;

            nickNameText.text = nick;
            nickNameText.color = PV.IsMine ? Color.blue : Color.gray;

            canvas.gameObject.SetActive(false);
        }

        void LobbyPlayerSetting() //로비 들어왔을 때, 게임에서 돌아왔을 때
        {
            canvas.gameObject.SetActive(false);

            LM = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();
            LM.players.Add(this);
            LM.RoomRenewal();

            Health = MaxHealth;
            IsDie = false;
            Direction = 0;
            anim.SetInteger("Direction", Direction);

            rigid.bodyType = RigidbodyType2D.Static;
        }

        void GamePlayerSetting() //게임에 들어갔을 때
        {
            MM = GameObject.Find("MultiManager").GetComponent<MultiManager>();
            GM = GameObject.Find("GameManager").GetComponent<GameManager>();
            GM.players.Add(this);
            GM.SortPlayers();
            canvas.gameObject.SetActive(true);

            rigid.bodyType = RigidbodyType2D.Dynamic;
        }

        void Update()
        {
            //로비 <-> 게임 씬 전환 세팅
            if (!singleton.isStart) {
                if (LM == null && SceneManager.GetActiveScene().name == "Lobby")
                    LobbyPlayerSetting();
                else if (MM == null && SceneManager.GetActiveScene().name == "Game")
                    GamePlayerSetting();
                return;
            }

            if (!PV.IsMine) OtherMove();

            if (Forbidden()) return;
                
            Move();
            Shot();
            Defence();
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

        bool Forbidden() { return !PV.IsMine || !singleton.isStart || IsDie; }
        
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
            rigid.velocity = Speed * curPos;

            anim.SetInteger("Direction", Direction);
            anim.SetBool("IsMoving", curPos.magnitude > 0);
        }

        void Shot()
        {
            if (attackable && Input.GetKeyDown(KeyCode.Mouse0)) {
                attackable = false;
                StartCoroutine(AttackDelayCoroutine(attackDelay));
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - attackPosition.transform.position;
                float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
                attackPosition.rotation = Quaternion.AngleAxis(angle , Vector3.forward);

                GameObject go = PhotonNetwork.Instantiate("Bullet", attackPosition.transform.position, attackPosition.rotation);
                go.GetComponent<SpriteRenderer>().sortingLayerID = renderer.sortingLayerID;
                go.GetComponent<BulletScript>().SetDamage(Damage);
            }
        }

        void Defence()
        {
            if (defensible && Input.GetKeyDown(KeyCode.Space)) {
                defensible = false;
                StartCoroutine(DefenceDelayCoroutine(defenceDelay));
                
                PV.RPC("DefenceRPC", RpcTarget.AllBuffered);
            }
        }

        IEnumerator AttackDelayCoroutine(float attackDelay)
        {
            yield return new WaitForSeconds(attackDelay);
            attackable = true;
        }

        IEnumerator DefenceDelayCoroutine(float defenceDelay)
        {
            yield return new WaitForSeconds(defenceDelay);
            defensible = true;
        }

        [PunRPC] void DefenceRPC()
        {
            defencePosition.GetChild(Direction).gameObject.SetActive(true);
            defencePosition.GetChild(Direction).gameObject.GetComponent<DefenceScript>().DefenceActiveFalse();
        }

        public void Hit(float damage) => Health -= damage;

        public void OnTriggerEnter2D(Collider2D col) {
            if (Forbidden())
                return;

            OhterSendMaster(col.GetComponent<PhotonView>(), col.GetComponent<BulletScript>().damage);
        }

        public void OhterSendMaster(PhotonView colPV, float damage)
        {
            if (colPV != null && PV.Owner != colPV.Owner) {
                Hit(damage);

                if (Health <= 0) {
                    IsDie = true;
                    MM.PV.RPC("MasterReceiveRPC", RpcTarget.MasterClient, DIE, singleton.ActorNum(), colPV.Owner.ActorNumber);
                    GameObject.Find("Canvas").transform.Find("RespawnPanel").gameObject.SetActive(true);
                }  
            }
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
            if (SceneManager.GetActiveScene().name == "Lobby")
                LM.players.Remove(this);

            if (SceneManager.GetActiveScene().name == "Game") {
                GM.players.Remove(this);
                GM.SortPlayers();
            }
                
        }

        void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info)
        {
            info.Sender.TagObject = gameObject;
        }
    }
}