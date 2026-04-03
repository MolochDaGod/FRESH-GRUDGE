using UnityEngine;

// TARGET HEAL SKILL

[CreateAssetMenu(menuName = "ADDON/Skills/Target/Heal", order = 999)]
public partial class Skill_TargetHeal : HealSkill
{
    [Header("[-=-[ Target Heal Skill ]-=-]")]

    [Header("[Effects]")]
    public LinearInt unstunSeconds;

    public LinearFloat unstunChance;
    public LinearInt modifyCooldown;
    public LinearFloat modifyCooldownChance;
    public LinearInt modifyBuffDuration;
    public LinearFloat modifyBuffDurationChance;
    public BuffType buffType;

    [Header("[Experience]")]
    [Range(0, 9999)] public float expPerHealth;

    [Range(0, 9999)] public float skillExpPerHealth;

    [Header("[Targets]")]
    public bool canHealSelf = true;

    public bool canHealOthers = false;

    // -----------------------------------------------------------------------------------
    // CorrectedTarget
    // -----------------------------------------------------------------------------------
    private Entity CorrectedTarget(Entity caster)
    {
        // Si caster n'a pas de target et que l'on peux se soigné, on retourne caster.targer = caster (soi même)
        if (caster.target == null || caster.target == caster)
            return canHealSelf ? caster : null;

        // Permet de pouvoir l'utilisé sur un monstre par exemple un monstre peut soigné un monstre
        if (caster.target.GetType() == caster.GetType())
        {
            // peut soigné quelu'un d'autre du même Type (player,monstre,npc...)
            if (canHealOthers)
                return caster.target;
            // Peut se soigné soit même
            else if (canHealSelf)
                return caster;
            // sinon rien
            else
                return null;
        }

        return canHealSelf ? caster : null;
    }

    // -----------------------------------------------------------------------------------
    // CheckTarget
    // -----------------------------------------------------------------------------------
    public override bool CheckTarget(Entity caster)
    {
        caster.target = CorrectedTarget(caster);
        return caster.target != null && caster.target.isAlive;
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
#if _SERVER
        if (caster.target != null && caster.target.isAlive)
        {
            int maxHealth = caster.target.health.max;
            int oldHealth = caster.target.health.current;

            caster.target.health.current += healsHealth.Get(skillLevel);
            caster.target.mana.current += healsMana.Get(skillLevel);
            if (oldHealth < maxHealth)
            {
                // 109- 90 = 19
                int diffmax = (maxHealth - oldHealth);
                int healValue = (diffmax > healsHealth.Get(skillLevel)) ? healsHealth.Get(skillLevel) : diffmax;
                caster.target.combat.RpcOnReceivedHeal(healValue);
            }
            // -- check for unstun
            if (unstunChance.Get(skillLevel) > 0 && caster.target.Tools_Stunned() && UnityEngine.Random.value <= unstunChance.Get(skillLevel))
            {
                caster.target.Tools_SetStun(unstunSeconds.Get(skillLevel));
            }

            // -- check for skill cooldown
            if (modifyCooldownChance.Get(skillLevel) > 0)
            {
                for (int i = 0; i < caster.target.skills.skills.Count; ++i)
                {
                    Skill skill = caster.target.skills.skills[i];
                    if (skill.IsOnCooldown() && UnityEngine.Random.value <= modifyCooldownChance.Get(skillLevel))
                    {
                        skill.cooldownEnd += modifyCooldown.Get(skillLevel);
                        caster.target.skills.skills[i] = skill;
                        
                    }
                }
            }

            // -- check for buff duration
            if (modifyBuffDurationChance.Get(skillLevel) > 0)
            {
                for (int i = 0; i < caster.target.skills.buffs.Count; ++i)
                {
                    Buff buff = caster.target.skills.buffs[i];
                    if (buff.CheckBuffType(buffType) && buff.BuffTimeRemaining() > 0 && UnityEngine.Random.value <= modifyBuffDurationChance.Get(skillLevel))
                    {
                        buff.buffTimeEnd += modifyBuffDuration.Get(skillLevel);
                        caster.target.skills.buffs[i] = buff;
                    }
                }
            }

            // -- check for experience gain on heal
            if (caster is Player)
            {
                if (expPerHealth > 0)
                    ((Player)caster).experience.current += (int)Mathf.Round((caster.target.health.current - oldHealth) * expPerHealth);

                if (skillExpPerHealth > 0)
                    ((PlayerSkills)caster.skills).skillExperience += (int)Mathf.Round((caster.target.health.current - oldHealth) * skillExpPerHealth);
            }

            SpawnEffect(caster, caster.target);
        }
#endif
    }
    // -----------------------------------------------------------------------------------
}
