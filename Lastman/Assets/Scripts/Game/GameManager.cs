using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using static Singleton;
using ExitGames.Client.Photon;

public class GameManager : MonoBehaviourPunCallbacks
{
    public PhotonView PV;
    //public Button GameEndBtn;
    [SerializeField] MultiManager MM;
    public List<TopDown.PlayerController> players = new List<TopDown.PlayerController>();

    //아이템 오브젝트 생성될 위치
    public Transform fixed_Props_Position_Layer1;
    public Transform random_Props_Position_Layer1;
    public Transform fixed_Props_Position_Layer2;
    public Transform random_Props_Position_Layer2;
    [Space (10f)]

    [SerializeField] string[] fixedPropsName = {"PF Props Chest"};
    [SerializeField] string[] randomPropsName = {"PF Props Crate Small", "PF Props Crate"
                        , "PF Props Pot A", "PF Props Pot B", "PF Props Pot C"};

    public GameObject[] fixedItem;
    public GameObject[] item;

    public Transform PlayerStartPosition;
    [SerializeField] bool[] isUsingPosition;
    bool isFinish = false;

    [Header("-----UI-----")]
    public GameObject DiePanel;
    public Transform PlayerNameListPanel;
    public Text timerText;
    public float timer;
    public float time_current;
    public bool imDie = false;
    public GameObject interactionPanel;
    public Cainos.PixelArtTopDown_Basic.CameraPlayerFollow CameraFollw;
    public int playerViewkey = -1;

    void Start()
    {
        MM = FindObjectOfType<MultiManager>();
        //GameEndBtn.onClick.AddListener(()=> StartCoroutine(MM.FinishGame()));

        Invoke("SetUi_PlayersName", 1f);

        isUsingPosition = new bool[PlayerStartPosition.childCount];
        if (singleton.Master()) {
            //플레이어 스타트 포인터 설정 게임 들어오고 플레이어 리스트 추가 기다려야함
            Invoke("SetStartPlayerPosotionInvoke", 1f);
            Invoke("SetStartPropsPosotionInvoke", 1f);
        }

        //타이머 설정
        StartCoroutine(TimerCoroution(timer));
    }

    IEnumerator TimerCoroution(float time)
    {    
        time_current = time;
        timerText.text = "Timer : " + ((int)(time_current / 60 % 60)).ToString("D2") + " : " + ((int)(time_current % 60)).ToString("D2");
        yield return new WaitUntil(() => singleton.isStart);

        while(time_current > 0)
        {
            time_current -= Time.deltaTime;
            timerText.text = "Timer : " + ((int)(time_current / 60 % 60)).ToString("D2") + " : " + ((int)(time_current % 60)).ToString("D2");
            yield return null;
        }
        timerText.text = "Timer : 00 : 00";
    }

    //처음 게임 시작 시 있는 모든 사람 (강제종료 전 모든 사람 포함)
    void SetUi_PlayersName()
    {
        for (int i = 0; i < MM.playerInfos.Count; i++) {
            PlayerNameListPanel.GetChild(i).GetComponent<Text>().text = MM.playerInfos[i].nickName;
        }
    }

    //강제 종료 시 UI이름 색상 변경
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        for (int i = 0; i < PlayerNameListPanel.childCount; i++) {
            if (PlayerNameListPanel.GetChild(i).GetComponent<Text>().text == otherPlayer.NickName)
                PlayerNameListPanel.GetChild(i).GetComponent<Text>().color = Color.red;
        }
    }

    public void DieUi()
    {
        imDie = true;
        DiePanel.SetActive(true);
        DiePanel.transform.GetChild(0).GetComponent<Text>().text = "Lose...";
        DiePanel.transform.GetChild(1).GetComponent<Text>().text = "스페이스바로 카메라 전환";

        interactionPanel.SetActive(false);
    }

    public void WinUi()
    {
        DiePanel.SetActive(true);
        DiePanel.transform.GetChild(0).GetComponent<Text>().text = "Win";
    }

    public void TimeUpUi()
    {
        DiePanel.SetActive(true);
        DiePanel.transform.GetChild(0).GetComponent<Text>().text = "Drow";
    }

    public void SetDiePlayer(string playerName) => PV.RPC("SetDiePlayerRPC", RpcTarget.AllBuffered, playerName);

    [PunRPC] public void SetDiePlayerRPC(string playerName)
    {
        for (int i = 0; i < PlayerNameListPanel.childCount; i++) {
            Text PlayerNametext = PlayerNameListPanel.GetChild(i).GetComponent<Text>();
            if (PlayerNametext.text == playerName) {
                PlayerNametext.color = new Color(PlayerNametext.color.r, 
                PlayerNametext.color.g, PlayerNametext.color.b, 0.5f);
            }
                
        }
    }

    void Update()
    {   
        if (!singleton.isStart)
            return;

        //게임 종료 설정 - 한 명 살아 남거나 시간 종료
        if (CheckGameState() && isFinish == false) {
            if (time_current <= 0 && !imDie) TimeUpUi();
            DiePanel.SetActive(true);
            isFinish = true;
            StartCoroutine(MM.FinishGame());
        }
        
        //죽으면 다른 플레이어 시점 변환 가능
        if (imDie && Input.GetKeyDown(KeyCode.Space)) {
            DiePanel.SetActive(false);
            if (playerViewkey == -1 ) {
                for (int i = 0; i < players.Count; i++) {
                    if (players[i].isMinePlayer()) {
                        playerViewkey = i+1;
                        break;
                    }
                }
            }
            while(true) {
                if (playerViewkey+1 == players.Count) playerViewkey = -1;
                if (players[++playerViewkey].IsDie == false)
                    break;
            }
            CameraFollw.ChagePlayerView(playerViewkey);
        }
    }

    bool CheckGameState()
    {
        if (MM.AlivePlayerNum() <= 1 || time_current <= 0) return true;
        else return false;
    }

    void SetStartPlayerPosotionInvoke()
    {
        int[] randomStartPosition = new int[players.Count];
        randomStartPosition = RandomPosition(0, PlayerStartPosition.childCount, MM.playerInfos.Count);
        PV.RPC("SetPlayerPositionRPC", RpcTarget.AllBufferedViaServer, randomStartPosition);
    }

    [PunRPC]
    IEnumerator SetPlayerPositionRPC(int[] randomStartPosition)
    {
        yield return null;

        for (int i = 0; i < players.Count; i++) {
            players[i].transform.position = PlayerStartPosition.GetChild(randomStartPosition[i]).transform.position;
        }
    }
    #region Player Position Setting
    

    int[] RandomPosition(int min, int max, int count)
    {
        int[] intArray = new int[count];
        var rand = new System.Random();

        for (int loop = 0; loop < count; loop++) {
            int randNumber = rand.Next(min, max);

            if (intArray.Contains(randNumber)) loop--;
            else intArray[loop] = randNumber;

        }
        return intArray;
    }
    #endregion

    #region Props Position Setting
    
    void SetStartPropsPosotionInvoke()
    {
        /*1, 2층 고정 위치*/
        int[] fixedLayer1Position = new int[fixed_Props_Position_Layer1.childCount];
        int[] fixedLayer2Position = new int[fixed_Props_Position_Layer2.childCount];

        /*1, 2층 고정위치 랜덤 오브젝트*/
        int[] fixedLayer1Prop = new int[fixedLayer1Position.Length];
        int[] fixedLayer2Prop = new int[fixedLayer2Position.Length];

        for (int i = 0; i < fixedLayer1Prop.Length; i++) {
            fixedLayer1Prop[i] = Random.Range(0, fixedPropsName.Length);
        }
        for (int i = 0; i < fixedLayer2Prop.Length; i++) {
            fixedLayer2Prop[i] = Random.Range(0, fixedPropsName.Length);
        }

        /*1, 2층 랜덤 위치*/
        int[] randomLayer1Position = new int[random_Props_Position_Layer1.childCount / 2];
        int[] randomLayer2Position = new int[random_Props_Position_Layer2.childCount / 2];

        randomLayer1Position = RandomPosition(0, random_Props_Position_Layer1.childCount, randomLayer1Position.Length);
        randomLayer2Position = RandomPosition(0, random_Props_Position_Layer2.childCount, randomLayer2Position.Length);

        /*1, 2층 위치의 랜덤 오브젝트*/
        int[] randomLayer1Prop = new int[randomLayer1Position.Length];
        int[] randomLayer2Prop = new int[randomLayer2Position.Length];
        
        for (int i = 0; i < randomLayer1Prop.Length; i++) {
            randomLayer1Prop[i] = Random.Range(0, randomPropsName.Length);
        }
        for (int i = 0; i < randomLayer2Prop.Length; i++) {
            randomLayer2Prop[i] = Random.Range(0, randomPropsName.Length);
        }

        StartCoroutine(SetPropsPositionCoroutine(
        fixedLayer1Position, fixedLayer2Position
        , fixedLayer1Prop, fixedLayer2Prop
        , randomLayer1Position, randomLayer2Position
        , randomLayer1Prop, randomLayer2Prop));
    }

    IEnumerator SetPropsPositionCoroutine(
        int[] fixedLayer1Position, int[] fixedLayer2Position, int[] fixedLayer1Prop, int[] fixedLayer2Prop
        , int[] randomLayer1Position, int[] randomLayer2Position, int[] randomLayer1Prop, int[] randomLayer2Prop)
    {   
        yield return null;
        //1층 고정 오브젝트 배치
        for (int i = 0; i < fixedLayer1Position.Length; i++) {
            PhotonNetwork.InstantiateSceneObject("Props/" + fixedPropsName[fixedLayer1Prop[i]], fixed_Props_Position_Layer1.GetChild(fixedLayer1Position[i]).position, Singleton.QI);
        }

        //2층 고정 오브젝트 배치
        for (int i = 0; i < fixedLayer2Position.Length; i++) {
            PhotonNetwork.InstantiateSceneObject("Props/Layer2/" + fixedPropsName[fixedLayer2Prop[i]], fixed_Props_Position_Layer2.GetChild(fixedLayer2Position[i]).position, Singleton.QI);
        }   

        //1층 랜덤 오브젝트 배치
        for (int i = 0; i < randomLayer1Position.Length; i++) {
            PhotonNetwork.InstantiateSceneObject("Props/" + randomPropsName[randomLayer1Prop[i]], random_Props_Position_Layer1.GetChild(randomLayer1Position[i]).position, Singleton.QI);
        }

        //2층 랜덤 오브젝트 배치
        for (int i = 0; i < randomLayer2Position.Length; i++) {
            PhotonNetwork.InstantiateSceneObject("Props/Layer2/" + randomPropsName[randomLayer2Prop[i]], random_Props_Position_Layer2.GetChild(randomLayer2Position[i]).position, Singleton.QI);
        }
    }
    #endregion

    public void SortPlayers() => players.Sort((p1, p2) => p1.actor.CompareTo(p2.actor));
}