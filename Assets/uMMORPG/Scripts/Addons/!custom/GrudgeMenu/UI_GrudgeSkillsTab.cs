using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ─────────────────────────────────────────────────────────────────────────────
// UI_GrudgeSkillsTab
//
// Skills tab for the Grudge hub. Replaces the MainPanel skill list.
//
// PREFAB SETUP:
//   skillSlotPrefab  — a UISkillSlot-like prefab with:
//                        iconImage (Image), nameText (Text), levelText (Text),
//                        descText (Text), upgradeButton (Button),
//                        lockedOverlay (GameObject), categoryTag (Text)
//   content          — ScrollRect content Transform
//   categoryFilter   — Dropdown for filtering by skill category
//   searchField      — optional InputField for name search
//   skillXpText      — Text showing current skill XP
//   upgradeAllButton — optional Button to upgrade every eligible skill at once
// ─────────────────────────────────────────────────────────────────────────────
[DefaultExecutionOrder(150)]
public class UI_GrudgeSkillsTab : MonoBehaviour
{
    [Header("Slot Prefab")]
    public GrudgeSkillSlot slotPrefab;

    [Header("Layout")]
    public Transform content;

    [Header("Filters")]
    public Dropdown   categoryFilter;
    public InputField searchField;

    [Header("Info")]
    public Text skillXpText;
    public Button upgradeAllButton;

    // ── State ─────────────────────────────────────────────────────────────────
    List<string> _categories = new List<string> { "All" };
    string _activeCategory   = "All";
    string _searchText       = "";

    // ── Unity ─────────────────────────────────────────────────────────────────
    void Awake()
    {
        if (categoryFilter != null)
            categoryFilter.onValueChanged.AddListener(idx => {
                _activeCategory = (idx == 0) ? "All" : _categories[idx];
            });

        if (searchField != null)
            searchField.onValueChanged.AddListener(text => _searchText = text);

        if (upgradeAllButton != null)
            upgradeAllButton.onClick.AddListener(UpgradeAll);
    }

    // ── Called by UI_GrudgeMenu each frame while this tab is active ───────────
    public void Refresh(Player player)
    {
        if (player == null || player.skills == null) return;

        PlayerSkills skills = (PlayerSkills)player.skills;

        // Skill XP display
        if (skillXpText != null)
            skillXpText.text = $"Skill XP: {skills.skillExperience}";

        // Build category list from skills
        RebuildCategoryDropdown(skills);

        // Count visible skills
        List<int> visible = new List<int>();
        for (int i = 0; i < skills.skills.Count; i++)
        {
            Skill s = skills.skills[i];
            if (s.level <= 0 && !skills.CanUpgrade(s)) continue; // hide unlearnable
            if (!PassesFilter(s)) continue;
            visible.Add(i);
        }

        UIUtils.BalancePrefabs(slotPrefab.gameObject, visible.Count, content);

        for (int vi = 0; vi < visible.Count; vi++)
        {
            int      idx   = visible[vi];
            Skill    skill = skills.skills[idx];
            GrudgeSkillSlot slot = content.GetChild(vi).GetComponent<GrudgeSkillSlot>();

            bool canUpgrade = skills.CanUpgrade(skill);
            bool classLocked = IsClassLocked(player, skill);

            slot.Refresh(skill, canUpgrade, classLocked, () => {
                if (skills.CanUpgrade(skills.skills[idx]))
                    skills.CmdUpgrade(idx);
            });
        }

        // Upgrade-all button: active only if anything can be upgraded
        if (upgradeAllButton != null)
            upgradeAllButton.interactable = visible.Exists(i => skills.CanUpgrade(skills.skills[i]));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    void RebuildCategoryDropdown(PlayerSkills skills)
    {
        if (categoryFilter == null) return;

        var cats = new List<string> { "All" };
        foreach (Skill s in skills.skills)
        {
            string cat = s.data != null ? (s.data.requiredWeaponCategory ?? "") : "";
            if (!string.IsNullOrEmpty(cat) && !cats.Contains(cat)) cats.Add(cat);
        }

        if (!ListsEqual(cats, _categories))
        {
            _categories = cats;
            categoryFilter.ClearOptions();
            categoryFilter.AddOptions(cats);
        }
    }

    bool PassesFilter(Skill s)
    {
        // Search text
        if (!string.IsNullOrEmpty(_searchText) &&
            !s.name.ToLower().Contains(_searchText.ToLower())) return false;

        // Category filter
        if (_activeCategory != "All")
        {
            string cat = s.data != null ? (s.data.requiredWeaponCategory ?? "") : "";
            if (cat != _activeCategory) return false;
        }
        return true;
    }

    bool IsClassLocked(Player player, Skill skill)
    {
#if _IMMOCLASSTREE
        if (player.classTree == null) return false;
        var required = skill.data.requiredClassTreeNodes;
        if (required == null || required.Length == 0) return false;
        foreach (var node in required)
            if (player.classTree.HasChosenNode(node)) return false;
        return true;
#else
        return false;
#endif
    }

    void UpgradeAll()
    {
        Player player = Player.localPlayer;
        if (player == null) return;
        PlayerSkills skills = (PlayerSkills)player.skills;
        for (int i = 0; i < skills.skills.Count; i++)
            if (skills.CanUpgrade(skills.skills[i]))
                skills.CmdUpgrade(i);
    }

    static bool ListsEqual(List<string> a, List<string> b)
    {
        if (a.Count != b.Count) return false;
        for (int i = 0; i < a.Count; i++) if (a[i] != b[i]) return false;
        return true;
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// GrudgeSkillSlot — individual row inside the Skills tab
// ─────────────────────────────────────────────────────────────────────────────
public class GrudgeSkillSlot : MonoBehaviour
{
    [Header("UI")]
    public Image     iconImage;
    public Text      nameText;
    public Text      levelText;
    public Text      descText;
    public Text      categoryTag;
    public Button    upgradeButton;
    public GameObject lockedOverlay;      // red/greyed overlay when class-tree locked
    public GameObject upgradableGlow;     // optional glow when upgrade is available

    public void Refresh(Skill skill, bool canUpgrade, bool classLocked, UnityEngine.Events.UnityAction onUpgrade)
    {
        if (iconImage  != null) { iconImage.sprite = skill.image; iconImage.enabled = skill.image != null; }
        if (nameText   != null) nameText.text  = skill.name;
        if (levelText  != null) levelText.text = $"Lv. {skill.level} / {skill.maxLevel}";
        if (descText   != null) descText.text  = skill.ToolTip();

        string cat = skill.data != null ? (skill.data.requiredWeaponCategory ?? "") : "";
        if (categoryTag != null)
        {
            categoryTag.text    = string.IsNullOrEmpty(cat) ? "" : $"[{cat}]";
            categoryTag.enabled = !string.IsNullOrEmpty(cat);
        }

        if (lockedOverlay   != null) lockedOverlay.SetActive(classLocked);
        if (upgradableGlow  != null) upgradableGlow.SetActive(canUpgrade && !classLocked);

        if (upgradeButton != null)
        {
            upgradeButton.interactable = canUpgrade && !classLocked;
            upgradeButton.onClick.RemoveAllListeners();
            if (canUpgrade && !classLocked) upgradeButton.onClick.AddListener(onUpgrade);
        }
    }
}
