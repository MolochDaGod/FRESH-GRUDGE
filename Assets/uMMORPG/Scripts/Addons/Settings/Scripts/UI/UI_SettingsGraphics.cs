using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_SettingsGraphics : MonoBehaviour
{
    public UI_Settings uiSettings;

    [Header("[-=-[ Video Settings ]-=-]")]
    public TMP_Dropdown resolutionDropdown;             //Dropdown menu of resolutions.
    public TMP_Dropdown antiAliasing;                   //Dropdown menu of resolutions.
    public TMP_Dropdown vSync;                          //Dropdown menu of resolutions.
    public TMP_Dropdown textureQuality;                 //Dropdown menu of resolutions.
    public TMP_Dropdown anisotropic;                    //Dropdown menu of resolutions.
    public TMP_Dropdown overallQuality;                 //Dropdown menu of resolutions.
    public TMP_Dropdown shadow;                         //Dropdown menu of resolutions.
    public TMP_Dropdown shadowResolution;
    public TMP_Dropdown shadowDistance;
    public TMP_Dropdown shadowCascade;
    public TMP_Dropdown skinWeigth;
    public Toggle fullScreen;
    public Toggle softParticles;
    public Toggle shadowProjection;

    public Resolution[] resolutions;                //Array of possible resolutions.

#if _CLIENT
    private void Awake()
    {
        PopulateDropdowns(); // distance option is in scriptable settings
    }
    
    private void PopulateDropdowns()
    {

        // Populate dropdown resolution
        resolutionDropdown.options.Clear();
        if (uiSettings.settingsBinding.resolution.Length > 0)
        {
            List<string> resolutionOptions = new List<string>();

            foreach (SettingResolution item in uiSettings.settingsBinding.resolution)
            {
                resolutionOptions.Add(item.width.ToString() + " * " + item.height.ToString());
            }
            resolutionDropdown.AddOptions(resolutionOptions);
        }

        // Populate dropdown shadowDistance
        shadowDistance.options.Clear();
        if (uiSettings.settingsBinding.shadowDistance.Length > 0)
        {
            List<string> shadowDistanceOptions = new List<string>();

            foreach (int item in uiSettings.settingsBinding.shadowDistance)
            {
                shadowDistanceOptions.Add(item.ToString());
            }
            shadowDistance.AddOptions(shadowDistanceOptions);
        }

        // Populate dropdown ShadowResolution
        shadowResolution.options.Clear();
        if(uiSettings.settingsBinding.shadowResolutions.Length > 0)
        {
            List<string> shadowResolutionsOptions = new List<string>();

            foreach (ShadowResolution item in uiSettings.settingsBinding.shadowResolutions)
            {
                shadowResolutionsOptions.Add(item.ToString());
            }
            shadowResolution.AddOptions(shadowResolutionsOptions);
        }

        // Populate dropdown shadows
        shadow.options.Clear();
        if (uiSettings.settingsBinding.shadows.Length > 0)
        {
            List<string> shadowResolutionsOptions = new List<string>();

            foreach (ShadowQuality item in uiSettings.settingsBinding.shadows)
            {
                shadowResolutionsOptions.Add(item.ToString());
            }
            shadow.AddOptions(shadowResolutionsOptions);
        }

        // Populate dropdown skinWeights
        skinWeigth.options.Clear();
        if (uiSettings.settingsBinding.shadows.Length > 0)
        {
            List<string> skinWeightsOptions = new List<string>();

            foreach (SkinWeights item in uiSettings.settingsBinding.skinWeights)
            {
                skinWeightsOptions.Add(item.ToString());
            }
            skinWeigth.AddOptions(skinWeightsOptions);
        }
    }

    //Loads all resolutions on enable incase screen swap happens.
    private void OnEnable()
    {
        Player player = Player.localPlayer;                         //Grab the player from utils.
        if (player == null) return;                                 //Don't continue if there is no player found.

        LoadSettingGraphics();
    }

    // Load all of the player video settings.
    public void LoadSettingGraphics()
    {
        overallQuality.value = (PlayerPrefs.HasKey("OverallQuality")) ? PlayerPrefs.GetInt("OverallQuality") : 3;
        textureQuality.value = (PlayerPrefs.HasKey("TextureQuality")) ? PlayerPrefs.GetInt("TextureQuality") : 0;
        anisotropic.value = (PlayerPrefs.HasKey("Anisotropic")) ? PlayerPrefs.GetInt("Anisotropic") : 1;
        antiAliasing.value = (PlayerPrefs.HasKey("AntiAliasing")) ? PlayerPrefs.GetInt("AntiAliasing") : 1;
        softParticles.isOn = !PlayerPrefs.HasKey("SoftParticles") || (PlayerPrefs.GetInt("SoftParticles") == 1);
        shadow.value = (PlayerPrefs.HasKey("Shadows")) ? PlayerPrefs.GetInt("Shadows") : 1;
        shadowResolution.value = (PlayerPrefs.HasKey("ShadowResolution")) ? PlayerPrefs.GetInt("ShadowResolution") : 1;
        shadowProjection.isOn = !PlayerPrefs.HasKey("ShadowProjection") || (PlayerPrefs.GetInt("ShadowProjection") == 1);
        shadowDistance.value = (PlayerPrefs.HasKey("ShadowDistance")) ? PlayerPrefs.GetInt("ShadowDistance") : 3;
        shadowCascade.value = (PlayerPrefs.HasKey("ShadowCascades")) ? PlayerPrefs.GetInt("ShadowCascades") : 1;
        skinWeigth.value = (PlayerPrefs.HasKey("BlendWeights")) ? PlayerPrefs.GetInt("BlendWeights") : 2;
        vSync.value = (PlayerPrefs.HasKey("Vsync")) ? PlayerPrefs.GetInt("Vsync") : 1;
        fullScreen.isOn = !PlayerPrefs.HasKey("Fullscreen") || (PlayerPrefs.GetInt("Fullscreen") == 1);
        resolutionDropdown.value = (PlayerPrefs.HasKey("Resolution")) ? PlayerPrefs.GetInt("Resolution") : resolutionDropdown.options.Count;
    }


    public void SaveVideoSettings()
    {
        SaveResolution();
        SaveFullscreen();
        SaveVsync();
        SaveBlendWeight();
        SaveShadowCascade();
        SaveShadowDistance();
        SaveShadowProjection();
        SaveShadowResolution();
        SaveShadows();
        SaveSoftParticles();
        SaveAntiAliasing();
        SaveAnisotropic();
        SaveTextureQuality();
        uiSettings.ApplySettings();
        uiSettings.ShowHideSetting();
    }

    // Set the vsync level and save its settings.
    private void SaveVsync()
    {
        PlayerPrefs.SetInt("Vsync", vSync.value);
    }

    // Set fullscreen and save its settings.
    private void SaveFullscreen()
    {
        PlayerPrefs.SetInt("Fullscreen", fullScreen.isOn ? 1 : 0);
    }

    // Set the resolution level and save its settings.
    private void SaveResolution()
    {
        PlayerPrefs.SetInt("Resolution", resolutionDropdown.value);
    }

    // Set the blend weight level and save its settings.
    private void SaveBlendWeight()
    {
        PlayerPrefs.SetInt("BlendWeights", skinWeigth.value);
    }

    // Set the shadow cascade level and save its settings.
    private void SaveShadowCascade()
    {
        PlayerPrefs.SetInt("ShadowCascades", shadowCascade.value);
    }

   // Set the shadow distance and save its settings.
    private void SaveShadowDistance()
    {
        if (uiSettings.settingsBinding.shadowDistance.Length > 0 && shadowDistance.value <= uiSettings.settingsBinding.shadowDistance.Length-1)
        {
            PlayerPrefs.SetInt("ShadowDistance", shadowDistance.value);
        }
    }

    // Set the shadow projection level and save its settings.
    private void SaveShadowProjection()
    {
        PlayerPrefs.SetInt("ShadowProjection", shadowProjection.isOn ? 1 : 0);
    }

    // Set the shadow resolution level and save its settings.
    private void SaveShadowResolution()
    {
        PlayerPrefs.SetInt("ShadowResolution", shadowResolution.value);
    }

    // Set the shadows level and save its settings.
    private void SaveShadows()
    {
        PlayerPrefs.SetInt("Shadows", shadow.value);
    }

    // Set the soft particles and save its settings.
    private void SaveSoftParticles()
    {
        PlayerPrefs.SetInt("SoftParticles", softParticles.isOn ? 1 : 0);
    }

    // Set the anti-aliasing level and save its settings.
    private void SaveAntiAliasing()
    {
        PlayerPrefs.SetInt("AntiAliasing", antiAliasing.value);
    }

    // Set the anisotropic level and save its settings.
    private void SaveAnisotropic()
    {
        PlayerPrefs.SetInt("Anisotropic", anisotropic.value);
    }

    // Set the texture quality level and save its settings.
    private void SaveTextureQuality()
    {
        PlayerPrefs.SetInt("TextureQuality", textureQuality.value);
    }

    // Set the overall quality level and save its settings.
    private void SaveOverallQuality()
    {
        Debug.Log("lol...");
       /* switch (overallQuality.value)
        {
            case 0:
                textureQuality.value = textureQuality.options.Count - 1;
                SaveTextureQuality();

                anisotropic.value = 0;
                SaveAnisotropic();

                antiAliasing.value =0;
                SaveAntiAliasing();

                softParticles.isOn = false;
                SaveSoftParticles();

                shadow.value = 0;
                SaveShadows();

                shadowResolution.value = 0;
                SaveShadowResolution();

                shadowProjection.isOn = false;
                SaveShadowProjection();

                shadowDistance.value = 0;
                SaveShadowDistance();

                shadowCascade.value = 0;
                SaveShadowCascade();

                skinWeigth.value = 0;
                SaveBlendWeight();

                vSync.value = 0;
                SaveVsync();

                resolutionDropdown.value = 0;
                SaveResolution();
                break;
            case 1:
                textureQuality.value = (int)Math.Ceiling((double)(textureQuality.options.Count / 2));
                SaveTextureQuality();

                anisotropic.value = (int)Math.Ceiling((double)(anisotropic.options.Count / 2));
                SaveAnisotropic();

                antiAliasing.value = (int)Math.Ceiling((double)(antiAliasing.options.Count / 2));
                SaveAntiAliasing();

                softParticles.isOn = false;
                SaveSoftParticles();

                shadow.value = (int)Math.Ceiling((double)(shadow.options.Count / 2));
                SaveShadows();

                shadowResolution.value = (int)Math.Ceiling((double)(shadowResolution.options.Count / 2));
                SaveShadowResolution();

                shadowProjection.isOn = false;
                SaveShadowProjection();

                shadowDistance.value = (int)Math.Ceiling((double)(shadowDistance.options.Count / 2));
                SaveShadowDistance();

                shadowCascade.value = (int)Math.Ceiling((double)(shadowCascade.options.Count / 2));
                SaveShadowCascade();

                skinWeigth.value = (int)Math.Ceiling((double)(skinWeigth.options.Count / 2));
                SaveBlendWeight();

                vSync.value = (int)Math.Ceiling((double)(vSync.options.Count / 2));
                SaveVsync();

                resolutionDropdown.value = (int)Math.Ceiling((double)(resolutionDropdown.options.Count / 2));
                SaveResolution();
                break;
            case 2:
                textureQuality.value = 0;
                SaveTextureQuality();

                anisotropic.value = anisotropic.options.Count - 1;
                SaveAnisotropic();

                antiAliasing.value = antiAliasing.options.Count - 1;
                SaveAntiAliasing();

                softParticles.isOn = true;
                SaveSoftParticles();

                shadow.value = shadow.options.Count - 1;
                SaveShadows();

                shadowResolution.value = shadowResolution.options.Count - 1;
                SaveShadowResolution();

                shadowProjection.isOn = true;
                SaveShadowProjection();

                shadowDistance.value = shadowDistance.options.Count - 1;
                SaveShadowDistance();

                shadowCascade.value = shadowCascade.options.Count - 1;
                SaveShadowCascade();

                skinWeigth.value = skinWeigth.options.Count - 1;
                SaveBlendWeight();

                vSync.value = vSync.options.Count - 1; ;
                SaveVsync();

                //resolutionDropdown.value = resolutionDropdown.options.Count - 1;
                //Debug.Log("> " + resolutionDropdown.value);
                //SaveResolution();
                break;
            default:
                break;
       
        }
       */
        QualitySettings.SetQualityLevel(overallQuality.value);
        PlayerPrefs.SetInt("OverallQuality", overallQuality.value);
       // LoadSettingVideo();
    }
#endif
}