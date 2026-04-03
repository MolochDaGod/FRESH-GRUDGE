using Mirror;
using System.Collections.Generic;
using UnityEngine;

// SKILL TARGETTED AOE

[CreateAssetMenu(menuName = "ADDON/Skills/Aura/AOE", order = 999)]
public class Skill_AuraAOE : DamageSkill
{
    [Header("[-=-[ Target AOE Skill ]-=-]")]
    public OneTimeTargetSkillEffect effect;

    public LinearFloat castRadius;
    public LinearFloat triggerAggroChance;

    [Tooltip("[Optional] Add caster damage to total damage or not?")]
    public bool useCasterDamage;

#if _iMMOATTRIBUTES

    [Tooltip("[Optional] Add caster accuracy to the buff chance?")]
    public bool useCasterAccuracy;

#endif

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
        Debug.Log("on passe la ?");
        if (caster is Player)
            targets = ((Player)caster).Tools_GetCorrectedTargetsInSphere(caster.transform, castRadius.Get(skillLevel), false, affectSelf, affectOwnParty, affectOwnGuild, affectOwnRealm, reverseTargeting, affectPlayers, affectNpcs, affectMonsters, affectPets);
        else
            targets = caster.Tools_GetCorrectedTargetsInSphere(caster.transform, castRadius.Get(skillLevel), false, affectSelf, affectOwnParty, affectOwnGuild, affectOwnRealm, reverseTargeting, affectPlayers, affectNpcs, affectMonsters, affectPets);

        foreach (Entity targeted in targets)
        {
            int dmg = damage.Get(skillLevel);

            if (useCasterDamage)
                dmg += caster.combat.damage;

#if _iMMOATTRIBUTES
            float buffModifier = 0;
            if (useCasterAccuracy) buffModifier = targeted.Tools_HarmonizeChance(buffModifier, caster.combat.accuracy);
#endif

            // deal damage directly with base damage + skill damage
            caster.combat.DealDamageAt(targeted, dmg, stunChance.Get(skillLevel), stunTime.Get(skillLevel));

            targeted.Tools_ApplyBuff(applyBuff, buffLevel.Get(skillLevel), buffChance.Get(skillLevel));

            SpawnEffect(caster, targeted);

            if (UnityEngine.Random.value <= triggerAggroChance.Get(skillLevel))
                targeted.target = caster;
        }

        targets.Clear();
#endif
    }

#if _SERVER
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

#endif
}
