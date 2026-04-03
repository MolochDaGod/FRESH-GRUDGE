using UnityEngine;

public class NpcMail : NpcOffer
{
    public Npc npc;

    [Header("[-=-[ Mail ]-=-]")]
    public bool offersMailRead = true;

    public string buttonName = "Mail";

    public override bool HasOffer(Player player) => offersMailRead;

    public override string GetOfferName() => buttonName;

    public override void OnSelect(Player player)
    {
            UI_ReadMail.singleton.panel.SetActive(true);
            UIInventory.singleton.panel.SetActive(true); // better feedback
            UINpcDialogue.singleton.panel.SetActive(false);
    }
}