using UnityEngine;

public class UIMountSkillBar : MonoBehaviour
{
    public GameObject panel;
    public UISkillbarSlot slotPrefab;
    public Transform content;

    [Header("Durability Colors")]
    public Color brokenDurabilityColor = Color.red;
    public Color lowDurabilityColor = Color.magenta;
    [Range(0.01f, 0.99f)] public float lowDurabilityThreshold = 0.1f;

    void Update()
    {
        Player player = Player.localPlayer;
        if (player != null && player.mountControl.activeMount)
        {
            MountSkills mountSkills = player.mountControl.activeMount.GetComponent<MountSkills>();
            MountSkillBar mountSkillBar = player.mountControl.activeMount.GetComponent<MountSkillBar>();

            if (mountSkills != null && mountSkills.skillTemplates.Length > 0 && mountSkillBar != null)
            {
                panel.SetActive(true);

                // instantiate/destroy enough slots
                UIUtils.BalancePrefabs(slotPrefab.gameObject, mountSkillBar.slots.Length, content);

                // refresh all
                for (int i = 0; i < mountSkillBar.slots.Length; ++i)
                {
                    SkillbarEntry entry = mountSkillBar.slots[i];

                    UISkillbarSlot slot = content.GetChild(i).GetComponent<UISkillbarSlot>();
                    slot.dragAndDropable.name = i.ToString(); // drag and drop index

                    // hotkey overlay (without 'Alpha' etc.)
                    string pretty = entry.hotKey.ToString().Replace("Alpha", "");
                    slot.hotkeyText.text = pretty;

                    // skill, inventory item or equipment item?
                    int skillIndex = mountSkills.GetSkillIndexByName(entry.reference);
                    if (skillIndex != -1)
                    {
                        Skill skill = mountSkills.skills[skillIndex];
                        bool canCast = mountSkills.CastCheckSelf(skill);

                        // if movement does NOT support navigation then we need to
                        // check distance too. otherwise distance doesn't matter
                        // because we can navigate anywhere.
                        if (!player.movement.CanNavigate())
                            canCast &= mountSkills.CastCheckDistance(skill, out Vector3 _);

                        // hotkey pressed and not typing in any input right now?
                        if (Input.GetKeyDown(entry.hotKey) &&
                            !UIUtils.AnyInputActive() &&
                            canCast) // checks mana, cooldowns, etc.
                        {
                            // try use the skill or walk closer if needed
                            mountSkills.TryUse(skillIndex);
                        }

                        // refresh skill slot
                        slot.button.interactable = canCast; // check mana, cooldowns, etc.
                        slot.button.onClick.SetListener(() =>
                        {
                            // try use the skill or walk closer if needed
                            mountSkills.TryUse(skillIndex);
                        });
                        // only build tooltip while it's actually shown. this
                        // avoids MASSIVE amounts of StringBuilder allocations.
                        slot.tooltip.enabled = true;
                        if (slot.tooltip.IsVisible())
                            slot.tooltip.text = skill.ToolTip();
                        slot.dragAndDropable.dragable = true;
                        slot.image.color = Color.white;
                        slot.image.sprite = skill.image;
                        float cooldown = skill.CooldownRemaining();
                        slot.cooldownOverlay.SetActive(cooldown > 0);
                        slot.cooldownText.text = cooldown.ToString("F0");
                        slot.cooldownCircle.fillAmount = skill.cooldown > 0 ? cooldown / skill.cooldown : 0;
                        slot.amountOverlay.SetActive(false);
                    }
                    else
                    {
                        // clear the outdated reference
                        // (need to assign directly because it's a struct)
                        mountSkillBar.slots[i].reference = "";

                        // refresh empty slot
                        slot.button.onClick.RemoveAllListeners();
                        slot.tooltip.enabled = false;
                        slot.dragAndDropable.dragable = false;
                        slot.image.color = Color.clear;
                        slot.image.sprite = null;
                        slot.cooldownOverlay.SetActive(false);
                        slot.cooldownCircle.fillAmount = 0;
                        slot.amountOverlay.SetActive(false);
                    }
                }
            }
            else panel.SetActive(false);
        }
        else panel.SetActive(false);
    }
}