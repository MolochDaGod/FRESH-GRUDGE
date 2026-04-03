using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountSkills : Skills
{
    
    // the last skill that was casted, to decide which one to cast next
    [HideInInspector] public int lastSkill = -1;
    public override void OnStartServer()
    {
        // load skills based on skill templates
        foreach (ScriptableSkill skillData in skillTemplates)
            skills.Add(new Skill(skillData));
    }

    // helper function to get the current cast range (if casting anything)
    public float CurrentCastRange()
    {
        return 0 <= currentSkill && currentSkill < skills.Count
               ? skills[currentSkill].castRange
               : 0;
    }

    [Command]
    public void CmdUse(int skillIndex)
    {
        // validate
        if ((entity.state == "IDLE" || entity.state == "MOVING" || entity.state == "CASTING") &&
            0 <= skillIndex && skillIndex < skills.Count)
        {
            // skill learned and can be casted?
            if (skills[skillIndex].level > 0 && skills[skillIndex].IsReady())
            {
                currentSkill = skillIndex;
            }
        }
    }

    public int NextSkill()
    {
        // find the next ready skill, starting at 'lastSkill+1' (= next one)
        // and looping at max once through them all (up to skill.Count)
        //  note: no skills.count == 0 check needed, this works with empty lists
        //  note: also works if lastSkill is still -1 from initialization
        Debug.Log("countSkill = " + skills.Count + " countBuffs = " + buffs.Count);
        for (int i = 0; i < skills.Count; ++i)
        {
            int index = (lastSkill + 1 + i) % skills.Count;

            if (CastCheckSelf(skills[index]) && skills[index].CheckSkillConditions(entity))
            {
                return index;
            }
        }
        return -1;
    }

    // helper function: try to use a skill and walk into range if necessary
    [Client]
    public void TryUse(int skillIndex, bool ignoreState = false)
    {
        // only if not casting already
        // (might need to ignore that when coming from pending skill where
        //  CASTING is still true)
        if (entity.state != "CASTING" || ignoreState)
        {
            Skill skill = skills[skillIndex];
            if (CastCheckSelf(skill) && CastCheckTarget(skill))
            {
                // check distance between self and target
                Vector3 destination;
                if (CastCheckDistance(skill, out destination))
                {
                    // cast
                    CmdUse(skillIndex);
                }
                else
                {
                    // move to the target first
                    // (use collider point(s) to also work with big entities)
                    //float stoppingDistance = skill.castRange * ((player)entity).attackToMoveRangeRatio;
                    //movement.Navigate(destination, stoppingDistance);

                    // use skill when there
                    entity.GetComponent<PassengerStateMachine>().useSkillWhenCloser = skillIndex;
                }
            }
        }
        else
        {
            entity.GetComponent<PassengerStateMachine>().pendingSkill = skillIndex;
        }
    }
}