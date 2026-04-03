using UnityEngine;

// PLAYER WAREHOUSE - NPC
public class NpcWarehouse : NpcOffer
{
    public Npc npc;
    public string buttonName = "Warehouse";

    [Header("[-=-[ PLAYER WAREHOUSE ]-=-]")]
    public bool offersPlayerWarehouse = false;

    public GameEvent uiPlayerWarehouse;

    public override bool HasOffer(Player player) => offersPlayerWarehouse;

    public override string GetOfferName() => buttonName;

    public override void OnSelect(Player player)
    {
        uiPlayerWarehouse.TriggerEvent();
        UIInventory.singleton.panel.SetActive(true); // better feedback
        UINpcDialogue.singleton.panel.SetActive(false);
    }
}