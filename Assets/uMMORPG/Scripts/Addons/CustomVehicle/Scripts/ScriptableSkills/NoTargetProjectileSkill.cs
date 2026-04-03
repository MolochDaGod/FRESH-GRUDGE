
//(c8

using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "bMMORPG Skill/NO Target Projectile", order = 999)]
public class NoTargetProjectileSkill : DamageSkill
{
    [Header("Projectile")]
    public NoTargetProjectileSkillEffect projectile; // Arrows, Bullets, Fireballs, ...

    private void ConsumeRequiredWeaponsAmmo(Entity caster)
    {
        // requires no weapon category?
        // then we can't find weapon and check ammo. just allow it.
        // (monsters have no weapon requirements and don't even have an
        //  equipment component)
        if (string.IsNullOrWhiteSpace(requiredWeaponCategory))
            return;

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
    }

    public override void Apply(Entity caster, int skillLevel)
    {
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
            NoTargetProjectileSkillEffect effect = go.GetComponent<NoTargetProjectileSkillEffect>();
            effect.target = caster.target;
            effect.caster = caster;
            effect.damage = damage.Get(skillLevel);
            effect.stunChance = stunChance.Get(skillLevel);
            effect.stunTime = stunTime.Get(skillLevel);
            NetworkServer.Spawn(go);
        }
        else Debug.LogWarning(name + ": missing projectile");
    }

    public override bool CheckDistance(Entity caster, int skillLevel, out Vector3 destination)
    {
        // Destination is to move to the target because out of range
        destination = default;

        // No target projectile always say distance ok
        return true;
    }

    public override bool CheckTarget(Entity caster)
    {
        // No target projectile always think they have a target
        return true;
    }
}
