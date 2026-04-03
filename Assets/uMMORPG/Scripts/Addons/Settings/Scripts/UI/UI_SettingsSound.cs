using UnityEngine;
using UnityEngine.UI;

public class UI_SettingsSound : MonoBehaviour
{
    public UI_Settings uiSettings;
    [Header("[-=-[ Sound Settings ]-=-]")]
    public Slider musicLevel;                   //Sliders for all sound volume.
    public Slider effectLevel;                   //Sliders for all sound volume.
    public Slider ambientLevel;                   //Sliders for all sound volume.
    public Toggle soundToggles;                   //Togggles for all sound volume.

#if _CLIENT
    // Load all of the player sound settings.
    public void LoadSettingsSound()
    {
        musicLevel.value = (PlayerPrefs.HasKey("MusicLevel")) ? PlayerPrefs.GetFloat("MusicLevel") : .50f;
        effectLevel.value = (PlayerPrefs.HasKey("EffectLevel")) ? PlayerPrefs.GetFloat("EffectLevel") : .50f;
        ambientLevel.value = (PlayerPrefs.HasKey("AmbientLevel")) ? PlayerPrefs.GetFloat("AmbientLevel") : .50f;
        soundToggles.isOn = ((PlayerPrefs.HasKey("SoundMute") && PlayerPrefs.GetInt("SoundMute") == 1));
    }

    // Set the music level and save its settings.
    public void SaveMusicLevel()
    {
        PlayerPrefs.SetFloat("MusicLevel", musicLevel.value);
        uiSettings.ApplySettings();
    }

    // Set the effects level and save its settings.
    public void SaveEffectLevel()
    {
        PlayerPrefs.SetFloat("EffectLevel", effectLevel.value);
        uiSettings.ApplySettings();
    }

    // Set the ambient level and save its settings.
    public void SaveAmbientLevel()
    {
        PlayerPrefs.SetFloat("AmbientLevel", ambientLevel.value);
        uiSettings.ApplySettings();
    }

    // Set sound mute and save its settings.
    public void SaveSoundMute()
    {
        PlayerPrefs.SetInt("SoundMute", soundToggles.isOn ? 1 : 0);
        uiSettings.ApplySettings();
    }
#endif
}
