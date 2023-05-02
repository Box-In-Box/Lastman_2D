using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyDefenceScript : MonoBehaviour
{
    [SerializeField] float lifeTime;

    public void DefenceActiveFalse() => StartCoroutine(DefenceActiveFalseCoroutine());

    IEnumerator DefenceActiveFalseCoroutine()
    {
        yield return new WaitForSeconds(lifeTime);
        gameObject.SetActive(false);
    }
}
