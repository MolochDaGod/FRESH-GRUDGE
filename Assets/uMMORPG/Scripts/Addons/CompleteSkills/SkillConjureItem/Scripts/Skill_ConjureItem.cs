using UnityEngine;

// SKILL CONJURE ITEM
[CreateAssetMenu(menuName = "ADDON/Skills/Self/Conjure Item", order = 999)]
public class Skill_ConjureItem : ScriptableSkill
{
    [Header("[-=-[ Conjured Items ]-=-]")]
    [Tooltip("The conjured item, one per level of skill")]
    public Item_Conjureable[] conjuredItems;

    [Header("[-=-[ Feedback Message ]-=-]")]
    public string conjuredMessage;

    public string failedMessage;
    [Range(0, 255)] public byte iconId;
    [Range(0, 255)] public byte soundId;

    // -----------------------------------------------------------------------------------
    // CheckTarget
    // -----------------------------------------------------------------------------------
    public override bool CheckTarget(Entity caster)
    {
        return true;
    }

    // -----------------------------------------------------------------------------------
    // CheckDistance
    // -----------------------------------------------------------------------------------
#if _iMMO2D
    public override bool CheckDistance(Entity caster, int skillLevel, out Vector2 destination)
#else
    public override bool CheckDistance(Entity caster, int skillLevel, out Vector3 destination)
#endif
    {
        destination = caster.transform.position;
        return true;
    }

    // -----------------------------------------------------------------------------------
    // Apply
    // -----------------------------------------------------------------------------------
#if _iMMO2D
    public override void Apply(Entity caster, int skillLevel, Vector2 target)
#else
    public override void Apply(Entity caster, int skillLevel)
#endif
    {
#if _SERVER
        Player player = (Player)caster;

        skillLevel--;

        if (conjuredItems.Length >= skillLevel)
        {
            if (conjuredItems[skillLevel] != null && player.inventory.CanAdd(new Item(conjuredItems[skillLevel].item), conjuredItems[skillLevel].amount))
            {
                if (UnityEngine.Random.value <= conjuredItems[skillLevel].baseSuccessChance + conjuredItems[skillLevel].bonusChancePerLevel * skillLevel)
                {
                    player.inventory.Add(new Item(conjuredItems[skillLevel].item), conjuredItems[skillLevel].amount);
                    player.Tools_TargetAddMessage(conjuredMessage + conjuredItems[skillLevel].item.name);
                    player.Tools_ShowPopup(conjuredMessage + conjuredItems[skillLevel].item.name);
                }
                else
                {
                    player.Tools_TargetAddMessage(failedMessage + conjuredItems[skillLevel].item.name);
                }
            }
            else
            {
                player.Tools_TargetAddMessage(failedMessage + conjuredItems[skillLevel].item.name);
            }
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // Item_Conjureable
    // -----------------------------------------------------------------------------------
    [System.Serializable]
    public class Item_Conjureable
    {
        public ScriptableItem item;
        public int amount;
        [Range(0, 1)] public float baseSuccessChance;
        [Range(0, 1)] public float bonusChancePerLevel;
    }

    // -----------------------------------------------------------------------------------
}
