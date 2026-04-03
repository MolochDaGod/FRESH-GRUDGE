using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

// PLAYER
public class PlayerNetworkLobby : NetworkBehaviour
{
    public Player player;

    [Header("[-=-=-[ NETWORK LOBBY ]-=-=-]")]
    public ScriptableSceneList sceneList;
    public UnityScene startingScene;
    [HideInInspector] public UnityScene currentScene;


    public override void OnStartLocalPlayer()
    {
        if (SceneManager.GetActiveScene().name == sceneList.CharacterManager.unityScenes.SceneName || SceneManager.GetActiveScene().name == sceneList.LobbyScene.SceneName)
            Cmd_ToChangeSceneOnStartLocalPlayer();
    }

    [Command]
    public void Cmd_ToChangeSceneOnStartLocalPlayer()
    {
        Tools_UI_Tools.FadeOutScreen(false);
#if _SERVER
        string sceneStart = Database.singleton.GetCharacterScene(player.name);

        OnPlayerEnter(sceneStart);
#endif
    }

    [Command]
    public void Cmd_CharacterChange()
    {
        Tools_UI_Tools.FadeOutScreen(false);
#if _SERVER
        OnPlayerEnter(sceneList.CharacterManager.unityScenes.SceneName);
#endif
    }

#if _SERVER
    [Server]
    public void OnPlayerEnter(string targetScene)
    {
        //Move player to new position
        transform.position = player.transform.position;

        //Send 
        ConnectionSendSwitch(targetScene, false);
    }

    // -----------------------------------------------------------------------------------
    // OnPortal
    // @Server
    // -----------------------------------------------------------------------------------
    [Server]
    public void OnPortal(SceneLocation targetScene)
    {
        //Move player to new position
        transform.position = targetScene.position;

        //Send 
        ConnectionSendSwitch(targetScene.mapScene.SceneName);
    }


#if _iMMOBINDPOINT

    // -----------------------------------------------------------------------------------
    // OnPortal
    // @Server
    // -----------------------------------------------------------------------------------
    [Server]
    public void OnPortal(BindPoint bindpoint)
    {
        //Move player to position
        transform.position = bindpoint.position;

        //Send 
        ConnectionSendSwitch(bindpoint.SceneName);
    }

#endif

    [Server]
    private void ConnectionSendSwitch(string scene, bool save = true)
    {
        // save required or not
        if (save)
        {
            Database.singleton.CharacterSave(player, false);
            Database.singleton.SaveCharacterScene(name, scene);
        }

        // ask client to switch server
        player.connectionToClient.Send(
            new SwitchServerMsg
            {
                scenePath = scene,
                characterName = name
            }
        );

        // Destroy player 
        NetworkServer.Destroy(gameObject); 
    }
#endif
    // -----------------------------------------------------------------------------------
}
