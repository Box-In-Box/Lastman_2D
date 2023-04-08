using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Tilemaps;

public class BulletScript : MonoBehaviourPunCallbacks
{
    public PhotonView PV;
    public SpriteRenderer SR;
    Vector3 dir;
    public float speed;

    void Start() => Destroy(gameObject, 3f);

    void Update() => transform.Translate(dir * speed * Time.deltaTime);

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Wall" || col.tag == "Props") {
            if (col.TryGetComponent(out SpriteRenderer colRenderer)) {
                if (colRenderer.sortingLayerID == SR.sortingLayerID)
                    PV.RPC("DestroyRPC", RpcTarget.AllBuffered);
            }
            if (col.TryGetComponent(out TilemapRenderer colTileRenderer)) {
                if (colTileRenderer.sortingLayerID == SR.sortingLayerID)
                    PV.RPC("DestroyRPC", RpcTarget.AllBuffered);
            }
        }
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
