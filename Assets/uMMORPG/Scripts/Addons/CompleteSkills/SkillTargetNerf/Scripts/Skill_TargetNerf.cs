using Mirror;
using UnityEngine;

// SKILL TARGET NERF

[CreateAssetMenu(menuName = "ADDON/Skills/Target/Nerf", order = 999)]
public class Skill_TargetNerf : DamageSkill
{
    [Header("[-=-[ Skill Effect on Caster ]-=-]")]
    public OneTimeTargetSkillEffect effect;

    [Header("[-=-[ New Buff on Target? ]-=-]")]
    public BuffSkill applyBuff;

    public LinearInt buffLevel;
    public LinearFloat buffChance;

    public override bool CheckTarget(Entity caster)
    {
        // target exists, alive, not self, oktype?
        return caster.target != null && caster.CanAttack(caster.target);
    }

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

    // helper function to spawn the skill effect on caster
    // (used by all the buff implementations and to load them after saving)
    public void SpawnEffect(Entity caster, Entity spawnTarget)
    {
        if (effect != null)
        {
            GameObject go = Instantiate(effect.gameObject, caster.skills.effectMount.transform.position, Quaternion.identity);
            go.transform.LookAt(new Vector3(spawnTarget.transform.position.x, spawnTarget.transform.position.y, spawnTarget.transform.position.z));
            go.GetComponent<OneTimeTargetSkillEffect>().target = caster;
            NetworkServer.Spawn(go);
        }
    }

    // events for client sided effects /////////////////////////////////////////
    // [Client]
    public override void OnCastStarted(Entity caster)
    {
        base.OnCastStarted(caster);

        SpawnEffect(caster, caster.target);
    }



#if _iMMO2D
    public override void Apply(Entity caster, int skillLevel, Vector2 target)
#else
    public override void Apply(Entity caster, int skillLevel)
#endif
    {
#if _SERVER
        // deal damage directly with base damage + skill damage
        caster.combat.DealDamageAt(caster.target, caster.combat.damage + damage.Get(skillLevel), stunChance.Get(skillLevel), stunTime.Get(skillLevel));

        // apply the buff
        caster.target.Tools_ApplyBuff(applyBuff, buffLevel.Get(skillLevel), buffChance.Get(skillLevel));
#endif
    }
}
