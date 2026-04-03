#if _SQLITE
#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(UICommands))]
public class CommandsEditor : Editor
{
    Color dark = new Color32(169, 169, 169, 255);
    Color light = new Color32(211, 211, 211, 255);

    public override void OnInspectorGUI()
    {
        UICommands myScript = (UICommands)target;

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
        myScript.banButton = (Button)EditorGUILayout.ObjectField("Ban Button", myScript.banButton, typeof(Button), true);
        myScript.downButton = (Button)EditorGUILayout.ObjectField("Down Button", myScript.downButton, typeof(Button), true);
        myScript.upButton = (Button)EditorGUILayout.ObjectField("Up Button", myScript.upButton, typeof(Button), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.immortalButton = (Button)EditorGUILayout.ObjectField("Immortal Button", myScript.immortalButton, typeof(Button), true);
        myScript.immortalButtonText = (TMP_Text)EditorGUILayout.ObjectField("Immortal Button Text", myScript.immortalButtonText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.black;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.hideButton = (Button)EditorGUILayout.ObjectField("Hide Button", myScript.hideButton, typeof(Button), true);
        myScript.hideButtonText = (TMP_Text)EditorGUILayout.ObjectField("Hide Button Text", myScript.hideButtonText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.freezeButton = (Button)EditorGUILayout.ObjectField("Freeze Button", myScript.freezeButton, typeof(Button), true);
        myScript.freezeButtonText = (TMP_Text)EditorGUILayout.ObjectField("Freeze Button Text", myScript.freezeButtonText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.black;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.muteButton = (Button)EditorGUILayout.ObjectField("Mute Button", myScript.muteButton, typeof(Button), true);
        myScript.muteButtonText = (TMP_Text)EditorGUILayout.ObjectField("Mute Button Text", myScript.muteButtonText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.kickButton = (Button)EditorGUILayout.ObjectField("Kick Button", myScript.kickButton, typeof(Button), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.black;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.killButton = (Button)EditorGUILayout.ObjectField("Kill Button", myScript.killButton, typeof(Button), true);
        myScript.killButtonText = (TMP_Text)EditorGUILayout.ObjectField("Kill Button Text", myScript.killButtonText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.fetchButton = (Button)EditorGUILayout.ObjectField("Fetch Button", myScript.fetchButton, typeof(Button), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.black;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.goButton = (Button)EditorGUILayout.ObjectField("Go Button", myScript.goButton, typeof(Button), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.goldButton = (Button)EditorGUILayout.ObjectField("Gold Button", myScript.goldButton, typeof(Button), true);
        myScript.goldInput = (TMP_InputField)EditorGUILayout.ObjectField("Gold Input", myScript.goldInput, typeof(TMP_InputField), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.black;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.coinsButton = (Button)EditorGUILayout.ObjectField("Coins Button", myScript.coinsButton, typeof(Button), true);
        myScript.coinsInput = (TMP_InputField)EditorGUILayout.ObjectField("Coins Input", myScript.coinsInput, typeof(TMP_InputField), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.expButton = (Button)EditorGUILayout.ObjectField("Exp Button", myScript.expButton, typeof(Button), true);
        myScript.expInput = (TMP_InputField)EditorGUILayout.ObjectField("Exp Input", myScript.expInput, typeof(TMP_InputField), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.black;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.levelButton = (Button)EditorGUILayout.ObjectField("Level Button", myScript.levelButton, typeof(Button), true);
        myScript.levelInput = (TMP_InputField)EditorGUILayout.ObjectField("Level Input", myScript.levelInput, typeof(TMP_InputField), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.strengthButton = (Button)EditorGUILayout.ObjectField("Strength Button", myScript.strengthButton, typeof(Button), true);
        myScript.strengthInput = (TMP_InputField)EditorGUILayout.ObjectField("Strength Input", myScript.strengthInput, typeof(TMP_InputField), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.black;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.intelligenceButton = (Button)EditorGUILayout.ObjectField("Intelligence Button", myScript.intelligenceButton, typeof(Button), true);
        myScript.intelligenceInput = (TMP_InputField)EditorGUILayout.ObjectField("Intelligence Input", myScript.intelligenceInput, typeof(TMP_InputField), true);
        GUILayout.EndVertical();

        EditorGUILayout.Space(20);

        //GUI.backgroundColor = Color.white;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        //GUI.backgroundColor = Color.white;
        myScript.nameText = (TMP_Text)EditorGUILayout.ObjectField("Name", myScript.nameText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.healthSlider = (Slider)EditorGUILayout.ObjectField("Health Slider", myScript.healthSlider, typeof(Slider), true);
        myScript.healthText = (TMP_Text)EditorGUILayout.ObjectField("Health", myScript.healthText, typeof(TMP_Text), true);
        myScript.healthMaxText = (TMP_Text)EditorGUILayout.ObjectField("Health Max", myScript.healthMaxText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        //GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        //GUI.backgroundColor = Color.white;
        myScript.manaSlider = (Slider)EditorGUILayout.ObjectField("Mana Slider", myScript.manaSlider, typeof(Slider), true);
        myScript.manaText = (TMP_Text)EditorGUILayout.ObjectField("Mana", myScript.manaText, typeof(TMP_Text), true);
        myScript.manaMaxText = (TMP_Text)EditorGUILayout.ObjectField("Mana Max", myScript.manaMaxText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.accountText = (TMP_Text)EditorGUILayout.ObjectField("Account", myScript.accountText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        //GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        //GUI.backgroundColor = Color.white;
        myScript.accessText = (TMP_Text)EditorGUILayout.ObjectField("Access", myScript.accessText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.classText = (TMP_Text)EditorGUILayout.ObjectField("Class", myScript.classText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        //GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        //GUI.backgroundColor = Color.white;
        myScript.levelText = (TMP_Text)EditorGUILayout.ObjectField("Level", myScript.levelText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.coinsText = (TMP_Text)EditorGUILayout.ObjectField("Coins", myScript.coinsText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        //GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        //GUI.backgroundColor = Color.white;
        myScript.goldText = (TMP_Text)EditorGUILayout.ObjectField("Gold", myScript.goldText, typeof(TMP_Text), true);
        GUILayout.EndVertical();              

        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.strengthText = (TMP_Text)EditorGUILayout.ObjectField("Strength", myScript.strengthText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        //GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        //GUI.backgroundColor = Color.white;
        myScript.intelligenceText = (TMP_Text)EditorGUILayout.ObjectField("Intelligence", myScript.intelligenceText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.immortalText = (TMP_Text)EditorGUILayout.ObjectField("Immortal", myScript.immortalText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        //GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        //GUI.backgroundColor = Color.white;
        myScript.invisibleText = (TMP_Text)EditorGUILayout.ObjectField("Invisible", myScript.invisibleText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        myScript.frozenText = (TMP_Text)EditorGUILayout.ObjectField("Frozen", myScript.frozenText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        //GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        //GUI.backgroundColor = Color.white;
        myScript.mutedText = (TMP_Text)EditorGUILayout.ObjectField("Muted", myScript.mutedText, typeof(TMP_Text), true);
        GUILayout.EndVertical();

        GUILayout.EndVertical();

        if (GUI.changed)
        {
            EditorSceneManager.MarkSceneDirty(myScript.gameObject.scene);
        }
    }
}
#endif
#endif
