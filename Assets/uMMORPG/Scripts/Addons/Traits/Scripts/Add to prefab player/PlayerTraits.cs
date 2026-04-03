using Mirror;
using UnityEngine;

// PLAYER
public class PlayerTraits : NetworkBehaviour
{
    public Player player;
    [Header("[-=-[ Traits ]-=-]")]
    public int TraitPoints;

    public readonly SyncList<Trait> Traits = new SyncList<Trait>();

    // -----------------------------------------------------------------------------------
}