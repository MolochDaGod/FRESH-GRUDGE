# Class Leveling Tree Addon
**Location:** `Assets/uMMORPG/Scripts/Addons/!custom/ClassLevelingTree/`

A branching class leveling tree with milestone choices at **Levels 1, 5, 10, 15, and 20**.  
Each milestone unlocks a new tier of class upgrade options. Players make one choice per tier — permanent decisions that shape their skill access, weapon proficiency bonuses, and stat growth.

---

## Milestone Tiers

| Tier | Level | Description |
|------|-------|-------------|
| 0 | 1 | **Initial Path Selection** — choose your base class archetype |
| 1 | 5 | **1st Class Upgrade** |
| 2 | 10 | **2nd Class Upgrade** |
| 3 | 15 | **3rd Class Upgrade** |
| 4 | 20 | **4th / Final Class Upgrade** |

---

## Step 1 — Add Scripting Define

**Project Settings → Player → Scripting Define Symbols**, add:
```
_iMMOCLASSTREE
```

---

## Step 2 — Add `PlayerClassTree` to Player Prefabs

1. Open each Player prefab.
2. Add Component → `PlayerClassTree`.
3. Assign in the Inspector:
   - **player** → Player component
   - **equipment** → PlayerEquipment component
   - **skills** → PlayerSkills component
4. The `classTree` field on Player will appear automatically (from the partial class).

---

## Step 3 — Create Class Tree Nodes (ScriptableObjects)

Right-click in the Project window → **Create → GRUDGE / ClassTree / Node**

**Required fields:**
- `nodeName` — display name (e.g., "Berserker")
- `tier` — 0 through 4 (maps to levels 1/5/10/15/20)
- `prerequisites` — parent nodes that must be chosen first (leave empty for tier 0)
- `unlocksSkillCategory` — string matching the SkillCategories addon category name
- `weaponProficiencies` — weapon type name + damage multiplier (e.g., "Sword", 0.15)
- `grantedSkills` — skills auto-set to level 1 on choosing this node
- `damageBonus / defenseBonus / healthBonus / manaBonus` — flat stat bonuses

**Example tree (Warrior path):**
```
Tier 0: "Warrior"         (no prerequisites)
Tier 1: "Berserker"       (requires Warrior) — Axe proficiency +20%
        "Guardian"        (requires Warrior) — Shield bonus +15 defense
Tier 2: "Blood Berserker" (requires Berserker) — unlocks "Rage" skill category
        "Warlord"         (requires Berserker or Guardian)
```

Store assets in any **Resources/** subfolder (they're loaded via `Resources.LoadAll`).

---

## Step 4 — Wire Up the Database

In your main `Database.cs` Connect/Load/Save methods, add:

```csharp
// In Connect():
Connect_ClassLevelingTree();

// In CharacterLoad(), after skills:
CharacterLoad_ClassLevelingTree(player);

// In CharacterSave():
CharacterSave_ClassLevelingTree(player, online);
```

---

## Step 5 — Build the UI Panel

Create a UI panel prefab with:

```
UI_ClassTreePanel (root)
├── Panel (background)
│   ├── TitleText
│   ├── TierColumns (HorizontalLayoutGroup)
│   │   ├── Tier0Column (VerticalLayoutGroup) — "Lv.1 Path Select"
│   │   ├── Tier1Column (VerticalLayoutGroup) — "Lv.5 Upgrade I"
│   │   ├── Tier2Column (VerticalLayoutGroup) — "Lv.10 Upgrade II"
│   │   ├── Tier3Column (VerticalLayoutGroup) — "Lv.15 Upgrade III"
│   │   └── Tier4Column (VerticalLayoutGroup) — "Lv.20 Upgrade IV"
│   └── WeaponProficiencyPanel
│       └── Content (VerticalLayoutGroup) ← filled with UI_WeaponProficiencyEntry
```

Assign `UI_ClassTreePanel.tierContents[0..4]` to the five tier column Transforms.

**Node Prefab** (`UI_ClassTreeNodeButton`):
```
NodeButton
├── Icon (Image)
├── NameText (Text)
├── TierLevelText (Text) — "Lv. 5"
├── DescriptionText (Text)
├── ChosenBorder (GameObject)      ← outline when chosen
├── AvailableBorder (GameObject)   ← glow when can pick
├── LockedOverlay (GameObject)     ← dim when locked
├── UnavailableDim (GameObject)    ← dim when tier taken by another choice
└── Button
```

---

## Step 6 — Gate Skills Behind Tree Nodes (Optional)

On any `ScriptableSkill` asset, assign `requiredClassTreeNodes[]`.  
The skill will only appear in the `UI_Skills` panel if the player has chosen at least one of those nodes.

---

## Weapon Proficiency

The `GetDamageBonus()` on `PlayerClassTree` implements `ICombatBonus`.  
`Combat.cs` already aggregates all `ICombatBonus` components via `GetComponents<ICombatBonus>()` — so **weapon proficiency bonuses apply automatically** with no extra wiring.

The multiplier bonus (`damageMultiplierBonus`) shows in UI only.  
The **flat bonus** (`flatDamageBonus`) is included in `GetDamageBonus()` when the matching weapon is equipped.

> **For percentage-based damage bonuses:** You'd need to modify the damage formula in `Combat.cs`  
> to call `player.classTree.GetWeaponProficiencyBonus(weaponCategory)` as a multiplier.

---

## Automatic Panel Pop-Up

When the player reaches a milestone level **and hasn't chosen that tier yet**, the  
`UI_ClassTreePanel` opens automatically. Assign the panel prefab to the scene.

---

## Files Summary

| File | Purpose |
|------|---------|
| `Scriptable/ClassTreeNodeTemplate.cs` | ScriptableObject for each tree node |
| `Scripts/PlayerClassTree.cs` | NetworkBehaviour, ICombatBonus, IHealthBonus, IManaBonus |
| `Scripts/ClassLevelingTree.Player.cs` | Adds `classTree` field to Player |
| `Scripts/ClassLevelingTree.ScriptableSkill.cs` | Adds `requiredClassTreeNodes` to skills |
| `Database/ClassLevelingTree.Database.cs` | SQLite save/load |
| `UI/UI_ClassTreePanel.cs` | Main tree panel UI |
| `UI/UI_ClassTreeNodeButton.cs` | Individual node button |
| `UI/UI_WeaponProficiencyEntry.cs` | Weapon proficiency summary row |
| `SkillCategories/.../UI_Skills.cs` | Modified: added `#if _iMMOCLASSTREE` check to canShow() |
