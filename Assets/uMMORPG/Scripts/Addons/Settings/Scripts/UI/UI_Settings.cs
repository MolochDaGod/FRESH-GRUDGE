using UnityEngine;
using UnityEngine.UI;

public class UI_Settings : MonoBehaviour
{
    public Tmpl_SettingsConfiguration settingsBinding;
    #region _iMMOMAINMENU

#if _iMMOMAINMENU

#else
    public KeyCode hotKey = KeyCode.Escape;         //The hotkey used to open the settings menu if Main Menu is not used.

#endif

    #endregion _iMMOMAINMENU

    public GameObject panel;                        //Options menu object.


    [Header("[-=-[ UI List ]-=-]")]
    public bool showUIList = false;
    [BoolShowConditional(conditionFieldName: "showUIList", conditionValue: true)]
    public UIParty uiParty;
    [BoolShowConditional(conditionFieldName: "showUIList", conditionValue: true)]
    public UICharacterInfo uiCharacterInfo;
#if !_iMMOCOMPLETECHAT
    [BoolShowConditional(conditionFieldName: "showUIList", conditionValue: true)]
    public UIChat uiChat;
#endif
    [BoolShowConditional(conditionFieldName: "showUIList", conditionValue: true)]
    public UICrafting uiCrafting;
    [BoolShowConditional(conditionFieldName: "showUIList", conditionValue: true)]
    public UIEquipment uiEquipment;
    [BoolShowConditional(conditionFieldName: "showUIList", conditionValue: true)]
    public UIGuild uiGuild;
    [BoolShowConditional(conditionFieldName: "showUIList", conditionValue: true)]
    public UIInventory uiInventory;
    [BoolShowConditional(conditionFieldName: "showUIList", conditionValue: true)]
    public UIItemMall uiItemMall;
    [BoolShowConditional(conditionFieldName: "showUIList", conditionValue: true)]
    public UIQuests uiQuests;
    [BoolShowConditional(conditionFieldName: "showUIList", conditionValue: true)]
    public UISkills uiSkills;
#if _iMMOSKILLCATEGORY
    [BoolShowConditional(conditionFieldName: "showUIList", conditionValue: true)]
    public UI_Skills ui_Skills;
#endif

    [Header("[-=-[ Scalanble panels ]-=-]")]
    public GameObject[] uiScalablePanel;

    [Header("[-=-[ Setting UI hotKey binding ]-=-]")]
    public UI_SettingsGameplay uiSettingsGameplay;

    [Header("[-=-[ Setting UI hotKey binding ]-=-]")]
    public UI_SettingsKeybind uiSettingsKeybind;

    [Header("[-=-[ Video Settings ]-=-]")]
    public UI_SettingsGraphics uiSettingsVideo;

    [Header("[-=-[ Video Settings ]-=-]")]
    public UI_SettingsSound uiSettingsSound;
    [Space(80)]
    [HideInInspector] public bool waitingForKey = false;
    [HideInInspector] public GameObject currentButton;
    [HideInInspector] public KeyCode currentKey = KeyCode.W;

#if _CLIENT
    //Loads all of our settings on start.
    private void Start()
    {
        ApplySettings();
    }

#if !_iMMOMAINMENU
    //Initiates every frame.
    private void Update()
    {
        Player player = Player.localPlayer;                         //Grab the player from utils.
        if (player == null) return;                                 //Don't continue if there is no player found.

        if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())  //If the hotkey is pressed and chat is not active then progress.
        {
            ShowHideSetting();
        }
    }
#endif


    //Close the options menu, all settings save on change.
    /*public void OnApplyClick()
    {
        LoadSettings();
        ShowHideSetting();
    }*/

    public void ShowHideSetting()
    {
        panel.SetActive(!panel.activeSelf);
    }

    // Load all of the player saved settings.
    private void LoadSettings()
    {
        
        uiSettingsKeybind.LoadKeybindings();
        uiSettingsGameplay.LoadSettingsGameplay();
        uiSettingsVideo.LoadSettingGraphics();
        uiSettingsSound.LoadSettingsSound();
    }

    public void ApplySettings()
    {
        GameLog.LogMessage("<color=orange>Apply Settings</color>");
        // Graphics Setting Apply
        if(PlayerPrefs.HasKey("OverallQuality"))
            QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("OverallQuality"));
        if(PlayerPrefs.HasKey("TextureQuality"))
#if UNITY_2022_1_OR_NEWER
            QualitySettings.globalTextureMipmapLimit = PlayerPrefs.GetInt("TextureQuality");
#else 
            QualitySettings.masterTextureLimit = PlayerPrefs.GetInt("TextureQuality");
#endif
        if (PlayerPrefs.HasKey("Anisotropic"))
            QualitySettings.anisotropicFiltering = settingsBinding.anisotropicFiltering[PlayerPrefs.GetInt("Anisotropic")];
        if (PlayerPrefs.HasKey("AntiAliasing"))
            QualitySettings.antiAliasing = PlayerPrefs.GetInt("AntiAliasing");
        if (PlayerPrefs.HasKey("SoftParticles"))
            QualitySettings.softParticles = PlayerPrefs.GetInt("SoftParticles") == 1;
        if (PlayerPrefs.HasKey("Shadows"))
            QualitySettings.shadows = settingsBinding.shadows[PlayerPrefs.GetInt("Shadows")];
        if (PlayerPrefs.HasKey("ShadowResolution"))
            QualitySettings.shadowResolution = settingsBinding.shadowResolutions[PlayerPrefs.GetInt("ShadowResolution")];
        if (PlayerPrefs.HasKey("ShadowProjection"))
            QualitySettings.shadowProjection = PlayerPrefs.GetInt("ShadowProjection") == 1 ? ShadowProjection.StableFit : ShadowProjection.CloseFit;
        if (PlayerPrefs.HasKey("ShadowDistance"))
            QualitySettings.shadowDistance = settingsBinding.shadowDistance[PlayerPrefs.GetInt("ShadowDistance")];
        if (PlayerPrefs.HasKey("ShadowCascades"))
            QualitySettings.shadowCascades = PlayerPrefs.GetInt("ShadowCascades");
        if (PlayerPrefs.HasKey("BlendWeights"))
            QualitySettings.skinWeights = settingsBinding.skinWeights[PlayerPrefs.GetInt("BlendWeights")];
        if (PlayerPrefs.HasKey("Vsync"))
            QualitySettings.vSyncCount = PlayerPrefs.GetInt("Vsync");
        if (PlayerPrefs.HasKey("Fullscreen"))
            Screen.fullScreen = PlayerPrefs.GetInt("Fullscreen") == 1;
        if(PlayerPrefs.HasKey("resolution"))
            Screen.SetResolution(settingsBinding.resolution[PlayerPrefs.GetInt("resolution")].width, settingsBinding.resolution[PlayerPrefs.GetInt("resolution")].height, Screen.fullScreen);

        // Sound
#if _iMMOJUKEBOX
        if(PlayerPrefs.HasKey("MusicLevel"))
            Jukebox.singleton.SetVolume(PlayerPrefs.GetFloat("MusicLevel"));
#endif
        if (PlayerPrefs.HasKey("MusicLevel"))
            SoundManager.instance.SetMusicVolume(PlayerPrefs.GetFloat("MusicLevel"));
        if (PlayerPrefs.HasKey("EffectLevel"))
            SoundManager.instance.SetEffectsVolume(PlayerPrefs.GetFloat("EffectLevel"));
        if (PlayerPrefs.HasKey("AmbientLevel"))
            SoundManager.instance.SetAmbientVolume(PlayerPrefs.GetFloat("AmbientLevel"));
        if (PlayerPrefs.HasKey("SoundMute"))
            SoundManager.instance.MuteAll(PlayerPrefs.GetInt("SoundMute") == 1);

        // keybind
        if (uiParty != null) 
            uiParty.SetHotkey(settingsBinding.GetKeyCodeForUIType(UIType.Party));
        if (uiSkills != null) 
            uiSkills.SetHotkey(settingsBinding.GetKeyCodeForUIType(UIType.Skills));
        if (uiQuests != null) 
            uiQuests.SetHotkey(settingsBinding.GetKeyCodeForUIType(UIType.Quest));
        if (uiCharacterInfo != null) 
            uiCharacterInfo.SetHotkey(settingsBinding.GetKeyCodeForUIType(UIType.CharacterInfo));
        if (uiInventory != null)
            uiInventory.SetHotkey(settingsBinding.GetKeyCodeForUIType(UIType.Inventory));
        if (uiItemMall != null)
            uiItemMall.SetHotkey(settingsBinding.GetKeyCodeForUIType(UIType.ItemMall));
        if (uiCrafting != null)
            uiCrafting.SetHotkey(settingsBinding.GetKeyCodeForUIType(UIType.Crafting));
        if (uiEquipment != null)
            uiEquipment.SetHotkey(settingsBinding.GetKeyCodeForUIType(UIType.Equipment));

        // Gameplay
        //showChat.isOn = ui_Settings.settingsBinding.GetBoolPreference("ShowChat", ui_Settings.settingsBinding.isShowChat);
        //showFPS.isOn = ui_Settings.settingsBinding.GetBoolPreference("ShowFps", ui_Settings.settingsBinding.isShowFps);
        //showPing.isOn = ui_Settings.settingsBinding.GetBoolPreference("ShowPing", ui_Settings.settingsBinding.isShowPing);
        //showOverheads.isOn = ui_Settings.settingsBinding.GetBoolPreference("ShowOverhead", ui_Settings.settingsBinding.isShowOverhead);
        //blockTradeRequest.isOn = ui_Settings.settingsBinding.GetBoolPreference("BlockTrades", ui_Settings.settingsBinding.blockTradeInvit);
        //blockPartiInvit.isOn = ui_Settings.settingsBinding.GetBoolPreference("BlockParty", ui_Settings.settingsBinding.blockPartyInvit);
        //blockGuildInvit.isOn = ui_Settings.settingsBinding.GetBoolPreference("BlockGuild", ui_Settings.settingsBinding.blockGuildInvit);

        for (int i = 0; i < uiScalablePanel.Length; i++)
        {
            uiScalablePanel[i].transform.localScale = new Vector3((PlayerPrefs.HasKey("UiScale")) ? PlayerPrefs.GetFloat("UiScale") : 1, (PlayerPrefs.HasKey("UiScale")) ? PlayerPrefs.GetFloat("UiScale") : 1, 1);
        }
    }
#endif
}
