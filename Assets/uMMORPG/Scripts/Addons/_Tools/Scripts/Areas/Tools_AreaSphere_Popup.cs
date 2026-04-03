#if _iMMOTOOLS
using Mirror;
using UnityEngine;

// ===================================================================================
// POPUP AREA - SPHERE
// ===================================================================================
#if _iMMO2D
[RequireComponent(typeof(CircleCollider2D))]
#else
[RequireComponent(typeof(SphereCollider))]
#endif
public partial class Tools_AreaSphere_Popup : NetworkBehaviour
{
#if _iMMOPVP
    [Tooltip("Show the messages only to members or allies of this realm")]
    public Tmpl_Realm realm;
    public Tmpl_Realm alliedRealm;
#endif
    public string messageOnEnter;
    public string messageOnExit;
    [Range(0, 255)] public byte iconId;
    [Range(0, 255)] public byte soundId;

#if _CLIENT
    // -----------------------------------------------------------------------------------
    // OnTriggerEnter
    // @Client
    // -----------------------------------------------------------------------------------
#if _iMMO2D
    private void OnTriggerEnter2D(Collider2D co)
#else
    private void OnTriggerEnter(Collider co)
#endif
    {
        if (messageOnEnter != "")
        {
            Player player = co.GetComponentInParent<Player>();
            if (player)
            {
#if _iMMOPVP
                if (player.GetAlliedRealms(((realm) ? realm.name.GetStableHashCode() : 0), ((alliedRealm) ? alliedRealm.name.GetStableHashCode() : 0)))
#endif
                    player.Tools_ShowPopup(messageOnEnter);
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // OnTriggerExit
    // @Client
    // -----------------------------------------------------------------------------------
#if _iMMO2D
    private void OnTriggerExit2D(Collider2D co)
#else
    private void OnTriggerExit(Collider co)
#endif
    {
        if (messageOnExit != "")
        {
            Player player = co.GetComponentInParent<Player>();
            if (player)
            {
#if _iMMOPVP
                if (player.GetAlliedRealms(((realm) ? realm.name.GetStableHashCode() : 0), ((alliedRealm) ? alliedRealm.name.GetStableHashCode() : 0)))
#endif
                    player.Tools_ShowPopup(messageOnExit);
            }
        }
    }
#endif
    // -----------------------------------------------------------------------------------
}
#endif