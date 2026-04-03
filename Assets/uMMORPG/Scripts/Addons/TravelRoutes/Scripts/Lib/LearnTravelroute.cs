using Mirror;
using UnityEngine;

// TRAVELROUTE - BOX COLLIDER
#if _iMMO2D
[RequireComponent(typeof(BoxCollider2D))]
#else
[RequireComponent(typeof(BoxCollider))]
#endif
[RequireComponent(typeof(NetworkIdentity))]
public class LearnTravelroute : NetworkBehaviour
{

    [Header("[-=-[ TRAVEL ROUTES ]-=-]")]
    [Tooltip("[Optional] One click deactivation")]
    public bool isActive = true;


    [Tooltip("[Optional] Travelroutes unlocked on enter")]
    public Unlockroute[] Unlockroutes;

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
        if (player != null && player.isAlive && isActive && (player == player.isLocalPlayer || isServer))
        {
            player.playerTravelroute.UnlockTravelroutes();
        }
    }
    // -----------------------------------------------------------------------------------
}