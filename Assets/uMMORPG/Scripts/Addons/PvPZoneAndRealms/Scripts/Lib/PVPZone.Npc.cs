using UnityEngine;

// MONSTER

public partial class Npc
{

    [Header("[Component : Npc PVP Zone")]
    public NpcPVPZone npcPVPZone;


    // -------------------------------------------------------------------------------
    // GetAttackAllowance
    // -------------------------------------------------------------------------------
    public override bool GetAttackAllowance(Entity target)
    {
        if (npcPVPZone.aggroBehaviour == AggroBehaviour.Neutral || npcPVPZone.aggroBehaviour == AggroBehaviour.Pacifistic)
        {
            return false;
        }
        else
        {
#if _iMMOBUILDSYSTEM
            // ---------- Build System: Attack non-owner groups
            if (PvpCheckBuildSystem(target))
                return true;
#endif

            // ---------- Check Realm PVE
            // You can attack other realm/neutral, but not own realm or allied realm
            //Debug.Log("Npc > GetHostileRealms result : " + GetHostileRealms(target));
            return GetHostileRealms(target);
        }
    }

    // -----------------------------------------------------------------------------------
}