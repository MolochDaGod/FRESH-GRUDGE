#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MToolsSettings))]
public class MToolsSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        MToolsSettings script = (MToolsSettings)target;

        GUILayout.Space(16);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Restore Defaults"))
        {
            script.DefaultSettings();

            EditorUtility.SetDirty(script);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }
}
#endif