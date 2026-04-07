using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main Class Leveling Tree UI panel.
///
/// PREFAB SETUP:
///  - panel          : root panel GameObject
///  - tierContents   : 5 Transforms (one per tier column). Each is a layout group
///                     that will be filled with UI_ClassTreeNodeButton prefabs.
///                     Label them: "Tier 0 (Lv1)", "Tier 1 (Lv5)", etc.
///  - nodePrefab     : drag the UI_ClassTreeNodeButton prefab here
///  - titleText      : optional text showing "Class Leveling Tree"
///  - tierHeaderTexts: optional per-tier level labels ("Level 1", "Level 5" …)
///  - weaponProficiencyPanel / Content / prefab : summary of acquired proficiencies
///  - hotKey         : default KeyCode.T
/// </summary>
[DefaultExecutionOrder(120)]
public class UI_ClassTreePanel : MonoBehaviour
{
    [Header("Core")]
    public KeyCode        hotKey = KeyCode.T;
    public GameObject     panel;
    public UI_ClassTreeNodeButton nodePrefab;

    [Header("Tier Columns (index 0-4 = Lv1/5/10/15/20)")]
    public Transform[]    tierContents;   // length must be 5

    [Header("Labels (optional)")]
    public Text           titleText;
    public Text[]         tierHeaderTexts; // length 5 — "Level 1", "Level 5" …

    [Header("Weapon Proficiency Summary")]
    public GameObject               weaponProficiencyPanel;
    public Transform                weaponProficiencyContent;
    public UI_WeaponProficiencyEntry weaponProficiencyPrefab;

    // Internal
    int _focusTier = -1;

    // ── Unity callbacks ───────────────────────────────────────────────────

    void Start()
    {
        // Set tier header labels
        string[] headers = { "Lv. 1\nPath Select", "Lv. 5\nUpgrade I", "Lv. 10\nUpgrade II",
                             "Lv. 15\nUpgrade III", "Lv. 20\nUpgrade IV" };
        if (tierHeaderTexts != null)
            for (int i = 0; i < Mathf.Min(tierHeaderTexts.Length, headers.Length); i++)
                if (tierHeaderTexts[i] != null)
                    tierHeaderTexts[i].text = headers[i];
    }

    void Update()
    {
        // Hotkey toggle
        if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
            panel.SetActive(!panel.activeSelf);

        if (panel.activeSelf)
            Refresh();
    }

    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>Open the panel and visually highlight the given tier.</summary>
    public void OpenForTier(int tier)
    {
        _focusTier = tier;
        panel.SetActive(true);
    }

    // ── Refresh ───────────────────────────────────────────────────────────

    void Refresh()
    {
        Player player = Player.localPlayer;
        if (player == null) return;

#if _IMMOCLASSTREE
        PlayerClassTree classTree = player.classTree;
        if (classTree == null) return;

        // Group all tree nodes by tier
        var byTier = ClassTreeNodeTemplate.All.Values
            .GroupBy(n => n.tier)
            .ToDictionary(g => g.Key, g => g.OrderBy(n => n.nodeName).ToList());

        for (int tier = 0; tier < 5; tier++)
        {
            if (tier >= tierContents.Length || tierContents[tier] == null) continue;

            var nodes = byTier.TryGetValue(tier, out var list) ? list
                        : new System.Collections.Generic.List<ClassTreeNodeTemplate>();

            UIUtils.BalancePrefabs(nodePrefab.gameObject, nodes.Count, tierContents[tier]);

            bool tierLocked   = player.level.current < ClassTreeNodeTemplate.MilestoneLevels[tier];
            bool tierChosen   = classTree.HasChosenTier(tier);

            for (int i = 0; i < nodes.Count; i++)
            {
                var btn = tierContents[tier].GetChild(i)
                                            .GetComponent<UI_ClassTreeNodeButton>();
                btn.Setup(nodes[i], classTree, tierLocked, tierChosen);
            }
        }

        RefreshWeaponProficiencies(classTree);
#endif
    }

    void RefreshWeaponProficiencies(PlayerClassTree classTree)
    {
        if (weaponProficiencyPanel == null) return;

        var profMap = new Dictionary<string, float>();
        foreach (int h in classTree.chosenNodes)
        {
            if (!ClassTreeNodeTemplate.All.TryGetValue(h, out var node)) continue;
            foreach (var prof in node.weaponProficiencies)
            {
                if (!profMap.ContainsKey(prof.weaponCategory)) profMap[prof.weaponCategory] = 0f;
                profMap[prof.weaponCategory] += prof.damageMultiplierBonus;
            }
        }

        if (weaponProficiencyContent != null && weaponProficiencyPrefab != null)
        {
            UIUtils.BalancePrefabs(weaponProficiencyPrefab.gameObject, profMap.Count, weaponProficiencyContent);
            int idx = 0;
            foreach (var kvp in profMap)
            {
                weaponProficiencyContent.GetChild(idx)
                    .GetComponent<UI_WeaponProficiencyEntry>()
                    .Setup(kvp.Key, kvp.Value);
                idx++;
            }
        }

        weaponProficiencyPanel.SetActive(profMap.Count > 0);
    }
}
