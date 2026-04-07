#if _iMMOTOOLS_LEGACY_CORE

public partial class Monster
{
    // ================================= MISC FUNCS ======================================
    // A bunch of very common utility functions that are missing on the core asset
    // ===================================================================================

    // -----------------------------------------------------------------------------------
    // Tools_CanAttack
    // Replaces the built-in CanAttack check. This one can be expanded, the built-in one not.
    // -----------------------------------------------------------------------------------
    public override bool Tools_CanAttack(Entity entity)
    {
        //Debug.Log("Tools_CanAttack : " + base.Tools_CanAttack(entity) + " | GetAttackAllowance" + GetAttackAllowance(entity));
        return base.Tools_CanAttack(entity)
#if _iMMOPVP
            && GetAttackAllowance(entity)
#endif
            ;
    }

    // -----------------------------------------------------------------------------------
}
#endif