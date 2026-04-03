using Mirror;
using UnityEngine;

// ===================================================================================
// HAZARD FLOOR
// ===================================================================================
#if _iMMO2D
[RequireComponent(typeof(CircleCollider2D))]
#else
[RequireComponent(typeof(SphereCollider))]
#endif
public class UsageRequirementsArea : NetworkBehaviour
{
    public int usageAreaId;

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
        if (player && usageAreaId > 0)
            player.UsageRequirementsAreaEnter(usageAreaId);
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
        if (player)
            player.UsageRequirementsAreaExit();
    }

    // -------------------------------------------------------------------------------
}
