#if _iMMOTOOLS
using Mirror;
using UnityEngine;

// ===================================================================================
// POPUP AREA - BOX
// ===================================================================================
#if _iMMO2D
[RequireComponent(typeof(BoxCollider2D))]
#else
[RequireComponent(typeof(BoxCollider))]
#endif
public partial class Tools_AreaBox_Popup : NetworkBehaviour
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
#if _iMMO2D
    private void OnTriggerEnter2D(Collider2D co)
#else
    private void OnTriggerEnter(Collider co)
#endif
    {

        Player player = co.GetComponentInParent<Player>();
        if (messageOnEnter != "")
        {
            //player player = co.GetComponentInParent<player>();
            if (player)
            {
#if _iMMOPVP
                if (player.GetAlliedRealms(realmId, alliedRealmId))
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
                if (player.GetAlliedRealms(realmId, alliedRealmId))
#endif
                
                    player.Tools_ShowPopup(messageOnExit);
            }
        }
    }

    // -----------------------------------------------------------------------------------
}
#endif