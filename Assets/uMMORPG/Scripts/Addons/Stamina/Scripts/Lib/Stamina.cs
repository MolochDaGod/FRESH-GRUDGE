using UnityEngine;

// inventory, attributes etc. can influence max Stamina
public interface IStaminaBonus
{
    int GetStaminaBonus(int baseStamina);
    int GetStaminaRecoveryBonus();
}

[RequireComponent(typeof(Level))]
[DisallowMultipleComponent]
public class Stamina : Energy
{
    public Level level;
    public LinearInt baseStamina = new LinearInt { baseValue = 100 };
    public int baseRecoveryRate = 1;

    // cache components that give a bonus (attributes, inventory, etc.)
    // (assigned when needed. NOT in Awake because then prefab.max doesn't work)
    IStaminaBonus[] _bonusComponents;
    IStaminaBonus[] bonusComponents =>
        _bonusComponents ?? (_bonusComponents = GetComponents<IStaminaBonus>());

    // calculate max
    public override int max
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int bonus = 0;
            int baseThisLevel = baseStamina.Get(level.current);
            foreach (IStaminaBonus bonusComponent in bonusComponents)
                bonus += bonusComponent.GetStaminaBonus(baseThisLevel);
            return baseThisLevel + bonus;
        }
    }

    public override int recoveryRate
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int bonus = 0;
            foreach (IStaminaBonus bonusComponent in bonusComponents)
                bonus += bonusComponent.GetStaminaRecoveryBonus();
            return baseRecoveryRate + bonus;
        }
    }
}