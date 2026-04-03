/// Partial ScriptableSkill extension — adds class tree node requirements.
///
/// Usage: On any skill ScriptableObject, assign nodes in "requiredClassTreeNodes".
/// The skill will only appear in the UI (and be upgradeable) if the player
/// has chosen at least one of the listed nodes.
/// Leave the array empty for unrestricted access.

using UnityEngine;

public abstract partial class ScriptableSkill
{
#if _IMMOCLASSTREE
    [Header("[-=-[ Class Leveling Tree ]-=-]")]
    [Tooltip("Player must have chosen at least one of these tree nodes to see/learn this skill. " +
             "Leave empty = no class tree restriction.")]
    public ClassTreeNodeTemplate[] requiredClassTreeNodes;
#endif
}
