using System.Text;
using UnityEngine;

// TARGET REVIVE ITEM

[CreateAssetMenu(menuName = "ADDON/Item/Item Target Revive", order = 999)]
public class Item_TargetRevive : UsableItem
{
    [Header("[-=-[ Target Revive Item ]-=-]")]
    [Range(0, 1)] public float successChance;

    public float distance;
    [Range(1, 100)] public int reviveHealthPercent = 10;
    [Range(1, 100)] public int reviveManaPercent     = 10;
    public bool ReviveInCasterPosition;

    [Header("[-=-[ Buff on Target ]-=-]")]
    public BuffSkill applyBuff;

    public int buffLevel;
    [Range(0, 1)] public float buffChance;

    public string successMessage = "You revived: ";
    public string failedMessage = "You failed to revive: ";

    [Header("[-=-[ Targeting ]-=-]")]

    [Tooltip("[Optional] Does affect members of the own party")]
    public bool affectOwnParty;

    [Tooltip("[Optional] Does affect members of the own guild")]
    public bool affectOwnGuild;

    [Tooltip("[Optional] Does affect members of the own realm (requires PVP ZONE AddOn")]
    public bool affectOwnRealm;

    public bool affectPlayers;
    [Tooltip("Decrease amount by how many each use (can be 0)?")]
    public int decreaseAmount = 1;

    // -----------------------------------------------------------------------------------
    // CheckTarget
    // -----------------------------------------------------------------------------------
    public bool CheckTarget(Entity caster)
    {
        return caster.target != null && caster.CanAttack(caster.target);
    }

    // -----------------------------------------------------------------------------------
    // CheckDistance
    // -----------------------------------------------------------------------------------
#if _iMMO2D
    public bool CheckDistance(Entity caster, int skillLevel, out Vector2 destination)
 #else
    public bool CheckDistance(Entity caster, int skillLevel, out Vector3 destination)
#endif
    {
        if (caster.target != null)
        {
            destination = caster.target.collider.ClosestPointOnBounds(caster.transform.position);
            return Utils.ClosestDistance(caster, caster.target) <= distance;
        }
        destination = caster.transform.position;
        return false;
    }

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
            if (player.target != null && player.target is Player && !player.target.isAlive && player.Tools_GetCorrectedTargets(player.target, false, false, affectOwnParty, affectOwnGuild, affectOwnRealm, false, false, false, false, false) && Utils.ClosestDistance(player, player.target) <= distance)
            {
                // always call base function too
                base.Use(player, inventoryIndex);

                if (UnityEngine.Random.value <= successChance)
                {
                    player.target.health.current += Mathf.RoundToInt((player.target.health.max / 100) * reviveHealthPercent);
                    player.target.mana.current += Mathf.RoundToInt((player.target.mana.max / 100) * reviveManaPercent);
                    player.target.Tools_ApplyBuff(applyBuff, buffLevel, buffChance);
                    player.target.Tools_OverrideState("IDLE");
                    if(ReviveInCasterPosition)
                        player.target.movement.Warp(player.transform.position);
                    player.Tools_TargetAddMessage(successMessage + player.target.name);
                }
                else
                {
                    player.Tools_TargetAddMessage(failedMessage + player.target.name);
                }

                // decrease amount
                slot.DecreaseAmount(decreaseAmount);
                player.inventory.slots[inventoryIndex] = slot;
            }
        }
    }
#endif

    // tooltip
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{SUCCESSCHANCE}", Mathf.RoundToInt(successChance*100).ToString());
        tip.Replace("{MAXDISTANCE}", distance.ToString());
        tip.Replace("{HEALSHEALTH}", reviveHealthPercent.ToString());
        tip.Replace("{HEALSMANA}", reviveManaPercent.ToString());
        if (applyBuff != null)
        {
            tip.Replace("{ADDBUFF}", applyBuff.name.ToString());
            tip.Replace("{ADDBUFFLEVEL}", buffLevel.ToString());
            tip.Replace("{ADDBUFFCHANCE}", Mathf.RoundToInt(buffChance * 100).ToString());
        }
        tip.Replace("{COSTUSE}", decreaseAmount.ToString());
        return tip.ToString();
    }
    // -----------------------------------------------------------------------------------
}
