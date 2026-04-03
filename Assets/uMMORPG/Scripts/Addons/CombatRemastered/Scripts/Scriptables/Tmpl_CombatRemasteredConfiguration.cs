using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Combat remastered configuration", menuName = "ADDON/Templates/New Combat remastered", order = 999)]
public class Tmpl_CombatRemasteredConfiguration : ScriptableObject
{
    [Header("[-=-[ Slot Weapon name ]-=-]")]
    public ArmorPart mainHand = ArmorPart.MainHand;
    public ArmorPart OffHand = ArmorPart.OffHand;
    public ArmorPart twoHand = ArmorPart.TwoHand;
    public ArmorPart ranged = ArmorPart.Ranged;

    [Header("[-=-[ (idea) Authorized Armor ]-=-]")]
    public ArmorType[] armorTypes;

    [Header("[-=-[ (idea) Weapon Skill MainHand ]-=-]")]
    public WeaponSkill[] weaponMainHand;

    [Header("[-=-[ (idea) Weapon Skill OffHand ]-=-]")]
    public WeaponSkill[] weaponOffHand;

    [Header("[-=-[ (idea) Weapon Skill TwoHand ]-=-]")]
    public WeaponSkill[] weaponTwoHand;

    [Header("[-=-[ (idea) Weapon Skill Ranged ]-=-]")]
    public WeaponSkill[] weaponRanged;
    
    [Header("[-=-[ (idea) Default Animation ]-=-]")]
    public DefaultSkills[] defaultSkill;
}

[Serializable]
public class DefaultSkills
{
    public DefaultSkillType skillType;
    public ScriptableSkill skillDefault;
}