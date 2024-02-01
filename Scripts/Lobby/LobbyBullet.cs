using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyBullet : MonoBehaviour
{
    public float speed;

    void Start() => Destroy(gameObject, 1f);

    void Update() => transform.Translate(Vector3.right * speed * Time.deltaTime);

    void OnTriggerEnter2D(Collider2D col)
    {
        Destroy(gameObject);
    }
}
