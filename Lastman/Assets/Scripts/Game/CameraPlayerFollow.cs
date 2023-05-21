using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using static Singleton;

namespace Cainos.PixelArtTopDown_Basic
{
    //let camera follow target
    public class CameraPlayerFollow : MonoBehaviourPun
    {
        public Transform target;
        public float lerpSpeed = 1.0f;

        private Vector3 offset;

        private Vector3 targetPos;

        [SerializeField]
        GameManager GM;
        public GameObject[] Player;

        private void Start()
        {   
            GM = FindObjectOfType<GameManager>();
            SetPlayerPosition();
        }

        void SetPlayerPosition()
        {
            Player = GameObject.FindGameObjectsWithTag("Player");
            
            for (int i = 0; i < Player.Length; i++) {
                if (Player[i].GetComponent<TopDown.PlayerController>().isMinePlayer()) {
                    target = Player[i].transform;
                    transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
                    break;
                } 
            }
            offset = transform.position - target.position;
        }

        private void Update()
        {
            if (target == null) return;

            targetPos = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPos, lerpSpeed * Time.deltaTime);
        }

        public void ChagePlayerView(int key)
        {
            target = GM.players[key].gameObject.transform;
            transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
        }
    }
}
