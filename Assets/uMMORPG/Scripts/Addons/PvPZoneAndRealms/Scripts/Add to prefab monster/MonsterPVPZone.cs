using Mirror;
using UnityEngine;

// MONSTER
public class MonsterPVPZone : NetworkBehaviour
{ 
    public Monster monster;
    public Health health;

    [Header("[-=-[ PvP Zone Settings ]-=-]")]
    [Tooltip("[Optional] Set to true and RealmId + AlliedRealmId will be set to the Attacker's after the Monster respawned")]
    public bool captureable;

    [Tooltip("[Required] Neutral: Only retaliates, Aggressive: Attacks & retaliates, Pacifistic: Never attacks or retaliates")]
    public Entity.AggroBehaviour aggroBehaviour;

    // -------------------------------------------------------------------------------
    // Awake
    // -------------------------------------------------------------------------------
    private void Awake()
    {
        if (monster.Realm != null)
            monster.hashRealm = monster.Realm.name.GetStableHashCode();

        if (monster.Ally != null)
            monster.hashAlly = monster.Ally.name.GetStableHashCode();

        monster._originalRealm = monster.hashRealm;
        monster._originalAlly = monster.hashAlly;
        monster._aggroBehaviour = aggroBehaviour;
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
        if (captureable && monster.lastAggressor != null)
        {
            // -- set to realm of attacker
            monster.SetRealm(monster.lastAggressor.hashRealm, monster.lastAggressor.hashAlly);
            monster.lastAggressor = null;
        }
        else if (!captureable)
        {
            // -- revert to original realm
            RevertRealm();
            aggroBehaviour = monster._aggroBehaviour;
        }
    }

    // -------------------------------------------------------------------------------
    // RevertRealm
    // -------------------------------------------------------------------------------
    public void RevertRealm()
    {
        monster.SetRealm(monster._originalRealm, monster._originalAlly);
    }
#endif
    // -----------------------------------------------------------------------------------
}