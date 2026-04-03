
//(c8

using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VehicleSkillBar : NetworkBehaviour
{
    [Header("Components")]
    public VehicleSkills skills;

    [Header("Skillbar")]
    public SkillbarEntry[] slots =
    {
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha1},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha2},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha3},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha4},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha5},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha6},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha7},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha8},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha9},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha0},
    };

    public override void OnStartClient()
    {
        // load skillbar after player data was loaded
        Load();
    }

    // skillbar ////////////////////////////////////////////////////////////////
    [Client]
    public void Load()
    {
        List<Skill> learned = skills.skills.Where(skill => skill.level > 0).ToList();
        for (int i = 0; i < slots.Length; ++i)
        {
            if (i < learned.Count)
            {
                slots[i].reference = learned[i].name;
            }
        }
    }
}
