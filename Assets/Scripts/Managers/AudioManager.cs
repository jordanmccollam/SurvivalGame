using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;

public class AudioManager : MonoBehaviour
{
    public string mixer;
    public AudioMixerGroup musicMixer;
    public AudioMixerGroup soundMixer;
    public bool playMainMusic;
    public Sound[] sounds;
    public int volumeRef;

    public static AudioManager musicInstance { get; private set; }
    public static AudioManager soundInstance { get; private set; }


    // Start is called before the first frame update
    void Awake()
    {
        switch (mixer)
        {
            case "sounds":
                if (mixer == "sounds" && soundInstance == null) {
                    soundInstance = this;
                    DontDestroyOnLoad(gameObject);
                } else if (soundInstance != this) {
                    Destroy(gameObject);
                }
                break;
            case "music":
                if (mixer == "music" && musicInstance == null) {
                    musicInstance = this;
                    DontDestroyOnLoad(gameObject);
                } else if (musicInstance != this) {
                    Destroy(gameObject);
                }
                break;
            default:
                break;
        }

        foreach (Sound s in sounds) {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.panStereo = s.panning;

            if (gameObject.tag == "AudioManager") {
                s.source.outputAudioMixerGroup = soundMixer;
            }
            else if (gameObject.tag == "MusicManager") {
                s.source.outputAudioMixerGroup = musicMixer;
            }
        }

        // Loop("music");
    }

    private void Start() {
        if (playMainMusic) {
            Loop("main");
        }
    }

    public void Play (string name) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.PlayOneShot(s.clip);
    }

    public void Loop (string name) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Play(0);
    }

    public void Stop (string sound)
    {
        Sound s = Array.Find(sounds, item => item.name == sound);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        s.source.Stop ();
    }

    public void SetVolume(string sound, float volume) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.volume = volume;
    }
}
