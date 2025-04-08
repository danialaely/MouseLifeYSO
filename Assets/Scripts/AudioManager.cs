using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class AudioManager : MonoBehaviour
{

    public static AudioManager instance;

    public Sound[] musicsounds, sfxSounds, creakingsounds;
    public AudioSource musicSource, sfxSource, creakingSource;

    public bool continuedFromGame;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
           // DontDestroyOnLoad(gameObject);
            continuedFromGame = false;
        }
        else
        {
            //Destroy(gameObject);
        }
    }

    public void PlayMusic(String name)
    {
        Sound s = Array.Find(musicsounds, x => x.mName == name);

        if (s == null)
        {
            Debug.Log("Sound Not Found");
        }
        else
        {
            musicSource.clip = s.clip;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void PlaySFX(string name)
    {
        Sound s = Array.Find(sfxSounds, x => x.mName == name);

        if (s == null)
        {
            Debug.Log("Sfx Not Found");
        }
        else
        {
            sfxSource.PlayOneShot(s.clip);
        }
    }

    public void StopSFX()
    {
        if (sfxSource != null)
        {
            sfxSource.Stop();
        }
    }

    public void PlayCreaking(string name)
    {
        Sound s = Array.Find(creakingsounds, x => x.mName == name);

        if (s == null)
        {
            Debug.Log("Sfx Not Found");
        }
        else
        {
            creakingSource.PlayOneShot(s.clip);
        }
    }

    public void ToggleMusic()
    {
        if (musicSource != null)
        {
            musicSource.mute = !musicSource.mute;
        }
    }


}
