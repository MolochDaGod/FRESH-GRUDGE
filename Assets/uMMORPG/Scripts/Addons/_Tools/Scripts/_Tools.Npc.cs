#if _iMMOTOOLS_LEGACY_CORE
using Mirror;
using TMPro;
using UnityEngine;

// ===================================================================================
// NPC
// ===================================================================================
public partial class Npc
{
    public TMP_Text displayName;
    // Required to keep track of how many players are within this Npcs interaction range
    [SyncVar, HideInInspector] public int accessingPlayers = 0;
}
#endif
