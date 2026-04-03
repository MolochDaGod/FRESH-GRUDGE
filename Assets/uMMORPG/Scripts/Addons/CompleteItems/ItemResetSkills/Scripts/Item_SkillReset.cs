using UnityEngine;

// Reset ITEM

[CreateAssetMenu(menuName = "ADDON/Item/Item Reset Skill", order = 999)]
public class Item_SkillReset : UsableItem
{
    [Header("[-=-[ Reset Skill Item ]-=-]")]
    public string resetMessage = "Your skill points have been reset!";

    [Tooltip("Decrease amount by how many each use (can be 0)?")]
    public int decreaseAmount = 1;

#if _SERVER
    // -----------------------------------------------------------------------------------
    // Use
    // -----------------------------------------------------------------------------------
    public override void Use(Player player, int inventoryIndex)
    {
        ItemSlot slot = player.inventory.slots[inventoryIndex];

        // -- Only activate if enough charges left
        if (decreaseAmount == 0 || slot.amount >= decreaseAmount)
        {
            // always call base function too
            base.Use(player, inventoryIndex);

            long mySkillPoints = 0;

            for (int skillIndex = 0; skillIndex < player.skills.skills.Count; skillIndex++)
            {
                Skill skill = player.skills.skills[skillIndex];

                if (skill.level > 0)
                {
                    for (int j = skill.level; j > 0; j--)
                    {
                        mySkillPoints += skill.data.requiredSkillExperience.Get(j);
                    }
                    skill.level = 0;
                    player.skills.skills[skillIndex] = skill;
                }
            }

            if (mySkillPoints > 0)
            {
                ((PlayerSkills)player.skills).skillExperience += mySkillPoints;
            }

            player.Tools_TargetAddMessage(resetMessage);

            // decrease amount
            slot.DecreaseAmount(decreaseAmount);
            player.inventory.slots[inventoryIndex] = slot;
        }
    }
#endif
    // -----------------------------------------------------------------------------------
}
