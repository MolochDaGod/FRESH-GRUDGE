using UnityEngine;
using UnityEngine.UI;

// ─────────────────────────────────────────────────────────────────────────────
// GrudgeHotbarLayout
//
// Adds Grudge-specific visual rules on top of the existing UI_Skillbar.
// Attach to the same GameObject as UI_Skillbar.
//
// Layout contract (matches PlayerSkillbar slot indices):
//   Slots 0–3  (keys 1–4)  → Skills only   (gold tint when active)
//   Slot  4    (key 5)     → Reserved/blank (darker overlay enforced)
//   Slots 5–7  (keys 6–8)  → Consumables   (blue tint when occupied)
//   Slots 8–9  (keys 9–0)  → Extra / open
//
// This script runs AFTER UI_Skillbar.Update() because it relies on OnGUI /
// LateUpdate to override colors without touching the skillbar's core logic.
// ─────────────────────────────────────────────────────────────────────────────
[RequireComponent(typeof(UI_Skillbar))]
public class GrudgeHotbarLayout : MonoBehaviour
{
    [Header("Slot Role Colors")]
    public Color skillSlotTint       = new Color(1.00f, 0.85f, 0.30f, 1f); // gold
    public Color consumableSlotTint  = new Color(0.40f, 0.70f, 1.00f, 1f); // blue
    public Color reservedSlotTint    = new Color(0.20f, 0.20f, 0.20f, 0.6f);
    public Color emptySlotTint       = new Color(1f, 1f, 1f, 0.3f);

    [Header("Slot Role Overlays (optional GameObjects)")]
    [Tooltip("One per slot (10 total). Shown/hidden based on slot role.")]
    public GameObject[] roleOverlays;  // length 10, one label per slot

    [Header("Role Label Texts (optional, one per slot)")]
    public Text[] roleLabels;          // e.g. \"SKILL\", \"—\", \"ITEM\"

    // ── Cached refs ───────────────────────────────────────────────────────────
    UI_Skillbar _bar;

    void Awake() => _bar = GetComponent<UI_Skillbar>();

    // LateUpdate runs after UI_Skillbar.Update() so we can safely override
    void LateUpdate()
    {
        Player player = Player.localPlayer;
        if (player == null || _bar == null || _bar.content == null) return;

        int slotCount = _bar.content.childCount;

        for (int i = 0; i < slotCount; i++)
        {
            Transform slotT = _bar.content.GetChild(i);
            UISkillbarSlot slot = slotT.GetComponent<UISkillbarSlot>();
            if (slot == null) continue;

            SlotRole role = GetRole(i);
            bool isEmpty  = IsSlotEmpty(player, i);

            // ── Background tint ──────────────────────────────────────────────
            Image bg = GetBackgroundImage(slotT);
            if (bg != null)
            {
                if (role == SlotRole.Reserved)
                    bg.color = reservedSlotTint;
                else if (isEmpty)
                    bg.color = emptySlotTint;
                else if (role == SlotRole.Skill)
                    bg.color = skillSlotTint;
                else if (role == SlotRole.Consumable)
                    bg.color = consumableSlotTint;
                else
                    bg.color = Color.white;
            }

            // ── Reserved slot enforcement ─────────────────────────────────────
            // Slot 4 (index 4) is kept blank — disable button interaction
            if (role == SlotRole.Reserved)
            {
                if (slot.button != null) slot.button.interactable = false;
                // Clear any accidentally placed reference
                if (!string.IsNullOrEmpty(player.skillbar.slots[i].reference))
                    player.skillbar.slots[i].reference = "";
            }

            // ── Role overlay ─────────────────────────────────────────────────
            if (roleOverlays != null && i < roleOverlays.Length && roleOverlays[i] != null)
                roleOverlays[i].SetActive(role == SlotRole.Reserved || isEmpty);

            // ── Role label ───────────────────────────────────────────────────
            if (roleLabels != null && i < roleLabels.Length && roleLabels[i] != null)
            {
                roleLabels[i].text = role switch {
                    SlotRole.Reserved   => "—",
                    SlotRole.Skill      => "SKILL",
                    SlotRole.Consumable => "ITEM",
                    _                   => ""
                };
            }
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    enum SlotRole { Skill, Reserved, Consumable, Open }

    static SlotRole GetRole(int slotIndex)
    {
        if (slotIndex <= 3) return SlotRole.Skill;
        if (slotIndex == 4) return SlotRole.Reserved;
        if (slotIndex >= 5 && slotIndex <= 7) return SlotRole.Consumable;
        return SlotRole.Open;
    }

    static bool IsSlotEmpty(Player player, int i)
    {
        if (i >= player.skillbar.slots.Length) return true;
        return string.IsNullOrEmpty(player.skillbar.slots[i].reference);
    }

    /// <summary>
    /// Try to get the slot's background Image — usually the first Image child
    /// that is NOT the skill icon itself.
    /// Falls back to the slot's own Image component.
    /// </summary>
    static Image GetBackgroundImage(Transform slotT)
    {
        // Prefer a child named "Background" or "Bg"
        Transform bg = slotT.Find("Background") ?? slotT.Find("Bg") ?? slotT.Find("BG");
        if (bg != null) return bg.GetComponent<Image>();
        return slotT.GetComponent<Image>();
    }
}
