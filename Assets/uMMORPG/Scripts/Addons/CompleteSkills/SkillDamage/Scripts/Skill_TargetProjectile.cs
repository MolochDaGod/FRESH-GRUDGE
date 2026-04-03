using System.Text;
using UnityEngine;
using Mirror;

// =======================================================================================
// TARGET PROJECTILE SKILL
// =======================================================================================
[CreateAssetMenu(menuName= "ADDON/Skills/Target/Projectile", order=999)]
public class Skill_TargetProjectile : Skill_BaseDamage
{
    [Header("[-=-[ Projectile ]-=-]")]
    public SkillEffect_Projectile projectile; // Arrows, Bullets, Fireballs, ...

    bool HasRequiredWeaponAndAmmo(Entity caster)
    {
        int weaponIndex = caster.equipment.GetEquippedWeaponIndex();
        if (weaponIndex != -1)
        {
            // no ammo required, or has that ammo equipped?
            WeaponItem itemData = (WeaponItem)caster.equipment.slots[weaponIndex].item.data;
            return itemData.requiredAmmo == null ||
                   caster.equipment.GetItemIndexByName(itemData.requiredAmmo.name) != -1;
        }
        return true;
    }


    public override bool CheckSelf(Entity caster, int skillLevel)
    {
        // check base and ammo
        return base.CheckSelf(caster, skillLevel) &&
               HasRequiredWeaponAndAmmo(caster);
    }

    public override bool CheckTarget(Entity caster)
    {
        // target exists, alive, not self, oktype?
        return caster.target != null && caster.CanAttack(caster.target);
    }

#if _iMMO2D
    public override bool CheckDistance(Entity caster, int skillLevel, out Vector2 destination)
#else
    public override bool CheckDistance(Entity caster, int skillLevel, out Vector3 destination)
#endif
    {
        // target still around?
        if (caster.target != null)
        {
            destination = caster.target.collider.ClosestPointOnBounds(caster.transform.position);
            return Utils.ClosestDistance(caster, caster.target) <= castRange.Get(skillLevel);
        }
        destination = caster.transform.position;
        return false;
    }

#if _iMMO2D
    public override void Apply(Entity caster, int skillLevel, Vector2 target)
#else
    public override void Apply(Entity caster, int skillLevel)
#endif
    {
#if _SERVER
        // consume ammo if needed
        ConsumeRequiredWeaponsAmmo(caster);

        // spawn the skill effect. this can be used for anything ranging from
        // blood splatter to arrows to chain lightning.
        // -> we need to call an RPC anyway, it doesn't make much of a diff-
        //    erence if we use NetworkServer.Spawn for everything.
        // -> we try to spawn it at the weapon's projectile mount
        if (projectile != null)
        {
            GameObject go = Instantiate(projectile.gameObject, caster.skills.effectMount.position, caster.skills.effectMount.rotation);
            SkillEffect_Projectile effect = go.GetComponent<SkillEffect_Projectile>();
            effect.target = caster.target;
            effect.caster = caster;
            effect.skillLevel = skillLevel;
            effect.parentSkill = this;
            NetworkServer.Spawn(go);
        }
        else Debug.LogWarning(name + ": missing projectile");
    }
    void ConsumeRequiredWeaponsAmmo(Entity caster)
    {
        int weaponIndex = caster.equipment.GetEquippedWeaponIndex();
        if (weaponIndex != -1)
        {
            // no ammo required, or has that ammo equipped?
            WeaponItem itemData = (WeaponItem)caster.equipment.slots[weaponIndex].item.data;
            if (itemData.requiredAmmo != null)
            {
                int ammoIndex = caster.equipment.GetItemIndexByName(itemData.requiredAmmo.name);
                if (ammoIndex != 0)
                {
                    // reduce it
                    ItemSlot slot = caster.equipment.slots[ammoIndex];
                    --slot.amount;
                    caster.equipment.slots[ammoIndex] = slot;
                }
            }
        }
#endif
    }
}
