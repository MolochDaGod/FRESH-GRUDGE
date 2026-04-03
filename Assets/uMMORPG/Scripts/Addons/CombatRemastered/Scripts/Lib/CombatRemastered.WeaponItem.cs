using UnityEngine;

public partial class WeaponItem
{
    [Header("[-=-[ Weapon Position ]-=-]")]
    public WeaponPosition WeaponPosition = WeaponPosition.MainHand;
    public WeaponPosition WeaponSecondPosition = WeaponPosition.None;

    [Header("[-=-[ Weapon Type ]-=-]")]
    public WeaponType weaponType = WeaponType.Pierce1Hand;

    [Header("[-=-[ Default Skill ]-=-]")]
    public ScriptableSkill autoAttackSkill;
}
