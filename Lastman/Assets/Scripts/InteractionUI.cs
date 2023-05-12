using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum UIType { Attack_0, Attack_1, Defence, Damage, Speed };

public class InteractionUI : MonoBehaviour
{
    [SerializeField] UIType uIType;
    [SerializeField] TopDown.PlayerController player;
    [SerializeField] GameManager GM;

    public Text collTimeText;
    public Image imgFill;

    private float time_cooltime;
    private float time_current;
    private float time_start;

    void Start()
    {
        Invoke("SetPlayerStatus", 1.0f);
    }

    void SetPlayerStatus()
    {
        //내 플레이어 설정
        for (int i = 0; i < GM.players.Count; i++) {
            if (GM.players[i].isMinePlayer()) {
                player = GM.players[i];
                break;
            } 
        }

        imgFill.fillAmount = 0f;
    }

    void Update()
    {   
        if (player == null)
            return;

        switch(uIType) {
            case UIType.Attack_0 :
                collTimeText.text = player.attackDelay0.ToString();
                if (!player.attackable0) CoolTime(player.attackDelay0);
                else End_CoolTime(player.attackDelay0);
                break;
            case UIType.Attack_1 :
                collTimeText.text = player.attackDelay1.ToString();
                if (!player.attackable1) CoolTime(player.attackDelay1);
                else End_CoolTime(player.attackDelay1);
                break;
            case UIType.Defence :
                collTimeText.text = player.defenceDelay.ToString();
                if (!player.defensible) CoolTime(player.defenceDelay);
                else End_CoolTime(player.defenceDelay);
                break;
            case UIType.Damage : collTimeText.text = player.Damage.ToString();
                break;
            case UIType.Speed : collTimeText.text = player.Speed.ToString();
                break;
        }      
    }

    private void CoolTime(float collTime)
    {
        imgFill.gameObject.SetActive(true);
        time_cooltime = collTime;
        time_current = Time.time - time_start;
        if (time_current < collTime)
            Set_FillAmount(collTime - time_current);
    }

    private void End_CoolTime(float collTime)
    {
        Set_FillAmount(0);
        imgFill.gameObject.SetActive(false);
        collTimeText.text = collTime.ToString();
        time_current = time_cooltime;
        time_start = Time.time;
    }

     private void Set_FillAmount(float _value)
    {
        imgFill.fillAmount = _value/time_cooltime;
        string txt = _value.ToString("0.0");
        collTimeText.text = txt;
    }
}
