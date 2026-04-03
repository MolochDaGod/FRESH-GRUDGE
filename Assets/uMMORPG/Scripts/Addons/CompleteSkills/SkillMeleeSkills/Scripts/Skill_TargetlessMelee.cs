
using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// TARGETLESS MELEE SKILL

[CreateAssetMenu(menuName = "ADDON/Skills/Targetless/Melee", order = 999)]
public partial class Skill_TargetlessMelee : Tools_DamageSkill
{
    [Header("[-=-[ Targetless Melee Settings ]-=-]")]

    public LinearInt minAngle = new LinearInt { baseValue = -45 };

    public LinearInt maxAngle = new LinearInt { baseValue = 45 };

    public LinearInt minHitsPerTarget = new LinearInt { baseValue = 1 };
    public LinearInt maxHitsPerTarget = new LinearInt { baseValue = 1 };

    protected List<Entity> correctedTargets = new List<Entity>();
    protected Entity _target = null;
    
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

#if _SERVER
    // -----------------------------------------------------------------------------------
    // Apply
    // -----------------------------------------------------------------------------------
#if _iMMO2D
    public override void Apply(Entity caster, int skillLevel, Vector2 target)
#else
    public override void Apply(Entity caster, int skillLevel)
#endif
    {
#if _iMMO2D
        base.Apply(caster, skillLevel, target);
#else
        base.Apply(caster, skillLevel);
#endif
        AffectDetectedTargets(caster, skillLevel);
    }

    // -----------------------------------------------------------------------------------
    // AffectDetectedTargets
    // -----------------------------------------------------------------------------------
    protected void AffectDetectedTargets(Entity caster, int skillLevel)
    {
        int hits = UnityEngine.Random.Range(minHitsPerTarget.Get(skillLevel), maxHitsPerTarget.Get(skillLevel));

        if (hits <= 0 || castRange.Get(skillLevel) <= 0) return;

        correctedTargets.Clear();

        int layerMask = ~(1 << 2); //2= ignore raycast
        Collider[] hitColliders = Physics.OverlapSphere(caster.transform.position, castRange.Get(skillLevel), layerMask);

        foreach (Collider hitCollider in hitColliders)
        {
            Entity target = hitCollider.GetComponentInParent<Entity>();

            if (target != null && target != caster && caster.CanAttack(target) && !correctedTargets.Any(x => x == target))
            {
                Vector3 direction = target.transform.position - caster.transform.position;
                float angle = Vector3.Angle(direction, caster.transform.forward);

                if (angle >= minAngle.Get(skillLevel) && angle <= maxAngle.Get(skillLevel))
                    correctedTargets.Add(target);
            }
        }

        foreach (Entity detectedTarget in correctedTargets)
        {
            for (int i = 0; i < hits; i++)
                OnSkillImpact(caster, detectedTarget, skillLevel);
        }
    }

    // -----------------------------------------------------------------------------------
    // OnSkillImpact
    // -----------------------------------------------------------------------------------
    protected void OnSkillImpact(Entity caster, Entity _target, int skillLevel)
    {
        List<Entity> targets = new List<Entity>();

        // ------ spawn visual effect if any
        if (visualEffectOnMainTargetOnly || impactRadius.Get(skillLevel) <= 0)
            SpawnEffect(caster, _target);

        // ------ get all valid targets
        if (impactRadius.Get(skillLevel) > 0)
        {
            if (caster is Player)
                targets = ((Player)caster).Tools_GetCorrectedTargetsInSphere(_target.transform, impactRadius.Get(skillLevel), false, notAffectSelf, notAffectOwnParty, notAffectOwnGuild, notAffectOwnRealm, reverseTargeting, notAffectPlayers, notAffectNpcs, notAffectMonsters, notAffectPets);
            else
                targets = caster.Tools_GetCorrectedTargetsInSphere(_target.transform, impactRadius.Get(skillLevel), false, notAffectSelf, notAffectOwnParty, notAffectOwnGuild, notAffectOwnRealm, reverseTargeting, notAffectPlayers, notAffectNpcs, notAffectMonsters, notAffectPets);
        }
        else
        {
            targets.Add(_target);
        }

        // ----- apply effects to targets
        foreach (Entity target in targets)
        {
            // ------ Deal Damage

            int dmg = damage.Get(skillLevel);
            if (addCasterDamage) dmg += caster.combat.damage;

            float stunChnce = stunChance.Get(skillLevel);
#if _iMMOATTRIBUTES
            if (stunAddAccuracy) stunChnce = target.Tools_HarmonizeChance(stunChnce, caster.combat.accuracy);
#endif

            caster.combat.DealDamageAt(target, dmg, stunChnce, UnityEngine.Random.Range(minStunTime.Get(skillLevel), maxStunTime.Get(skillLevel)));

            // ------ Remove random Buff
            if (removeRandomBuff.Get(skillLevel) > 0 && caster.target.skills.buffs.Count > 0)
            {
                float removeChnce = 0;
#if _iMMOATTRIBUTES
                if (removeAddAccuracy) removeChnce = caster.combat.accuracy;
#endif
                caster.target.Tools_CleanupStatusBuffs(removeChance.Get(skillLevel), removeChnce, removeRandomBuff.Get(skillLevel));
            }

            // ------ Cooldown Target
            if (cooldownChance.Get(skillLevel) > 0)
            {
                float cldwnChnce = cooldownChance.Get(skillLevel);
#if _iMMOATTRIBUTES
                if (cooldownAddAccuracy) cldwnChnce = target.Tools_HarmonizeChance(cldwnChnce, caster.combat.accuracy);
#endif
                for (int i = 0; i < target.skills.skills.Count; ++i)
                {
                    Skill skill = target.skills.skills[i];
                    if (skill.IsOnCooldown() && UnityEngine.Random.value <= cldwnChnce)
                    {
                        skill.cooldownEnd += cooldownDuration.Get(skillLevel);
                        target.skills.skills[i] = skill;
                    }
                }
            }

            // ------ Recoil Target
            if (recoilChance.Get(skillLevel) > 0 && minRecoilTarget.Get(skillLevel) > -100f && maxRecoilTarget.Get(skillLevel) > -100f)
            {
                float recoilChnce = recoilChance.Get(skillLevel);
#if _iMMOATTRIBUTES
                if (recoilAddAccuracy) recoilChnce = target.Tools_HarmonizeChance(recoilChnce, caster.combat.accuracy);
#endif
                if (UnityEngine.Random.value <= recoilChnce)
                    target.Tools_Recoil(caster, UnityEngine.Random.Range(minRecoilTarget.Get(skillLevel), maxRecoilTarget.Get(skillLevel)));
            }

            // ------ Apply Buff  (if any)
            if (applyBuff.Length > 0 && applyBuff.Length >= skillLevel && applyBuff[skillLevel - 1] != null)
            {
                float buffModifier = 0;
#if _iMMOATTRIBUTES
                if (buffAddAccuracy) buffModifier = caster.combat.accuracy;
#endif
                target.Tools_ApplyBuff(applyBuff[skillLevel - 1], buffLevel.Get(skillLevel), buffChance.Get(skillLevel), buffModifier);
            }

            // ------ Spawn visual effect (if any)
            if (!visualEffectOnMainTargetOnly && impactRadius.Get(skillLevel) > 0)
                SpawnEffect(caster, target);

            // ------ Check for Aggro Trigger
            target.Tools_OnAggro(caster, triggerAggroChance.Get(skillLevel));
        }

        // ------ create object at impact loaction
        if (createOnTarget.Length > 0 && createOnTarget.Length >= skillLevel - 1 && createOnTarget[skillLevel - 1] != null && UnityEngine.Random.value <= createChance.Get(skillLevel))
        {
            GameObject go = Instantiate(createOnTarget[skillLevel - 1], caster.target.transform.position, caster.target.transform.rotation);
            NetworkServer.Spawn(go);
        }

        /*if (impactEffect != null)
        {
            // Instantiate the prefab at the target's position
            GameObject go = Instantiate(impactEffect, _target.transform.position, _target.transform.rotation);

            // Set the target as the parent of the instantiated GameObject
            go.transform.parent = _target.transform;

            // Spawn the object on the server
            NetworkServer.Spawn(go);

            // Set a delay before destroying the prefab
            Destroy(go, destroyDelay);
        }*/
    }
#endif
    // -----------------------------------------------------------------------------------
}
