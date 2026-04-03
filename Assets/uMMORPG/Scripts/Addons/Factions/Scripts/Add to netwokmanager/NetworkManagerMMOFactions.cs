using UnityEngine;

public partial class NetworkManagerMMOFactions : MonoBehaviour
{
    public NetworkManagerMMO networkManagerMMO;
    // -----------------------------------------------------------------------------------
    // OnServerCharacterCreate_Tools_Factions
    // -----------------------------------------------------------------------------------
#if _SERVER
    public void Start()
    {
        networkManagerMMO.onServerCharacterCreate.AddListener(OnServerCharacterCreate_Factions);
    }
    private void OnServerCharacterCreate_Factions(CharacterCreateMsg message, Player player)
    {
        if(player.playerFactions)
            foreach (FactionRating faction in player.playerFactions.startingFactions)
                player.playerFactions.AddFactionRating(faction.faction, faction.startRating);
    }
#endif
    // -----------------------------------------------------------------------------------
}