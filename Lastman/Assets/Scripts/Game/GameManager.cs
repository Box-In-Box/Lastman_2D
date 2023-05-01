using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using static Singleton;

public class GameManager : MonoBehaviourPun
{
    public Button GameEndBtn;
    MultiManager multiManager;
    public List<TopDown.PlayerController> players = new List<TopDown.PlayerController>();

    
    public Transform fixedPropPosition_Layer1;
    public Transform randomPropPosition_Layer1;
    [Space (10f)]
    public Transform fixedPropPosition_Layer2;
    public Transform randomPropPosition_Layer2;

    [Space (10f)]
    [SerializeField] string[] fixedPropsName = {"PF Props Chest"};
    [SerializeField] string[] randomPropsName = {"PF Props Crate Small", "PF Props Crate"
                        , "PF Props Pot A", "PF Props Pot B", "PF Props Pot C"};

    bool isFinish;

    void Start()
    {
        multiManager = FindObjectOfType<MultiManager>();
        GameEndBtn.onClick.AddListener(()=> StartCoroutine(multiManager.FinishGame()));
        SettingProps();
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

    public void SortPlayers() => players.Sort((p1, p2) => p1.actor.CompareTo(p2.actor));

    public void SettingProps()
    {   
        if (!singleton.Master())
            return;
        
        //1층 고정 오브젝트 배치
        for (int i = 0; i < fixedPropPosition_Layer1.childCount; i++) {
            int ran = Random.Range(0, fixedPropsName.Length);

            PhotonNetwork.Instantiate("Props/" + fixedPropsName[ran], fixedPropPosition_Layer1.GetChild(i).position, Singleton.QI);
        }

        //2층 고정 오브젝트 배치
        for (int i = 0; i < fixedPropPosition_Layer2.childCount; i++) {
            int ran = Random.Range(0, fixedPropsName.Length);

            GameObject go =  PhotonNetwork.Instantiate("Props/" + fixedPropsName[ran], fixedPropPosition_Layer2.GetChild(i).position, Singleton.QI);
            go.layer = LayerMask.NameToLayer("Layer 2");
            go.GetComponent<SpriteRenderer>().sortingLayerName = "Layer 2";
            go.transform.GetChild(1).gameObject.layer = LayerMask.NameToLayer("Layer 2"); //Floor 레이어 변경
        }

        //1층 랜덤 오브젝트 배치
        for (int i = 0; i < randomPropPosition_Layer1.childCount; i++) {
            int ran = Random.Range(0, randomPropsName.Length * 2);
            
            if (ran < randomPropsName.Length)
                PhotonNetwork.Instantiate("Props/" + randomPropsName[ran], randomPropPosition_Layer1.GetChild(i).position, Singleton.QI);
        }

        //2층 랜덤 오브젝트 배치
        for (int i = 0; i < randomPropPosition_Layer2.childCount; i++) {
            int ran = Random.Range(0, randomPropsName.Length * 2);
            
            if (ran < randomPropsName.Length) {
                GameObject go = PhotonNetwork.Instantiate("Props/" + randomPropsName[ran], randomPropPosition_Layer2.GetChild(i).position, Singleton.QI);
                go.layer = LayerMask.NameToLayer("Layer 2");
                go.GetComponent<SpriteRenderer>().sortingLayerName = "Layer 2";
                go.transform.GetChild(1).gameObject.layer = LayerMask.NameToLayer("Layer 2"); //Floor 레이어 변경
            }
                
        }
    }
}