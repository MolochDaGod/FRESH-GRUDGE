using UnityEngine;

// Unlockroute

[System.Serializable]
public class Unlockroute
{
    [Header("[-=-[ UNLOCK ROUTE ]-=-]")]
    [Tooltip("[Required] Any on scene Transform or GameObject OR off scene coordinates (requires Network Zones AddOn)")]
    public Tools_TeleportationTarget teleportationTarget;

    [Tooltip("[Optional] The amount of experience is granted only once, when the route is first discovered")]
    public int ExpGain;
}
