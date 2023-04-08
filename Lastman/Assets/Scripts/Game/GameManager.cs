using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Singleton;

public class GameManager : MonoBehaviour
{
    public Button GameEndBtn;
    MultiManager multiManager;
    public List<TopDown.PlayerController> players = new List<TopDown.PlayerController>();

    bool isFinish;

    void Start()
    {
        multiManager = FindObjectOfType<MultiManager>();
        GameEndBtn.onClick.AddListener(()=> StartCoroutine(multiManager.FinishGame()));
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
}
