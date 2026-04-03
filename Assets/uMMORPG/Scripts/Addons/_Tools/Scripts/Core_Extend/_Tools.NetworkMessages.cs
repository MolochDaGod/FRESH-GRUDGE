#if _iMMOTOOLS
using System.Collections.Generic;
using Mirror;
public partial struct CharacterCreateMsg : NetworkMessage
{
#if _iMMOUMACHARACTERS
    public string dna;
#endif
}

public partial class CharactersAvailableMsg
{
#pragma warning disable CS0282
    public partial class CharacterPreview
    {
#if _iMMOUMACHARACTERS
        public string umaDna;
#endif

#if _iMMOSCENELOADER || _iMMOLOBBY
        public string startingScene;
#endif
    }
#pragma warning restore CS0282

    private void Load_Tools(List<Player> players)
    {
        for (int i = 0; i < players.Count; ++i)
        {
#if _iMMOUMACHARACTERS
            if (players[i].playerAddonsConfigurator.tmpl_UMACharacterCreation)
                characters[i].umaDna = players[i].playerAddonsConfigurator.umaDna;
#endif

#if _iMMOLOBBY
            if(players[i].playerNetworkLobby)
                characters[i].startingScene = (players[i].playerNetworkLobby.currentScene.SceneName != null) ? players[i].playerNetworkLobby.currentScene.SceneName : players[i].playerNetworkLobby.startingScene.SceneName;
#endif
        }
    }
}
#endif