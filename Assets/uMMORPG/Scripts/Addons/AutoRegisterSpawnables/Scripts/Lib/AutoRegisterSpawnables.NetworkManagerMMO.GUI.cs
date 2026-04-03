using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

#if UNITY_EDITOR

[CustomEditor(typeof(NetworkManagerMMOAutoRegister))]
public class LevelScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NetworkManagerMMOAutoRegister mNetwork = (NetworkManagerMMOAutoRegister)target;
        if (GUILayout.Button("Search and add network prefabs"))
        {
           mNetwork.AutoRegisterSpawnables();
        }
    }
}

#endif
