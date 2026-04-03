using UnityEngine;

public class NetworkManagerMMOHarvesting : MonoBehaviour
{
    public NetworkManagerMMO networkManagerMMO;

#if _SERVER
    public void Start()
    {
        networkManagerMMO.onServerCharacterCreate.AddListener(OnServerCharacterCreate_Harvesting);
    }

    private void OnServerCharacterCreate_Harvesting(CharacterCreateMsg message, Player player)
    {
#if _iMMOHARVESTING
        if(player.playerHarvesting)
            // -- check starting profession
            foreach (HarvestingProfessionTemplate profession in player.playerHarvesting.startingProfession)
            {
                HarvestingProfession tmpProf = new HarvestingProfession(profession.name);
                player.playerHarvesting.Professions.Add(tmpProf);
            }
#endif
    }
#endif
}