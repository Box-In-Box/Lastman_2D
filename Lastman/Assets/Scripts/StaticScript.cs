using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Serialization<T>
{
    public List<T> target;
    public Serialization(List<T> _target) => target = _target;
}

[System.Serializable]
public class PlayerInfo
{
    public string nickName;
    public int actorNum;
    public int killDeath;
    public double lifeTime;
    public bool isDie;

    public PlayerInfo(string _nickName, int _actorNum, int _killDeath, double _lifeTime, bool _isDie)
    {
        nickName = _nickName;
        actorNum = _actorNum;
        killDeath = _killDeath;
        lifeTime = _lifeTime;
        isDie = _isDie;
    }
}
