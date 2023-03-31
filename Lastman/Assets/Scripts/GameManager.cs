using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Singleton;

public class GameManager : MonoBehaviour
{
    public Button GameEndBtn;
    MultiManager multiManager;

    void Start()
    {
        multiManager = FindObjectOfType<MultiManager>();
        GameEndBtn.onClick.AddListener(()=> StartCoroutine(multiManager.FinishGame()));
    }
}
