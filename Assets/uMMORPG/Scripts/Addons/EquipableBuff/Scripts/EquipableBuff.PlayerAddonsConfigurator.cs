using Mirror;
using System.Collections.Generic;
using System.Linq;

// PLAYER
public partial class PlayerAddonsConfigurator
{

    protected List<TargetBuffSkill> equipmentBuffs = new List<TargetBuffSkill>();

    // -----------------------------------------------------------------------------------
    // OnStartLocalPlayer
    // -----------------------------------------------------------------------------------
#if _SERVER
    public void OnStartServer_EquipableBuff()
    {
        base.OnStartServer();
        combat.onDamageDealtTo.AddListener(OnDealDamageAt_EquipableBuff);
        health.onEmpty.AddListener(EquipmentChanged);
    }
#endif

    public void OnStartLocalPlayer_EquipableBuff()
    {
        playerEquipment.onEquipmentChanged.AddListener(OnEquipmentChanged_EquipableBuff);
        combat.onClientReceivedDamage.AddListener(OnDamageDealt_EquipableBuff);
        equipmentBuffs = new List<TargetBuffSkill>();
        OnEquipmentChanged_EquipableBuff();
    }

    private void OnDamageDealt_EquipableBuff(int amount, DamageType damageType)
    {
        OnDamageDealt(amount, damageType);
    }

    private void OnEquipmentChanged_EquipableBuff()
    {
        // -- remove buffs
        Cmd_OnEquipmentChanged();
    }

    // -----------------------------------------------------------------------------------
    // OnDamageDealt_EquipableBuff
    // -----------------------------------------------------------------------------------

    [Command]
    private void OnDamageDealt(int amount, DamageType damageType)
    {
#if _SERVER
        _OnDamageDealt(amount, damageType);
#endif
    }
 
    // -----------------------------------------------------------------------------------
    // OnEquipmentChanged_EquipableBuff
    // -----------------------------------------------------------------------------------

    [Command]
    private void Cmd_OnEquipmentChanged()
    {
#if _SERVER
        EquipmentChanged();
#endif
    }

#if _SERVER
    // -----------------------------------------------------------------------------------
    // DealDamageAt_EquipableBuff
    // -----------------------------------------------------------------------------------
    [Server]
    private void OnDealDamageAt_EquipableBuff(Entity entity)
    {
        if (entity == null) return;

        for (int i = 0; i < player.equipment.slots.Count; ++i)
        {
            ItemSlot slot = player.equipment.slots[i];

            if (slot.amount > 0)
            {
                EquipmentItem itemData = (EquipmentItem)slot.item.data;

                if (itemData != null && itemData.onAttackBuff != null && itemData.onAttackBuffApplyChance > 0)
                    itemData.ApplyOnAttackBuffTarget(player, entity);
            }
        }
    }

    [Server]
    private void _OnDamageDealt(int amount, DamageType damageType)
    {
        for (int i = 0; i < player.equipment.slots.Count; ++i)
        {
            ItemSlot slot = player.equipment.slots[i];

            if (slot.amount > 0)
            {
                EquipmentItem itemData = (EquipmentItem)slot.item.data;

                if (itemData != null && itemData.onHitBuffSelf != null && itemData.onHitBuffSelfApplyChance > 0 && amount > 0)
                    itemData.ApplyOnHitBuffSelf(player);
            }
        }
    }

    [Server]
    private void EquipmentChanged()
    {
        for (int i = 0; i < player.skills.buffs.Count; ++i)
        {
            if (equipmentBuffs.Any(x => x == player.skills.buffs[i].data))
            {
                player.skills.buffs.RemoveAt(i);
                --i;
            }
        }

        // -- clear list
        equipmentBuffs.Clear();

        // -- apply buffs & add to list again
        for (int i = 0; i < player.equipment.slots.Count; ++i)
        {
            ItemSlot slot = player.equipment.slots[i];

            if (slot.amount > 0)
            {
                EquipmentItem itemData = (EquipmentItem)slot.item.data;

                if (itemData != null && itemData.onEquipBuffSelf != null)
                {
                    itemData.ApplyEquipableBuff(player);
                    equipmentBuffs.Add(itemData.onEquipBuffSelf);
                }
            }
        }
    }
#endif
    // -----------------------------------------------------------------------------------
}