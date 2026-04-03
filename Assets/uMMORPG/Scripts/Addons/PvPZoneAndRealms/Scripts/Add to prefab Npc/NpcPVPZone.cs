using Mirror;
using UnityEngine;

// MONSTER
public class NpcPVPZone : NetworkBehaviour
{
    public Npc npc;
    public Health health;

    [Header("[-=-[ PvP Zone Settings ]-=-]")]
    public bool noRealmCombat;
    [Tooltip("[Optional] Set to true and RealmId + AlliedRealmId will be set to the Attacker's after the Npc respawned")]
    public bool captureable;

    [Tooltip("[Required] Neutral: Only retaliates, Aggressive: Attacks & retaliates, Pacifistic: Never attacks or retaliates")]
    public Entity.AggroBehaviour aggroBehaviour;

    
    // -------------------------------------------------------------------------------
    // Awake
    // -------------------------------------------------------------------------------
    private void Awake()
    {
        if (npc.Realm != null)
            npc.hashRealm = npc.Realm.name.GetStableHashCode();

        if (npc.Ally != null)
            npc.hashAlly = npc.Ally.name.GetStableHashCode();

        npc._originalRealm = npc.hashRealm;
        npc._originalAlly = npc.hashAlly;
        npc._aggroBehaviour = aggroBehaviour;
    }

#if _SERVER
    // ----------------------------------------------------------------------------------
    // Auto Add in Health event
    // ---------------------------------------------------------------------------------- 
    public override void OnStartServer()
    {
        health.onEmpty.AddListener(OnDeath);
    }

    // -------------------------------------------------------------------------------
    // OnDeath
    // -------------------------------------------------------------------------------
    [Server]
    private void OnDeath()
    {
        if (captureable && npc.lastAggressor != null)
        {
            // -- set to realm of attacker
            npc.SetRealm(npc.lastAggressor.hashRealm, npc.lastAggressor.hashAlly);
            npc.lastAggressor = null;
        }
        else if (!captureable)
        {
            // -- revert to original realm
            RevertRealm();
            aggroBehaviour = npc._aggroBehaviour;
        }
    }

    // -------------------------------------------------------------------------------
    // RevertRealm
    // -------------------------------------------------------------------------------
    public void RevertRealm()
    {
        npc.SetRealm(npc._originalRealm, npc._originalAlly);
    }
#endif
    // -----------------------------------------------------------------------------------
}