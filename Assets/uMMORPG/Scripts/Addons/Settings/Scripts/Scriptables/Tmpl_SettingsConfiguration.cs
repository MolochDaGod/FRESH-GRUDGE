using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "ADDON/Templates/Settings Variable Template", order = 999)]
public class Tmpl_SettingsConfiguration : ScriptableObject
{
    [Header("[-=-[ UI Binding ]-=-]")]
    public SettingVariableValue[] uiBinding;

    [Header("[-=-[ SkillBar ]-=-]")]
    public SettingVariableValue[] skillBarBinding;




    [Header("[-=-[ Anisotropic Filtering ]-=-]")]
    public AnisotropicFiltering[] anisotropicFiltering;

    [Header("[-=-[ Shadow Options ]-=-]")]
    public int[] shadowDistance = { 10, 25, 50, 75, 100, 125, 150, 200, 250, 300 };
    public ShadowQuality[] shadows;
    public ShadowResolution[] shadowResolutions;

    public SettingResolution[] resolution;

    [Header("[-=-[ Skin Weight ]-=-]")]
    public SkinWeights[] skinWeights;

    [Header("[-=-[ Other ]-=-]")]
    public bool isShowOverhead = true;
    public bool isShowChat = true;
    public bool isShowPing = true;
    public bool isShowFps = true;
    public bool blockTradeInvit = false;
    public bool blockGuildInvit = false;
    public bool blockPartyInvit = false;

    // Méthode pour obtenir le KeyCode associé à un UIType donné
    public KeyCode GetKeyCodeForUIType(UIType uiType)
    {
        // Vérifie si un PlayerPref existe pour ce UIType
        if (PlayerPrefs.HasKey(uiType.ToString()))
        {
            return (KeyCode)PlayerPrefs.GetInt(uiType.ToString());
        }

        // Recherche dans uiBinding
        var binding = uiBinding.FirstOrDefault(b => b.uIType == uiType);
        if (binding.uIType == uiType) return binding.keyCode;

        // Recherche dans skillBarBinding
        binding = skillBarBinding.FirstOrDefault(b => b.uIType == uiType);
        if (binding.uIType == uiType) return binding.keyCode;

        // Si aucun binding trouvé, retourne KeyCode.None
        return KeyCode.None;
    }

    // Méthode pour sauvegarder un KeyCode pour un UIType donné dans les PlayerPrefs
    public void SaveKeyCodeForUIType(UIType uiType, KeyCode keyCode)
    {
        PlayerPrefs.SetInt(uiType.ToString(), (int)keyCode);
        PlayerPrefs.Save();
    }

    // Méthode pour obtenir une valeur booléenne à partir des PlayerPrefs ou la valeur par défaut
    public bool GetBoolPreference(string key, bool defaultValue)
    {
        if (PlayerPrefs.HasKey(key))
        {
            return PlayerPrefs.GetInt(key) == 1;
        }
        return defaultValue;
    }

    // Méthode pour sauvegarder une valeur booléenne dans les PlayerPrefs
    public void SaveBoolPreference(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
        PlayerPrefs.Save();
    }
}

[Serializable]
public struct SettingVariableValue
{
    public UIType uIType;
    public KeyCode keyCode;
}

[Serializable]
public struct SettingResolution
{
    public int width;
    public int height;
}

public enum UIType
{
    MoveUp,
    MoveDown,
    MoveLeft,
    MoveRight,
    Jump,

    Party,
    Equipment,
    ItemMall,
    Skills,
    Guild,
    Quest,
    CharacterInfo,
    Inventory,
    Crafting,
    SkillBar,
    Fps,
    Ping,

}