#if _SQLITE
using UnityEngine;

public class RAHR_Npc : NpcOffer {
    public override bool HasOffer(Player player) => true;

    public override string GetOfferName() => "Auction House";

    public override void OnSelect(Player player) {
        RAHR_UI_AuctionHouse.singleton.main_Panel.SetActive(true);
        UIInventory.singleton.panel.SetActive(true);
        UINpcDialogue.singleton.panel.SetActive(false);
    }
}
#endif
