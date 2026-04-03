using UnityEngine;

public partial class NetworkManagerMMOAttributes : MonoBehaviour
{

    public NetworkManagerMMO networkManagerMMO;
#if _SERVER

    public void Start()
    {
        networkManagerMMO.onServerCharacterCreate.AddListener(OnServerCharacterCreate_Attributes);
    }
    private void OnServerCharacterCreate_Attributes(CharacterCreateMsg message, Player player)
    {
        //Debug.Log("OnServerCharacterCreate_Tools_Attributes");
        // -- this is to make sure the maximum value is calculated before loading to the player
        //player.health.current = player.health.max;
        //player.mana.current = player.mana.max;

#if _iMMOSTAMINA
        player.stamina.current  = player.stamina.max;
#endif


    }
#endif
}