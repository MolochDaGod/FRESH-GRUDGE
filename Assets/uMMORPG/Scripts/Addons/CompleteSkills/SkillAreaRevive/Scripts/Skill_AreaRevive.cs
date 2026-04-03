using System.Collections.Generic;
using System.Text;
using UnityEngine;

// SKILL AREA REVIVE

[CreateAssetMenu(menuName = "ADDON/Skills/Area/Revive", order = 999)]
public class Skill_AreaRevive : HealSkill
{
    [Header("[-=-[ Buff on Target ]-=-]")]
    public BuffSkill applyBuff;

    public LinearInt buffLevel;
    public LinearFloat buffChance;

    public bool reverseTargeting;

    public bool affectOwnParty;
    public bool affectOwnGuild;
    public bool affectOwnRealm;

    public bool affectPlayers;
    public bool affectNpcs;
    public bool affectEnemies;
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
            targets = ((Player)caster).Tools_GetCorrectedTargetsInSphere(caster.transform, castRange.Get(skillLevel), true, affectOwnParty, affectOwnGuild, affectOwnRealm, reverseTargeting, affectPlayers, affectNpcs, affectEnemies, affectPets);
        else
            targets = caster.Tools_GetCorrectedTargetsInSphere(caster.transform, castRange.Get(skillLevel), true, affectOwnParty, affectOwnGuild, affectOwnRealm, reverseTargeting, affectPlayers, affectNpcs, affectEnemies, affectPets);

        foreach (Entity targeted in targets)
        {
            targeted.health.current += healsHealth.Get(skillLevel);
            targeted.mana.current += healsMana.Get(skillLevel);
            targeted.Tools_ApplyBuff(applyBuff, buffLevel.Get(skillLevel), buffChance.Get(skillLevel));
            targeted.Tools_OverrideState("IDLE");
            SpawnEffect(caster, targeted);
        }

        targets.Clear();
#endif
    }

    /*public override string ToolTip(int skillLevel, bool showRequirements = false)
    {
        StringBuilder tip = new StringBuilder(base.ToolTip(skillLevel, showRequirements));
        tip.Replace("{HEALSHEALTH}", healsHealth.Get(skillLevel).ToString());
        tip.Replace("{HEALSMANA}", healsMana.Get(skillLevel).ToString());
        return tip.ToString();
    }*/
}
