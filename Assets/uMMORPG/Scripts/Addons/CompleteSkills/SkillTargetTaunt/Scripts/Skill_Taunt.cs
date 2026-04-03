using UnityEngine;  

[CreateAssetMenu(menuName = "ADDON/Skills/Target/Taunt", order = 999)]  
public class Skill_Taunt : TargetBuffSkill  
{  
    [Header("[-=-[ Taunt Skill ]-=-]")]  
    public LinearFloat successChance;  

    public string tauntMessage = "You taunted: ";  
    public string failedMessage = "You failed to taunt: ";  

    // Override CheckTarget to ensure we can only taunt attackable targets  
    public override bool CheckTarget(Entity caster)  
    {  
        // First, use the base CheckTarget to correct the target  
        if (!base.CheckTarget(caster))  
            return false;  

        // Then, check if the target is attackable  
        return caster.target != null && caster.CanAttack(caster.target);  
    }  

    // We don't need to override CheckDistance as the base implementation is sufficient  

    // Override Apply to implement the taunt logic  
    public override void Apply(Entity caster, int skillLevel)  
    {  
#if _SERVER  
        if (caster is Player player && player.target is Monster monster)  
        {  
            if (Random.value <= successChance.Get(skillLevel))  
            {  
                // Apply the buff (taunt effect)  
                base.Apply(caster, skillLevel);  

                // Additional taunt logic  
                monster.Tools_OnAggro(player, 1);  
                player.Tools_TargetAddMessage(tauntMessage + monster.name);  
            }  
            else  
            {  
                player.Tools_TargetAddMessage(failedMessage + monster.name);  
            }  
        }  
#endif  
    }  
}