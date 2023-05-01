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
    public float damage;

    public void SetDamage(float damage) => PV.RPC("SetDamageRPC", RpcTarget.AllBuffered, damage);
    [PunRPC] void SetDamageRPC(float _damage) => damage = _damage;

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
        else if (col.tag == "ItemProps") {
            if (col.TryGetComponent(out SpriteRenderer colRenderer)) {
                if (colRenderer.sortingLayerID == SR.sortingLayerID) {
                    col.gameObject.GetComponent<ItemPropObject>().Hit(damage);
                    PV.RPC("DestroyRPC", RpcTarget.AllBuffered);
                }
            }
        }
        else if (col.tag == "Defence") {
            damage = 0; //방어 후 플레이어 히트 판정 수정용
            if (PV.Owner != col.GetComponent<PhotonView>().Owner)
            PV.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void DestroyRPC()
    {
        gameObject.SetActive(false);
        Destroy(gameObject, 0.2f);
    } 
}
