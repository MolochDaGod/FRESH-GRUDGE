using UnityEngine;

// CONDITIONAL SKILL - CLASS
[System.Serializable]
public partial class ConditionalSkill
{
    [Header("Random")]
    [Tooltip("Basic, random activation chance of this skill (0.1 = 10%)")]
    [Range(0, 1)] public float activationChance = 1f;

    [Header("Health")]
    [Tooltip("Health of the caster must be 'below' or 'above' the threshold")]
    public Entity.ParentThreshold healthThreshold;

    [Tooltip("Health treshold of the caster in order to trigger condition")]
    [Range(0, 1)] public float casterHealth;

#if _iMMOMORALE
    [Header("Morale")]
    [Tooltip("Morale of the caster must be 'below' or 'above' the threshold")]
    public Monster.ParentThreshold moraleThreshold;

    [Tooltip("Morale treshold of the caster in order to trigger condition")]
    [Range(0, 1)] public float casterMorale;
#endif

    [Header("Require active Buff")]
    [Tooltip("Caster must have this active Buff in order to trigger condition")]
    public BuffSkill activeBuff;
}
