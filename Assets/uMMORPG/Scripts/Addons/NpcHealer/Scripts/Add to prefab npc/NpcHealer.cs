using UnityEngine;

public class NpcHealer : NpcOffer
{
    public Npc npc;

    [Header("[-=-=-[ NPC HEALER ]-=-=-]")]
    public HealingServices healingServices;


    public string buttonName = "Heal Me";

    [Header("[-=-=-[ Offert Healer ]-=-=-]")]
    [Tooltip("One click deactivation")]
    public bool offersHealer;

    public override bool HasOffer(Player player) => offersHealer;

    public override string GetOfferName() => buttonName;

    public override void OnSelect(Player player)
    {
        UI_Healer.singleton.panel.SetActive(true);
        UIInventory.singleton.panel.SetActive(true); // better feedback
        UINpcDialogue.singleton.panel.SetActive(false);
    }
}