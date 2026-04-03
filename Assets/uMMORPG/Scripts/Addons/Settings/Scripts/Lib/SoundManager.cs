using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    private List<AudioTypeList> musicSources = new List<AudioTypeList>();
    private List<AudioTypeList> ambientSources = new List<AudioTypeList>();
    private List<AudioTypeList> effectsSources = new List<AudioTypeList>();
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterAudioSource(AudioType audioType)
    {
        AudioTypeList item = new AudioTypeList
        {
            volumeMax = audioType.volumeMax,
            audiotype = audioType
        };
        switch (audioType.category)
        {
            case AudioCategory.Music:
                musicSources.Add(item);
                break;
            case AudioCategory.Ambient:
                ambientSources.Add(item);
                break;
            case AudioCategory.Effects:
                effectsSources.Add(item);
                break;
        }
    }

    public void SetMusicVolume(float volume)
    {
        foreach (var source in musicSources)
        {
            source.audiotype.SetVolume((volume > source.volumeMax) ? source.volumeMax : volume);
        }
    }

    public void SetAmbientVolume(float volume)
    {
        foreach (var source in ambientSources)
        {
            source.audiotype.SetVolume((volume > source.volumeMax) ? source.volumeMax : volume);
        }
    }

    public void SetEffectsVolume(float volume)
    {
        foreach (var source in effectsSources)
        {
            source.audiotype.SetVolume((volume > source.volumeMax) ? source.volumeMax : volume);
        }
    }

    public void MuteAll(bool isMuted)
    {
        foreach (var source in musicSources)
        {
            source.audiotype.Mute(isMuted);
        }
        foreach (var source in ambientSources)
        {
            source.audiotype.Mute(isMuted);
        }
        foreach (var source in effectsSources)
        {
            source.audiotype.Mute(isMuted);
        }
#if _iMMOJUKEBOX
        Jukebox.singleton.Mute(isMuted);
#endif
    }
}

public struct AudioTypeList
{
    public AudioType audiotype;
    public float volumeMax;
}
