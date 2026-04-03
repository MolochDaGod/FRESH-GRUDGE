#if _iMMOPVP
using UnityEditor;
using UnityEngine;

// CONVERT SKILL
[HelpURL("https://mmo-indie.com/addon/complete-skills")]
[CreateAssetMenu(menuName = "ADDON/Skills/Self/Convert", order = 999)]
public class Skill_Convert : ScriptableSkill
{

    //[Header("[-=-[ REQUIRE PVP&REALM ADDON ]-=-]", order = 0), Header("For use this please before install PVP&Realm addon !", order = 1)]
    //public bool iUnderstant;
  
    [Header("[-=-[ Convert Skill ]-=-]")]
    public LinearFloat successChance;

    [Tooltip("[Optional] Maximum level of the target (0 to disable)")]
    public LinearInt maxTargetLevel;

    [Tooltip("[Optional] Uses player level as a base to calculate max Level of monster")]
    public bool basePlayerLevel;

    public string convertMessage = "You converted: ";
    public string failedMessage = "You failed to convert: ";
    public string errorMessage = "You cannot convert that target!";
    public string errorLevel = "This target at too high a level!";
    public string errorNotCapturable = "this target cannot be captured!";

    //private bool checkedTarget = false; // require for only one message in chat
    // -----------------------------------------------------------------------------------
    // CheckTarget
    // -----------------------------------------------------------------------------------
    public override bool CheckTarget(Entity caster)
    {
        if (caster.target != null && caster.target is Monster monster)
        {
            // CheckiIf if level max monster is level player and modify maxLever is necessary
            maxLevel = (basePlayerLevel) ? caster.level.current : maxLevel;

            // Check if monster is capturable or not
            bool capturable = monster.monsterPVPZone.captureable;

            // 
            if (monster.level.current <= maxLevel)
            {
                if (capturable)
                    return caster.CanAttack(caster.target);
                else
                    ((Player)caster).Tools_TargetAddMessage(errorNotCapturable);
            }
            else
            {
                ((Player)caster).Tools_TargetAddMessage(errorLevel);
            }
        }
        else
        {
            ((Player)caster).Tools_TargetAddMessage(errorMessage);
        }
        return false;// caster.target != null && caster.CanAttack(caster.target); // false ??
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
        if (caster.target != null && caster.target is Monster monster)
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
#if _SERVER
        Player player = (Player)caster;

        if (player.target != null && player.target is Monster monster)
        {
            int maxLevel = maxTargetLevel.Get(skillLevel);

            if (basePlayerLevel)
                maxLevel += player.level.current;

            // Monster monster = player.target.GetComponent<Monster>();
            if (monster.level.current <= maxLevel && UnityEngine.Random.value <= successChance.Get(skillLevel))
            {
                monster.SetRealm(player.hashRealm, player.hashAlly);
                monster.health.current = monster.health.max;
                monster.target = null;
                player.Tools_TargetAddMessage(convertMessage + monster.name);
            }
            else
            {
                player.Tools_TargetAddMessage(failedMessage + monster.name);
            }
        }
#endif
    }
    // -----------------------------------------------------------------------------------
}
#endif