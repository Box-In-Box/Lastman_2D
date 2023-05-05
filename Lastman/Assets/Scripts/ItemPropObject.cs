using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using static Singleton;

public class ItemPropObject : MonoBehaviourPun
{
    GameManager gameManager;
    SpriteRenderer spriteRenderer;

    //원래 체력
    [SerializeField] float maxHealth;

    //현제 체력
    [SerializeField] float health;
    public float Health { get => health; set => ActionRPC(nameof(SetHealthRPC), value); }
    [PunRPC] void SetHealthRPC(float value) => health = value;

    //아이템소품인지
    
    [SerializeField] bool isFixedItemProps;
    [SerializeField] bool isItemProps;
    [SerializeField] GameObject item;

    void ActionRPC(string functionName, object value)
    {
        photonView.RPC(functionName, RpcTarget.All, value);
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        if(singleton.Master()) SetRandomItem();
    }

    void SetRandomItem()
    {
        if (maxHealth >= 500) {
            int itemNum = Random.Range(0, gameManager.fixedItem.Length);
            photonView.RPC("ItemSettingRPC", RpcTarget.AllBuffered, itemNum, true);
        }
        else {
            if (Random.Range(0, 10) < 3) {
                int itemNum = Random.Range(0, gameManager.item.Length);
                photonView.RPC("ItemSettingRPC", RpcTarget.AllBuffered, itemNum, false);
            }
        }
    }

    [PunRPC]
    IEnumerator ItemSettingRPC(int itemNum, bool isFixed)
    {
        yield return null;

        isFixedItemProps = isFixed;
        isItemProps = true;
        
        if (isFixed == true) //고정오브젝트 아이템
            item = gameManager.fixedItem[itemNum];
        
        else //랜덤오브젝트 아이템
            item = gameManager.item[itemNum];

    }

    public void Hit(float damage)
    {
        Health -= damage;

        photonView.RPC("SetRGB", RpcTarget.AllBuffered);

        if (Health <= 0)
            photonView.RPC("DestroyRPC", RpcTarget.AllBuffered);
    }

    [PunRPC] void SetRGB()
    {
        float rgb = 128 * (Health / maxHealth) + 128;
        rgb /= 256; 

        spriteRenderer.color = new Color(rgb, rgb, rgb);
    }

    [PunRPC] void DestroyRPC()
    {
        if (singleton.Master() && isItemProps && isFixedItemProps) {
            if (GetComponent<SpriteRenderer>().sortingLayerName == "Layer 1")
                PhotonNetwork.Instantiate("Item/FixedItem/" + item.name.ToString(), transform.position, transform.rotation);
            else if (GetComponent<SpriteRenderer>().sortingLayerName == "Layer 2")
                PhotonNetwork.Instantiate("Item/FixedItem/Layer2/" + item.name.ToString(), transform.position, transform.rotation);
            
        }
        else if (singleton.Master() && isItemProps && !isFixedItemProps) {
            if (GetComponent<SpriteRenderer>().sortingLayerName == "Layer 1")
                PhotonNetwork.Instantiate("Item/" + item.name.ToString(), transform.position, transform.rotation);
            else if (GetComponent<SpriteRenderer>().sortingLayerName == "Layer 2")
                PhotonNetwork.Instantiate("Item/Layer2/" + item.name.ToString(), transform.position, transform.rotation);
            
        }
        Destroy(gameObject);
    }
}
