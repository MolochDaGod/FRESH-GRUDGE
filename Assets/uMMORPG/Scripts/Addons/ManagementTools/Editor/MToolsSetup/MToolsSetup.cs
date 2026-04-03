#if UNITY_EDITOR
//using Controller2k;
using UnityEditor;
using UnityEngine;

public static class MToolsSetup
{
    public static void AdditionalStep(bool enabled)
    {
        // initially commented out to avoid errors in other versions, 
        // because the effects of changes in the ScriptingDefineSymbols will not be included in the codeblock right after the change
        /*
        UIGameMasterTool gmTool = Object.FindObjectOfType<UIGameMasterTool>();
        if (gmTool != null)
        {
            gmTool.enabled = enabled;
        }

        UICharacterCreation characterCreation = Object.FindObjectOfType<UICharacterCreation>();
        if (characterCreation != null)
        {
            characterCreation.gameMasterToggle.gameObject.SetActive(enabled);
        }
        */
    }

    public static void PreparePlayerAsset(Player asset, bool enable)
    {
        // initially commented out to avoid errors in other versions, 
        // because the effects of changes in the ScriptingDefineSymbols will not be included in the codeblock right after the change
        /*
        if (asset.gameObject.TryGetComponent(out PlayerGameMasterTool component))
        {
            component.enabled = enable;
        }

        if (asset.gameObject.TryGetComponent(out CharacterController2k component2))
        {
            component2.collisionLayerMask = FindLayer("Environment");
        }
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        */
    }

    public static LayerMask FindLayer(string layer)
    {
        string[] layers = UnityEditorInternal.InternalEditorUtility.layers;
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i].Equals(layer))
            {
                return LayerMask.GetMask("Default", layer);
            }
        }
        return LayerMask.GetMask("Default");
    }
}
#endif