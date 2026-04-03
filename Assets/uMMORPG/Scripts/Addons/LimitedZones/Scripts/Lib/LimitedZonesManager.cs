using System.Linq;
using System.Collections.Generic;
using UnityEngine;

// SHARED INSTANCE MANAGER
public class LimitedZonesManager : MonoBehaviour
{
    public LimitedZonesEntry[] sharedInstances;

    // -----------------------------------------------------------------------------------
    // getAvailableSharedInstances
    // Retrieve a list of shared instances the player is allowed to see
    // -----------------------------------------------------------------------------------
    public List<LimitedZonesEntry> getAvailableSharedInstances(Player player, int instanceCategory)
    {
        return sharedInstances.Where(x => x.instanceCategory == instanceCategory && x.canPlayerSee(player)).ToList();
    }

    // -----------------------------------------------------------------------------------
}