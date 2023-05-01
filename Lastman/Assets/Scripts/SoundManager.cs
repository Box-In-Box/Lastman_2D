using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public AudioMixer audioMixer;
    public AudioSource bgmSound;
    public AudioClip[] bgmList;
    public static SoundManager instance;
    
    void Awake()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(this);
            SceneManager.sceneLoaded += OnSceneLoaded;
		}
		else Destroy(gameObject);
	}

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        for (int i = 0; i < bgmList.Length; i++) {
            if (arg0.name == bgmList[i].name)
                BgmSoundPlay(bgmList[i]);
        }
    }

    public void PlayerSFXPlay(AudioSource audioSource, string sfxName, Transform position, AudioClip clip)
    {
        GameObject go = new GameObject(sfxName + "_Sound");
        go.transform.SetParent(position);
        audioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("PlayerSFX")[0];
        audioSource.clip = clip;
        audioSource.Play();
        Destroy(go, clip.length);
    }

    public void SFXPlay(string sfxName, AudioClip clip)
    {
        GameObject go = new GameObject(sfxName + "_Sound");
        go.transform.SetParent(this.transform);
        AudioSource audioSource = go.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];
        audioSource.clip = clip;
        audioSource.Play();

        Destroy(go, clip.length);
    }

    public void BgmSoundPlay(AudioClip clip)
    {
        bgmSound.outputAudioMixerGroup = audioMixer.FindMatchingGroups("BGMSound")[0];
        bgmSound.clip = clip;
        bgmSound.loop = true;
        bgmSound.Play();
    }

    public void MasterSoundVolume(float value)
    {
        audioMixer.SetFloat("Master", Mathf.Log10(value) * 20);
    }

    public void BGMSoundVolume(float value)
    {
        audioMixer.SetFloat("BGMSound", Mathf.Log10(value) * 20);
    }

    public void PlayerSFXSoundVolume(float value)
    {
        audioMixer.SetFloat("PlayerSFX", Mathf.Log10(value) * 20);
    }

    public void SFXSoundVolume(float value)
    {
        audioMixer.SetFloat("SFX", Mathf.Log10(value) * 20);
    }
}
