using Mirror;
using UnityEngine;

// ===================================================================================
// TIMEGATE
// ===================================================================================
#if _iMMO2D
[RequireComponent(typeof(Collider2D))]
#else
[RequireComponent(typeof(Collider))]
#endif
public class Area_Timegate : NetworkBehaviour
{
    [Header("[-=-[ TIMEGATE ]-=-]")]
    [Tooltip("One click deactivation")]
    public bool isActive = true;

    [Tooltip("[Required] Any on scene Transform or GameObject OR off scene coordinates (requires Network Zones AddOn)")]
    public Tools_TeleportationTarget teleportationTarget;

    [Tooltip("Maximum number of visits while the gate is open")]
    [Min(0)] public int maxVisits = 10;

    [Tooltip("Minimum number of hours that must pass between visits while open")]
    [Min(0)] public int hoursBetweenVisits = 10;

    [Tooltip("The day this timegate will open (set 0 to disable)"), Range(0, 31)]
    public int dayStart = 1;

    [Tooltip("The day this timegate will close (set 0 to disable)"), Range(0, 31)]
    public int dayEnd = 1;

    [Tooltip("The month this timegate is open (set 0 to disable)"), Range(0, 12)]
    public int activeMonth = 1;

#if _SERVER
    // -------------------------------------------------------------------------------
    // OnTriggerEnter
    // -------------------------------------------------------------------------------
#if _iMMO2D
    private void OnTriggerEnter2D(Collider2D co)
#else
    private void OnTriggerEnter(Collider co)
#endif
    {
        Player player = co.GetComponentInParent<Player>();
        if (player && player.isActiveAndEnabled && isActive && teleportationTarget.Valid)
        {
            player.playerAddonsConfigurator.myTimegate = this;
            player.playerAddonsConfigurator.EnteredInTimegate(this);
        }
    }

    // -------------------------------------------------------------------------------
    // OnTriggerExit
    // -------------------------------------------------------------------------------
#if _iMMO2D
    private void OnTriggerExit2D(Collider2D co)
#else
    private void OnTriggerExit(Collider co)
#endif
    {
        Player player = co.GetComponentInParent<Player>();
        if (player && isActive && teleportationTarget.Valid)
        {
            player.playerAddonsConfigurator.ExitTimegate();
        }
    }
#endif
    // -------------------------------------------------------------------------------
}
