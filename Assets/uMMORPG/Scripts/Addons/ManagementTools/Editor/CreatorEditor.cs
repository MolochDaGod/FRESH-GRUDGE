#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(UICreator))]
public class CreatorEditor : Editor
{
    Color dark = new Color32(169, 169, 169, 255);
    Color light = new Color32(211, 211, 211, 255);

    public override void OnInspectorGUI()
    {
        UICreator myScript = (UICreator)target;

        //DrawDefaultInspector();

        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUI.backgroundColor = dark;
        GUILayout.BeginVertical("box");
        GUI.backgroundColor = light;
        myScript.canvas = (Canvas)EditorGUILayout.ObjectField("Canvas", myScript.canvas, typeof(Canvas), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.black;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.categorySlotPrefab = (Button)EditorGUILayout.ObjectField("Category Slot Prefab", myScript.categorySlotPrefab, typeof(Button), true);
        myScript.categoryContent = (Transform)EditorGUILayout.ObjectField("Category Content", myScript.categoryContent, typeof(Transform), true);
        myScript.objectsContent = (Transform)EditorGUILayout.ObjectField("Objects Content", myScript.objectsContent, typeof(Transform), true);
        GUILayout.EndVertical();

        //GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        //GUI.backgroundColor = Color.white;
        myScript.objectHeader = (GameObject)EditorGUILayout.ObjectField("Object Header", myScript.objectHeader, typeof(GameObject), true);
        myScript.currentObject = (TMP_Text)EditorGUILayout.ObjectField("Current Object", myScript.currentObject, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.objectsScrollRect = (ScrollRect)EditorGUILayout.ObjectField("Objects ScrollRect", myScript.objectsScrollRect, typeof(ScrollRect), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.objectSlotPrefab = (UIManagementButton)EditorGUILayout.ObjectField("Object Slot Prefab", myScript.objectSlotPrefab, typeof(UIManagementButton), true);
        GUILayout.EndVertical();

        //GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        //GUI.backgroundColor = Color.white;
        myScript.nameObjectInput = (TMP_InputField)EditorGUILayout.ObjectField("Name Object Input", myScript.nameObjectInput, typeof(TMP_InputField), true);
        GUILayout.EndVertical();

        GUILayout.EndVertical();
        /*
        if (GUILayout.Button("Load objects"))
        {
            // previous solution
        }

        SerializedObject serializedObject = new SerializedObject(myScript);
        EditorGUI.indentLevel = 1;
        SerializedProperty property = serializedObject.FindProperty("creatorCategories");
        serializedObject.Update();

        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical("box");
        GUI.backgroundColor = Color.white;
        EditorGUILayout.PropertyField(property, true);

        GUILayout.EndVertical();

        GUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
        */
        if (GUI.changed)
        {
            EditorSceneManager.MarkSceneDirty(myScript.gameObject.scene);
        }
    }
}
#endif