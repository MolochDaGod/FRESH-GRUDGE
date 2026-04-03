using UnityEngine;

public class NpcGuildWarehouse : NpcOffer
{
    public Npc npc;

    [Header("[-=-[ Guild Warehouse ]-=-]")]
    public bool offersGuildWarehouse = false;
    
    public string buttonName = "Guild Warehouse";

    public bool accessViceOrMasterOnly;

    public GameEvent uiGuildWarehouse;

    // -----------------------------------------------------------------------------------
    // checkWarehouseAccess
    // -----------------------------------------------------------------------------------
    public bool checkWarehouseAccess(Player player)
    {
        if (!offersGuildWarehouse || !player.guild.InGuild()) return false;
        if (!accessViceOrMasterOnly) return true;
        return player.guild.guild.CanNotify(player.name);
    }

    public override bool HasOffer(Player player) => checkWarehouseAccess(player);

    public override string GetOfferName() => buttonName;

    public override void OnSelect(Player player)
    {
        uiGuildWarehouse.TriggerEvent();
        UIInventory.singleton.panel.SetActive(true); // better feedback
        UINpcDialogue.singleton.panel.SetActive(false);
    }
}