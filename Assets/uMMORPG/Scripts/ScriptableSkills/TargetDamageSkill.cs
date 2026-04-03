using UnityEngine;
using Mirror;

[CreateAssetMenu(menuName = "uMMORPG Skill/Target Damage", order = 999)]
public class TargetDamageSkill : DamageSkill
{
    public GameObject effectPrefab; // Prefab to spawn on the target

    public override bool CheckTarget(Entity caster)
    {
        // target exists, alive, not self, ok type?
        return caster.target != null && caster.CanAttack(caster.target);
    }

    public override bool CheckDistance(Entity caster, int skillLevel, out Vector3 destination)
    {
        // target still around?
        if (caster.target != null)
        {
            destination = Utils.ClosestPoint(caster.target, caster.transform.position);
            return Utils.ClosestDistance(caster, caster.target) <= castRange.Get(skillLevel);
        }
        destination = caster.transform.position;
        return false;
    }

    public override void Apply(Entity caster, int skillLevel)
    {
        // Deal damage directly with base damage + skill damage  
        caster.combat.DealDamageAt(caster.target,
                                   caster.combat.damage + damage.Get(skillLevel),
                                   stunChance.Get(skillLevel),
                                   stunTime.Get(skillLevel));

        // Spawn the effect prefab at the target's position  
        if (effectPrefab != null && caster.target != null)
        {
            GameObject effectInstance = Instantiate(effectPrefab, caster.target.transform.position, Quaternion.identity);
            NetworkServer.Spawn(effectInstance); // Spawn the effect on the network  
        }
    }
}
