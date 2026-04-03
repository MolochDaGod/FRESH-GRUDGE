using UnityEngine;
using Mirror;

public partial class Mount : Summonable
{
    [Range(0.1f, 1)] public float attackToMoveRangeRatio = 0.8f; // move as close as 0.8 * attackRange to a target

    [Header("Seat Position")]
    public Transform seat;

    [Header("Death")]
    public float deathTime = 2; // enough for animation
    [HideInInspector] public double deathTimeEnd; // double for long term precision

    [Header("Glide Mount")]
    public bool glideMount = false;
    public float gravityMultiplier = 2;
    public float jumpSpeed = 7;

    [Header("World Mount")]
    public WorldMount worldMount;
    public int mountIndex;


    [Header("Offsets - Standing")]
    public Vector2 thirdPersonOffset = Vector2.up;
    public Vector2 thirdPersonOffsetMultiplier = Vector2.zero;

    // networkbehaviour ////////////////////////////////////////////////////////
    void LateUpdate()
    {
        // pass parameters to animation state machine
        // => passing the states directly is the most reliable way to avoid all
        //    kinds of glitches like movement sliding, attack twitching, etc.
        // => make sure to import all looping animations like idle/run/attack
        //    with 'loop time' enabled, otherwise the client might only play it
        //    once
        // => only play moving animation while the owner is actually moving. the
        //    MOVING state might be delayed to due latency or we might be in
        //    MOVING while a path is still pending, etc.
        if (isClient) // no need for animations on the server
        {
            // use owner's moving state for maximum precision (not if dead)
            // (if owner spawn reached us yet)
            // bMMORPGAddon : CustomVehicle
            animator.SetBool("MOVING", health.current > 0 && owner != null && owner.movement.IsMoving() && owner.playerVehicle.IsPlayerMountedMoving(transform));
            animator.SetBool("CASTING", state == "CASTING");
            animator.SetBool("STUNNED", state == "STUNNED");
            animator.SetBool("DEAD", state == "DEAD");

            if (state == "CASTING")
            {
                foreach (Skill skill in skills.skills)
                {
                    animator.SetBool(skill.name, skill.CastTimeRemaining() > 0);
                }

            }

            if ((owner.movement as PlayerCharacterControllerMovement).state == MoveState.MOUNTED_AIRBORNE)
            {
                animator.SetBool("Jump", true);
            }
            else
                animator.SetBool("Jump", false);
        }
    }

    void OnDestroy()
    {
        // keep player's pet item up to date
        if (isServer)
            SyncToOwnerItem();
    }

    // always update pets. IsWorthUpdating otherwise only updates if has observers,
    // but pets should still be updated even if they are too far from any observers,
    // so that they continue to run to their owner.
    public override bool IsWorthUpdating() => true;

    // finite state machine events /////////////////////////////////////////////
    bool EventOwnerDisappeared()=>
        owner == null;

    bool EventTargetDisappeared() =>
    target == null;

    bool EventTargetDied() =>
        target != null && target.health.current == 0;

    bool EventTargetTooFarToAttack() =>
        target != null &&
        0 <= skills.currentSkill && skills.currentSkill < skills.skills.Count &&
#if _iMMO2D
        !skills.CastCheckDistance(skills.skills[skills.currentSkill], out Vector2 destination);
#else
        !skills.CastCheckDistance(skills.skills[skills.currentSkill], out Vector3 destination);
#endif

    bool EventOwnerDied() =>
        owner != null && owner.health.current == 0;

    bool EventDied() =>
        health.current == 0;

    bool EventDeathTimeElapsed() =>
        state == "DEAD" && NetworkTime.time >= deathTimeEnd;

    bool EventAggro() =>
    target != null && target.health.current > 0;

    bool EventSkillRequest() =>
        0 <= skills.currentSkill && skills.currentSkill < skills.skills.Count;

#if !_iMMOTIMEDSKILLFINISH
    bool EventSkillFinished() =>
        0 <= skills.currentSkill && skills.currentSkill < skills.skills.Count &&
               skills.skills[skills.currentSkill].CastTimeRemaining() == 0;
#endif

    bool EventMoveEnd() =>
        state == "MOVING" && !movement.IsMoving();

    bool EventStunned() =>
        NetworkTime.time <= stunTimeEnd;

    // finite state machine - server ///////////////////////////////////////////
    // copy owner's positionPlaceableObject and rotationPlaceableObject. no need for NetworkTransform.
    void CopyOwnerPositionAndRotation()
    {
        if (owner != null)
        {
            // mount doesn't need a movement system with a .Warp function.
            // we simply copy the owner positionPlaceableObject/rotationPlaceableObject at all times.
            transform.position = owner.transform.position;
            transform.rotation = owner.transform.rotation;
        }
    }

    [Server]
    string UpdateServer_MOVING()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventOwnerDisappeared())
        {
            // owner might disconnect or get destroyed for some reason
            NetworkServer.Destroy(gameObject);
            return "IDLE";
        }
        if (EventDied())
        {
            // we died.
            movement.Reset();
            return "DEAD";
        }
        if (EventStunned())
        {
            movement.Reset();
            return "STUNNED";
        }
        if (EventMoveEnd())
        {
            // we reached our destination.
            return "IDLE";
        }
        if (EventTargetDied())
        {
            // we had a target before, but it died now. clear it.
            target = null;
            skills.CancelCast();
            movement.Reset();
            return "IDLE";
        }
        if (EventTargetTooFarToAttack())
        {
            // we had a target before, but it's out of attack range now.
            // follow it. (use collider point(s) to also work with big entities)
            float stoppingDistance = ((MountSkills)skills).CurrentCastRange() * attackToMoveRangeRatio;
            Vector3 destination = Utils.ClosestPoint(target, transform.position);
            movement.Navigate(destination, stoppingDistance);
            return "MOVING";
        }
        if (EventAggro())
        {
            // target in attack range. try to cast a first skill on it
            // (we may get a target while randomly wandering around)
            if (skills.skills.Count > 0)
                skills.currentSkill = ((MountSkills)skills).NextSkill();
            else
                Debug.LogError(name + " has no skills to attack with.");
            movement.Reset();
            return "IDLE";
        }
        if (EventDeathTimeElapsed()) { } // don't care
        if (EventSkillFinished()) { } // don't care
        if (EventSkillRequest()) { } // don't care, finish movement first

        return "MOVING"; // nothing interesting happened
    }

    // finite state machine - server ///////////////////////////////////////////
    [Server]
    string UpdateServer_IDLE()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventOwnerDisappeared())
        {
            // owner might disconnect or get destroyed for some reason
            NetworkServer.Destroy(gameObject);
            return "IDLE";
        }
        if (EventDied())
        {
            // we died.
            return "DEAD";
        }
        if (EventStunned())
        {
            movement.Reset();
            return "STUNNED";
        }
        if (EventTargetDied())
        {
            // we had a target before, but it died now. clear it.
            target = null;
            skills.CancelCast();
            return "IDLE";
        }
        if (EventTargetTooFarToAttack())
        {
            // we had a target before, but it's out of attack range now.
            // follow it. (use collider point(s) to also work with big entities)
            float stoppingDistance = ((MountSkills)skills).CurrentCastRange() * attackToMoveRangeRatio;
            Vector3 destination = Utils.ClosestPoint(target, transform.position);
            movement.Navigate(destination, stoppingDistance);
            return "MOVING";
        }
        if (EventSkillRequest())
        {
            // we had a target in attack range before and trying to cast a skill
            // on it. check self (alive, mana, weapon etc.) and target
            Skill skill = skills.skills[skills.currentSkill];
            if (skills.CastCheckSelf(skill) && skills.CastCheckTarget(skill))
            {
                // start casting
                skills.StartCast(skill);
                return "CASTING";
            }
            else
            {
                // invalid target. reset attempted current skill cast.
                target = null;
                skills.currentSkill = -1;
                return "IDLE";
            }
        }
        if (EventAggro())
        {
            // target in attack range. try to cast a first skill on it
            if (skills.skills.Count > 0)
                skills.currentSkill = ((MountSkills)skills).NextSkill();
            else
                Debug.LogError(name + " has no skills to attack with.");
            return "IDLE";
        }
        if (EventMoveEnd()) { } // don't care
        if (EventDeathTimeElapsed()) { } // don't care
        if (EventSkillFinished()) { } // don't care
        if (EventTargetDisappeared()) { } // don't care

        return "IDLE"; // nothing interesting happened
    }

    [Server]
    string UpdateServer_CASTING()
    {
        // keep looking at the target for server & clients (only Y rotation)
#if !_iMMO2D
        if (target)
            movement.LookAtY(target.transform.position);
#endif

        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventOwnerDisappeared())
        {
            // owner might disconnect or get destroyed for some reason
            NetworkServer.Destroy(gameObject);
            return "IDLE";
        }
        if (EventDied())
        {
            // we died.
            return "DEAD";
        }
        if (EventStunned())
        {
            skills.CancelCast();
            movement.Reset();
            return "STUNNED";
        }
        if (EventTargetDisappeared())
        {
            // cancel if the target matters for this skill
            if (skills.skills[skills.currentSkill].cancelCastIfTargetDied)
            {
                skills.CancelCast();
                target = null;
                return "IDLE";
            }
        }
        if (EventTargetDied())
        {
            // cancel if the target matters for this skill
            if (skills.skills[skills.currentSkill].cancelCastIfTargetDied)
            {
                skills.CancelCast();
                target = null;
                return "IDLE";
            }
        }
        if (EventSkillFinished())
        {
            // finished casting. apply the skill on the target.
            skills.FinishCast(skills.skills[skills.currentSkill]);

            // did the target die? then clear it so that the monster doesn't
            // run towards it if the target respawned
            if (target.health.current == 0) target = null;

            // go back to IDLE. reset current skill.
            ((MountSkills)skills).lastSkill = skills.currentSkill;
            skills.currentSkill = -1;
            return "IDLE";
        }
        if (EventMoveEnd()) { } // don't care
        if (EventDeathTimeElapsed()) { } // don't care
        if (EventTargetTooFarToAttack()) { } // don't care, we were close enough when starting to cast
        if (EventAggro()) { } // don't care, always have aggro while casting
        if (EventSkillRequest()) { } // don't care, that's why we are here

        return "CASTING"; // nothing interesting happened
    }

    [Server]
    string UpdateServer_DEAD()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventOwnerDisappeared())
        {
            // owner might disconnect or get destroyed for some reason
            NetworkServer.Destroy(gameObject);
            return "DEAD";
        }
        if (EventDeathTimeElapsed())
        {
            // we were lying around dead for long enough now.
            // hide while respawning, or disappear forever
            NetworkServer.Destroy(gameObject);
            return "DEAD";
        }
        if (EventSkillRequest()) { } // don't care
        if (EventSkillFinished()) { } // don't care
        if (EventMoveEnd()) { } // don't care
        if (EventTargetDied()) { } // don't care
        if (EventTargetTooFarToAttack()) { } // don't care
        if (EventAggro()) { } // don't care
        if (EventOwnerDied()) {} // don't care
        if (EventDied()) {} // don't care, of course we are dead

        return "DEAD"; // nothing interesting happened
    }

    [Server]
    string UpdateServer_STUNNED()
    {
        if (EventDied())
        {
            // we died.
            skills.CancelCast(); // in case we died while trying to cast
            return "DEAD";
        }
        if (EventStunned())
        {
            return "STUNNED";
        }

        // go back to idle if we aren't stunned anymore and process all new
        // events there too
        return "IDLE";
    }

    [Server]
    protected override string UpdateServer()
    {
        if (state == "IDLE")    return UpdateServer_IDLE();
        if (state == "MOVING") return UpdateServer_MOVING();
        if (state == "CASTING") return UpdateServer_CASTING();
        if (state == "STUNNED") return UpdateServer_STUNNED();
        if (state == "DEAD")    return UpdateServer_DEAD();
        Debug.LogError("invalid state:" + state);
        return "IDLE";
    }

    // finite state machine - client ///////////////////////////////////////////
    [Client]
    protected override void UpdateClient()
    {
        if (state == "IDLE" || state == "MOVING")
        {
            // copy owner's positionPlaceableObject and rotationPlaceableObject. no need for NetworkTransform.
            CopyOwnerPositionAndRotation();
        }
    }

    // selection handling //////////////////////////////////////////////////////
    [Command]
    public void CmdSetTarget(NetworkIdentity ni)
    {
#if _SERVER
        // validate
        if (ni != null)
        {
            // can directly change it, or change it after casting?
            // bMMORPGAddon : CustomVehicle
            if (state == "IDLE" || state == "MOVING" || state == "STUNNED" || state == "DRIVING")
                target = ni.GetComponent<Entity>();
        }
#endif
    }

    // aggro ///////////////////////////////////////////////////////////////////
    // this function is called by entities that attack us
    [ServerCallback]
    public override void OnAggro(Entity entity)
    {
        if (attackTarget != null && attackTarget != entity) return;
        // call base function
        base.OnAggro(entity);

        // are we alive, and is the entity alive and of correct type?
        if (CanAttack(entity))
        {
            // no target yet(==self), or closer than current target?
            // => has to be at least 20% closer to be worth it, otherwise we
            //    may end up nervously switching between two targets
            // => we do NOT use Utils.ClosestDistance, because then we often
            //    also end up nervously switching between two animated targets,
            //    since their collides moves with the animation.
            if (target == null)
            {
                target = entity;
            }
            else
            {
#if _iMMO2D
                float oldDistance = Vector2.Distance(transform.position, target.transform.position);
                float newDistance = Vector2.Distance(transform.position, entity.transform.position);
#else
                float oldDistance = Vector3.Distance(transform.position, target.transform.position);
                float newDistance = Vector3.Distance(transform.position, entity.transform.position);
#endif
                if (newDistance < oldDistance * 0.8) target = entity;
            }
        }
    }

    // death ///////////////////////////////////////////////////////////////////
    [Server]
    public override void OnDeath()
    {
        // take care of entity stuff
        base.OnDeath();

        // set death end time. we set it now to make sure that everything works
        // fine even if a pet isn't updated for a while. so as soon as it's
        // updated again, the death/respawn will happen immediately if current
        // time > end time.
        deathTimeEnd = NetworkTime.time + deathTime;

        // keep player's pet item up to date
        SyncToOwnerItem();
    }

    // attack //////////////////////////////////////////////////////////////////
    public override bool CanAttack(Entity entity)
    {
        return base.CanAttack(entity) &&
               (entity is Monster ||
                (entity is Player && entity != owner) ||
                (entity is Pet pet && pet.owner != owner) ||
                (entity is Mount mount && mount.owner != owner));
    }

    public override void OnInteract()
    {
        Player player = Player.localPlayer;

        // not local player's pet?
        if (this != player.mountControl.activeMount)
        {
            // attackable and has skills? => attack
            if (player.CanAttack(this) && player.skills.skills.Count > 0)
            {
                // then try to use that one
                ((PlayerSkills)player.skills).TryUse(0);
            }
            // otherwise just walk there
            // (e.g. if clicking on it in a safe zone where we can't attack)
            else
            {
                // use collider point(s) to also work with big entities
#if _iMMO2D
                Vector2 destination = Utils.ClosestPoint(this, player.transform.position);
#else
                Vector3 destination = Utils.ClosestPoint(this, player.transform.position);
#endif
                player.movement.Navigate(destination, player.interactionRange);
            }
        }
    }
}
