using Mirror;
using System.Linq;
using UnityEngine;

// PLAYER

public partial class PlayerWeightSystem : NetworkBehaviour
{
    public Player player;
    public Inventory inventory;
    public PlayerEquipment playerEquipment;
    public PlayerInventory playerInventory;

    [Header("[-=-=-[ WEIGHT SYSTEM ] -=-=-]")]
    public WeightSystem weightSystem;

    protected int totalWeight;
    protected int maxWeight;

    public override void OnStartLocalPlayer()
    {
        if (playerEquipment != null && playerInventory != null)
        {
            playerEquipment.onEquipmentChanged.AddListener(Cmd_CalculateWeight);
            playerInventory.onInventoryChanged.AddListener(Cmd_CalculateWeight);
            Cmd_CalculateWeight();
        }
        else
        {
            Debug.LogWarning("PlayerEquipment and/or PlayerInnventory in component PlayerWeightSystem is not assigned in " + player.className + " prefab !");
        }
    }

    [Command]
    public void Cmd_CalculateWeight()
    {
        UpdateEvent();
    }

    // -----------------------------------------------------------------------------------
    // UpdateEvent
    // @Server
    // -----------------------------------------------------------------------------------
    [ServerCallback]
    public void UpdateEvent()
    {
#if _SERVER
        if (weightSystem.burdenedBuff == null) return;
        {
            // -- calculate weight
            CalculateWeight();

            // -- check burdened
            int burdenLevel = IsBurdened();

            // -- apply or remove burdened
            if (burdenLevel > 0)
                player.skills.AddOrRefreshBuff(new Buff(weightSystem.burdenedBuff, burdenLevel));
            else
                player.Tools_RemoveBuff(weightSystem.burdenedBuff);
        }
#endif 
    }

#if _SERVER
    // -----------------------------------------------------------------------------------
    // CalculateWeight
    // -----------------------------------------------------------------------------------

    protected void CalculateWeight()
    {
        totalWeight = 0;

#if _iMMOATTRIBUTES
        if (weightSystem.weightAttribute != null)
        {
            // Linq is HEAVY on GC and performance
            foreach (TrugordAttribute attrib in player.playerAttribute.Attributes)
            {
                if(attrib.name == weightSystem.weightAttribute.name)
                {
                    maxWeight = ((weightSystem.maxWeight + (attrib.points + player.playerAttribute.calculateBonusAttribute(attrib))) * weightSystem.carryPerPoint);
                    break;
                }
            }        
        }
#else
        maxWeight = (weightSystem.maxWeight * weightSystem.carryPerPoint);
#endif

        for (int i = 0; i < inventory.slots.Count; ++i)
        {
            ItemSlot slot = inventory.slots[i];
            if (slot.amount > 0)
                totalWeight += slot.item.data.weight * slot.amount;
        }
        if (!weightSystem.equipmentNotIncluded)
        {
            for (int i = 0; i < player.equipment.slots.Count; ++i)
            {
                ItemSlot slot = player.equipment.slots[i];
                if (slot.amount > 0)
                    totalWeight += slot.item.data.weight * slot.amount;
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // IsBurdened
    // -----------------------------------------------------------------------------------
    protected int IsBurdened()
    {
        if (totalWeight <= maxWeight)
            return 0;
        else
            return Mathf.Min((int)totalWeight / maxWeight, weightSystem.maxBurdenLevel);
    }
#endif
    // -----------------------------------------------------------------------------------
}
