#if _iMMOTOOLS
using Mirror;
using UnityEngine;

// ===================================================================================
// POPUP AREA - SPHERE
// ===================================================================================
[RequireComponent(typeof(SphereCollider))]
public partial class Tools_AreaSphere_Popup : NetworkBehaviour
{
#if _iMMOPVP
    [Tooltip("Show the messages only to members or allies of this realm")]
    public int realmId;
    public int alliedRealmId;
#endif
    public string messageOnEnter;
    public string messageOnExit;
    [Range(0, 255)] public byte iconId;
    [Range(0, 255)] public byte soundId;

    // -----------------------------------------------------------------------------------
    // OnTriggerEnter
    // @Client
    // -----------------------------------------------------------------------------------
    private void OnTriggerEnter(Collider co)
    {
        if (messageOnEnter != "")
        {
            Player player = co.GetComponentInParent<Player>();
            if (player)
            {
#if _iMMOPVP
                if (player.GetAlliedRealms(realmId, alliedRealmId))
                {
#endif
                    player.Tools_ShowPopup(messageOnEnter, iconId, soundId);
#if _iMMOPVP
                }
#endif
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // OnTriggerExit
    // @Client
    // -----------------------------------------------------------------------------------
    private void OnTriggerExit(Collider co)
    {
        if (messageOnExit != "")
        {
            Player player = co.GetComponentInParent<Player>();
            if (player)
            {
#if _iMMOPVP
                if (player.GetAlliedRealms(realmId, alliedRealmId))
                {
#endif

                    player.Tools_ShowPopup(messageOnExit, iconId, soundId);
#if _iMMOPVP
                }
#endif
            }
        }
    }

    // -----------------------------------------------------------------------------------
}
#endif