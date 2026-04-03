using System;
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif
using UnityEngine;

public class MToolsSetupPage : ScriptableObject
{
#if UNITY_EDITOR
    [TextArea(1, 10)]
    public string title;

    [TextArea(1, 10)]
    public string description;

    [TextArea(1, 10)]
    public string path;

    [TextArea(1, 10)]
    public string fileName;

    [Serializable]
    public struct Text
    {
        public bool installed;
        [TextArea(1, 10)]
        public string search, replace;
    }
    [Header("Text")]
    public Text[] text;

    static MToolsSetupPage[] classic2DCache;
    public static MToolsSetupPage[] Classic2DAll => classic2DCache ?? (classic2DCache = FindResources<MToolsSetupPage>("Assets/uMMORPG/Scripts/Addons/ManagementTools/MToolsSetup/Classic/2D"));
    //public static MToolsSetupPage[] Classic2DAll => classic2DCache ?? (classic2DCache = Resources.LoadAll<MToolsSetupPage>("MToolsSetup/Classic/2D").OrderBy(p => p.name.Length).ThenBy(p => p.name).ToArray());

    static MToolsSetupPage[] remastered2DCache;
    public static MToolsSetupPage[] Remastered2DAll => remastered2DCache ?? (remastered2DCache = FindResources<MToolsSetupPage>("Assets/uMMORPG/Scripts/Addons/ManagementTools/MToolsSetup/Remastered/2D"));
    //public static MToolsSetupPage[] Remastered2DAll => remastered2DCache ?? (remastered2DCache = Resources.LoadAll<MToolsSetupPage>("MToolsSetup/Remastered/2D").OrderBy(p => p.name.Length).ThenBy(p => p.name).ToArray());

    static MToolsSetupPage[] classic3DCache;
    public static MToolsSetupPage[] Classic3DAll => classic3DCache ?? (classic3DCache = FindResources<MToolsSetupPage>("Assets/uMMORPG/Scripts/Addons/ManagementTools/MToolsSetup/Classic/3D"));
    //public static MToolsSetupPage[] Classic3DAll => classic3DCache ?? (classic3DCache = Resources.LoadAll<MToolsSetupPage>("MToolsSetup/Classic/3D").OrderBy(p => p.name.Length).ThenBy(p => p.name).ToArray());

    static MToolsSetupPage[] remastered3DCache;
    public static MToolsSetupPage[] Remastered3DAll => remastered3DCache ?? (remastered3DCache = FindResources<MToolsSetupPage>("Assets/uMMORPG/Scripts/Addons/ManagementTools/MToolsSetup/Remastered/3D"));
    //public static MToolsSetupPage[] Remastered3DAll => remastered3DCache ?? (remastered3DCache = Resources.LoadAll<MToolsSetupPage>("MToolsSetup/Remastered/3D").OrderBy(p => p.name.Length).ThenBy(p => p.name).ToArray());

    /// <summary>
    /// Returns all pages from the selected folder required for the installation.
    /// </summary>
    public static T[] FindResources<T>(string path) where T : UnityEngine.Object
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { path });
        return guids.Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid))).ToArray();
    }
#endif
}
