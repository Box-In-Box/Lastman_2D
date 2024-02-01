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
        [SerializeField] AudioSource audioSource;
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

        [Header("-----Audio Clip-----")]
        public AudioClip attck_0_clip;
        public AudioClip attck_1_clip;
        public AudioClip defence_clip;

        [Header("-----Player Max State-----")]
        #region Player Properties
        [SerializeField] float maxHealth; //최대 체력 100
        [SerializeField] float maxSpeed; //최대 스피드 4
        [SerializeField] float maxDamage; //최대 데미지 30
        [SerializeField] float maxAttackDelay0; //최소0 딜레이 0.2
        [SerializeField] float maxAttackDelay1; //최소1 딜레이 3
        [SerializeField] float maxDefenceDelay; //최소 딜레이 3

        [Header("-----Player State-----")]
        //현제 체력
        [SerializeField] float health;
        public float Health { get => health; set => ActionRPC(nameof(SetHealthRPC), value); }
        [PunRPC] void SetHealthRPC(float value) { health = value; healthImage.fillAmount = Health / maxHealth; }

        //스피드
        [SerializeField] float speed; //default = 2
        public float Speed { get => speed; set => ActionRPC(nameof(SetSpeedRPC), value); }
        [PunRPC] void SetSpeedRPC(float value) => speed = value;

        //데미지
        [SerializeField] float damage; //default = 20
        public float Damage { get => damage; set => ActionRPC(nameof(SetDamageRPC), value); }
        [PunRPC] void SetDamageRPC(float value) => damage = value;

        //바라보는 방향
        [SerializeField] int direction;
        public int Direction { get => direction; set => ActionRPC(nameof(SetDirectionRPC), value); }
        [PunRPC] void SetDirectionRPC(int value) => direction = value;

        //공격 딜레이
        [SerializeField] public float attackDelay0; //default = 0.5
        [SerializeField] public float attackDelay1; //default = 5
        [SerializeField] public float defenceDelay; //default = 5
        [SerializeField] public bool attackable0 = true;
        [SerializeField] public bool attackable1 = true;
        [SerializeField] public bool defensible = true;

        //Die
        [SerializeField] bool isDie;
        public bool IsDie { get => isDie; set => ActionRPC(nameof(SetIsDieRPC), value); }
        [PunRPC] void SetIsDieRPC(bool value) => isDie = value;

        void ActionRPC(string functionName, object value) => photonView.RPC(functionName, RpcTarget.All, value);

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
            audioSource = GetComponent<AudioSource>();

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

            Health = maxHealth;
            Speed = 2;
            Damage = 20;
            attackDelay0 = 0.5f;
            attackDelay1 = 5f;
            defenceDelay = 5f;
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
            if (attackable0 && Input.GetKeyDown(KeyCode.Mouse0)) {
                attackable0 = false;
                StartCoroutine(AttackDelayCoroutine0(attackDelay0));
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - attackPosition.transform.position;
                float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
                attackPosition.rotation = Quaternion.AngleAxis(angle , Vector3.forward);

                if (renderer.sortingLayerName == "Layer 1")
                    PhotonNetwork.Instantiate("Bullet/Bullet_L_Layer1", attackPosition.transform.position, attackPosition.rotation).GetComponent<BulletScript>().SetDamage(Damage);
                else if (renderer.sortingLayerName == "Layer 2")
                    PhotonNetwork.Instantiate("Bullet/Bullet_L_Layer2", attackPosition.transform.position, attackPosition.rotation).GetComponent<BulletScript>().SetDamage(Damage);
                else if (renderer.sortingLayerName == "Layer 3")
                    PhotonNetwork.Instantiate("Bullet/Bullet_L_Layer3", attackPosition.transform.position, attackPosition.rotation).GetComponent<BulletScript>().SetDamage(Damage);

                PV.RPC("Shot0RPC", RpcTarget.AllBuffered);
            }

            if (attackable1 && Input.GetKeyDown(KeyCode.Mouse1)) {
                attackable1 = false;
                StartCoroutine(AttackDelayCoroutine1(attackDelay1));
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - attackPosition.transform.position;
                float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
                attackPosition.rotation = Quaternion.AngleAxis(angle , Vector3.forward);

                if (renderer.sortingLayerName == "Layer 1")
                    PhotonNetwork.Instantiate("Bullet/Bullet_R_Layer1", attackPosition.transform.position, attackPosition.rotation).GetComponent<BulletScript>().SetDamage(Damage * 3);
                else if (renderer.sortingLayerName == "Layer 2")
                    PhotonNetwork.Instantiate("Bullet/Bullet_R_Layer2", attackPosition.transform.position, attackPosition.rotation).GetComponent<BulletScript>().SetDamage(Damage * 3);
                else if (renderer.sortingLayerName == "Layer 3")
                    PhotonNetwork.Instantiate("Bullet/Bullet_R_Layer3", attackPosition.transform.position, attackPosition.rotation).GetComponent<BulletScript>().SetDamage(Damage * 3);
                PV.RPC("Shot1RPC", RpcTarget.AllBuffered);
            }
        }

        [PunRPC] void Shot0RPC() => SoundManager.instance.PlayerSFXPlay(audioSource, "Attack_0", attackPosition, attck_0_clip);

        [PunRPC] void Shot1RPC() => SoundManager.instance.PlayerSFXPlay(audioSource, "Attack_1", attackPosition, attck_1_clip);

        void Defence()
        {
            if (defensible && Input.GetKeyDown(KeyCode.Space)) {
                defensible = false;
                StartCoroutine(DefenceDelayCoroutine(defenceDelay));
                
                PV.RPC("DefenceRPC", RpcTarget.AllBuffered);
            }
        }

        [PunRPC] void DefenceRPC()
        {
            defencePosition.GetChild(Direction).gameObject.SetActive(true);
            defencePosition.GetChild(Direction).gameObject.GetComponent<SpriteRenderer>().sortingLayerName = renderer.sortingLayerName;
            defencePosition.GetChild(Direction).gameObject.GetComponent<DefenceScript>().DefenceActiveFalse();

            SoundManager.instance.PlayerSFXPlay(audioSource, "Defence", attackPosition, defence_clip);
        }

        IEnumerator AttackDelayCoroutine0(float attackDelay)
        {
            yield return new WaitForSeconds(attackDelay);
            attackable0 = true;
        }

        IEnumerator AttackDelayCoroutine1(float attackDelay)
        {
            yield return new WaitForSeconds(attackDelay);
            attackable1 = true;
        }

        IEnumerator DefenceDelayCoroutine(float defenceDelay)
        {
            yield return new WaitForSeconds(defenceDelay);
            defensible = true;
        }

        public void Hit(float damage) => Health -= damage;

        public void OnTriggerEnter2D(Collider2D col) {
            if (Forbidden())
                return;
            if (col.tag == "Item")
                GetItem(col.GetComponent<ItemObject>());
            if (col.tag == "Bullet")
                OhterSendMaster(col.GetComponent<PhotonView>(), col.GetComponent<BulletScript>().damage);
        }

        public void GetItem(ItemObject itemObject)
        {
            switch (itemObject.itemType) {
                case itemType.AttackDelay0_Down:
                    attackDelay0 -= itemObject.value;
                    if (attackDelay0 < maxAttackDelay0)
                        attackDelay0 = maxAttackDelay0;
                    break;
                case itemType.AttackDelay1_Down:
                    attackDelay1 -= itemObject.value;
                    if (attackDelay1 < maxAttackDelay1)
                        attackDelay1 = maxAttackDelay1;
                    break;
                case itemType.Damage_Up:
                    Damage += itemObject.value;
                    if (Damage > maxDamage)
                        Damage = maxDamage;
                    break;
                case itemType.defenceDelay_Down:
                    defenceDelay -= itemObject.value;
                    if (defenceDelay < maxDefenceDelay)
                        defenceDelay = maxDefenceDelay;
                    break;
                case itemType.Health_Up:
                    Health += itemObject.value;
                    if (Health > maxHealth)
                        Health = maxHealth;
                    break;
                case itemType.Speed_Up:
                    Speed += itemObject.value;
                    if (Speed > maxSpeed)
                        Speed = maxSpeed;
                    break;
            }
        }

        public void OhterSendMaster(PhotonView colPV, float damage)
        {
            if (colPV != null && PV.Owner != colPV.Owner) {
                Hit(damage);

                if (Health <= 0) {
                    IsDie = true;
                    MM.PV.RPC("MasterReceiveRPC", RpcTarget.MasterClient, DIE, singleton.ActorNum(), colPV.Owner.ActorNumber);
                    if (GM != null) {
                        GM.DieUi();
                        SetDiePlayerUI(nick);
                    } 
                }  
            }
        }

        void SetDiePlayerUI(string playerName) => GM.SetDiePlayer(playerName);

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