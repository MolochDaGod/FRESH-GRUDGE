using UnityEngine;

// NPC
public class NpcHonorShop : NpcOffer
{
    public Npc npc;

    public string buttonName = "Honor Shop";

    [Header("[-=-=-[ Honor Shop ]-=-=-]")]
    [Tooltip("One click deactivation")]
    public bool offersShop;
    public HonorShopCategory[] itemShopCategories;

    public override bool HasOffer(Player player) => offersShop;

    public override string GetOfferName() => buttonName;

    public override void OnSelect(Player player)
    {
        UI_HonorShop.singleton.panel.SetActive(true);
        UIInventory.singleton.panel.SetActive(true); // better feedback
        UINpcDialogue.singleton.panel.SetActive(false);
    }
}