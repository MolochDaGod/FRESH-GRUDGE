using UnityEngine;

// TARGET PROJECTILE - ITEM

[CreateAssetMenu(menuName = "ADDON/Item/Item Target Projectile", order = 999)]
public class Item_TargetProjectile : UsableItem
{
    [Header("[-=-[ Target Projectile Item ]-=-]")]
    public TargetProjectileSkill projectile;

    public int level;

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
            return Utils.ClosestDistance(caster, caster.target) <= projectile.castRange.Get(skillLevel);
        }
        destination = caster.transform.position;
        return false;
    }

#if _SERVER
    // -----------------------------------------------------------------------------------
    // Use
    // @Server
    // -----------------------------------------------------------------------------------
    public override void Use(Player player, int inventoryIndex)
    {
#if _iMMO2D
        Vector2 destination;
#else
        Vector3 destination;
#endif

        if (projectile != null && level > 0 && player.target != null && player.CanAttack(player.target) && CheckDistance(player, level, out destination))
        {
            // always call base function too
            base.Use(player, inventoryIndex);

            // launch projectile
#if _iMMO2D
            projectile.Apply(player, level, destination);
#else
            projectile.Apply(player, level);
#endif
            // decrease amount
            ItemSlot slot = player.inventory.slots[inventoryIndex];
            slot.DecreaseAmount(1);
            player.inventory.slots[inventoryIndex] = slot;
        }
    }
#endif

    // tooltip
    /*public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{USAGEHEALTH}", usageHealth.ToString());
        tip.Replace("{USAGEMANA}", usageMana.ToString());
        tip.Replace("{USAGEEXPERIENCE}", usageExperience.ToString());
        tip.Replace("{USAGEPETHEALTH}", usagePetHealth.ToString());
        return tip.ToString();
    }*/

    // -----------------------------------------------------------------------------------
}
