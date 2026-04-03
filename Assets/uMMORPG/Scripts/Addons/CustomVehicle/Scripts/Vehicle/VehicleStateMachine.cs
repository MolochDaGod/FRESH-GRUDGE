
//(c8

using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleStateMachine : NetworkBehaviour
{
    public Transform aimTransform;
    [HideInInspector] public Player vehiclePlayer;
    public Entity vehicleEntity;

    public Skills skills;

    public KeyCode cancelActionKey = KeyCode.Escape;
    [Range(0.1f, 1)] public float attackToMoveRangeRatio = 0.8f;
    // helper variable to remember which skill to use when we walked close enough
    [HideInInspector] public int useSkillWhenCloser = -1;
    [HideInInspector] public int pendingSkill = -1;

    // finite state machine events - status based //////////////////////////////
    // status based events
    bool EventTargetDisappeared() =>
        vehiclePlayer.target == null;

    bool EventTargetDied() =>
        vehiclePlayer.target != null && vehiclePlayer.target.health.current == 0;

    bool EventSkillRequest() =>
        0 <= skills.currentSkill && skills.currentSkill < skills.skills.Count;

    bool EventSkillFinished() =>
        0 <= skills.currentSkill && skills.currentSkill < skills.skills.Count &&
        skills.skills[skills.currentSkill].CastTimeRemaining() == 0;

    // finite state machine events - command based /////////////////////////////
    // client calls command, command sets a flag, event reads and resets it
    // => we use a set so that we don't get ultra long queues etc.
    // => we use set.Return to read and clear values
    HashSet<string> cmdEvents = new HashSet<string>();

    [Command]
    public void CmdCancelAction() { cmdEvents.Add("CancelAction"); }
    bool EventCancelAction() { return cmdEvents.Remove("CancelAction"); }

    // finite state machine - server ///////////////////////////////////////////
    [Server]
    string UpdateServer_IDLE()
    {
        if (vehiclePlayer.target != null && vehicleEntity.CanAttack(vehiclePlayer.target))
            aimTransform.LookAt(vehiclePlayer.target.transform.position);

        if (EventCancelAction())
        {
            // the only thing that we can cancel is the target
            vehiclePlayer.target = null;
            vehicleEntity.target = null;
            return "IDLE";
        }
        if (EventSkillRequest())
        {
            // user wants to cast a skill.
            // check self (alive, mana, weapon etc.) and target and distance
            Skill skill = skills.skills[skills.currentSkill];
            vehiclePlayer.nextTarget = vehiclePlayer.target; // return to this one after any corrections by CastCheckTarget
            if (skills.CastCheckSelf(skill) &&
                skills.CastCheckTarget(skill) &&
                skills.CastCheckDistance(skill, out Vector3 destination))
            {
                skills.StartCast(skill);
                return "CASTING";
            }
            else
            {
                // checks failed. reset the attempted current skill.
                skills.currentSkill = -1;
                vehiclePlayer.nextTarget = null; // nevermind, clear again (otherwise it's shown in UITarget)
                return "IDLE";
            }
        }
        if (EventSkillFinished()) { } // don't care
        if (EventTargetDied()) { } // don't care
        if (EventTargetDisappeared()) { } // don't care

        return "IDLE"; // nothing interesting happened
    }

    void UseNextTargetIfAny()
    {
        // use next target if the user tried to target another while casting
        // (target is locked while casting so skill isn't applied to an invalid
        //  target accidentally)
        if (vehiclePlayer.nextTarget != null)
        {
            vehiclePlayer.target = vehiclePlayer.nextTarget;
            vehicleEntity.target = vehiclePlayer.nextTarget;
            vehiclePlayer.nextTarget = null;
        }
    }

    [Server]
    string UpdateServer_CASTING()
    {
        if (vehiclePlayer.target != null && vehicleEntity.CanAttack(vehiclePlayer.target))
            aimTransform.LookAt(vehiclePlayer.target.transform.position);

        // events sorted by priority (e.g. target doesn't matter if we died)
        //
        // IMPORTANT: nextTarget might have been set while casting, so make sure
        // to handle it in any case here. it should definitely be null again
        // after casting was finished.
        // => this way we can reliably display nextTarget on the client if it's
        //    != null, so that UITarget always shows nextTarget>target
        //    (this just feels better)
        if (EventCancelAction())
        {
            // cancel casting
            skills.CancelCast();
            UseNextTargetIfAny(); // if user selected a new target while casting
            return "IDLE";
        }
        if (EventTargetDisappeared())
        {
            // cancel if the target matters for this skill
            if (skills.skills[skills.currentSkill].cancelCastIfTargetDied)
            {
                skills.CancelCast();
                UseNextTargetIfAny(); // if user selected a new target while casting
                return "IDLE";
            }
        }
        if (EventTargetDied())
        {
            // cancel if the target matters for this skill
            if (skills.skills[skills.currentSkill].cancelCastIfTargetDied)
            {
                skills.CancelCast();
                UseNextTargetIfAny(); // if user selected a new target while casting
                return "IDLE";
            }
        }
        if (EventSkillFinished())
        {
            // apply the skill after casting is finished
            // note: we don't check the distance again. it's more fun if players
            //       still cast the skill if the target ran a few steps away
            Skill skill = skills.skills[skills.currentSkill];

            // apply the skill on the target
            skills.FinishCast(skill);

            // clear current skill for now
            skills.currentSkill = -1;

            // use next target if the user tried to target another while casting
            UseNextTargetIfAny();

            // go back to IDLE
            return "IDLE";
        }
        if (EventSkillRequest()) { } // don't care

        return "CASTING"; // nothing interesting happened
    }

    [Server]
    string UpdateServer_DEAD()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventSkillFinished()) { } // don't care
        if (EventCancelAction()) { } // don't care
        if (EventTargetDisappeared()) { } // don't care
        if (EventTargetDied()) { } // don't care
        if (EventSkillRequest()) { } // don't care

        return "DEAD"; // nothing interesting happened
    }

    // finite state machine - client ///////////////////////////////////////////
    [Client]
    public void UpdateClient()
    {
        if (vehiclePlayer == null)
        {
            return;
        }

        if (vehicleEntity.state == "IDLE")
        {
            if (vehiclePlayer.target != null && vehicleEntity.CanAttack(vehiclePlayer.target))
                aimTransform.LookAt(vehiclePlayer.target.transform.position);

            if (Player.localPlayer != null && isOwned)
            {
                // cancel action if escape key was pressed
                if (Input.GetKeyDown(cancelActionKey))
                {
                    CmdCancelAction();
                }

                // trying to cast a skill on a monster that wasn't in range?
                // then check if we walked into attack range by now
                if (useSkillWhenCloser != -1)
                {
                    // can we still attack the target? maybe it was switched.
                    if (Player.localPlayer.CanAttack(Player.localPlayer.target))
                    {
                        // in range already?
                        // -> we don't use CastCheckDistance because we want to
                        // move a bit closer (attackToMoveRangeRatio)
                        float range = skills.skills[useSkillWhenCloser].castRange * attackToMoveRangeRatio;
                        if (Utils.ClosestDistance(Player.localPlayer, Player.localPlayer.target) <= range)
                        {
                            // then stop moving and start attacking
                            ((VehicleSkills)skills).CmdUse(useSkillWhenCloser);

                            // reset
                            useSkillWhenCloser = -1;
                        }
                    }
                    // otherwise reset
                    else useSkillWhenCloser = -1;
                }
            }
        }
        else if (vehicleEntity.state == "CASTING")
        {
            if (vehiclePlayer.target != null && vehicleEntity.CanAttack(vehiclePlayer.target))
                aimTransform.LookAt(vehiclePlayer.target.transform.position);

            if (Player.localPlayer != null && isOwned)
            {
                // cancel action if escape key was pressed
                if (Input.GetKeyDown(cancelActionKey)) CmdCancelAction();
            }
        }
        else if (vehicleEntity.state == "DEAD") { }
        else Debug.LogError("invalid state:" + vehicleEntity.state);
    }

    [Server]
    public string UpdateServer()
    {
        if (vehiclePlayer != null)
        {
            // Use the player target, for using the skill on the vehicleEntity
            vehicleEntity.target = vehiclePlayer.target;

            if (vehicleEntity.state == "IDLE") return UpdateServer_IDLE();
            if (vehicleEntity.state == "CASTING") return UpdateServer_CASTING();
            if (vehicleEntity.state == "DEAD") return UpdateServer_DEAD();
            Debug.LogError("invalid state:" + vehicleEntity.state);
        }

        return "IDLE";
    }

    // client event when skill cast finished on server
    // -> useful for follow up attacks etc.
    //    (doing those on server won't really work because the target might have
    //     moved, in which case we need to follow, which we need to do on the
    //     client)
    [Client]
    public void OnSkillCastFinished(Skill skill)
    {
        if (!isLocalPlayer) return;

        // user pressed another skill button?
        if (pendingSkill != -1)
        {
            ((VehicleSkills)skills).TryUse(pendingSkill, true);
        }
        // otherwise do follow up attack if no interruptions happened
        else if (skill.followupDefaultAttack)
        {
            ((VehicleSkills)skills).TryUse(0, true);
        }

        // clear pending actions in any case
        pendingSkill = -1;
    }
}
