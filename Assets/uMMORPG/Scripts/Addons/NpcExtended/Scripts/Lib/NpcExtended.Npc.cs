using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
// NPC
public partial class Npc
{
#if _iMMONPCEXTENDED
    [Header("[Component : Npc Extended]")]
    public NpcExtended npcExtended;

    bool EventMoveRandomly() =>
        Random.value <= npcExtended.moveProbability * Time.deltaTime;

    public bool EventDeathTimeElapsed(Npc npc) =>
         npc.state == "DEAD" && NetworkTime.time >= npc.npcExtended.deathTimeEnd;

    public bool EventRespawnTimeElapsed(Npc npc) =>
        npc.state == "DEAD" && npc.npcExtended.respawn && NetworkTime.time >= npc.npcExtended.respawnTimeEnd;

    /*public bool EventTargetTooFarToFollow() =>
        target != null &&
        Vector3.Distance(npcExtended.startPosition, Utils.ClosestPoint(target, transform.position)) > npcExtended.followDistance;
    */
    bool EventTargetTooFarToFollow() =>
    target != null &&
#if _iMMO2D
        Vector2.Distance(startPosition, target.collider.ClosestPointOnBounds(transform.position)) > npcExtended.followDistance;
#else
        Vector3.Distance(startPosition, target.collider.ClosestPointOnBounds(transform.position)) > npcExtended.followDistance;
#endif

    bool EventTargetEnteredSafeZone() =>
        target != null && target.inSafeZone;
    bool EventMoveEnd() =>
        state == "MOVING" && !movement.IsMoving();
    bool EventDied() =>
        health.current == 0;

    bool EventTargetDied() =>
    target != null && target.health.current == 0;

    bool EventStunned() =>
        NetworkTime.time <= stunTimeEnd;

    bool EventTargetDisappeared() =>
        target == null;

    bool EventTargetTooFarToAttack() =>
        target != null &&
        0 <= skills.currentSkill && skills.currentSkill < skills.skills.Count &&
        !skills.CastCheckDistance(skills.skills[skills.currentSkill],
#if _iMMO2D
            out Vector2 destination);
#else
            out Vector3 destination);
#endif
    bool EventSkillRequest() =>
        0 <= skills.currentSkill && skills.currentSkill < skills.skills.Count;

#if !_iMMOTIMEDSKILLFINISH
    bool EventSkillFinished() =>
        0 <= skills.currentSkill && skills.currentSkill < skills.skills.Count &&
        skills.skills[skills.currentSkill].CastTimeRemaining() == 0;
#endif
    bool EventDeathTimeElapsed() =>
    state == "DEAD" && NetworkTime.time >= npcExtended.deathTimeEnd;

    bool EventRespawnTimeElapsed() =>
        state == "DEAD" && npcExtended.respawn && NetworkTime.time >= npcExtended.respawnTimeEnd;
    bool EventAggro() =>
        target != null && target.health.current > 0;

    // save the start position for random movement distance and respawning
#if _iMMO2D
    [HideInInspector] public Vector2 startPosition;

#else
    [HideInInspector] public Vector3 startPosition;
#endif
#if _iMMOANIMATIONTAGS
    List<string> validParameters = new List<string>();
#endif
    protected override void Start()
    {
        base.Start();
        if (npcExtended)
        {
            // remember start position in case we need to respawn later
            startPosition = transform.position;
#if _iMMOANIMATIONTAGS
            for (int i = 0; i < animator.parameters.Length; i++)
            {
                validParameters.Add(animator.parameters[i].name);
            }
#endif
        }
    }

    void LateUpdate()
    {
        // pass parameters to animation state machine
        // => passing the states directly is the most reliable way to avoid all
        //    kinds of glitches like movement sliding, attack twitching, etc.
        // => make sure to import all looping animations like idle/run/attack
        //    with 'loop time' enabled, otherwise the client might only play it
        //    once
        // => only play moving animation while the actually moving (velocity).
        //    the MOVING state might be delayed to due latency or we might be in
        //    MOVING while a path is still pending, etc.
        // => skill names are assumed to be boolean parameters in animator
        //    so we don't need to worry about an animation number etc.
        if (isClient) // no need for animations on the server
        {
#if _iMMO2D
            animator.SetBool("MOVING", state == "MOVING" && movement.GetVelocity() != Vector2.zero);
            animator.SetFloat("LookX", lookDirection.x);
            animator.SetFloat("LookY", lookDirection.y);
#else
            animator.SetBool("MOVING", state == "MOVING" && movement.GetVelocity() != Vector3.zero);
#endif
            animator.SetBool("CASTING", state == "CASTING");
            animator.SetBool("STUNNED", state == "STUNNED");
            animator.SetBool("DEAD", state == "DEAD");
            if (state == "CASTING")
            {
                foreach (Skill skill in skills.skills)
#if _iMMOANIMATIONTAGS
                {
                    // no animarion is unchecked
                    if (skill.animationType != SkillAnimationType.NoAnimation)
                    {
                        // use Scriptanle Name is  checked
                        if (skill.animationType == SkillAnimationType.ScriptableName)
                            animator.SetBool(skill.name, skill.CastTimeRemaining() > 0);
                        // Use  custom animation
                        else
                        {
                            // animation tag is not null and exist in animator
                            if (skill.animationType == SkillAnimationType.AnimationTag && !string.IsNullOrEmpty(skill.animationTag) && validParameters.Contains(skill.animationTag))
                                animator.SetBool(skill.animationTag, skill.CastTimeRemaining() > 0);
#if UNITY_EDITOR
                            else
                                Debug.LogWarning("In Animator for " + name + " prefab the animation parameter " + skill.animationTag + " not exist !");
#endif
                        }
                    }
                }
#else
                animator.SetBool(skill.name, skill.CastTimeRemaining() > 0);
#endif
            }
        }
    }
    // states //////////////////////////////////////////////////////////////////
    [Server]
    string UpdateServer_IDLE()
    {
        if (npcExtended && npcExtended.waypoints.Length > 0 && npcExtended.GetEntityNearby(npcExtended.playerRadiusDetect))
        {
           // Debug.Log("UpdateServer_IDLE > if (npcPatrol.waypoints.Length > 0 && npcPatrol.GetEntityNearby(playerRadiusDetect))");
            if (npcExtended.interactStopsMovement)
            {
            //    Debug.Log("a Entity is in radius in UpdateServer_IDLE");
                movement.Reset();
                return "IDLE";
            }
            npcExtended.GotoNextWayPoint();
            return "MOVING";
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
        if (EventTargetTooFarToFollow())
        {
            // we had a target before, but it's out of follow range now.
            // clear it and go back to start. don't stay here.
            target = null;
            skills.CancelCast();
            movement.Navigate(npcExtended.startPosition, 0);
            return "MOVING";
        }
        if (EventTargetTooFarToAttack())
        {
            // we had a target before, but it's out of attack range now.
            // follow it. (use collider point(s) to also work with big entities)
            float stoppingDistance = ((NpcSkills)skills).CurrentCastRange() * npcExtended.attackToMoveRangeRatio;
            Vector3 destination = Utils.ClosestPoint(target, transform.position);
            movement.Navigate(destination, stoppingDistance);
            return "MOVING";
        }
        if (EventTargetEnteredSafeZone())
        {
            // if our target entered the safe zone, we need to be really careful
            // to avoid kiting.
            // -> players could pull a monster near a safe zone and then step in
            //    and out of it before/after attacks without ever getting hit by
            //    the monster
            // -> running back to start won't help, can still kit while running
            // -> warping back to start won't help, we might accidentally placed
            //    a monster in attack range of a safe zone
            // -> the 100% secure way is to die and hide it immediately. many
            //    popular MMOs do it the same way to avoid exploits.
            // => call Entity.OnDeath without rewards etc. and hide immediately
            OnDeath(); // no looting
            npcExtended.respawnTimeEnd = NetworkTime.time + npcExtended.respawnTime; // respawn in a while
            return "DEAD";
        }
        if (EventSkillRequest())
        {
            // we had a target in attack range before and trying to cast a skill
            // on it. check self (alive, mana, weapon etc.) and target
            Skill skill = skills.skills[skills.currentSkill];
            if (skills.CastCheckSelf(skill))
            {
                if (skills.CastCheckTarget(skill))
                {
                    // start casting
                    skills.StartCast(skill);
                    return "CASTING";
                }
                else
                {
                    // invalid target. clear the attempted current skill.
                    target = null;
                    skills.currentSkill = -1;
                    return "IDLE";
                }
            }
            else
            {
                // we can't cast this skill at the moment (cooldown/low mana/...)
                // -> clear the attempted current skill, but keep the target to
                // continue later
                skills.currentSkill = -1;
                return "IDLE";
            }
        }
        if (EventAggro())
        {
            // target in attack range. try to cast a first skill on it
            if (skills.skills.Count > 0) skills.currentSkill = ((NpcSkills)skills).NextSkill();
            else Debug.LogError(name + " has no skills to attack with.");
            return "IDLE";
        }
        if (EventMoveRandomly())
        {
            // walk to a random position in movement radius (from 'start')
            // note: circle y is 0 because we add it to start.y
            //Vector2 circle2D = Random.insideUnitCircle * moveDistance;
            //monster.movement.Navigate(monster.startPosition + new Vector3(circle2D.x, 0, circle2D.y), 0);
            //Utils.Invo()keMany(typeof(MonsterBrain), this, "OnEventMoveRandomly_");
            if (UnityEngine.Random.value <= npcExtended.WaypointToRandomRatio || !npcExtended.DoPatrol())
            {
                //npcExtended.GotoRandomPoint();
#if _iMMO2D
                Vector2 circle2D = Random.insideUnitCircle * npcExtended.moveDistance;
                movement.Navigate(startPosition + circle2D, 0);
#else
                Vector3 circle2D = Random.insideUnitCircle * npcExtended.moveDistance;
                movement.Navigate(startPosition + new Vector3(circle2D.x, 0, circle2D.y), 0);
#endif
            }
            else
            {
                npcExtended.GotoNextWayPoint();
            }
            return "MOVING";
        }
        if (EventDeathTimeElapsed()) { } // don't care
        if (EventRespawnTimeElapsed()) { } // don't care
        if (EventMoveEnd()) { } // don't care
        if (EventSkillFinished()) { } // don't care
        if (EventTargetDisappeared()) { } // don't care

        return "IDLE"; // nothing interesting happened
    }
    [Server]
    string UpdateServer_MOVING()
    {

        if (npcExtended.GetEntityNearby(npcExtended.playerRadiusDetect) && npcExtended.interactStopsMovement)
        {
           // Debug.Log("UpdateServer_MOVING > if (npcPatrol.GetEntityNearby(playerRadiusDetect) && interactStopsMovement)");
            movement.Reset();
            return "IDLE";
        }
        
        // events sorted by priority (e.g. target doesn't matter if we died)
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
        if (EventTargetTooFarToFollow())
        {
            // we had a target before, but it's out of follow range now.
            // clear it and go back to start. don't stay here.
            target = null;
            skills.CancelCast();
            movement.Navigate(npcExtended.startPosition, 0);
            return "MOVING";
        }
//#if nextpdate
        if (EventTargetTooFarToAttack())
        {
            // we had a target before, but it's out of attack range now.
            // follow it. (use collider point(s) to also work with big entities)
            float stoppingDistance = ((NpcSkills)skills).CurrentCastRange() * npcExtended.attackToMoveRangeRatio;
            Vector3 destination = Utils.ClosestPoint(target, transform.position);
            movement.Navigate(destination, stoppingDistance);
            return "MOVING";
        }
//#endif
        if (EventTargetEnteredSafeZone())
        {
            // if our target entered the safe zone, we need to be really careful
            // to avoid kiting.
            // -> players could pull a npc near a safe zone and then step in
            //    and out of it before/after attacks without ever getting hit by
            //    the npc
            // -> running back to start won't help, can still kit while running
            // -> warping back to start won't help, we might accidentally placed
            //    a npc in attack range of a safe zone
            // -> the 100% secure way is to die and hide it immediately. many
            //    popular MMOs do it the same way to avoid exploits.
            // => call Entity.OnDeath without rewards etc. and hide immediately
            OnDeath(); // no looting
            npcExtended.respawnTimeEnd = NetworkTime.time + npcExtended.respawnTime; // respawn in a while
            return "DEAD";
        }
        //#if nextupdate
        if (EventAggro())
        {
            // target in attack range. try to cast a first skill on it
            // (we may get a target while randomly wandering around)
            // if (npc.skills.skills.Count > 0) npc.skills.currentSkill = ((NpcSkills)npc.skills).NextSkill();
            //  else Debug.LogError(name + " has no skills to attack with.");
            // Debug.Log("player dans la zone");
            movement.Reset();
            return "IDLE";

        }
        //#endif
        if (EventDeathTimeElapsed()) { } // don't care
        if (EventRespawnTimeElapsed()) { } // don't care
        if (EventSkillFinished()) { } // don't care
        if (EventTargetDisappeared()) { } // don't care
        if (EventSkillRequest()) { } // don't care, finish movement first
        if (EventMoveRandomly()) { } // don't care

        return "MOVING"; // nothing interesting happened
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
        if (EventTargetEnteredSafeZone())
        {
            // cancel if the target matters for this skill
            if (skills.skills[skills.currentSkill].cancelCastIfTargetDied)
            {
                // if our target entered the safe zone, we need to be really careful
                // to avoid kiting.
                // -> players could pull a npc near a safe zone and then step in
                //    and out of it before/after attacks without ever getting hit by
                //    the npc
                // -> running back to start won't help, can still kit while running
                // -> warping back to start won't help, we might accidentally placed
                //    a npc in attack range of a safe zone
                // -> the 100% secure way is to die and hide it immediately. many
                //    popular MMOs do it the same way to avoid exploits.
                // => call Entity.OnDeath without rewards etc. and hide immediately
                OnDeath(); // no looting
                npcExtended.respawnTimeEnd = NetworkTime.time + npcExtended.respawnTime; // respawn in a while
                return "DEAD";
            }
        }
        if (EventSkillFinished())
        {
            // finished casting. apply the skill on the target.
            skills.FinishCast(skills.skills[skills.currentSkill]);

            // did the target die? then clear it so that the npc doesn't
            // run towards it if the target respawned
            // (target might be null if disappeared or targetless skill)
            if (target != null && target.health.current == 0)
                target = null;

            // go back to IDLE, reset current skill
#if nextupdate
            ((NpcSkills)npc.skills).lastSkill = npc.skills.currentSkill;
            npc.skills.currentSkill = -1;
#endif
            return "IDLE";
        }
        if (EventDeathTimeElapsed()) { } // don't care
        if (EventRespawnTimeElapsed()) { } // don't care
        if (EventMoveEnd()) { } // don't care
        if (EventTargetTooFarToAttack()) { } // don't care, we were close enough when starting to cast
        if (EventTargetTooFarToFollow()) { } // don't care, we were close enough when starting to cast
        if (EventAggro()) { } // don't care, always have aggro while casting
        if (EventSkillRequest()) { } // don't care, that's why we are here
        if (EventMoveRandomly()) { } // don't care

        return "CASTING"; // nothing interesting happened
    }
    [Server]
    string UpdateServer_STUNNED()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventDied())
        {
            // we died.
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
    string UpdateServer_DEAD()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventRespawnTimeElapsed())
        {
            // respawn at the start position with full health, visibility, no loot
            gold = 0;
            //npc.inventory.slots.Clear();
            Show();
            movement.Warp(npcExtended.startPosition); // recommended over transform.position
            Revive();
            return "IDLE";
        }
        if (EventDeathTimeElapsed())
        {
            // we were lying around dead for long enough now.
            // hide while respawning, or disappear forever
            if (npcExtended.respawn) Hide();
            else NetworkServer.Destroy(gameObject);
            return "DEAD";
        }
        if (EventSkillRequest()) { } // don't care
        if (EventSkillFinished()) { } // don't care
        if (EventMoveEnd()) { } // don't care
        if (EventTargetDisappeared()) { } // don't care
        if (EventTargetDied()) { } // don't care
        if (EventTargetTooFarToFollow()) { } // don't care
        if (EventTargetTooFarToAttack()) { } // don't care
        if (EventTargetEnteredSafeZone()) { } // don't care
        if (EventAggro()) { } // don't care
        if (EventMoveRandomly()) { } // don't care
        if (EventStunned()) { } // don't care
        if (EventDied()) { } // don't care, of course we are dead

        return "DEAD"; // nothing interesting happened
    }

    [Server]
    protected override string UpdateServer()
    {
        if (npcExtended)
        {
            if (state == "IDLE") return UpdateServer_IDLE();
            if (state == "MOVING") return UpdateServer_MOVING();
            if (state == "CASTING") return UpdateServer_CASTING();
            if (state == "STUNNED") return UpdateServer_STUNNED();
            if (state == "DEAD") return UpdateServer_DEAD();
            Debug.LogError("invalid state:" + state);
        }
        return "IDLE";

    }


    // finite state machine - client ///////////////////////////////////////////
    [Client]
    protected override void UpdateClient()
    {
        if (state == "CASTING")
        {
            // keep looking at the target for server & clients (only Y rotation)
#if !_iMMO2D
            if (target)
                movement.LookAtY(target.transform.position);
#endif
        }
    }

    // DrawGizmos can be used for debug info
    public void OnDrawGizmos()
    {
        // draw the movement area (around 'start' if game running,
        // or around current position if still editing)
        if (npcExtended)
        {
            Vector3 startHelp = Application.isPlaying ? npcExtended.startPosition : transform.position;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(startHelp, npcExtended.moveDistance);

            // draw the follow dist
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(startHelp, npcExtended.followDistance);
        }
    }


    // aggro ///////////////////////////////////////////////////////////////////
    // this function is called by entities that attack us and by AggroArea
    [ServerCallback]
    public override void OnAggro(Entity entity)
    {
        // call base function
        base.OnAggro(entity);
        // are we alive, and is the entity alive and of correct type?
        if (CanAttack(entity) && !Tools_SameRealm(entity))
        {
            // no target yet(==self), or closer than current target?
            // => has to be at least 20% closer to be worth it, otherwise we
            //    may end up nervously switching between two targets
            // => we do NOT use Utils.ClosestDistance, because then we often
            //    also end up nervously switching between two animated targets,
            //    since their collides moves with the animation.
            //    => we don't even need closestdistance here because they are in
            //       the aggro area anyway. transform.position is perfectly fine
            if (target == null)
            {
                target = entity;
            }
            else if (entity != target) // no need to check dist for same target
            {
                float oldDistance = Vector3.Distance(transform.position, target.transform.position);
                float newDistance = Vector3.Distance(transform.position, entity.transform.position);
                if (newDistance < oldDistance * 0.8) target = entity;
            }
        }
    }
    public override bool CanAttack(Entity entity)
    {
        return Tools_CanAttack(entity);
    }



    public override void OnInteract()
    {
        Player player = Player.localPlayer;

        // attackable and has skills? => attack
        if (npcPVPZone != null && player.CanAttack(this) && player.skills.skills.Count > 0 && !player.Tools_SameRealm(player.target))
        {
            // then try to use that one
            ((PlayerSkills)player.skills).TryUse(0);
        }
        // alive, close enough? then talk
        // use collider point(s) to also work with big entities
        else if (health.current > 0 &&
            Utils.ClosestDistance(player, this) <= player.interactionRange)
        {
            UINpcDialogue.singleton.Show();
        }
        // dead, has loot, close enough?
        // use collider point(s) to also work with big entities
        /*else if (health.current == 0 &&
                 Utils.ClosestDistance(player, this) <= player.interactionRange &&
                 inventory.HasLoot()) // <- inventory n'est pas dans les npc
        {
            UILoot.singleton.Show();
        }*/
        // otherwise just walk there
        // (e.g. if clicking on it in a safe zone where we can't attack)
        else
        {
            // use collider point(s) to also work with big entities
            Vector3 destination = Utils.ClosestPoint(this, player.transform.position);
            player.movement.Navigate(destination, player.interactionRange);
        }




        // otherwise walk there
        // use collider point(s) to also work with big entities
        /*else
        {
            Vector3 destination = Utils.ClosestPoint(this, player.transform.position);
            player.movement.Navigate(destination, player.interactionRange);
        }*/
    }
#endif
        }
