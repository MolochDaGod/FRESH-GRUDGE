#if _SQLITE
#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(UIDatabaseInfo))]
public class DatabaseInfoEditor : Editor
{
    Color dark = new Color32(169, 169, 169, 255);
    Color light = new Color32(211, 211, 211, 255);

    public override void OnInspectorGUI()
    {
        UIDatabaseInfo myScript = (UIDatabaseInfo)target;

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
        myScript.deleteAccountsButton = (Button)EditorGUILayout.ObjectField("Delete Accounts Button", myScript.deleteAccountsButton, typeof(Button), true);
        GUILayout.EndVertical();

        //GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        //GUI.backgroundColor = Color.white;
        myScript.totalAccountsText = (TMP_Text)EditorGUILayout.ObjectField("Total Accounts Text", myScript.totalAccountsText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.bannedAccountsText = (TMP_Text)EditorGUILayout.ObjectField("Banned Accounts Text", myScript.bannedAccountsText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        //GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        //GUI.backgroundColor = Color.white;
        myScript.playerAccountsText = (TMP_Text)EditorGUILayout.ObjectField("player Accounts Text", myScript.playerAccountsText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.staffAccountsText = (TMP_Text)EditorGUILayout.ObjectField("Staff Accounts Text", myScript.staffAccountsText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        EditorGUILayout.Space(20);

        //GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        //GUI.backgroundColor = Color.white;
        myScript.playersText = (TMP_Text)EditorGUILayout.ObjectField("Players Text", myScript.playersText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.maxGoldText = (TMP_Text)EditorGUILayout.ObjectField("Max Gold Text", myScript.maxGoldText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        //GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        //GUI.backgroundColor = Color.white;
        myScript.spawnersText = (TMP_Text)EditorGUILayout.ObjectField("Spawners Text", myScript.spawnersText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.checkpointsText = (TMP_Text)EditorGUILayout.ObjectField("Checkpoints Text", myScript.checkpointsText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        //GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        //GUI.backgroundColor = Color.white;
        myScript.verifiedBugsText = (TMP_Text)EditorGUILayout.ObjectField("Verified Bugs Text", myScript.verifiedBugsText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.fixedBugsText = (TMP_Text)EditorGUILayout.ObjectField("Fixed Bugs Text", myScript.fixedBugsText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        //GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        //GUI.backgroundColor = Color.white;
        myScript.helpRequestsText = (TMP_Text)EditorGUILayout.ObjectField("Help Requests Text", myScript.helpRequestsText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;        
        myScript.monstersText = (TMP_Text)EditorGUILayout.ObjectField("Monsters Text", myScript.monstersText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        //GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        //GUI.backgroundColor = Color.white;      
        myScript.npcsText = (TMP_Text)EditorGUILayout.ObjectField("Npcs Text", myScript.npcsText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.environmentsText = (TMP_Text)EditorGUILayout.ObjectField("Environments Text", myScript.environmentsText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        //GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        //GUI.backgroundColor = Color.white;
        myScript.itemsText = (TMP_Text)EditorGUILayout.ObjectField("Items Text", myScript.itemsText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        GUILayout.EndVertical();

        EditorGUILayout.Space();
        #region Item Drop
        EditorGUILayout.LabelField("Item Drop", EditorStyles.boldLabel);
        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        /*
        GUILayout.BeginVertical("box");
        GUI.backgroundColor = Color.gray;
        myScript.ItemDrop = EditorGUILayout.Toggle("Is active?", myScript.ItemDrop);
        GUILayout.EndVertical();
        */
        GUI.backgroundColor = Color.black;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.itemsRow = (GameObject)EditorGUILayout.ObjectField("Items Row", myScript.itemsRow, typeof(GameObject), true);
        GUILayout.EndVertical();

        GUILayout.EndVertical();
        #endregion

        if (GUI.changed)
        {
            //myScript.itemsRow.SetActive(myScript.ItemDrop);

            EditorSceneManager.MarkSceneDirty(myScript.gameObject.scene);
        }
    }
}
#endif
#endif
