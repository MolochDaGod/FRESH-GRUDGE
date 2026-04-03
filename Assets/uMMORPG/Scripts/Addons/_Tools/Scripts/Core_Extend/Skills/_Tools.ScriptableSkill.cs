using UnityEngine;

public abstract partial class ScriptableSkill
{
#if _iMMOSTAMINA
    [Tooltip("stamina Special Rule: Can be cast with insufficient stamina as well!")]
    public LinearInt staminaCosts;
#endif

    [Tooltip("This skill cannot be learned via the Skill Window, only via other means")]
    public bool unlearnable;

    [Tooltip("Checked = negative skill, Unchecked = positive skill. Certain skills can debuff disadvantageous skills only")]
    public bool disadvantageous;

}
