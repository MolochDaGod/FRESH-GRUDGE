#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

// ADDON

[Serializable]
public partial class AddOn
{
    [HideInInspector] public string name;
    [HideInInspector] public string define;
    [ReadOnly] public string basis;
    [ReadOnly] public string author;
    [ReadOnly] public string version;
    [ReadOnly] public string dependencies;
    [ReadOnly] [TextArea(1, 30)]public string comments;
    public bool active;

    public void Copy(AddOn addon)
    {
        name            = addon.name;
        define          = addon.define;
        basis           = addon.basis;
        author          = addon.author;
        version         = addon.version;
        dependencies    = addon.dependencies;
        comments        = addon.comments;
        active          = addon.active;
    }

}

[InitializeOnLoad]
public static partial class DefineSymbols_EditorTools
{

    public static string[] GetListDefine()
    {
#if UNITY_2021_1_OR_NEWER
        NamedBuildTarget currentNameBuildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        string definesList = PlayerSettings.GetScriptingDefineSymbols(currentNameBuildTarget);
        string[] defines = definesList.Split(';');
#else
        BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        string definestring = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        string[] defines = definestring.Split(';');
#endif
        return defines;
    }

    // -------------------------------------------------------------------------------
    // AddScriptingDefine
    // -------------------------------------------------------------------------------
    public static void AddScriptingDefine(string define)
    {
#if UNITY_2021_1_OR_NEWER
        if (ArrayContains(GetListDefine(), define)) return;
        string definesList = string.Join(";", GetListDefine());
        EditDefineForAll((definesList + ";" + define));
#else
        BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        if (UMMO_Tools.ArrayContains(GetListDefine(), define)) return;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, (string.Join(";", GetListDefine()) + ";" + define));
#endif
    }

    // -------------------------------------------------------------------------------
    // RemoveScriptingDefine
    // -------------------------------------------------------------------------------
    public static void RemoveScriptingDefine(string define)
    {
#if UNITY_2021_1_OR_NEWER
        string[] defines = RemoveFromArray(GetListDefine(), define);
        string definesList = string.Join(";", defines);
        EditDefineForAll(definesList);
#else
        BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        string[] defines = UMMO_Tools.RemoveFromArray(GetListDefine(), define);
        string definestring = string.Join(";", defines);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, (definestring));
#endif
    }

#if UNITY_2021_1_OR_NEWER
    private static void EditDefineForAll(string definesList)
    {
        List<NamedBuildTarget> list = new();
        list.Add(NamedBuildTarget.Android);
        list.Add(NamedBuildTarget.EmbeddedLinux);
        list.Add(NamedBuildTarget.iOS);
        list.Add(NamedBuildTarget.LinuxHeadlessSimulation);
        //list.Add(NamedBuildTarget.NintendoSwitch);
        //list.Add(NamedBuildTarget.PS4);
        list.Add(NamedBuildTarget.Server);
        //list.Add(NamedBuildTarget.Stadia);
        list.Add(NamedBuildTarget.Standalone);
        //list.Add(NamedBuildTarget.tvOS);
        //list.Add(NamedBuildTarget.Unknown);
        list.Add(NamedBuildTarget.WebGL);
        //list.Add(NamedBuildTarget.WindowsStoreApps);
        //list.Add(NamedBuildTarget.XboxOne);

        foreach (NamedBuildTarget item in list)
        {
            try
            {
                PlayerSettings.SetScriptingDefineSymbols(item, (definesList));
            }
            catch (Exception e)
            {
                Debug.LogError("Sorry " + e + "is not valid");
            }
        }
    }
#endif


    public static bool ArrayContains(string[] defines, string define)
    {
        foreach (string def in defines)
        {
            if (def == define)
                return true;
        }
        return false;
    }

    public static string[] RemoveFromArray(string[] defines, string define)
    {
        return defines.Where(x => x != define).ToArray();
    }
    // -----------------------------------------------------------------------------------
}

#endif