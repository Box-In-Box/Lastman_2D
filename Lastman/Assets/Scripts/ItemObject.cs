using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public enum itemType {AttackDelay0_Down, AttackDelay1_Down
                    , Damage_Up ,defenceDelay_Down
                    , Health_Up, Speed_Up};
public class ItemObject : MonoBehaviourPun
{
    public itemType itemType;
    public float value;

    void OnTriggerEnter2D(Collider2D col) {
        if (col.tag == "Player")
            photonView.RPC("DestroyRPC", RpcTarget.AllBuffered);
    }

    [PunRPC] void DestroyRPC() => Destroy(gameObject);
}
