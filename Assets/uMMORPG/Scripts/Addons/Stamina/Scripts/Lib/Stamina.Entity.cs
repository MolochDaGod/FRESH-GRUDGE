#if _iMMOSTAMINA
using UnityEngine;

// =======================================================================================
// STAMINA
// =======================================================================================
[RequireComponent(typeof(Stamina))]
public partial class Entity
{
    public Stamina stamina;

    /*[Header("STAMINA")]
    public bool _staminaRecovery = true;
    [SerializeField] protected LinearInt _staminaRecoveryRate = new LinearInt { baseValue = -1 };
    [SerializeField] protected LinearInt _staminaMax = new LinearInt { baseValue = 100 };
    [SyncVar] protected int _stamina = 1;

    // -----------------------------------------------------------------------------------
    // StaminaPercent
    // -----------------------------------------------------------------------------------
    public float StaminaPercent()
    {
        return (Stamina != 0 && StaminaMax != 0) ? (float)Stamina / (float)StaminaMax : 0;
    }

    // -----------------------------------------------------------------------------------
    // StaminaMax
    // -----------------------------------------------------------------------------------
    public virtual int StaminaMax
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int passiveBonus = 0;
            foreach (Skill skill in skills.skills)
                if (skill.level > 0 && skill.data is PassiveSkill)
                    passiveBonus += ((PassiveSkill)skill.data).bonusStaminaMax.Get(skill.level);

            int buffBonus = 0;
            for (int i = 0; i < skills.buffs.Count; ++i)
                buffBonus += skills.buffs[i].bonusStaminaMax;

            // base + passives + buffs
            return _staminaMax.Get(level.current) + passiveBonus + buffBonus;
        }
    }

    // -----------------------------------------------------------------------------------
    // Stamina
    // -----------------------------------------------------------------------------------
    public virtual int Stamina
    {
        get { return Mathf.Min(_stamina, StaminaMax); } // min in case hp>hpmax after buff ends etc.
        set { _stamina = Mathf.Clamp(value, 0, StaminaMax); }
    }

    // -----------------------------------------------------------------------------------
    // staminaRecovery
    // -----------------------------------------------------------------------------------
    public virtual bool staminaRecovery
    {
        get
        {
            return StaminaRecoveryRate < 0 || (_staminaRecovery && !skills.buffs.Any(x => x.blockStaminaRecovery));
        }
    }

    // -----------------------------------------------------------------------------------
    // StaminaRecoveryRate
    // -----------------------------------------------------------------------------------
    public int StaminaRecoveryRate
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            float passivePercent = 0;
            foreach (Skill skill in skills.skills)
                if (skill.level > 0 && skill.data is PassiveSkill)
                    passivePercent += ((PassiveSkill)skill.data).bonusStaminaPercentPerSecond.Get(skill.level);

            float buffPercent = 0;
            foreach (Buff buff in skills.buffs)
                buffPercent += buff.bonusStaminaPercentPerSecond;

            // base + passives + buffs
            return _staminaRecoveryRate.Get(level.current) + Convert.ToInt32(passivePercent * StaminaMax) + Convert.ToInt32(buffPercent * StaminaMax);
        }
    }*/

    // -----------------------------------------------------------------------------------

}
#endif
