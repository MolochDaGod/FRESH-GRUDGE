using UnityEngine;

public enum AudioCategory
{
    Music,
    Ambient,
    Effects
}

[RequireComponent(typeof(AudioSource))]
public class AudioType : MonoBehaviour
{
    public AudioCategory category;
    private AudioSource audioSource;
    [Range(0f, 1f)] public float volumeMax = 1;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        SoundManager.instance.RegisterAudioSource(this);
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }

    public void Mute(bool isMuted)
    {
        audioSource.mute = isMuted;
    }
}
