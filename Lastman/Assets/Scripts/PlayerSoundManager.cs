using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSoundManager : MonoBehaviour
{
    AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayerSFXPlay(string sfxName, AudioClip clip)
    {
        GameObject go = new GameObject(sfxName + "_Sound");
        go.transform.SetParent(this.transform);
        audioSource.clip = clip;
        audioSource.Play();
        Destroy(go, clip.length);
    }
}
