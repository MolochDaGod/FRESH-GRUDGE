using Mirror;
using System.Collections.Generic;
using UnityEngine;

// SKILL TARGETTED AOE

[CreateAssetMenu(menuName = "ADDON/Skills/Targetted/AOE", order = 999)]
public class Skill_TargettedAOE : DamageSkill
{
    [Header("[-=-[ Target AOE Skill ]-=-]")]
    public OneTimeTargetSkillEffect effect;

    public LinearFloat castRadius;
    public LinearFloat triggerAggroChance;
    public bool SpawnEffectOnMainTargetOnly;

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
    public bool affectNpc;
    public bool affectMonsters;
    public bool affectPets;

    // -----------------------------------------------------------------------------------
    // SpawnEffect
    // -----------------------------------------------------------------------------------
    public void SpawnEffect(Entity caster, Entity spawnTarget)
    {
        if (effect != null)
        {
            GameObject go = Instantiate(effect.gameObject, spawnTarget.transform.position, Quaternion.identity);
            go.GetComponent<OneTimeTargetSkillEffect>().caster = caster;
            go.GetComponent<OneTimeTargetSkillEffect>().target = spawnTarget;
            NetworkServer.Spawn(go);
        }
    }

    // -----------------------------------------------------------------------------------
    // CheckTarget
    // -----------------------------------------------------------------------------------
    public override bool CheckTarget(Entity caster)
    {
        return caster.target != null && caster.target != caster;
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
            destination = caster.target.collider.ClosestPoint(caster.transform.position);
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

        if (caster is Player)
            targets = ((Player)caster).Tools_GetCorrectedTargetsInSphere(caster.target.transform, castRadius.Get(skillLevel), false, affectSelf, affectOwnParty, affectOwnGuild, affectOwnRealm, reverseTargeting, affectPlayers, affectNpc, affectMonsters, affectPets);
        else
            targets = caster.Tools_GetCorrectedTargetsInSphere(caster.target.transform, castRadius.Get(skillLevel), false, affectSelf, affectOwnParty, affectOwnGuild, affectOwnRealm, reverseTargeting, affectPlayers, affectNpc, affectMonsters, affectPets);

        foreach (Entity targeted in targets)
        {
            // deal damage directly with base damage + skill damage
            caster.combat.DealDamageAt(targeted,
                            caster.combat.damage + damage.Get(skillLevel),
                            stunChance.Get(skillLevel),
                            stunTime.Get(skillLevel));

            targeted.Tools_ApplyBuff(applyBuff, buffLevel.Get(skillLevel), buffChance.Get(skillLevel));

            if (!SpawnEffectOnMainTargetOnly)
                SpawnEffect(caster, targeted);

            if (UnityEngine.Random.value <= triggerAggroChance.Get(skillLevel))
                targeted.target = caster;
        }

        targets.Clear();
    }
}
