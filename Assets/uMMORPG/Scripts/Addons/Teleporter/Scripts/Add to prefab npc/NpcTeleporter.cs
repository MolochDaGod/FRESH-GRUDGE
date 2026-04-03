using UnityEngine;

// NPC
public class NpcTeleporter : NpcOffer
{
    public Npc npc;
    public GameEvent uiTeleportationEvent;

    [Header("[-=-[ Teleporter ]-=-]")]
    public TeleportationDestination[] teleportationDestinations;

    public string buttonName = "Teleporter";

    public bool offersPlayerTeleporter = false;

    public override bool HasOffer(Player player) => offersPlayerTeleporter;

    public override string GetOfferName() => buttonName;

    public override void OnSelect(Player player)
    {
#if _iMMOTELEPORTER
        uiTeleportationEvent.TriggerEvent();
        UIInventory.singleton.panel.SetActive(true); // better feedback
        UINpcDialogue.singleton.panel.SetActive(false);
#endif
    }

}