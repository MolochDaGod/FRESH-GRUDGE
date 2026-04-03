/// Partial extension to PlayerSkills — adds class tree node check to CanUpgrade.
///
/// With _IMMOCLASSTREE active: if a ScriptableSkill has requiredClassTreeNodes assigned,
/// the player must have chosen at least one of those nodes before they can upgrade/learn it.

public partial class PlayerSkills
{
#if _IMMOCLASSTREE
    // Called by CanUpgrade — returns false if class tree blocks this skill
    public bool ClassTree_CanUpgradeSkill(Skill skill)
    {
        Player p = entity as Player;
        if (p == null || p.classTree == null) return true;

        var required = skill.data.requiredClassTreeNodes;
        if (required == null || required.Length == 0) return true;

        foreach (var node in required)
            if (p.classTree.HasChosenNode(node)) return true;

        return false;
    }
#endif
}
