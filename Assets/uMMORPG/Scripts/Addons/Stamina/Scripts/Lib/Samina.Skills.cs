#if _iMMOSTAMINA
using System;
using UnityEngine;

public abstract partial class Skills
{
    [Header("[-=-=-[ Stamina ]-=-=-]")]
    public Stamina stamina;

    public int GetStaminaBonus(int baseHealth)
    {
        // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
        int passiveBonus = 0;
        foreach (Skill skill in skills)
            if (skill.level > 0 && skill.data is PassiveSkill passiveSkill)
                passiveBonus += passiveSkill.staminaMaxBonus.Get(skill.level);

        int buffBonus = 0;
        foreach (Buff buff in buffs)
            buffBonus += buff.staminaMaxBonus;

        return passiveBonus + buffBonus;
    }

    public int GetStaminaRecoveryBonus()
    {
        // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
        float passivePercent = 0;
        foreach (Skill skill in skills)
            if (skill.level > 0 && skill.data is PassiveSkill passiveSkill)
                passivePercent += passiveSkill.staminaPercentPerSecondBonus.Get(skill.level);

        float buffPercent = 0;
        foreach (Buff buff in buffs)
            buffPercent += buff.staminaPercentPerSecondBonus;

        return Convert.ToInt32(passivePercent * stamina.max) + Convert.ToInt32(buffPercent * stamina.max);
    }
}
#endif
