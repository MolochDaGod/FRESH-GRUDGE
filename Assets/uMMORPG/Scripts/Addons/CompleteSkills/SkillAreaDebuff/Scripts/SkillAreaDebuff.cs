using System.Collections.Generic;
using UnityEngine;

// SKILL AREA DEBUFF

[CreateAssetMenu(menuName = "ADDON/Skills/Area/Debuff", order = 999)]
public class SkillAreaDebuff : HealSkill
{
    [Header("[-=-[ Area Debuff Skill ]-=-]")]
    [Tooltip("% Chance to strip each Buff from the target (Buff = Buff NOT set to 'disadvantegous')")]
    public LinearFloat successChance;

    public LinearFloat triggerAggroChance;

    [Header("[-=-[ Apply Buff on Target ]-=-]")]
    public BuffSkill applyBuff;

    public LinearInt buffLevel;
    public LinearFloat buffChance;

    [Header("[-=-[ Targeting ]-=-]")]
    [Tooltip("[Optional] Changes 'affect' affect into 'not affect' and vice-versa")]
    public bool reverseTargeting;

    [Tooltip("[Optional] Does affect the caster")]
    public bool affectSelf;

    [Tooltip("[Optional] Does affect members of the own party")]
    public bool affectOwnParty;

    [Tooltip("[Optional] Does affect members of the own guild")]
    public bool affectOwnGuild;

    [Tooltip("[Optional] Does affect members of the own realm (requires PVP ZONE AddOn")]
    public bool affectOwnRealm;

    public bool affectPlayers;
    public bool affectNpcs;
    public bool affectMonsters;
    public bool affectPets;

    // -----------------------------------------------------------------------------------
    // CheckTarget
    // -----------------------------------------------------------------------------------
    public override bool CheckTarget(Entity caster)
    {
        // no target necessary, but still set to self so that LookAt(target)
        // doesn't cause the player to look at a target that doesn't even matter
        caster.target = caster;
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
        // can cast anywhere
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

        if (caster is Player)
            targets = ((Player)caster).Tools_GetCorrectedTargetsInSphere(caster.transform, castRange.Get(skillLevel), false, affectSelf, affectOwnParty, affectOwnGuild, affectOwnRealm, reverseTargeting, affectPlayers, affectNpcs, affectMonsters, affectPets);
        else
            targets = caster.Tools_GetCorrectedTargetsInSphere(caster.transform, castRange.Get(skillLevel), false, affectSelf, affectOwnParty, affectOwnGuild, affectOwnRealm, reverseTargeting, affectPlayers, affectNpcs, affectMonsters, affectPets);

        foreach (Entity targeted in targets)
        {
            targeted.Tools_CleanupStatusBuffs(successChance.Get(skillLevel));
            targeted.health.current += healsHealth.Get(skillLevel);
            targeted.mana.current += healsMana.Get(skillLevel);

            targeted.Tools_ApplyBuff(applyBuff, buffLevel.Get(skillLevel), buffChance.Get(skillLevel));

            SpawnEffect(caster, targeted);

            if (UnityEngine.Random.value <= triggerAggroChance.Get(skillLevel))
                targeted.target = caster;
        }

        targets.Clear();
#endif
    }
}
