using Mirror;
using UnityEngine;

// PLAYER

public partial class Entity
{
    public enum AggroBehaviour { Neutral, Aggressive, Pacifistic, SuperAggressive }

    //[Header("[-=-[ Realm Configuration ]-=-]")]
    //public Tmpl_RealmEntityConfiguration realmConfiguration;

    [Header("[-=-[ Entity : REALM ]-=-]")]
    public Tmpl_Realm Realm;
    public Tmpl_Realm Ally;

    [SyncVar, HideInInspector] public int hashRealm;
    [SyncVar, HideInInspector] public int hashAlly;

    [HideInInspector] public int _originalRealm;
    [HideInInspector] public int _originalAlly;
    [HideInInspector] public AggroBehaviour _aggroBehaviour;

#if _iMMOBUILDSYSTEM
    [Header("[-=-[ BUILD SYTEM ]-=-]")]
    public bool dontAttackOwner;
    public bool dontAttackOwnerParty;
    public bool dontAttackOwnerGuild;
#endif

    // -------------------------------------------------------------------------------
    // GetAttackAllowance
    // -------------------------------------------------------------------------------
    public virtual bool GetAttackAllowance(Entity entity)
    {
        return true;
    }

    // -----------------------------------------------------------------------------------
    // GetInPvpRegion
    // -----------------------------------------------------------------------------------
    public virtual bool GetInPvpRegion()
    {
        return true;
    }

    // -------------------------------------------------------------------------------
    // SetRealm
    // -------------------------------------------------------------------------------
    public void SetRealm(Tmpl_Realm newRealm, Tmpl_Realm newAlly)
    {
        int newRealmHash = (newRealm != null) ? newRealm.name.GetStableHashCode() : 0;
        int newAllyHash = (newAlly != null) ? newAlly.name.GetStableHashCode() : 0;
        SetRealm(newRealmHash, newAllyHash);
    }

    // -------------------------------------------------------------------------------
    // SetRealm
    // -------------------------------------------------------------------------------
    public void SetRealm(int newRealmHash, int newAllyHash)
    {
        if (newRealmHash != 0 && Tmpl_Realm.All.TryGetValue(newRealmHash, out Tmpl_Realm realmData))
        {
            Realm = realmData;
            hashRealm = realmData.name.GetStableHashCode();
        }

        if (newAllyHash != 0 && Tmpl_Realm.All.TryGetValue(newAllyHash, out Tmpl_Realm allyData))
        {
            Ally = allyData;
            hashAlly = allyData.name.GetStableHashCode();
        }
    }

    // -------------------------------------------------------------------------------
    // GetAlliedRealms
    // -------------------------------------------------------------------------------
    public bool GetAlliedRealms(Entity target)
    {
        return GetAlliedRealms(target.hashRealm, target.hashAlly);
    }

    // -------------------------------------------------------------------------------
    // GetHostileRealms
    // -------------------------------------------------------------------------------
    public bool GetHostileRealms(Entity target)
    {
        return GetHostileRealms(target.hashRealm, target.hashAlly);
    }

    // -----------------------------------------------------------------------------------
    // GetHostileRealms
    // -----------------------------------------------------------------------------------
    public bool GetHostileRealms(int targetRealm, int targetAlliedRealm)
    {
        if ((Realm == null && Ally == null) || (targetRealm == 0 && targetAlliedRealm == 0))
        {
            //Debug.Log("((Realm == null && Ally == null) || (targetRealm == 0 && targetAlliedRealm == 0))");
            return true;
        }

        if (hashRealm == 0 && targetRealm == 0 || hashAlly == 0 && targetAlliedRealm == 0)
        {
            //Debug.Log("(hashRealm == 0 && targetRealm == 0 || hashAlly == 0 && targetAlliedRealm == 0)");
            return true;
        }

        if (hashRealm == targetRealm || (hashAlly == targetAlliedRealm && hashAlly > 0 && targetAlliedRealm > 0) || hashRealm == targetAlliedRealm || hashAlly == targetRealm)
        {
            //Debug.Log("(hashRealm == targetRealm || hashAlly == targetAlliedRealm || hashRealm == targetAlliedRealm || hashAlly == targetRealm");
            return false;
        }

        return true;
    }

    // -----------------------------------------------------------------------------------
    // GetAlliedRealms
    // -----------------------------------------------------------------------------------
    public bool GetAlliedRealms(Tmpl_Realm requiredRealm, Tmpl_Realm requiredAlly)
    {
        if ((requiredRealm == null && requiredAlly == null) || (Realm == null && Ally == null))
            return true;

        if (requiredRealm == Realm || requiredAlly == Ally || requiredRealm == Ally || requiredAlly == Realm)
            return true;

        return false;
    }

    // -------------------------------------------------------------------------------
    // GetAlliedRealms
    // You can attack other realm/neutral, but not own realm or allied realm
    // -------------------------------------------------------------------------------
    public bool GetAlliedRealms(int targetRealm, int targetAlliedRealm)
    {

        if ((Realm == null && Ally == null) || (targetRealm == 0 && targetAlliedRealm == 0))
        {
            return false;
        }
        if (hashRealm == targetRealm || (hashAlly == targetAlliedRealm && hashAlly > 0 && targetAlliedRealm > 0) || hashRealm == targetAlliedRealm || hashAlly == targetRealm)
        {
            return true;
        }
        return false;
    }

    // -----------------------------------------------------------------------------------
    // PvpCheckBuildSystem
    // -----------------------------------------------------------------------------------
#if _iMMOBUILDSYSTEM
    public bool PvpCheckBuildSystem(Entity target)
    {
        PlaceableObject po = GetComponentInParent<PlaceableObject>();

        if (po == null) return false;

        if (!(target is Player) || (!dontAttackOwner && !dontAttackOwnerParty && !dontAttackOwnerGuild)) return true;

        Player player = (Player)target;

        return
                (!dontAttackOwner || (dontAttackOwner && player.name == po.ownerCharacter)) &&
                (!dontAttackOwnerParty || (dontAttackOwnerParty && player.party.InParty() && player.party.party.Contains(po.ownerCharacter))) &&
                (!dontAttackOwnerGuild || (dontAttackOwnerGuild && player.guild.guild.name == po.ownerGuild));
    }
#endif
    // -----------------------------------------------------------------------------------
}
