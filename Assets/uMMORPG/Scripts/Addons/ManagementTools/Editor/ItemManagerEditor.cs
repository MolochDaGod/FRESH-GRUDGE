#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(UIItemManager))]
public class ItemManagerEditor : Editor
{
    Color dark = new Color32(169, 169, 169, 255);
    Color light = new Color32(211, 211, 211, 255);

    public override void OnInspectorGUI()
    {
        UIItemManager myScript = (UIItemManager)target;

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
        myScript.itemsContent = (Transform)EditorGUILayout.ObjectField("Items Content", myScript.itemsContent, typeof(Transform), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.itemsScrollRect = (ScrollRect)EditorGUILayout.ObjectField("Items ScrollRect", myScript.itemsScrollRect, typeof(ScrollRect), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.itemSlotPrefab = (UIItemManagerSlot)EditorGUILayout.ObjectField("Item Slot Prefab", myScript.itemSlotPrefab, typeof(UIItemManagerSlot), true);
        GUILayout.EndVertical();

        //GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        //GUI.backgroundColor = Color.white;
        myScript.nameItemInput = (TMP_InputField)EditorGUILayout.ObjectField("Name Item Input", myScript.nameItemInput, typeof(TMP_InputField), true);
        GUILayout.EndVertical();

        GUILayout.EndVertical();
        /*
        if (GUILayout.Button("Load templates"))
        {
            // previous solution
        }

        EditorGUI.indentLevel = 1;
        SerializedObject serializedObject = new SerializedObject(myScript);
        SerializedProperty property = serializedObject.FindProperty("itemManagerCategories");

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