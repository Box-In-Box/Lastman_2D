using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class BulletScript : MonoBehaviourPunCallbacks
{
    public PhotonView PV;
    Vector3 dir;
    public float speed;

    void Start() => Destroy(gameObject, 3f);

    void Update() => transform.Translate(dir * speed * Time.deltaTime);

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Wall" || col.tag == "Props")
            PV.RPC("DestroyRPC", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void DirRPC(int dir)
    {
        switch(dir) {
            case 0: //down
                this.dir = Vector3.down;
                break;
            case 1: //up
                this.dir = Vector3.up;
                break;
            case 2: //right
                this.dir = Vector3.right;
                break;
            case 3: //left
                this.dir = Vector3.left;
                break;
        }
    } 

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);
}
