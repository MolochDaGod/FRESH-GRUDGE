/// ClassLevelingTree Addon
/// Scripting Define: _IMMOCLASSTREE
///
/// SETUP STEPS:
///  1. Add "_IMMOCLASSTREE" to Project Settings → Player → Scripting Define Symbols
///  2. Add "PlayerClassTree" component to your Player prefab(s)
///     and assign the Player, PlayerEquipment, and PlayerSkills fields.
///  3. Add "classTree" field to your Player prefab via the partial class below.
///  4. Create ClassTreeNodeTemplate ScriptableObjects in a Resources/ subfolder.
///  5. Build the UI_ClassTreePanel prefab and wire it up (see UI/ folder).
///  6. Database table is auto-created on first connect.
///
/// MILESTONE LEVELS: 1 (Tier 0), 5 (Tier 1), 10 (Tier 2), 15 (Tier 3), 20 (Tier 4)
/// 4 CLASS UPGRADE TIERS: Tiers 1-4 (levels 5, 10, 15, 20) are the four upgrade choices.
/// Tier 0 (level 1) is the initial class path selection.

// No runtime code here — this file is documentation only.
// All runtime logic is in the other files in this folder.
