using System.Text;
using UnityEngine;
using Mirror;

[CreateAssetMenu(menuName = "ADDON/Skills/Skill Drag", order = 998)]
public class DragTarget : DamageSkill
{
    [Header("Pull Target Skill")]
    [Tooltip("Pull Target when you use the skill. Yes/No")]
    public bool MoveTarget = false;
    [Tooltip("The distance between you and the entity when you Pull. Default is 1.")]
    public float setDistance = 1f;

    [Header("[-=-=-[ Taunt Skill ]-=-=-]")]
    public LinearFloat successChance;

    public string tauntMessage = "You taunted: ";
    public string failedMessage = "You failed to taunt: ";

    public override bool CheckTarget(Entity caster)
    {
        // target exists, alive, not self, oktype?
        return caster.target != null && caster.CanAttack(caster.target);
    }

    public override bool CheckDistance(Entity caster, int skillLevel, out Vector3 destination)
    {
        // target still around?
        if (caster.target != null)
        {
            destination = caster.target.collider.ClosestPoint(caster.transform.position);
            return Utils.ClosestDistance(caster, caster.target) <= castRange.Get(skillLevel);
        }
        destination = caster.transform.position;
        return false;
    }

    public override void Apply(Entity caster, int skillLevel)
    {
        Player player = (Player)caster;

        if (caster.target != null)
        {
            if (caster.CanAttack(caster.target))
            {
                var targetPos = caster.transform.position;
                var distance = Vector3.Distance(caster.target.transform.position, targetPos);
                var newPosition = Vector3.MoveTowards(caster.target.transform.position, targetPos, distance - setDistance);

                // Raycast to find a valid position above the ground
                RaycastHit hit;
                if (Physics.Raycast(newPosition + Vector3.up * 0.5f, Vector3.down, out hit, 2f)) // Adjust the raycast height and distance as needed
                {
                    newPosition = hit.point + Vector3.up * 0.5f; // Set the position to the ground point plus a small offset
                }

                // Move the target to the new position above ground
                caster.target.movement.Warp(newPosition);

                // Deal damage directly with base damage + skill damage
                caster.combat.DealDamageAt(caster.target,
                    caster.combat.damage + damage.Get(skillLevel),
                    stunChance.Get(skillLevel),
                    stunTime.Get(skillLevel));

                if (player.target != null && player.target is Monster)
                {
                    Monster monster = player.target.GetComponent<Monster>();

                    if (UnityEngine.Random.value <= successChance.Get(skillLevel))
                    {
                        monster.Tools_OnAggro(player, 1);
                        player.Tools_TargetAddMessage(tauntMessage + monster.name);
                    }
                    else
                    {
                        player.Tools_TargetAddMessage(failedMessage + monster.name);
                    }
                }
            }
        }
    }
}