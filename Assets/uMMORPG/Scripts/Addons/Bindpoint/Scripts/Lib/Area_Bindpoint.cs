using Mirror;
using UnityEngine;
using UnityEngine.AI; // Unity's built-in navigation system

// BINDPOINT AREA
#if _iMMO3D
    [RequireComponent(typeof(CircleCollider2D))]
#else
[RequireComponent(typeof(SphereCollider))]
#endif
public class Area_Bindpoint : NetworkBehaviour
{
    [Header("[-=-[ Collider ]-=-]")]
    public new SphereCollider collider;

    [Header("[-=-[ Bindpoint ]-=-]")]
    public Transform bindpoint;

    [Header("[-=-[ Popups ]-=-]")]
    public Tools_PopupClass enterPopup;

    // -------------------------------------------------------------------------------
    // OnTriggerEnter
    // -------------------------------------------------------------------------------
    [ServerCallback]
    private void OnTriggerEnter(Collider co)
    {
        Player player = co.GetComponentInParent<Player>();
        if (player && bindpoint != null && player.playerAddonsConfigurator.MyBindpoint.name != bindpoint.gameObject.name)
        {
            player.playerAddonsConfigurator.SetBindpointFromArea(bindpoint.gameObject.name, bindpoint.position.x, bindpoint.position.y, bindpoint.position.z);
            player.Tools_ShowPopup(enterPopup.message);
        }
    }

    // -------------------------------------------------------------------------------
}

