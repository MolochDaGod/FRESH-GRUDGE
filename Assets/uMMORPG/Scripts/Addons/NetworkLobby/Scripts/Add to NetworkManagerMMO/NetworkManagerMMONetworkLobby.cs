using Mirror;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic; // Add this line
using UnityEngine;
using UnityEngine.SceneManagement;

// NETWORK MANAGER MMO
[RequireComponent(typeof(NetworkManager))]
[RequireComponent(typeof(kcp2k.KcpTransport))]
public partial class NetworkManagerMMONetworkLobby : MonoBehaviour
{
    public bool active = true;

    [Header("Component")]
    public NetworkManagerMMO networkManagerMMO;
    public kcp2k.KcpTransport transport;

    [Header("[-=-=-[ List Scene ]-=-=-]")]
    public ScriptableSceneList listSceneScriptable;

    [HideInInspector] public bool isSibling;

    public static string autoSelectCharacter = "";
    public static bool autoConnectClient;

    private void Awake()
    {
        if (!active) return;

        SceneManager.sceneLoaded += OnSceneLoaded;

#if _SERVER && !_CLIENT
        int sceneIndex = listSceneScriptable.serverScenes.ToList().FindIndex(x => x.unityScenes.SceneName == ParseSceneNameFromArgs());
        if (sceneIndex >= 0)
        {
            isSibling = true;
            string sceneName = listSceneScriptable.serverScenes[sceneIndex].unityScenes.SceneName;
            int networkPort = listSceneScriptable.serverScenes[sceneIndex].portServer;

            transport.Port = (ushort)(networkPort);

            print("[Zones] changing server scene: " + sceneName);
            networkManagerMMO.onlineScene = sceneName; // loads scene automatically

            print("[Zones] StartServer NetworkZone");
            networkManagerMMO.StartServer();
        }
        else
        {
            string sceneName = listSceneScriptable.CharacterManager.unityScenes.SceneName;
            int networkPort = listSceneScriptable.CharacterManager.portServer;

            transport.Port = (ushort)(networkPort);

            networkManagerMMO.onlineScene = sceneName; // loads scene automatically

            print("[Zones] StartServer NetworkZone");
            networkManagerMMO.StartServer();
        }
#endif
    }

    public void Start()
    {
        networkManagerMMO.onClientCharactersAvailable.AddListener(OnClientCharactersAvailable_NetworkLobby);
#if _SERVER
        networkManagerMMO.onServerCharacterCreate.AddListener(OnServerCharacterCreate_NetworkLobby);
#endif
        networkManagerMMO.onClientConnect.AddListener(OnClientConnect_NetworkLobby);
    }

    private void OnClientConnect_NetworkLobby(NetworkConnection conn)
    {
        if (!NetworkClient.ready && SceneManager.GetActiveScene().name == listSceneScriptable.LobbyScene.SceneName)
            OnClient_SwitchServerScene(new SwitchServerMsg { characterName = "", scenePath = listSceneScriptable.CharacterManager.unityScenes.SceneName });
        NetworkClient.ReplaceHandler<SwitchServerMsg>(OnClient_SwitchServerScene);
    }

    private void OnClientCharactersAvailable_NetworkLobby(NetworkManagerMMO.CharactersAvailableMsg message)
    {
        int index = message.characters.ToList().FindIndex(c => c.name == autoSelectCharacter);
        if (index != -1)
        {
#if UNITY_EDITOR
            print("[Zones]: autoselect " + autoSelectCharacter + "(" + index + ")");
#endif
            byte[] extra = BitConverter.GetBytes(index);
            if (!NetworkClient.ready)
                NetworkClient.Ready();
            NetworkClient.AddPlayer();
            NetworkClient.Send(new CharacterSelectMsg { index = index });

            autoSelectCharacter = "";
            UIPopup.singleton.gameObject.SetActive(false);
            networkManagerMMO.ClearPreviews();
        }
    }

    public void OnClient_SwitchServerScene(SwitchServerMsg message)
    {
#if UNITY_EDITOR
        print("OnClient_SwitchScene : " + message.scenePath);
#endif
        if (!NetworkServer.active)
        {
            Tools_UI_Tools.FadeOutScreen(false, 1f);

            networkManagerMMO.StopClient();

            string sceneName = Path.GetFileNameWithoutExtension(message.scenePath);
            int sceneIndex = listSceneScriptable.serverScenes.ToList().FindIndex(x => x.unityScenes.SceneName == sceneName);

            networkManagerMMO.networkAddress = (sceneIndex == -1) ? listSceneScriptable.CharacterManager.ipServer : listSceneScriptable.serverScenes[sceneIndex].ipServer;
            networkManagerMMO.switching = NetworkSwitch.Yes;
#if UNITY_EDITOR
            Debug.Log("scene IP :" + listSceneScriptable.CharacterManager.ipServer);
#endif

            autoSelectCharacter = message.characterName;

            ServerSceneLoader.Instance.LoadScene(
                sceneName,
                (sceneIndex == -1) ? listSceneScriptable.CharacterManager.background : listSceneScriptable.serverScenes[sceneIndex].background,
                listSceneScriptable.displayTips[UnityEngine.Random.Range(0, listSceneScriptable.displayTips.Length - 1)]
            );

            autoConnectClient = true;
        }
    }

#if _SERVER
    private void OnServerCharacterCreate_NetworkLobby(CharacterCreateMsg message, Player player)
    {
        if (player.playerNetworkLobby.startingScene == null)
            Debug.LogError("Starting Scene in prefab " + player.gameObject.name + " is empty !");
        Database.singleton.SaveCharacterScene(player.name, player.playerNetworkLobby.startingScene.SceneName);
    }
#endif

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name);

        if (autoConnectClient)
        {
            networkManagerMMO.switching = NetworkSwitch.No;

            int sceneIndex = listSceneScriptable.serverScenes.ToList().FindIndex(x => x.unityScenes.SceneName == scene.name);
            transport.Port = (sceneIndex == -1) ? (ushort)listSceneScriptable.CharacterManager.portServer : (ushort)(listSceneScriptable.serverScenes[sceneIndex].portServer);
            networkManagerMMO.networkAddress = (sceneIndex == -1) ? listSceneScriptable.CharacterManager.ipServer : listSceneScriptable.serverScenes[sceneIndex].ipServer;

            if (sceneIndex == -1)
                UIPopup.singleton.panel.SetActive(false);
            print("[Zones]: automatically connecting client to new server at ip: " + networkManagerMMO.networkAddress + ", port: " + transport.Port);
            WebGLWssBootstrap.ApplyRuntimeOverrides(networkManagerMMO);
            networkManagerMMO.StartClient();
            autoConnectClient = false;
        }
    }

    public string ParseSceneNameFromArgs()
    {
        String[] args = System.Environment.GetCommandLineArgs();
        if (args != null)
        {
            int index = args.ToList().FindIndex(arg => arg == "-scene");
            return 0 <= index && index < args.Length - 1 ? args[index + 1] : "ParseSceneNameFromArgs() error";
        }
        return "ParseSceneNameFromArgs() error";
    }

    public CharacterPreview[] characters;

    public class CharacterPreview
    {
        public string name;
        public string className;
        public ItemSlot[] equipment;
#if _iMMOUMACHARACTERS
        public string umaDna;
#endif
    }

    public void Load(List<Player> players)
    {
        characters = new CharacterPreview[players.Count];
        for (int i = 0; i < players.Count; ++i)
        {
            Player player = players[i];
            characters[i] = new CharacterPreview
            {
                name = player.name,
                className = player.className,
                equipment = new List<ItemSlot>(player.equipment.slots).ToArray(),
#if _iMMOUMACHARACTERS
                umaDna = player.playerAddonsConfigurator.umaDna
#endif
            };
        }
    }
}
