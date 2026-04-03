using System.Collections.Generic;
using Mirror;

// =======================================================================================
// PLAYER
// =======================================================================================
public partial class Player
{

    LimitedZonesManager sharedInstanceManager;

    // -----------------------------------------------------------------------------------
    // Cmd_teleportPlayerToInstance
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_teleportPlayerToInstance(int index, int instanceCategory, int instanceIndex)
    {

        if (!sharedInstanceManager)
            sharedInstanceManager = FindObjectOfType<LimitedZonesManager>();

        List<LimitedZonesEntry> instancesAvailable = sharedInstanceManager.getAvailableSharedInstances(this, instanceCategory);

        instancesAvailable[instanceIndex].payEntranceCost(this);

        PlayerGroupLocations locations = instancesAvailable[instanceIndex].targetArea.playerGroupLocation[index];
        
        if (locations.teleportPosition.Length == 0) return;

        index = UnityEngine.Random.Range(0, locations.teleportPosition.Length - 1);

        movement.Warp(locations.teleportPosition[index].position);

    }
}