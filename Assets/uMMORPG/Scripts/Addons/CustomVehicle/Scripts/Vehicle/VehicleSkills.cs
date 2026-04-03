
//(c8

using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleSkills : Skills
{
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
