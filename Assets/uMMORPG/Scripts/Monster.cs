using UnityEngine;
using Mirror;
using UnityEngine.AI;

#if _iMMOANIMATIONTAGS
using System.Collections.Generic;
#endif

#if _iMMO2D
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(MonsterInventory))]
[RequireComponent(typeof(NetworkNavMeshAgent2D))]
#else
[RequireComponent(typeof(Inventory))]
[RequireComponent(typeof(MonsterSkills))]
[RequireComponent(typeof(NetworkNavMeshAgent))]
#endif
public partial class Monster : Entity
{
    [Header("Components")]
    public MonsterInventory inventory;

    [Header("Movement")]
    [Range(0, 1)] public float moveProbability = 0.1f; // Chance per second
    public float moveDistance = 10f;
    public float followDistance = 20f;
    [Range(0.1f, 1f)] public float attackToMoveRangeRatio = 0.8f; // Move to 80% of attack range

    [Header("Experience Reward")]
    public long rewardExperience = 10;
    public long rewardSkillExperience = 2;

    [Header("Respawn")]
    public float deathTime = 30f; // Time for death animation and looting
    [HideInInspector] public double deathTimeEnd;
    public bool respawn = true;
    public float respawnTime = 10f;
    [HideInInspector] public double respawnTimeEnd;

#if _iMMO2D
    [HideInInspector] public Vector2 startPosition;
#else
    [HideInInspector] public Vector3 startPosition;
#endif

#if _iMMOANIMATIONTAGS
    private List<string> validParameters = new List<string>();
#endif

    protected override void Start()
    {
        base.Start();
        startPosition = transform.position;

#if _iMMOANIMATIONTAGS
        foreach (var param in animator.parameters)
        {
            validParameters.Add(param.name);
        }
#endif
    }

    void LateUpdate()
    {
        if (!isClient) return; // Animations only on client

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
            {
#if _iMMOANIMATIONTAGS
                if (skill.animationType != SkillAnimationType.NoAnimation)
                {
                    if (skill.animationType == SkillAnimationType.ScriptableName)
                        animator.SetBool(skill.name, skill.CastTimeRemaining() > 0);
                    else if (skill.animationType == SkillAnimationType.AnimationTag && 
                             !string.IsNullOrEmpty(skill.animationTag) && 
                             validParameters.Contains(skill.animationTag))
                        animator.SetBool(skill.animationTag, skill.CastTimeRemaining() > 0);
#if UNITY_EDITOR
                    else
                        Debug.LogWarning($"Animator for {name} prefab missing parameter: {skill.animationTag}");
#endif
                }
#else
                animator.SetBool(skill.name, skill.CastTimeRemaining() > 0);
#endif
            }
        }
    }

    // FSM Events
    private bool EventDied() => health.current == 0;
    private bool EventDeathTimeElapsed() => state == "DEAD" && NetworkTime.time >= deathTimeEnd;
    private bool EventRespawnTimeElapsed() => state == "DEAD" && respawn && NetworkTime.time >= respawnTimeEnd;
    private bool EventTargetDisappeared() => target == null;
    private bool EventTargetDied() => target != null && (target.health.current == 0 ||
        (target is Player && ((Player)target).playerVehicle.IsInsideVehicle()) ||
        (target is VehicleEntity && !((VehicleEntity)target).enterVehicle.IsAnyPlayerInsideVehicle()));
    private bool EventTargetTooFarToAttack() => target != null && skills.currentSkill >= 0 &&
        skills.currentSkill < skills.skills.Count && !skills.CastCheckDistance(skills.skills[skills.currentSkill], out _);
    private bool EventTargetTooFarToFollow() => target != null &&
#if _iMMO2D
        Vector2.Distance(startPosition, target.collider.ClosestPointOnBounds(transform.position)) > followDistance;
#else
        Vector3.Distance(startPosition, target.collider.ClosestPointOnBounds(transform.position)) > followDistance;
#endif
    private bool EventTargetEnteredSafeZone() => target != null && target.inSafeZone;
    private bool EventAggro() => target != null && target.health.current > 0;
    private bool EventSkillRequest() => skills.currentSkill >= 0 && skills.currentSkill < skills.skills.Count;
#if !_iMMOTIMEDSKILLFINISH
    private bool EventSkillFinished() => skills.currentSkill >= 0 && skills.currentSkill < skills.skills.Count &&
        skills.skills[skills.currentSkill].CastTimeRemaining() == 0;
#endif
    private bool EventMoveEnd() => state == "MOVING" && !movement.IsMoving();
    private bool EventMoveRandomly() => Random.value <= moveProbability * Time.deltaTime;
    private bool EventStunned() => NetworkTime.time <= stunTimeEnd;

    // FSM Server States
    [Server]
    private string UpdateServer_IDLE()
    {
        if (EventDied()) return "DEAD";
        if (EventStunned()) { movement.Reset(); return "STUNNED"; }
        if (EventTargetDied()) { target = null; skills.CancelCast(); return "IDLE"; }
        if (EventTargetTooFarToFollow()) { target = null; skills.CancelCast(); movement.Navigate(startPosition, 0); return "MOVING"; }
        if (EventTargetTooFarToAttack()) NavigateToAttackRange();
        if (EventTargetEnteredSafeZone()) { base.OnDeath(); respawnTimeEnd = NetworkTime.time + respawnTime; return "DEAD"; }
        if (EventSkillRequest()) return HandleSkillRequest();
        if (EventAggro()) return PrepareAttack();
        if (EventMoveRandomly()) { NavigateRandomly(); return "MOVING"; }
        return "IDLE";
    }

    [Server]
    private string UpdateServer_MOVING()
    {
        if (EventDied()) { movement.Reset(); return "DEAD"; }
        if (EventStunned()) { movement.Reset(); return "STUNNED"; }
        if (EventMoveEnd()) return "IDLE";
        if (EventTargetDied()) { target = null; skills.CancelCast(); movement.Reset(); return "IDLE"; }
        if (EventTargetTooFarToFollow()) { target = null; skills.CancelCast(); movement.Navigate(startPosition, 0); return "MOVING"; }
        if (EventTargetTooFarToAttack()) NavigateToAttackRange();
        if (EventTargetEnteredSafeZone()) { base.OnDeath(); respawnTimeEnd = NetworkTime.time + respawnTime; return "DEAD"; }
        if (EventAggro()) return PrepareAttack();
        return "MOVING";
    }

    [Server]
    private string UpdateServer_CASTING()
    {
#if !_iMMO2D
        if (target) movement.LookAtY(target.transform.position);
#endif
        if (EventDied()) return "DEAD";
        if (EventStunned()) { skills.CancelCast(); movement.Reset(); return "STUNNED"; }
        if (EventTargetDisappeared() || EventTargetDied() || EventTargetEnteredSafeZone())
        {
            if (skills.skills[skills.currentSkill].cancelCastIfTargetDied)
            {
                if (EventTargetEnteredSafeZone()) { base.OnDeath(); respawnTimeEnd = NetworkTime.time + respawnTime; return "DEAD"; }
                skills.CancelCast(); target = null; return "IDLE";
            }
        }
#if _iMMOTIMEDSKILLFINISH
        if (monsterTimedSkillFinish.StateSkillFinished()) return "IDLE";
#else
        if (EventSkillFinished()) { skills.FinishCast(skills.skills[skills.currentSkill]); UpdateTarget(); return "IDLE"; }
#endif
        return "CASTING";
    }

    [Server]
    private string UpdateServer_STUNNED()
    {
        if (EventDied()) return "DEAD";
        if (EventStunned()) return "STUNNED";
        return "IDLE";
    }

    [Server]
    private string UpdateServer_DEAD()
    {
        if (EventRespawnTimeElapsed()) { RespawnMonster(); return "IDLE"; }
        if (EventDeathTimeElapsed()) { if (respawn) Hide(); else NetworkServer.Destroy(gameObject); return "DEAD"; }
        return "DEAD";
    }

    [Server]
    protected override string UpdateServer()
    {
        switch (state)
        {
            case "IDLE": return UpdateServer_IDLE();
            case "MOVING": return UpdateServer_MOVING();
            case "CASTING": return UpdateServer_CASTING();
            case "STUNNED": return UpdateServer_STUNNED();
            case "DEAD": return UpdateServer_DEAD();
            default: Debug.LogError($"Invalid state: {state}"); return "IDLE";
        }
    }

    [Client]
    protected override void UpdateClient()
    {
#if !_iMMO2D
        if (state == "CASTING" && target) movement.LookAtY(target.transform.position);
#endif
    }

#if !_iMMO2D
    public void OnDrawGizmos()
    {
        Vector3 gizmoPosition = Application.isPlaying ? startPosition : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(gizmoPosition, moveDistance);
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(gizmoPosition, followDistance);
    }
#endif

    public void OnDeathManageCollider(bool enable) => collider.enabled = enable;

    [ServerCallback]
    public override void OnAggro(Entity entity)
    {
        base.OnAggro(entity);
        if (!CanAttack(entity)) return;

        if (target == null) target = entity;
        else if (entity != target)
        {
#if _iMMO2D
            float oldDistance = Vector2.Distance(transform.position, target.transform.position);
            float newDistance = Vector2.Distance(transform.position, entity.transform.position);
#else
            float oldDistance = Vector3.Distance(transform.position, target.transform.position);
            float newDistance = Vector3.Distance(transform.position, entity.transform.position);
#endif
            if (newDistance < oldDistance * 0.8f) target = entity;
        }
    }

    [Server]
    public override void OnDeath()
    {
        base.OnDeath();
        deathTimeEnd = NetworkTime.time + deathTime;
        respawnTimeEnd = deathTimeEnd + respawnTime;
    }

    public override bool CanAttack(Entity entity)
    {
        return base.CanAttack(entity) && (
            (entity is Player && !((Player)entity).playerVehicle.IsInsideVehicle()) ||
            (entity is VehicleEntity && ((VehicleEntity)entity).enterVehicle.IsAnyPlayerInsideVehicle()) ||
            entity is Pet ||
#if _iMMONPCEXTENDED
            entity is Npc ||
#endif
            entity is Mount);
    }

    public override bool Tools_CanAttack(Entity entity)
    {
        return base.Tools_CanAttack(entity)
#if _iMMOPVP
            && GetAttackAllowance(entity)
#endif
            ;
    }

    // Helper Methods
    [Server]
    private void NavigateToAttackRange()
    {
#if _iMMO2D
        movement.Navigate(target.collider.ClosestPointOnBounds(transform.position),
            ((MonsterSkills)skills).CurrentCastRange() * attackToMoveRangeRatio);
#else
        float stoppingDistance = ((MonsterSkills)skills).CurrentCastRange() * attackToMoveRangeRatio;
        Vector3 destination = Utils.ClosestPoint(target, transform.position);
        movement.Navigate(destination, stoppingDistance);
#endif
        SetState("MOVING");
    }

    [Server]
    private void NavigateRandomly()
    {
#if _iMMO2D
        Vector2 circle2D = Random.insideUnitCircle * moveDistance;
        movement.Navigate(startPosition + circle2D, 0);
#else
        Vector3 circle2D = Random.insideUnitCircle * moveDistance;
        movement.Navigate(startPosition + new Vector3(circle2D.x, 0, circle2D.y), 0);
#endif
        SetState("MOVING");
    }

    [Server]
    private string HandleSkillRequest()
    {
        Skill skill = skills.skills[skills.currentSkill];
        if (skills.CastCheckSelf(skill))
        {
            if (skills.CastCheckTarget(skill))
            {
                skills.StartCast(skill);
                return "CASTING";
            }
            target = null;
            skills.currentSkill = -1;
        }
        else skills.currentSkill = -1;
        return "IDLE";
    }

    [Server]
    private string PrepareAttack()
    {
        if (skills.skills.Count > 0) skills.currentSkill = ((MonsterSkills)skills).NextSkill();
        else Debug.LogError($"{name} has no skills to attack with.");
        movement.Reset();
        return "IDLE";
    }

    [Server]
    private void UpdateTarget()
    {
        if (target != null && target.health.current == 0) target = null;
        ((MonsterSkills)skills).lastSkill = skills.currentSkill;
        skills.currentSkill = -1;
    }

    [Server]
    private void RespawnMonster()
    {
        gold = 0;
        inventory.slots.Clear();
        Show();
        movement.Warp(startPosition);
        Revive();
        OnDeathManageCollider(true);
#if _iMMOWORLDBOSSMESSAGE && _SERVER
        if (monsterWorldBossMessage != null && monsterWorldBossMessage.isWorldBoss)
            monsterWorldBossMessage.OnReviveMessage();
#endif
    }
    public override void OnInteract()
    {
        Player player = Player.localPlayer;
        if (player.CanAttack(this) && player.skills.skills.Count > 0)
        {
#if _iMMOCOMBATREMASTERED
            if (player.combatRemastered)
            {
                ((PlayerSkills)player.skills).TryUse(player.autoAttack);
            }
            else
            {
                ((PlayerSkills)player.skills).TryUse(0);
            }
#else
            ((PlayerSkills)player.skills).TryUse(0);
#endif
        }
        else if (health.current == 0 && Utils.ClosestDistance(player, this) <= player.interactionRange && inventory.HasLoot())
        {
            UILoot.singleton.Show();
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

    private void SetState(string newState)
    {
        // Assuming state is a property in the base class Entity
        typeof(Entity).GetProperty("state").SetValue(this, newState);
    }
}
