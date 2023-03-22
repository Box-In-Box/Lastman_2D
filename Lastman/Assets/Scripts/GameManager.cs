using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Singleton;

public class GameManager : MonoBehaviour
{
    public Button GameEndBtn;

    void Start()
    {
        GameEndBtn.onClick.AddListener(()=> singleton.GameEnd());
    }
}
