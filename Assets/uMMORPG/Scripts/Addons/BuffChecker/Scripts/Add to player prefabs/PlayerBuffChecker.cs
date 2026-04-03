using UnityEngine;
using Mirror;

// PlayerBuffChecker
public class PlayerBuffChecker : NetworkBehaviour
{
    [Header("[-=-[ BUFF CHECKER ]-=-]")]
    public BuffCheckerEntry[] buffEntry;

    protected Entity entity;
    protected Animator animator;
    protected float cacheTimerInterval = 1.0f;
    protected float _cacheTimer;
    protected string skillName = "";

    // -----------------------------------------------------------------------------------
    // Start
    // -----------------------------------------------------------------------------------
    private void Start()
    {
        entity = GetComponent<Entity>();
        animator = entity.animator;
        entity.skills.onSkillCastStarted.AddListener(AddSkillCurrent);
        entity.skills.onSkillCastFinished.AddListener(RemoveskillCurrent);
        entity.combat.onClientReceivedDamage.AddListener(OnDamageDealtTo_HideMesh);
        entity.health.onEmpty.AddListener(OnDeath_HideMesh); // TODO <- necesaire pour supprimé les mesh activé avant la mort
        entity.skills.CancelCastToggle.AddListener(RemoveskillCurrent2);
    }

    // -----------------------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------------------
    private void Update()
    {
        if (Time.time == 0 || Time.time > _cacheTimer)
        {
            if (entity == null) return;

            foreach (BuffCheckerEntry entry in buffEntry)
            {
                if (entry.skillName != "") continue;
                if (entry.isActive(entity))
                {
                    entry.ToggleGameObject(!entry.revert);

                    if (!string.IsNullOrWhiteSpace(entry.animationName))
                    {
                        animator.Play(entry.animationName);
                    }
                }
                else
                {
                    entry.ToggleGameObject(entry.revert);

                    if (!string.IsNullOrWhiteSpace(entry.animationName))
                    {
                        if (entity.state == "CASTING")
                            animator.SetInteger(entry.animationName, 0);
                    }
                }
            }

            if (entity.state == "STUNNED")
            {
                skillName = "";
                foreach (BuffCheckerEntry entry in buffEntry)
                {
                    entry.ToggleGameObject(entry.revert);
                }
            }

            if (entity.state != "CASTING")
            {
                foreach (BuffCheckerEntry entry in buffEntry)
                {
                    entry.ToggleGameObject(false);
                }
            }
            _cacheTimer = Time.time + cacheTimerInterval;

        }
    }

    private void AddSkillCurrent(Skill skill)
    {
        foreach (BuffCheckerEntry entry in buffEntry)
        {
            if (entry.isActive(entity, skill.name))
                entry.ToggleGameObject(!entry.revert);
        }
    }

    private void RemoveskillCurrent(Skill skill)
    {
        skillName = "";
        foreach (BuffCheckerEntry entry in buffEntry)
        {
            if (entry.isActive(entity, skill.name))
                entry.ToggleGameObject(entry.revert);
        }
    }

    private void RemoveskillCurrent2(Skill skill)
    {
        skillName = "";
        foreach (BuffCheckerEntry entry in buffEntry)
        {
            if (entry.isActive(entity, skill.name))
                entry.ToggleGameObject((entry.revert) ? true : false);
        }
    }

    private void OnDamageDealtTo_HideMesh(int value, DamageType type)
    {
        if (entity.state == "STUNNED")
        {
            skillName = "";
            foreach (BuffCheckerEntry entry in buffEntry)
            {
                entry.ToggleGameObject(false);
            }
        }
    }

    private void OnDeath_HideMesh()
    {
        skillName = "";
        foreach (BuffCheckerEntry entry in buffEntry)
        {
            entry.ToggleGameObject(entry.revert);
        }
    }
    // -----------------------------------------------------------------------------------
}