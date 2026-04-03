using UnityEngine;

public partial class Npc
{
    [Header("[Component : Npc Extended Quests]")]
    public NpcExtendedQuests extendedQuests;

    public void CheckAutoQuest()
    {
        Player player = Player.localPlayer;
        if (!player) return;
        
        if (player.target != null && player.target is Npc npc && Utils.ClosestDistance(player, player.target) <= player.interactionRange)
        {
            player.playerExtendedQuest.IncreaseQuestNpcCounterFor(npc);
        }
    }
}