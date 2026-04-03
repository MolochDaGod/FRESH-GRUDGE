using UnityEngine;

public partial class Monster
{
    [Header("[Component : Monster PVP Zone")]
    public MonsterPVPZone monsterPVPZone;

    // -------------------------------------------------------------------------------
    // GetAttackAllowance
    // -------------------------------------------------------------------------------
    public override bool GetAttackAllowance(Entity target)
    {


#if _iMMOBUILDSYSTEM
        // ---------- Build System: Attack non-owner groups
        if (PvpCheckBuildSystem(target))
            return true;
#endif

        // ---------- Check Realm PVE
        // You can attack other realm/neutral, but not own realm or allied realm
        return GetHostileRealms(target);
    }

    // -----------------------------------------------------------------------------------
}