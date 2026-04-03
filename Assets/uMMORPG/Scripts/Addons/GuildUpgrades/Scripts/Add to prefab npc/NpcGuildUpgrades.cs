using UnityEngine;

// GUILD UPGRADES - NPC
public class NpcGuildUpgrades : NpcOffer
{
    public Npc npc;

    [Header("[-=-[ GUILD UPGRADES ]-=-]")]
    public bool offersGuildUpgrade = false;

    public string buttonName = "Guil Upgrade";

    [Header("[-=-[ Event Guild Upgrade ]-=-]")]
    public GameEvent guildUpgradeEvent;

    public override bool HasOffer(Player player) => ((CheckGuildUpgradeAccess(player) && offersGuildUpgrade) && offersGuildUpgrade);

    public override string GetOfferName() => buttonName;

    public override void OnSelect(Player player)
    {
        if (CheckGuildUpgradeAccess(player))
        {
            guildUpgradeEvent.TriggerEvent();
            UIInventory.singleton.panel.SetActive(true); // better feedback
            UINpcDialogue.singleton.panel.SetActive(false);
        }
    }

    // -----------------------------------------------------------------------------------
    // CheckGuildUpgradeAccess
    // -----------------------------------------------------------------------------------
    public bool CheckGuildUpgradeAccess(Player player)
    {
        if (!offersGuildUpgrade || !player.guild.InGuild()) return false;

        return player.guild.guild.CanNotify(player.name);
    }

    // -----------------------------------------------------------------------------------
}