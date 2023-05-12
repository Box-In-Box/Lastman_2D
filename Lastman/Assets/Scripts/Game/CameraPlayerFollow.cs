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

        private void Start()
        {   
            SetPlayerPosition();
        }

        void SetPlayerPosition()
        {
            GameObject[] Player = GameObject.FindGameObjectsWithTag("Player");
            
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

    }
}
