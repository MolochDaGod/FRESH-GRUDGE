using UnityEngine;

// Teleportation Destination

[System.Serializable]
public partial class TeleportationDestination
{
    [Tooltip("[Optional] All of those requirements must be met in order for the teleporter to be active")]
    public Tools_Requirements teleportationRequirement;

    [Tooltip("[Required] Any on scene Transform or GameObject OR off scene coordinates (requires Network Zones AddOn)")]
    public Tools_TeleportationTarget teleportationTarget;
}
