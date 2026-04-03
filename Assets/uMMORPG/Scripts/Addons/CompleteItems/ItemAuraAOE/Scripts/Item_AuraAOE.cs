using Mirror;
using System.Collections.Generic;
using UnityEngine;

// ITEM TARGETTED AOE

[CreateAssetMenu(menuName = "ADDON/Item/Item Aura AOE", order = 999)]
public class Item_AuraAOE : UsableItem
{
    [Header("[-=-[ Target AOE Item ]-=-]")]
    public OneTimeTargetSkillEffect effect;

    public LinearInt damage = new LinearInt { baseValue = 1 };

    [Tooltip("[Optional] Add caster damage to total damage or not?")]
    public bool useCasterDamage;

#if _iMMOATTRIBUTES

    [Tooltip("[Optional] Add caster accuracy to the buff chance?")]
    public bool useCasterAccuracy;

#endif
    public LinearFloat stunChance; // range [0,1]
    public LinearFloat stunTime; // in seconds
    public int skillLevel;
    public float radius;
    [Range(0, 1)] public float triggerAggroChance;
    public string successMessage = "You damaged: ";

    [Header("[-=-[ Apply Buff on Target ]-=-]")]
    public BuffSkill applyBuff;

    public int buffLevel;
    [Range(0, 1)] public float buffChance;

    [Header("[-=-[ Targeting ]-=-]")]
    [Tooltip("[Optional] Changes 'affect' affect into 'not affect' and vice-versa")]
    public bool reverseTargeting;

    [Tooltip("[Optional] Does affect the caster")]
    public bool affectSelf;

    [Tooltip("[Optional] Does affect members of the own party")]
    public bool affectOwnParty;

    [Tooltip("[Optional] Does affect members of the own guild")]
    public bool affectOwnGuild;

    [Tooltip("[Optional] Does affect members of the own realm (requires PVP ZONE AddOn")]
    public bool affectOwnRealm;

    public bool affectPlayers;
    public bool affectNpcs;
    public bool affectMonsters;
    public bool affectPets;

    [Tooltip("Decrease amount by how many each use (can be 0)?")]
    public int decreaseAmount = 1;

    // -----------------------------------------------------------------------------------
    // CheckTarget
    // -----------------------------------------------------------------------------------
    public bool CheckTarget(Entity caster)
    {
        return true;
    }

    // -----------------------------------------------------------------------------------
    // CheckDistance
    // -----------------------------------------------------------------------------------
    public bool CheckDistance(Entity caster, int skillLevel, out Vector3 destination)
    {
        destination = caster.transform.position;
        return true;
    }
#if _SERVER
    // -----------------------------------------------------------------------------------
    // Use
    // -----------------------------------------------------------------------------------
    public override void Use(Player player, int inventoryIndex)
    {
        List<Entity> targets = new List<Entity>();
        ItemSlot slot = player.inventory.slots[inventoryIndex];

        // -- Only activate if enough charges left
        if (decreaseAmount == 0 || slot.amount >= decreaseAmount)
        {
            // always call base function too
            base.Use(player, inventoryIndex);

            targets = player.Tools_GetCorrectedTargetsInSphere(player.transform, radius, false, affectSelf, affectOwnParty, affectOwnGuild, affectOwnRealm, reverseTargeting, affectPlayers, affectNpcs, affectMonsters, affectPets);

            foreach (Entity target in targets)
            {
                int dmg = damage.Get(skillLevel);

                if (useCasterDamage)
                    dmg += player.combat.damage;

                float buffModifier = 0;
#if _iMMOATTRIBUTES
                if (useCasterAccuracy) buffModifier = target.Tools_HarmonizeChance(buffModifier, player.combat.accuracy);
#endif

                // deal damage directly with base damage + skill damage
                player.combat.DealDamageAt(target,
                            dmg,
                            stunChance.Get(skillLevel),
                            stunTime.Get(skillLevel));

                target.Tools_ApplyBuff(applyBuff, buffLevel, buffChance, buffModifier);
                player.Tools_TargetAddMessage(successMessage + target.name);

                SpawnEffect(player, player.target);

                if (UnityEngine.Random.value <= triggerAggroChance)
                    target.target = player;
            }

            // decrease amount
            slot.DecreaseAmount(decreaseAmount);
            player.inventory.slots[inventoryIndex] = slot;
        }

        targets.Clear();
    }

    // -----------------------------------------------------------------------------------
    // SpawnEffect
    // -----------------------------------------------------------------------------------
    public void SpawnEffect(Entity caster, Entity spawnTarget)
    {
        if (effect != null)
        {
            GameObject go = Instantiate(effect.gameObject, spawnTarget.transform.position, Quaternion.identity);
            go.GetComponent<OneTimeTargetSkillEffect>().caster = caster;
            go.GetComponent<OneTimeTargetSkillEffect>().target = spawnTarget;
            NetworkServer.Spawn(go);
        }
    }
#endif
    // -----------------------------------------------------------------------------------
}
