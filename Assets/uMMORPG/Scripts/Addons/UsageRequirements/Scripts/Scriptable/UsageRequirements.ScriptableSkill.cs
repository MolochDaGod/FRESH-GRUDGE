using UnityEngine;

public abstract partial class ScriptableSkill
{
    [Header("[-=-[ USAGE REQUIREMENTS ]-=-]")]
    public Tools_Requirements usageRequirements;

    [Tooltip("Can only be used while a item of the same ID is equipped (0 = disabled)")]
    public int usageEquipmentId;

    [Tooltip("Can only be used while the player is inside a usage area of the same ID (0 = disabled)")]
    public int usageAreaId;

#if _iMMOPRESTIGECLASSES

    [Header("[-=-[ Prestige Classes ]-=-]")]
    [Tooltip("Can only be learned/upgraded by one of those prestige classes")]
    public PrestigeClassTemplate[] learnablePrestigeClasses;
    public bool emptyPrestigeClass = true;

#endif

    // -----------------------------------------------------------------------------------
    // Skill_CanCast
    // -----------------------------------------------------------------------------------
    public bool Skill_CanCast(Entity caster, int skillLevel)
    {
        if (caster is Player player)
        {
            return (usageAreaId <= 0 || player.usageRequirementsAreaId == usageAreaId) &&
                    (usageEquipmentId <= 0 || player.UsageRequirementsGetEquipmentId(usageEquipmentId)) &&
                    usageRequirements.checkRequirements(player);
        }
        return true;
    }
    // -----------------------------------------------------------------------------------
}
