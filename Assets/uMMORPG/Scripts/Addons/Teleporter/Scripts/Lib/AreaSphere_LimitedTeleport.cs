using UnityEngine;

// ===================================================================================
// LIMITED TELEPORT AREA (SPHERE)
// ===================================================================================
#if _iMMO2D
[RequireComponent(typeof(CircleCollider2D))]
#else
[RequireComponent(typeof(SphereCollider))]
#endif
public class AreaSphere_LimitedTeleport : MonoBehaviour
{
    public Area_LimitedTeleport[] connectedTeleporters;

#if _SERVER
    // -----------------------------------------------------------------------------------
    // OnTriggerExit
    // -----------------------------------------------------------------------------------
#if _iMMO2D
    private void OnTriggerExit2D(Collider2D co)
#else
    private void OnTriggerExit(Collider co)
#endif
    {
        Player player = co.GetComponentInParent<Player>();

        if (player != null && connectedTeleporters != null && connectedTeleporters.Length > 0)
        {
            foreach (Area_LimitedTeleport teleporter in connectedTeleporters)
            {
                if (teleporter.enterLimit > 0)
                {
                    if (teleporter.groupType == Area_LimitedTeleport.GroupType.None)
                    {
                        teleporter.enterCount--;
                    }
                    else if (teleporter.groupType == Area_LimitedTeleport.GroupType.Party && player.party.InParty())
                    {
                        if (teleporter.enterParty == player.party.party.members[0])
                            teleporter.enterCount--;
                    }
                    else if (teleporter.groupType == Area_LimitedTeleport.GroupType.Guild && player.guild.InGuild())
                    {
                        if (teleporter.enterGuild == player.guild.name)
                            teleporter.enterCount--;
                    }
                    else if (teleporter.groupType == Area_LimitedTeleport.GroupType.Realm)
                    {
#if _iMMOPVP
                        if (player.GetAlliedRealms(teleporter.interactionRequirements.requiredRealm, teleporter.interactionRequirements.requiredAlly))
                            teleporter.enterCount--;
#endif
                    }

                    if (teleporter.enterCount <= 0)
                        teleporter.ResetLimits();
                }
            }
        }
    }
#endif
    // -----------------------------------------------------------------------------------
}
