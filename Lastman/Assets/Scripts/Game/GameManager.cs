using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using static Singleton;

public class GameManager : MonoBehaviourPun
{
    public PhotonView PV;
    public Button GameEndBtn;
    MultiManager multiManager;
    public List<TopDown.PlayerController> players = new List<TopDown.PlayerController>();

    
    public Transform fixed_Props_Position_Layer1;
    public Transform random_Props_Position_Layer1;
    public Transform fixed_Props_Position_Layer2;
    public Transform random_Props_Position_Layer2;
    [Space (10f)]

    public Transform fixed_Props_Layer1;
    public Transform random_Props_Layer1;
    public Transform fixed_Props_Layer2;
    public Transform random_Props_Layer2;


    [Space (10f)]
    [SerializeField] string[] fixedPropsName = {"PF Props Chest"};
    [SerializeField] string[] randomPropsName = {"PF Props Crate Small", "PF Props Crate"
                        , "PF Props Pot A", "PF Props Pot B", "PF Props Pot C"};

    public GameObject[] fixedItem;
    public GameObject[] item;

    public Transform PlayerStartPosition;
    [SerializeField] bool[] isUsingPosition;
    bool isFinish;

    void Start()
    {
        multiManager = FindObjectOfType<MultiManager>();
        GameEndBtn.onClick.AddListener(()=> StartCoroutine(multiManager.FinishGame()));

        isUsingPosition = new bool[PlayerStartPosition.childCount];

        if (singleton.Master()) {
            //플레이어 스타트 포인터 설정 게임 들어오고 플레이어 리스트 추가 기다려야함
            Invoke("SetStartPlayerPosotionInvoke", 1f);
            Invoke("SetStartPropsPosotionInvoke", 1f);
        }
        
        //if (singleton.Master()) PV.RPC("SetPropsPositionRPC", RpcTarget.AllBufferedViaServer);
    }

    void Update()
    {   
        if (!singleton.isStart)
            return;

        if (CheckGameState() && !isFinish) {
            StartCoroutine(multiManager.FinishGame());
            isFinish = true;
        }
    }

    bool CheckGameState()
    {
        if ( multiManager.playerInfos.Count <= 1)
            return false;
            
        int alivePlayerNum = 0;

        for (int i = 0; i < multiManager.playerInfos.Count; i++) {
            if (multiManager.playerInfos[i].isDie == false) alivePlayerNum++;
        }

        if (alivePlayerNum <= 1) return true;
        else return false;
    }

    void SetStartPlayerPosotionInvoke()
    {
        int[] randomStartPosition = new int[players.Count];
        randomStartPosition = RandomPosition(0, PlayerStartPosition.childCount, multiManager.playerInfos.Count);
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
            GameObject go = PhotonNetwork.Instantiate("Props/" + fixedPropsName[fixedLayer1Prop[i]], fixed_Props_Position_Layer1.GetChild(fixedLayer1Position[i]).position, Singleton.QI);
        }

        //2층 고정 오브젝트 배치
        for (int i = 0; i < fixedLayer2Position.Length; i++) {
            GameObject go = PhotonNetwork.Instantiate("Props/Layer2/" + fixedPropsName[fixedLayer2Prop[i]], fixed_Props_Position_Layer2.GetChild(fixedLayer2Position[i]).position, Singleton.QI);
        }   

        //1층 랜덤 오브젝트 배치
        for (int i = 0; i < randomLayer1Position.Length; i++) {
            GameObject go = PhotonNetwork.Instantiate("Props/" + randomPropsName[randomLayer1Prop[i]], random_Props_Position_Layer1.GetChild(randomLayer1Position[i]).position, Singleton.QI);
        }

        //2층 랜덤 오브젝트 배치
        for (int i = 0; i < randomLayer2Position.Length; i++) {
            GameObject go = PhotonNetwork.Instantiate("Props/Layer2/" + randomPropsName[randomLayer2Prop[i]], random_Props_Position_Layer2.GetChild(randomLayer2Position[i]).position, Singleton.QI);
        }
    }
    #endregion

    public void SortPlayers() => players.Sort((p1, p2) => p1.actor.CompareTo(p2.actor));
}