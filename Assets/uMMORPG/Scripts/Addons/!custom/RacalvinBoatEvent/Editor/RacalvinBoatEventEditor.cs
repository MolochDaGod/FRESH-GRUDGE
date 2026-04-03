#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Grudge;

[CustomEditor(typeof(RacalvinBoatEvent))]
public class RacalvinBoatEventEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RacalvinBoatEvent ev = (RacalvinBoatEvent)target;

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("── GM Controls ──", EditorStyles.boldLabel);

        GUI.enabled = Application.isPlaying && Mirror.NetworkServer.active && !ev.eventActive;
        if (GUILayout.Button("▶  Trigger Event Now (Server)"))
            ev.TriggerNow();

        GUI.enabled = true;

        if (Application.isPlaying)
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField($"Active: {ev.eventActive}   Phase: {ev.eventPhase}",
                EditorStyles.helpBox);
        }
    }
}
#endif
