using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class DefenceScript : MonoBehaviourPunCallbacks
{
    public PhotonView PV;
    [SerializeField] float lifeTime;

    public void DefenceActiveFalse() => StartCoroutine(DefenceActiveFalseCoroutine());

    IEnumerator DefenceActiveFalseCoroutine()
    {
        yield return new WaitForSeconds(lifeTime);
        PV.RPC("ActiveFalseRPC", RpcTarget.AllBuffered);
    }

    [PunRPC] void ActiveFalseRPC() => gameObject.SetActive(false);
}