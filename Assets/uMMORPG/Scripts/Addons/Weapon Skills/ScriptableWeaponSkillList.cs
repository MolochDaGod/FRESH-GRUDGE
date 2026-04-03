using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "uMMORPG Skill/WeaponSkillList", order = 999)]
public class ScriptableWeaponSkillList : ScriptableObject
{
    public ScriptableSkill[] weaponSkills;
}
