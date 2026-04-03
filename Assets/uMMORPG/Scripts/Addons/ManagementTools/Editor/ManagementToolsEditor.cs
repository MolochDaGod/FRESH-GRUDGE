#if _SQLITE
#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(UIManagement))]
public class ManagementEditor : Editor
{
    Color grey1 = new Color32(204, 204, 204, 255);
    Color grey2 = new Color32(230, 230, 230, 255);

    public override void OnInspectorGUI()
    {        
        UIManagement myScript = (UIManagement)target;

        GUI.backgroundColor = grey1;
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.BeginVertical("box");
        GUI.backgroundColor = grey2;
        myScript.canvas = (Canvas)EditorGUILayout.ObjectField("Canvas", myScript.canvas, typeof(Canvas), true);
        GUILayout.EndVertical();

        EditorGUI.indentLevel = 1;
        SerializedObject serializedPanel = new SerializedObject(myScript);
        SerializedProperty propertyPanel = serializedPanel.FindProperty("allPanels");

        GUI.backgroundColor = grey2;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        EditorGUILayout.PropertyField(propertyPanel, true);        
        GUILayout.EndVertical();

        GUILayout.EndVertical();

        serializedPanel.ApplyModifiedProperties();

        EditorGUI.indentLevel = 0;

        EditorGUILayout.Space();
        #region Players
        EditorGUILayout.LabelField("Players", EditorStyles.boldLabel);

        GUI.backgroundColor = grey1;
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.BeginVertical("box");
        GUI.backgroundColor = grey2;
        myScript.playerContent = (Transform)EditorGUILayout.ObjectField("player Content", myScript.playerContent, typeof(Transform), true);
        myScript.playerSlotPrefab = (Button)EditorGUILayout.ObjectField("player Slot Prefab", myScript.playerSlotPrefab, typeof(Button), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = grey2;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.onlineValueText = (TMP_Text)EditorGUILayout.ObjectField("Online Value", myScript.onlineValueText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = grey2;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.nameInput = (TMP_InputField)EditorGUILayout.ObjectField("Name Input", myScript.nameInput, typeof(TMP_InputField), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = grey2;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.nextButton = (Button)EditorGUILayout.ObjectField("Next Button", myScript.nextButton, typeof(Button), true);
        myScript.prevButton = (Button)EditorGUILayout.ObjectField("Prev Button", myScript.prevButton, typeof(Button), true);
        GUILayout.EndVertical();

        GUILayout.EndVertical();
        #endregion
        EditorGUILayout.Space();
        #region Toolbar
        EditorGUILayout.LabelField("Toolbar", EditorStyles.boldLabel);
        GUI.backgroundColor = grey1;
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUI.backgroundColor = grey2;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.itemsButton = (Button)EditorGUILayout.ObjectField("Items Button", myScript.itemsButton, typeof(Button), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = grey2;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.commandsButton = (Button)EditorGUILayout.ObjectField("Commands Button", myScript.commandsButton, typeof(Button), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = grey2;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.noteButton = (Button)EditorGUILayout.ObjectField("Note Button", myScript.noteButton, typeof(Button), true);
        GUILayout.EndVertical();

        GUILayout.EndVertical();
        #endregion

        if (GUI.changed)
        {
            EditorSceneManager.MarkSceneDirty(myScript.gameObject.scene);
        }
    }
}
#endif
#endif
