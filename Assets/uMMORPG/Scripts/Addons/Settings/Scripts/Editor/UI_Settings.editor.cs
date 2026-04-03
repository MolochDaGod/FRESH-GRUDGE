#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UI_Settings))]
public partial class UI_SettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UI_Settings uiSettings = (UI_Settings)target;

        // Vérifier si tous les champs sont assignés
        bool allAssigned = uiSettings.uiParty != null &&
                           uiSettings.uiCharacterInfo != null &&
#if !_iMMOCOMPLETECHAT
                           uiSettings.uiChat != null &&
#endif
                           uiSettings.uiCrafting != null &&
                           uiSettings.uiEquipment != null &&
                           uiSettings.uiGuild != null &&
                           uiSettings.uiInventory != null &&
                           uiSettings.uiItemMall != null &&
                           uiSettings.uiQuests != null &&
                           uiSettings.uiSkills != null
#if _iMMOSKILLCATEGORY
                           && uiSettings.ui_Skills != null
#endif
                           ;

        // Changer la couleur du bouton en fonction de l'état d'assignation
        GUI.backgroundColor = allAssigned ? Color.green : Color.red;

        if (GUILayout.Button("Auto Assign GameObject"))
        {
            //AssignGameObjectsAutomatically(uiSettings);
        }

        // Réinitialiser la couleur
        GUI.backgroundColor = Color.white;
    }
}
#endif