using System.Text;
using UnityEngine;
using Mirror;

// TARGET PROJECTILE AMMO SKILL
[CreateAssetMenu(menuName = "ADDON/Skills/Target/Ammo Projectile", order = 998)]
public class Skill_TargetProjectileAmmo : TargetProjectileSkill
{
    [Header("[-=-[ Ammunition ]-=-]")]
    [Tooltip("[Required] Having ANY of these weapons equipped is enough for the skill to be useable")]
    public ScriptableItem[] equippedWeapons;

    [Tooltip("[Required] The ammunition must be in the players inventory")]
    public ScriptableItem requiredAmmo;

    [Tooltip("[Required] How much ammo is deducted per skill use?")]
    public int ammoAmount;

    // -----------------------------------------------------------------------------------
    // CheckTarget
    // -----------------------------------------------------------------------------------
    public override bool CheckTarget(Entity caster)
    {
        bool valid = true;

        if (caster.target == null && !caster.CanAttack(caster.target))
            return false;

        Player player = caster.GetComponent<Player>();

        if (player != null)
        {
            if (requiredAmmo && player.inventory.Count(new Item(requiredAmmo)) < ammoAmount)
                return false;

            valid = false;

            foreach (ScriptableItem equippedWeapon in equippedWeapons)
            {
                if (equippedWeapon && player.Ammunition_checkHasEquipment(equippedWeapon))
                {
                    valid = true;
                    break;
                }
            }
        }

        return valid;
    }

    // -----------------------------------------------------------------------------------
    // CheckDistance
    // -----------------------------------------------------------------------------------
#if _iMMO2D
    public override bool CheckDistance(Entity caster, int skillLevel, out Vector2 destination)
#else
    public override bool CheckDistance(Entity caster, int skillLevel, out Vector3 destination)
#endif
    {
        if (caster.target != null)
        {
            destination = caster.target.collider.ClosestPointOnBounds(caster.transform.position);
            return Utils.ClosestDistance(caster, caster.target) <= castRange.Get(skillLevel);
        }
        destination = caster.transform.position;
        return false;
    }

#if _SERVER
    // -----------------------------------------------------------------------------------
    // Apply
    // -----------------------------------------------------------------------------------
#if _iMMO2D
    public override void Apply(Entity caster, int skillLevel, Vector2 target)
#else
    public override void Apply(Entity caster, int skillLevel)
#endif
    {
        if (projectile != null)
        {
            // ---- Deduct Ammunition
            Player player = caster.GetComponent<Player>();
            if (player)
            {
                if (player.inventory.Count(new Item(requiredAmmo)) >= ammoAmount)
                    player.inventory.Remove(new Item(requiredAmmo), ammoAmount);
            }

            GameObject go = Instantiate(projectile.gameObject, caster.skills.effectMount.position, caster.skills.effectMount.rotation);
#if _iMMO2D
            TargetProjectileSkillEffect effect = go.GetComponent<TargetProjectileSkillEffect>();
#else
            ProjectileSkillEffect effect = go.GetComponent<ProjectileSkillEffect>();
#endif
            effect.target = caster.target;
            effect.caster = caster;
            effect.damage = damage.Get(skillLevel);
            NetworkServer.Spawn(go);
        }
        else Debug.LogWarning(name + ": missing projectile");
    }
#endif
    // -----------------------------------------------------------------------------------
    // ToolTip
    // -----------------------------------------------------------------------------------
    public override string ToolTip(int skillLevel, bool showRequirements = false)
    {
        StringBuilder tip = new StringBuilder(base.ToolTip(skillLevel, showRequirements));
        tip.Replace("{DAMAGE}", damage.Get(skillLevel).ToString());

        string s = "";
        if (equippedWeapons.Length > 0)
        {
            s += "Allowed Weapon(s): \n";
            foreach (ScriptableItem equippedWeapon in equippedWeapons)
            {
                if (equippedWeapon)
                    s += "* " + equippedWeapon.name + "\n";
            }
        }
        tip.Replace("{EQUIPPEDWEAPON}", s);

        if (requiredAmmo)
        {
            tip.Replace("{REQUIREDAMMO}", "Required Ammunition: \n" + requiredAmmo.name + "[x" + ammoAmount + "]\n");
        }
        else
        {
            tip.Replace("{REQUIREDAMMO}", "");
        }

        return tip.ToString();
    }

    // -----------------------------------------------------------------------------------
}
