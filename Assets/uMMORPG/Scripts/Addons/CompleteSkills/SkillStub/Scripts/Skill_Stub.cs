using UnityEngine;

// SKILL STUB
[CreateAssetMenu(menuName = "ADDON/Skills/Self/Stub", order = 999)]
public class Skill_Stub : ScriptableSkill
{
    public override bool CheckTarget(Entity entity)
    {
        return true;
    }

#if _iMMO2D
    public override bool CheckDistance(Entity caster, int skillLevel, out Vector2 destination)
#else
    public override bool CheckDistance(Entity caster, int skillLevel, out Vector3 destination)
#endif
    {
        destination = Vector3.zero;
        return true;
    }

#if _iMMO2D
    public override void Apply(Entity caster, int skillLevel, Vector2 target)
#else
    public override void Apply(Entity caster, int skillLevel)
#endif
    {
#if _SERVER
#endif
    }
}
