using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using static Singleton;

public class ItemPropObject : MonoBehaviourPun
{
    SpriteRenderer spriteRenderer;

    //원래 체력
    [SerializeField] float maxHealth;

    //현제 체력
    [SerializeField] float health;
    public float Health { get => health; set => ActionRPC(nameof(SetHealthRPC), value); }
    [PunRPC] void SetHealthRPC(float value) => health = value;

    void ActionRPC(string functionName, object value)
    {
        photonView.RPC(functionName, RpcTarget.All, value);
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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

    [PunRPC] void DestroyRPC() => Destroy(gameObject);
}
