using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public AudioMixer audioMixer;

    public Sound[] sounds;

    RestartManager restartManager;

    private void Awake()
    {
        restartManager = FindObjectOfType<RestartManager>();
        restartManager.DontDestroyOnLoadButDestroyWhenRestarting(gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            if (s.isMusic)
                s.source.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Music")[0];
            else
                s.source.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Sounds")[0];

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if(s == null)
        {
            Debug.LogWarning("Sound: \"" + name + "\" not found, aborting sound play");
            return;
        }
        s.source.Play();
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: \"" + name + "\" not found, aborting sound stop");
            return;
        }
        s.source.Stop();
    }
}
