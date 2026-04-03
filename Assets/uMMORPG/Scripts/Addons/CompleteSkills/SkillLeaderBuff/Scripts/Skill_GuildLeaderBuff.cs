using UnityEngine;

// SKILL PARTY LEADER BUFF

[CreateAssetMenu(menuName = "ADDON/Skills/Guild/Leader Buff", order = 999)]
public class Skill_GuildLeaderBuff : BuffSkill
{
    [Header("[-=-[ Leader Buff on Target ]-=-]")]
    public BuffSkill applyBuff;

    public bool CasterMustBeLeader;

    // -----------------------------------------------------------------------------------
    // CheckTarget
    // -----------------------------------------------------------------------------------
    public override bool CheckTarget(Entity caster)
    {
        if (((Player)caster).guild.InGuild() &&
            (
            (!CasterMustBeLeader ||
            (CasterMustBeLeader &&
            ((Player)caster).guild.guild.master == caster.name))
            ))
        {
            return true;
        }
        else
        {
            return false;
        }
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
        foreach (GuildMember member in ((Player)caster).guild.guild.members)
        {
            if (Player.onlinePlayers.ContainsKey(member.name))
            {
                Player player = Player.onlinePlayers[member.name];
                if (player.isAlive)
                {
                    player.Tools_ApplyBuff(applyBuff, skillLevel, 1);
                }
            }
        }
#endif
    }
    // -----------------------------------------------------------------------------------
}
