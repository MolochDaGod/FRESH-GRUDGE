using UnityEngine;

// SKILL PARTY LEADER BUFF

[CreateAssetMenu(menuName = "ADDON/Skills/Party/Teleport", order = 999)]
public class Skill_PartyTeleport : ScriptableSkill
{
    [Header("[-=-[ Party Teleport ]-=-]")]
    public bool CasterMustBeLeader;

    [Tooltip("[Optional] Members must be within distance to leader in order to teleport (0 for unlimited distance)")]
    public float maxDistanceToCaster;

    [Tooltip("[Required] GameObject prefab with coordinates OR off scene coordinates (requires Network Zones AddOn)")]
    public Tools_TeleportationTarget teleportationTarget;

    [Tooltip("This will ignore the teleport Location and choose the nearest spawn point instead")]
    public bool teleportToClosestSpawnpoint;

    // -----------------------------------------------------------------------------------
    // CheckTarget
    // -----------------------------------------------------------------------------------
    public override bool CheckTarget(Entity caster)
    {
        if (((Player)caster).party.InParty() && ( (!CasterMustBeLeader || (CasterMustBeLeader && ((Player)caster).party.party.master == caster.name)) ))
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
        foreach (string member in ((Player)caster).party.party.members)
        {
            Player player = UMMO_Tools.FindOnlinePlayerByName(member);

            // -- Teleport everybody but not the caster
            if (player != null && player != (Player)caster)
            {
                // -- Check Distance
                if (maxDistanceToCaster <= 0 || Utils.ClosestDistance(player, caster) <= maxDistanceToCaster || member == ((Player)caster).party.party.master)
                {
                    // -- Determine Teleportation Target
                    if (teleportToClosestSpawnpoint)
                    {
                        Transform targeted = NetworkManagerMMO.GetNearestStartPosition(player.transform.position);
                        player.Tools_Warp(targeted.position);
                    }
                    else
                    {
                        teleportationTarget.OnTeleport(player);
                    }
                }

                player = null;
            }
        }

        // -- Teleport the caster now
        if (teleportToClosestSpawnpoint)
        {
            Transform targeted = NetworkManagerMMO.GetNearestStartPosition(((Player)caster).transform.position);
            ((Player)caster).Tools_Warp(targeted.position);
        }
        else
        {
            teleportationTarget.OnTeleport(((Player)caster));
        }
#endif
    }
    // -----------------------------------------------------------------------------------
}
