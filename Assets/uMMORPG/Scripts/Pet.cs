using UnityEngine;
using Mirror;
using TMPro;
#if _iMMOANIMATIONTAGS
using System.Collections.Generic;
#endif
using UnityEngine.AI;

#if _iMMO2D
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Experience))]
[RequireComponent(typeof(NetworkNavMeshAgent2D))]
#else
[RequireComponent(typeof(Experience))]
[RequireComponent(typeof(PetSkills))]
[RequireComponent(typeof(NavMeshAgent))] // Changed NavMeshMovement to NavMeshAgent
[RequireComponent(typeof(NetworkNavMeshAgent))]
#endif
public partial class Pet : Summonable
{
    [Header("Components")]
    public Experience experience;

    [Header("Icons")]
    public Sprite portraitIcon; // for pet status UI

    [Header("Text Meshes")]
    public TextMeshPro ownerNameOverlay;

    [Header("Movement")]
    public float returnDistance = 25; // return to player if dist > ...
    public float followDistance = 20; // pets should follow their targets even if they run out of the movement radius
    public float teleportDistance = 30; // pet should teleport if the owner gets too far away
    [Range(0.1f, 1)] public float attackToMoveRangeRatio = 0.8f; // move as close as 0.8 * attackRange to a target

    // use owner's speed if found, so that the pet can still follow the owner if he is riding a mount, etc.
    public override float speed => owner != null ? owner.speed : base.speed;

    [Header("Death")]
    public float deathTime = 2; // enough for animation
    [HideInInspector] public double deathTimeEnd; // double for long term precision

    [Header("Behaviour")]
    [SyncVar] public bool stayPut = false; // stay on the spot
    [SyncVar] public bool followingOwner = false; // follow owner at all times
    [SyncVar] public bool defendOwner = true; // attack what attacks the owner
    [SyncVar] public bool autoAttack = true; // attack what the owner attacks

    // sync to item ////////////////////////////////////////////////////////////
    protected override ItemSlot SyncStateToItemSlot(ItemSlot slot)
    {
        slot = base.SyncStateToItemSlot(slot);
        slot.item.summonedExperience = experience.current;
        return slot;
    }

#if _iMMOANIMATIONTAGS
    List<string> validParameters = new List<string>();

    protected override void Start()
    {
        base.Start(); // Call base Start if needed
        for (int i = 0; i < animator.parameters.Length; i++)
        {
            validParameters.Add(animator.parameters[i].name);
        }
    }
#endif

    void LateUpdate()
    {
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
                    if (skill.animationType != SkillAnimationType.NoAnimation)
                    {
                        if (skill.animationType == SkillAnimationType.ScriptableName)
                            animator.SetBool(skill.name, skill.CastTimeRemaining() > 0);
                        else if (skill.animationType == SkillAnimationType.AnimationTag && !string.IsNullOrEmpty(skill.animationTag) && validParameters.Contains(skill.animationTag))
                            animator.SetBool(skill.animationTag, skill.CastTimeRemaining() > 0);
#if UNITY_EDITOR
                        else
                            Debug.LogWarning("In Animator for " + name + " prefab the animation parameter " + skill.animationTag + " not exist !");
#endif
                    }
                }
#else
                    animator.SetBool(skill.name, skill.CastTimeRemaining() > 0);
#endif
            }
        }

        if (!isServerOnly)
        {
            if (ownerNameOverlay != null)
            {
                if (owner != null)
                {
                    ownerNameOverlay.text = owner.name;
                    if (Player.localPlayer != null)
                    {
                        if (owner.IsMurderer())
                            ownerNameOverlay.color = owner.nameOverlayMurdererColor;
                        else if (owner.IsOffender())
                            ownerNameOverlay.color = owner.nameOverlayOffenderColor;
                        else if (Player.localPlayer.party.InParty() && Player.localPlayer.party.party.Contains(owner.name))
                            ownerNameOverlay.color = owner.nameOverlayPartyColor;
                        else
                            ownerNameOverlay.color = owner.nameOverlayDefaultColor;
                    }
                }
                else ownerNameOverlay.text = "?";
            }
        }
    }

    void OnDrawGizmos()
    {
#if _iMMO2D
        Vector2 startHelp = Application.isPlaying ? owner.petControl.petDestination : (Vector2)transform.position;
#else
        Vector3 startHelp = Application.isPlaying ? owner.petControl.petDestination : transform.position;
#endif
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(startHelp, returnDistance);
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(startHelp, followDistance);
    }

    void OnDestroy()
    {
        if (isServer)
            SyncToOwnerItem();
    }

    public override bool IsWorthUpdating() { return true; }

    // finite state machine events /////////////////////////////////////////////
    bool EventOwnerDisappeared() => owner == null;
    bool EventStayingPut() => stayPut;
    bool EventDied() => health.current == 0;
    bool EventDeathTimeElapsed() => state == "DEAD" && NetworkTime.time >= deathTimeEnd;
    bool EventTargetDisappeared() => target == null;
    bool EventTargetDied() => target != null && target.health.current == 0;

    bool EventTargetTooFarToAttack() =>
        target != null &&
        0 <= skills.currentSkill && skills.currentSkill < skills.skills.Count &&
#if _iMMO2D
        !skills.CastCheckDistance(skills.skills[skills.currentSkill], out Vector2 destination);
#else
        !skills.CastCheckDistance(skills.skills[skills.currentSkill], out Vector3 destination);
#endif

    bool EventTargetTooFarToFollow() =>
        target != null &&
#if _iMMO2D
        Vector2.Distance(owner.petControl.petDestination, target.collider.ClosestPointOnBounds(transform.position)) > followDistance;
#else
        Vector3.Distance(owner.petControl.petDestination, target.collider.ClosestPointOnBounds(transform.position)) > followDistance;
#endif

    bool EventNeedReturnToOwner() =>
#if _iMMO2D
        Vector2.Distance(owner.petControl.petDestination, transform.position) > returnDistance;
#else
        Vector3.Distance(owner.petControl.petDestination, transform.position) > returnDistance;
#endif

    bool EventNeedTeleportToOwner() =>
#if _iMMO2D
        Vector2.Distance(owner.petControl.petDestination, transform.position) > teleportDistance;
#else
        Vector3.Distance(owner.petControl.petDestination, transform.position) > teleportDistance;
#endif

    bool EventAggro() => target != null && target.health.current > 0;
    bool EventSkillRequest() => 0 <= skills.currentSkill && skills.currentSkill < skills.skills.Count;

#if !_iMMOTIMEDSKILLFINISH
    bool EventSkillFinished() =>
        0 <= skills.currentSkill && skills.currentSkill < skills.skills.Count &&
        skills.skills[skills.currentSkill].CastTimeRemaining() == 0;
#endif

    bool EventMoveEnd() => state == "MOVING" && !movement.IsMoving();
    bool EventStunned() => NetworkTime.time <= stunTimeEnd;

    // finite state machine - server ///////////////////////////////////////////
    [Server]
    string UpdateServer_IDLE()
    {
        if (EventStayingPut())
        {
            target = null;
            skills.CancelCast();
            movement.Reset();
            return "IDLE";
        }
        if (EventOwnerDisappeared())
        {
            NetworkServer.Destroy(gameObject);
            return "IDLE";
        }
        if (EventDied()) return "DEAD";
        if (EventStunned())
        {
            movement.Reset();
            return "STUNNED";
        }
        if (EventTargetDied())
        {
            target = null;
            skills.CancelCast();
            return "IDLE";
        }
        if (EventNeedTeleportToOwner())
        {
            movement.Warp(owner.petControl.petDestination);
            return "IDLE";
        }
        if (EventNeedReturnToOwner() || (followingOwner && Vector3.Distance(owner.transform.position, transform.position) > 3))
        {
            target = null;
            skills.CancelCast();
            movement.Navigate(owner.petControl.petDestination, 0);
            return "MOVING";
        }
        if (EventTargetTooFarToFollow())
        {
            target = null;
            skills.CancelCast();
            movement.Navigate(owner.petControl.petDestination, 0);
            return "MOVING";
        }
        if (EventTargetTooFarToAttack())
        {
#if _iMMO2D
            movement.Navigate(target.collider.ClosestPointOnBounds(transform.position),
                              ((PetSkills)skills).CurrentCastRange() * attackToMoveRangeRatio);
#else
            float stoppingDistance = ((PetSkills)skills).CurrentCastRange() * attackToMoveRangeRatio;
            Vector3 destination = Utils.ClosestPoint(target, transform.position);
            movement.Navigate(destination, stoppingDistance);
#endif
            return "MOVING";
        }
        if (EventSkillRequest())
        {
            Skill skill = skills.skills[skills.currentSkill];
            if (skills.CastCheckSelf(skill) && skills.CastCheckTarget(skill))
            {
                skills.StartCast(skill);
                return "CASTING";
            }
            else
            {
                target = null;
                skills.currentSkill = -1;
                return "IDLE";
            }
        }
        if (EventAggro())
        {
            if (skills.skills.Count > 0)
                skills.currentSkill = ((PetSkills)skills).NextSkill();
            else
                Debug.LogError(name + " has no skills to attack with.");
            return "IDLE";
        }
        return "IDLE";
    }

    [Server]
    string UpdateServer_MOVING()
    {
        if (EventStayingPut())
        {
            target = null;
            skills.CancelCast();
            movement.Reset();
            return "IDLE";
        }
        if (EventOwnerDisappeared())
        {
            NetworkServer.Destroy(gameObject);
            return "IDLE";
        }
        if (EventDied())
        {
            movement.Reset();
            return "DEAD";
        }
        if (EventStunned())
        {
            movement.Reset();
            return "STUNNED";
        }
        if (EventMoveEnd()) return "IDLE";
        if (followingOwner && Vector3.Distance(owner.transform.position, transform.position) > 3)
        {
            target = null;
            skills.CancelCast();
            movement.Navigate(owner.petControl.petDestination, 0);
            return "MOVING";
        }
        if (EventTargetDied())
        {
            target = null;
            skills.CancelCast();
            movement.Reset();
            return "IDLE";
        }
        if (EventNeedTeleportToOwner())
        {
            movement.Warp(owner.petControl.petDestination);
            return "IDLE";
        }
        if (EventTargetTooFarToFollow())
        {
            target = null;
            skills.CancelCast();
            movement.Navigate(owner.petControl.petDestination, 0);
            return "MOVING";
        }
        if (EventTargetTooFarToAttack())
        {
#if _iMMO2D
            movement.Navigate(target.collider.ClosestPointOnBounds(transform.position),
                              ((PetSkills)skills).CurrentCastRange() * attackToMoveRangeRatio);
#else
            float stoppingDistance = ((PetSkills)skills).CurrentCastRange() * attackToMoveRangeRatio;
            Vector3 destination = Utils.ClosestPoint(target, transform.position);
            movement.Navigate(destination, stoppingDistance);
#endif
            return "MOVING";
        }
        if (EventAggro())
        {
            if (skills.skills.Count > 0)
                skills.currentSkill = ((PetSkills)skills).NextSkill();
            else
                Debug.LogError(name + " has no skills to attack with.");
            movement.Reset();
            return "IDLE";
        }
        return "MOVING";
    }

    [Server]
    string UpdateServer_CASTING()
    {
#if !_iMMO2D
        if (target) movement.LookAtY(target.transform.position);
#endif
        if (EventOwnerDisappeared())
        {
            NetworkServer.Destroy(gameObject);
            return "IDLE";
        }
        if (EventDied()) return "DEAD";
        if (EventStunned())
        {
            skills.CancelCast();
            movement.Reset();
            return "STUNNED";
        }
        if (EventTargetDisappeared() || EventTargetDied())
        {
            if (skills.skills[skills.currentSkill].cancelCastIfTargetDied)
            {
                skills.CancelCast();
                target = null;
                return "IDLE";
            }
        }
#if _iMMOTIMEDSKILLFINISH
        // Assuming monsterTimedSkillFinish is meant to be petTimedSkillFinish or similar
        // You may need to adjust this based on your actual implementation
        if (petTimedSkillFinish != null && petTimedSkillFinish.StateSkillFinished()) return "IDLE";
#else
        if (EventSkillFinished())
        {
            skills.FinishCast(skills.skills[skills.currentSkill]);
            if (target != null && target.health.current == 0) target = null;
            ((PetSkills)skills).lastSkill = skills.currentSkill;
            skills.currentSkill = -1;
            return "IDLE";
        }
#endif
        return "CASTING";
    }

    [Server]
    string UpdateServer_STUNNED()
    {
        if (EventOwnerDisappeared())
        {
            NetworkServer.Destroy(gameObject);
            return "IDLE";
        }
        if (EventDied())
        {
            skills.CancelCast();
            return "DEAD";
        }
        if (EventStunned()) return "STUNNED";
        return "IDLE";
    }

    [Server]
    string UpdateServer_DEAD()
    {
        if (EventOwnerDisappeared())
        {
            NetworkServer.Destroy(gameObject);
            return "DEAD";
        }
        if (EventDeathTimeElapsed())
        {
            NetworkServer.Destroy(gameObject);
            return "DEAD";
        }
        return "DEAD";
    }

    [Server]
    protected override string UpdateServer()
    {
        if (state == "IDLE") return UpdateServer_IDLE();
        if (state == "MOVING") return UpdateServer_MOVING();
        if (state == "CASTING") return UpdateServer_CASTING();
        if (state == "STUNNED") return UpdateServer_STUNNED();
        if (state == "DEAD") return UpdateServer_DEAD();
        Debug.LogError("invalid state:" + state);
        return "IDLE";
    }

    [Client]
    protected override void UpdateClient() { }

#if _iMMO2D
    protected override void UpdateOverlays()
    {
        base.UpdateOverlays();
        if (ownerNameOverlay != null)
        {
            if (owner != null)
            {
                ownerNameOverlay.text = owner.name;
                if (Player.localPlayer != null)
                {
                    if (owner.IsMurderer())
                        ownerNameOverlay.color = owner.nameOverlayMurdererColor;
                    else if (owner.IsOffender())
                        ownerNameOverlay.color = owner.nameOverlayOffenderColor;
                    else if (Player.localPlayer.party.InParty() && Player.localPlayer.party.party.Contains(owner.name))
                        ownerNameOverlay.color = owner.nameOverlayPartyColor;
                    else
                        ownerNameOverlay.color = owner.nameOverlayDefaultColor;
                }
            }
            else ownerNameOverlay.text = "?";
        }
    }
#endif

    [ServerCallback]
    public override void OnAggro(Entity entity)
    {
        if (attackTarget != null && attackTarget != entity) return;
        base.OnAggro(entity);
        if (CanAttack(entity))
        {
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

    [Server]
    public override void OnDeath()
    {
        base.OnDeath();
        deathTimeEnd = NetworkTime.time + deathTime;
        SyncToOwnerItem();
    }

    public override bool CanAttack(Entity entity)
    {
        return base.CanAttack(entity) &&
               (entity is Monster ||
                (entity is Player && entity != owner) ||
                (entity is Pet pet && pet.owner != owner) ||
                (entity is Mount mount && mount.owner != owner));
    }

    [Command]
    public void CmdSetAutoAttack(bool value) => autoAttack = value;
    [Command]
    public void CmdSetStayPut(bool value) => stayPut = value;
    [Command]
    public void CmdFollowOnly(bool value)
    {
        attackTarget = null;
        followingOwner = value;
    }
    [Command]
    public void CmdAttackTarget(Entity victim)
    {
        attackTarget = victim;
        OnAggro(victim);
    }
    [Command]
    public void CmdSetDefendOwner(bool value)
    {
#if _SERVER
        attackTarget = null;
        defendOwner = value;
#endif
    }

    public override void OnInteract()
    {
        Player player = Player.localPlayer;
        if (this != player.petControl.activePet)
        {
            if (player.CanAttack(this) && player.skills.skills.Count > 0)
            {
                ((PlayerSkills)player.skills).TryUse(0);
            }
            else
            {
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