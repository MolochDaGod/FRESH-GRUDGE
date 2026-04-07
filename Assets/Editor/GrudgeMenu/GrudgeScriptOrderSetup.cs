#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// GrudgeScriptOrderSetup
//
// Open via:  Grudge Studio ▶ Apply Script Execution Order
//
// Writes execution order to ProjectSettings so Unity loads all !custom scripts
// in the correct tier every play and every build.
//
// TIERS:
//   -300  Network / SyncVar data components (must awake before UI reads them)
//   -200  Player-attached manager behaviours
//   -100  Scene-level singleton managers
//     0   Default (uMMORPG core, Mirror, etc.)
//   +100  UI panels that read player/network data
//   +200  UI overlays that post-process other UI (hotbar layout, compass, etc.)
// ─────────────────────────────────────────────────────────────────────────────
public static class GrudgeScriptOrderSetup
{
    // Map: MonoBehaviour class name → desired execution order integer
    static readonly Dictionary<string, int> OrderMap = new Dictionary<string, int>
    {
        // ── Tier -300: Network data (NetworkBehaviour SyncVars) ───────────────
        { "PlayerGrudgeProfessions",    -300 },
        { "PlayerClassTree",            -280 },
        { "PlayerArmory",               -260 },
        { "PlayerResearch",             -260 },
        { "PlayerStable",               -260 },
        { "PlayerFurnaceUsage",         -250 },
        { "PlayerStorageUsage",         -250 },
        { "PlayerWorldMount",           -250 },
        { "PlayerHousing",              -240 },
        { "MountSkills",                -230 },

        // ── Tier -200: Player-attached behaviour managers ─────────────────────
        { "MountManager",               -200 },
        { "MountSkillBar",              -190 },
        { "canRoll",                    -180 },
        { "chargeattackfromscratch",    -170 },
        { "PlayerDashController",       -170 },
        { "PlayerMouseCursorManager",   -160 },

        // ── Tier -100: Scene-level singleton managers ─────────────────────────
        { "PanelManager",               -100 },
        { "ObjectManager",              -90  },
        { "BossManager",                -80  },
        { "audiomaster",                -70  },
        { "FramerateController",        -60  },
        { "TutorialManager",            -50  },
        { "HarvestableSpawner",         -40  },
        { "LobbySceneCameraManager",    -30  },
        { "Instance",                   -20  },

        // ── Tier +100: UI panels that READ player/network data ────────────────
        { "PanelActivator",             100  },
        { "PanelUse",                   100  },
        { "MainPanel",                  110  },
        { "UI_GrudgeMenu",              110  },
        { "UI_ClassTreePanel",          120  },
        { "UI_ClassTreeNodeButton",     120  },
        { "UIFurnace",                  130  },
        { "UIStorage",                  130  },
        { "ArmoryPanel",                130  },
        { "ResearchPanel",              130  },
        { "StablePanel",                130  },
        { "PlayerHousingUI",            130  },
        { "UI_QuickCraft",              130  },
        { "SkinsPanel",                 130  },
        { "UIBossTimes",                130  },
        { "UIMountSkillBar",            140  },
        { "UIinteraction",              140  },

        // Grudge sub-tab UIs (read player data set up at -300)
        { "UI_GrudgeSkillsTab",         150  },
        { "UI_GrudgeProfessionsTab",    150  },
        { "UI_GrudgeCharacterTab",      150  },
        { "UI_GrudgeOnboarding",        150  },

        // ── Tier +200: Post-process overlays (run after all other UI) ─────────
        { "GrudgeHotbarLayout",         200  },
        { "CompassBar",                 210  },
        { "CanvasController",           220  },
        { "CameraSensitivitySlider",    230  },
        { "CameraZoom",                 230  },
    };

    [MenuItem("Grudge Studio/Apply Script Execution Order", priority = 2)]
    public static void Apply()
    {
        int applied = 0;
        int skipped = 0;

        foreach (var kv in OrderMap)
        {
            MonoScript script = FindScript(kv.Key);
            if (script == null)
            {
                Debug.LogWarning($"[ScriptOrder] Script not found: {kv.Key}");
                skipped++;
                continue;
            }

            int current = MonoImporter.GetExecutionOrder(script);
            if (current != kv.Value)
            {
                MonoImporter.SetExecutionOrder(script, kv.Value);
                Debug.Log($"[ScriptOrder] {kv.Key}: {current} → {kv.Value}");
                applied++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[ScriptOrder] Done. Applied: {applied}, Already correct / not found: {skipped}");
        EditorUtility.DisplayDialog(
            "Grudge Script Execution Order",
            $"Complete.\n\nUpdated: {applied} scripts\nSkipped / not found: {skipped}\n\n" +
            "Check the Console for details.",
            "OK");
    }

    static MonoScript FindScript(string className)
    {
        // Search all MonoScript assets for a matching class name
        string[] guids = AssetDatabase.FindAssets($"t:MonoScript {className}");
        foreach (string guid in guids)
        {
            string path   = AssetDatabase.GUIDToAssetPath(guid);
            MonoScript ms = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (ms != null && ms.GetClass() != null && ms.GetClass().Name == className)
                return ms;
        }
        return null;
    }
}
#endif
