using System.Collections.Generic;
using UnityEngine;

// TARGET CURATIVE SKILL

[CreateAssetMenu(menuName = "ADDON/Skills/Target/Curative", order = 999)]
public class Skill_TargetCurative : Skill_Curative
{
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
        List<Entity> targets = new List<Entity>();

        if (SpawnEffectOnMainTargetOnly)
            SpawnEffect(caster, caster.target);

        if (
            (caster is Player && caster.target is Player && ((Player)caster).Tools_SameCheck((Player)caster.target, affectSelf, affectPlayers, affectOwnParty, affectOwnGuild, affectOwnRealm, reverseTargeting)) ||
            (caster.target is Monster && affectMonsters) ||
            (caster.target == caster && affectSelf)
            )
        {
            if (reviveChance.Get(skillLevel) > 0 && !caster.target.isAlive)
                targets.Add(caster.target);
            else if (reviveChance.Get(skillLevel) <= 0 && caster.target.isAlive)
                targets.Add(caster.target);
        }

        if (castRadius.Get(skillLevel) > 0)
        {
            if (caster is Player)
                targets.AddRange(((Player)caster).Tools_GetCorrectedTargetsInSphere(caster.target.transform, castRadius.Get(skillLevel), reviveChance.Get(skillLevel) > 0, affectSelf, affectOwnParty, affectOwnGuild, affectOwnRealm, reverseTargeting, affectPlayers, affectNpcs, affectMonsters, affectPets));
            else
                targets.AddRange(caster.Tools_GetCorrectedTargetsInSphere(caster.target.transform, castRadius.Get(skillLevel), reviveChance.Get(skillLevel) > 0, affectSelf, affectOwnParty, affectOwnGuild, affectOwnRealm, reverseTargeting, affectPlayers, affectNpcs, affectMonsters, affectPets));
        }

        ApplyToTargets(targets, caster, skillLevel);

        targets.Clear();
#endif
    }
    // -----------------------------------------------------------------------------------
}
