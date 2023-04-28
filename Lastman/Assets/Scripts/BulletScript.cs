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
    public float speed;

    void Start() => Destroy(gameObject, 3f);

    void Update() => transform.Translate(Vector3.right * speed * Time.deltaTime);

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

        if (col.tag == "ItemProps") {
            col.gameObject.GetComponent<ItemPropObject>().Hit();
            PV.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);
}
