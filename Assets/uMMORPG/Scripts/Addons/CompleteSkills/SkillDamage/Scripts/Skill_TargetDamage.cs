using System.Collections.Generic;
using UnityEngine;

// =======================================================================================
// 
// =======================================================================================
[CreateAssetMenu(menuName = "ADDON/Skills/Target/Damage", order = 999)]
public class Skill_TargetDamage : Skill_BaseDamage
{
	
	// -----------------------------------------------------------------------------------
    // CheckTarget
    // -----------------------------------------------------------------------------------
    public override bool CheckTarget(Entity caster)
    {
        return caster.target != null || affectSelf;
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
        // target still around?
        if (caster.target != null)
        {
            destination = caster.target.collider.ClosestPointOnBounds(caster.transform.position);
            return Utils.ClosestDistance(caster, caster.target) <= castRange.Get(skillLevel);
        }
        destination = caster.transform.position;
        return false;
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
        List<Entity> targets = new List<Entity>();

        if (SpawnEffectOnMainTargetOnly)
            SpawnEffect(caster, caster.target);
		
		if (caster.target is Player && caster is Player && ((Player)caster).Tools_SameCheck((Player)caster.target, affectSelf, affectPlayers, affectOwnParty, affectOwnGuild, affectOwnRealm, reverseTargeting) ||
			(caster.target is Monster && affectMonsters ) ||
			(caster is Monster && caster.target is Monster && affectMonsters) ||
			(caster is Monster && caster.target is Player && affectPlayers)
		)
        	if (caster.target.isAlive)
                targets.Add(caster.target);
		
		if (castRadius.Get(skillLevel) > 0) {
		
        	if (caster is Player)
        	    targets.AddRange( ((Player)caster).Tools_GetCorrectedTargetsInSphere(caster.target.transform, castRadius.Get(skillLevel), false, affectSelf, affectOwnParty, affectOwnGuild, affectOwnRealm, reverseTargeting, affectPlayers, affectNpcs, affectMonsters, affectPets) );
        	else
            	targets.AddRange( caster.Tools_GetCorrectedTargetsInSphere(caster.target.transform, castRadius.Get(skillLevel), false, affectSelf, affectOwnParty, affectOwnGuild, affectOwnRealm, reverseTargeting, affectPlayers, affectNpcs, affectMonsters, affectPets) );
		
		}
		
        ApplyToTargets(targets, caster, skillLevel);

        targets.Clear();
    }
	
	// -----------------------------------------------------------------------------------
	
}

// =======================================================================================