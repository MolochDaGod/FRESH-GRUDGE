using Mirror;
using UnityEngine;

// PET
public class PetPVPZone : NetworkBehaviour
{
    public Pet pet;
    public Health health;

    [Header("[-=-=-[ PvP Zone Settings ]-=-=-]")]
    [Tooltip("[Optional] Set to true and RealmId + AlliedRealmId will be set to the Attacker's after the Monster respawned")]
    public bool captureable;

    [Tooltip("[Required] Neutral: Only retaliates, Aggressive: Attacks & retaliates, Pacifistic: Never attacks or retaliates")]
    public Entity.AggroBehaviour aggroBehaviour;

    // -------------------------------------------------------------------------------
    // Awake
    // -------------------------------------------------------------------------------
    private void Awake()
    {
        if (pet.Realm != null)
			pet.hashRealm = pet.Realm.name.GetStableHashCode();

		if (pet.Ally != null)
			pet.hashAlly = pet.Ally.name.GetStableHashCode();

		pet._originalRealm = pet.hashRealm;
		pet._originalAlly = pet.hashAlly;
		pet._aggroBehaviour = aggroBehaviour;
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
		if (captureable && pet.lastAggressor != null)
        {
            // -- set to realm of attacker
			pet.SetRealm(pet.lastAggressor.hashRealm, pet.lastAggressor.hashAlly);
			pet.lastAggressor = null;
        }
        else if (!captureable)
        {
            // -- revert to original realm
            RevertRealm();
			aggroBehaviour = pet._aggroBehaviour;
        }
    }

    // -------------------------------------------------------------------------------
    // RevertRealm
    // -------------------------------------------------------------------------------
    public void RevertRealm()
    {
		pet.SetRealm(pet._originalRealm, pet._originalAlly);
    }
#endif
    // -----------------------------------------------------------------------------------
}