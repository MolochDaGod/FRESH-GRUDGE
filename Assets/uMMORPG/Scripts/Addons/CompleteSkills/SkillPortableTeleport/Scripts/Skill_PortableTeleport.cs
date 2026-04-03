using UnityEngine;

// Skill Portable Teleport

[CreateAssetMenu(menuName = "ADDON/Skills/Self/Portable Teleport", order = 999)]
public class Skill_PortableTeleport : ScriptableSkill
{
    [Header("[-=-[ Skill Portable Teleport ]-=-]")]
    [Tooltip("[Required] GameObject prefab with coordinates OR off scene coordinates (requires Network Zones AddOn)")]
    public Tools_TeleportationTarget teleportationTarget;

    [Tooltip("This will ignore the teleport Location and choose the nearest spawn point instead")]
    public bool teleportToClosestSpawnpoint;

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public override bool CheckTarget(Entity caster)
    {
        return true;
    }

    // -----------------------------------------------------------------------------------
    //
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
    //
    // -----------------------------------------------------------------------------------
#if _iMMO2D
    public override void Apply(Entity caster, int skillLevel, Vector2 target)
#else
    public override void Apply(Entity caster, int skillLevel)
#endif
    {
#if _SERVER
        // apply only to alive people
        if (caster.isAlive)
        {
            // -- Determine Teleportation Target
            if (teleportToClosestSpawnpoint)
            {
                Transform targeted = NetworkManagerMMO.GetNearestStartPosition(caster.transform.position);
                ((Player)caster).Tools_Warp(targeted.position);
            }
            else
            {
                teleportationTarget.OnTeleport((Player)caster);
            }
        }
#endif
    }
    // -----------------------------------------------------------------------------------
}
