using UnityEngine;

// NPC
public class NpcBindpoint : NpcOffer
{
    public Npc npc;

    [Header("[-=-[ Bindpoint ]-=-]")]
    public bool offertBindpoint;

    public string buttonName = "Bindpoint here";

    public override bool HasOffer(Player player) => offertBindpoint;

    public override string GetOfferName() => buttonName;

    public override void OnSelect(Player player)
    {
        UI_BindpointPanel.singleton.panel.SetActive(true);
        //UIInventory.singleton.panel.SetActive(true); // better feedback
        UINpcDialogue.singleton.panel.SetActive(false);
    }
}