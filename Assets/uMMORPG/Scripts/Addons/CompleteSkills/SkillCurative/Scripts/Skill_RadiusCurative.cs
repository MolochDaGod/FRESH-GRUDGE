using System.Collections.Generic;
using UnityEngine;

// RADIUS CURATIVE SKILL

[CreateAssetMenu(menuName = "ADDON/Skills/Aura/Curative", order = 999)]
public class Skill_RadiusCurative : Skill_Curative
{
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
        List<Entity> targets = new List<Entity>();

        if (affectSelf)
            targets.Add(caster);

        if (caster is Player player)
            targets.AddRange(player.Tools_GetCorrectedTargetsInSphere(caster.transform, castRadius.Get(skillLevel), reviveChance.Get(skillLevel) > 0, affectSelf, affectOwnParty, affectOwnGuild, affectOwnRealm, reverseTargeting, affectPlayers, affectNpcs, affectMonsters, affectPets));
        else
            targets.AddRange(caster.Tools_GetCorrectedTargetsInSphere(caster.transform, castRadius.Get(skillLevel), reviveChance.Get(skillLevel) > 0, affectSelf, affectOwnParty, affectOwnGuild, affectOwnRealm, reverseTargeting, affectPlayers, affectNpcs, affectMonsters, affectPets));

        ApplyToTargets(targets, caster, skillLevel);

        targets.Clear();
#endif
    }

    // -----------------------------------------------------------------------------------
}
