/// ClassTreeNodeCreator — Editor utility to create example tree node assets.
///
/// HOW TO USE:
///   In Unity Editor menu: Tools → Class Tree → Create Example Nodes
///
/// This creates a full 4-class tree example with 5 tiers each.
/// You can freely rename, duplicate, or delete these assets.
///
/// TREE STRUCTURE CREATED:
///
///   TIER 0 (Lv1) — Initial path (choose one):
///     Warrior_T0  |  Mage_T0  |  Ranger_T0  |  Rogue_T0
///
///   TIER 1 (Lv5) — First upgrade (choose one per branch):
///     Warrior → Berserker_T1 or Guardian_T1
///     Mage    → Pyromancer_T1 or Cryomancer_T1
///     Ranger  → Archer_T1 or Trapper_T1
///     Rogue   → Assassin_T1 or Shadowblade_T1
///
///   TIER 2 (Lv10), TIER 3 (Lv15), TIER 4 (Lv20) — further specialisations

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class ClassTreeNodeCreator
{
    const string FOLDER = "Assets/uMMORPG/Resources/ClassTreeNodes";

    [MenuItem("Tools/Class Tree/Create Example Nodes")]
    static void CreateExampleNodes()
    {
        if (!AssetDatabase.IsValidFolder(FOLDER))
            Directory.CreateDirectory(Application.dataPath + "/uMMORPG/Resources/ClassTreeNodes");

        // ── TIER 0 — Starting Paths ───────────────────────────────────────
        var warrior  = Make("Warrior_T0",  "Warrior",  0, "A sturdy frontline fighter specialising in melee combat.", "Sword",  0.05f, 2, 0,  5, 0);
        var mage     = Make("Mage_T0",     "Mage",     0, "A wielder of arcane forces. Masters of magical skill trees.", "Staff",  0.05f, 0, 0,  0, 10);
        var ranger   = Make("Ranger_T0",   "Ranger",   0, "A skilled ranged combatant with affinity for bows.", "Bow",    0.05f, 1, 0,  3, 3);
        var rogue    = Make("Rogue_T0",    "Rogue",    0, "A swift and deadly melee fighter striking from the shadows.", "Dagger", 0.05f, 1, 0,  3, 0);

        warrior.unlocksSkillCategory = "Warrior";
        mage.unlocksSkillCategory    = "Mage";
        ranger.unlocksSkillCategory  = "Ranger";
        rogue.unlocksSkillCategory   = "Rogue";

        // ── TIER 1 — Lv5 Upgrades ─────────────────────────────────────────
        var berserker     = Make("Berserker_T1",     "Berserker",     1, "Unleash raw fury. Axes deal crushing blows.",             "Axe",    0.10f, 4, 0,  8, 0,  warrior);
        var guardian      = Make("Guardian_T1",      "Guardian",      1, "Stalwart defender. Shield proficiency mastered.",         "Shield", 0.05f, 2, 5, 10, 0,  warrior);
        var pyromancer    = Make("Pyromancer_T1",    "Pyromancer",    1, "Harness fire. Staff and wand mastery.",                   "Staff",  0.08f, 1, 0,  0, 12, mage);
        var cryomancer    = Make("Cryomancer_T1",    "Cryomancer",    1, "Command frost. Slow and freeze enemies.",                 "Staff",  0.06f, 0, 2,  0, 15, mage);
        var archer        = Make("Archer_T1",        "Archer",        1, "Precision shots. Bow damage greatly increased.",          "Bow",    0.12f, 2, 0,  4, 0,  ranger);
        var trapper       = Make("Trapper_T1",       "Trapper",       1, "Lay traps. Crossbow proficiency unlocked.",               "Crossbow",0.08f,1, 2,  5, 0,  ranger);
        var assassin      = Make("Assassin_T1",      "Assassin",      1, "Strike fast. Dagger damage at its peak.",                 "Dagger", 0.12f, 3, 0,  3, 0,  rogue);
        var shadowblade   = Make("Shadowblade_T1",   "Shadowblade",   1, "Fight with dual blades. Sword+Dagger combo.",             "Sword",  0.08f, 2, 0,  4, 0,  rogue);

        berserker.unlocksSkillCategory  = "Berserker";
        guardian.unlocksSkillCategory   = "Guardian";
        pyromancer.unlocksSkillCategory = "Pyromancer";
        cryomancer.unlocksSkillCategory = "Cryomancer";
        archer.unlocksSkillCategory     = "Archer";
        trapper.unlocksSkillCategory    = "Trapper";
        assassin.unlocksSkillCategory   = "Assassin";
        shadowblade.unlocksSkillCategory = "Shadowblade";

        // ── TIER 2 — Lv10 Upgrades ────────────────────────────────────────
        Make("BloodBerserker_T2",  "Blood Berserker",  2, "Life steal and unstoppable rage. Axes bite deeper.",       "Axe",    0.08f, 6, 0, 10, 0,  berserker);
        Make("Warlord_T2",         "Warlord",          2, "Command the battlefield. War-hammer mastery.",             "Mace",   0.06f, 4, 3, 12, 0,  berserker, guardian);
        Make("IronWarden_T2",      "Iron Warden",      2, "Immovable fortress. Shield blocks become formidable.",     "Shield", 0.04f, 1, 8, 15, 0,  guardian);
        Make("Infernomancer_T2",   "Infernomancer",    2, "Explosive AOE fire skills. Gun-magic hybrid potential.",   "Gun",    0.06f, 2, 0,  0, 18, pyromancer);
        Make("Glaciomancer_T2",    "Glaciomancer",     2, "Absolute zero mastery. Heavy CC, extreme mana regen.",     "Staff",  0.05f, 0, 3,  0, 22, cryomancer);
        Make("Hawkeye_T2",         "Hawkeye",          2, "Eagle precision. Extreme bow critical chance.",            "Bow",    0.10f, 3, 0,  5, 0,  archer);
        Make("SnareHunter_T2",     "Snare Hunter",     2, "Traps upon traps. Crossbow AOE shots.",                   "Crossbow",0.07f,2, 2,  6, 0,  trapper);
        Make("Deathbringer_T2",    "Deathbringer",     2, "Poisoned daggers. Highest single-target damage.",         "Dagger", 0.10f, 5, 0,  4, 0,  assassin);
        Make("DualProwler_T2",     "Dual Prowler",     2, "Dual wield mastery. Off-hand attacks trigger.",           "Sword",  0.07f, 4, 0,  5, 0,  shadowblade);

        // ── TIER 3 — Lv15 Upgrades ────────────────────────────────────────
        Make("ChaosBerserker_T3",  "Chaos Berserker",  3, "Uncontrolled destruction. AXE crit multiplier +50%.",     "Axe",    0.07f, 8, 0, 12, 0);
        Make("WarChief_T3",        "War Chief",        3, "Leader of warriors. Mace and Sword both mastered.",       "Mace",   0.05f, 5, 5, 15, 0);
        Make("StoneFortress_T3",   "Stone Fortress",   3, "Nearly unstoppable wall. HP and block sky-high.",         "Shield", 0.03f, 2,12, 20, 0);
        Make("PhoenixMage_T3",     "Phoenix Mage",     3, "Fire and revival synergy. Extreme fire AOE.",             "Staff",  0.06f, 3, 0,  0, 20);
        Make("ArcticSage_T3",      "Arctic Sage",      3, "Ice field mastery. Entire battlefields frozen.",          "Staff",  0.04f, 0, 4,  0, 25);
        Make("CelestialArcher_T3", "Celestial Archer", 3, "Divine arrows pierce all defense.",                       "Bow",    0.09f, 5, 0,  6, 0);
        Make("TrapMaster_T3",      "Trap Master",      3, "Whole map covered in deadly traps.",                      "Crossbow",0.06f,3, 3,  8, 0);
        Make("GrimStalker_T3",     "Grim Stalker",     3, "Unseen. Instant kill procs on poisons.",                  "Dagger", 0.09f, 7, 0,  5, 0);
        Make("ProwlKing_T3",       "Prowl King",       3, "Dual blades and bow. Extreme mobility damage.",           "Sword",  0.06f, 6, 0,  6, 0);

        // ── TIER 4 — Lv20 Upgrades (Final) ────────────────────────────────
        Make("TitanBerserker_T4",  "Titan Berserker",  4, "FINAL: Raw power incarnate. Axe hits like a siege engine.", "Axe",   0.10f,12, 0, 15, 0);
        Make("DivineWarden_T4",    "Divine Warden",    4, "FINAL: Blessed tank. Shields explode on block.",           "Shield", 0.04f, 3,15, 25, 0);
        Make("DragonMage_T4",      "Dragon Mage",      4, "FINAL: Dragon fire incarnate. Guns + fire combo.",         "Gun",    0.08f, 5, 0,  0, 30);
        Make("FrostGod_T4",        "Frost God",        4, "FINAL: Absolute dominion over cold. Area freeze permanent.","Staff", 0.05f, 0, 5,  0, 35);
        Make("LegendaryArcher_T4", "Legendary Archer", 4, "FINAL: One arrow = one kill. Crossbow + Bow mastery.",    "Bow",    0.12f, 6, 0,  8, 0);
        Make("PoisonLord_T4",      "Poison Lord",      4, "FINAL: Every wound festers. Poison on all attacks.",      "Dagger", 0.12f,10, 0,  6, 0);
        Make("BladeStorm_T4",      "Blade Storm",      4, "FINAL: A tornado of blades. Sword + Dagger max speed.",   "Sword",  0.10f, 8, 0,  8, 0);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[ClassTree] Created example class tree nodes in {FOLDER}");
        EditorUtility.DisplayDialog("Class Tree Nodes Created",
            $"Example nodes created in:\n{FOLDER}\n\nYou can now assign prerequisites and icons in the Inspector.",
            "OK");
    }

    static ClassTreeNodeTemplate Make(string assetName, string displayName, int tier,
        string desc, string weaponCat, float weaponMult, int dmg, int def, int hp, int mp,
        params ClassTreeNodeTemplate[] prereqs)
    {
        string path = $"{FOLDER}/{assetName}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<ClassTreeNodeTemplate>(path);
        if (existing != null) return existing;

        var node = ScriptableObject.CreateInstance<ClassTreeNodeTemplate>();
        node.nodeName    = displayName;
        node.tier        = tier;
        node.description = desc;
        node.damageBonus  = dmg;
        node.defenseBonus = def;
        node.healthBonus  = hp;
        node.manaBonus    = mp;
        node.prerequisites = prereqs;

        if (!string.IsNullOrEmpty(weaponCat))
            node.weaponProficiencies = new[] { new WeaponProficiency
            {
                weaponCategory = weaponCat,
                damageMultiplierBonus = weaponMult,
                flatDamageBonus = Mathf.RoundToInt(dmg * 0.5f)
            }};

        AssetDatabase.CreateAsset(node, path);
        return node;
    }
}
#endif
