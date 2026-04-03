// We use a custom NetworkManager that also takes care of login, character
// selection, character creation and more.
//
// We don't use the playerPrefab, instead all available player classes should be
// dragged into the spawnable objects property.
//
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

// we need a clearly defined state to know if we are offline/in world/in lobby
// otherwise UICharacterSelection etc. never know 100% if they should be visible
// or not.
public enum NetworkState { Offline, Handshake, Lobby, World }

[Serializable] public class UnityEventCharactersAvailableMsg : UnityEvent<NetworkManagerMMO.CharactersAvailableMsg> { }
[Serializable] public class UnityEventCharacterCreateMsgPlayer : UnityEvent<CharacterCreateMsg, Player> { }
[Serializable] public class UnityEventStringGameObjectNetworkConnectionCharacterSelectMsg : UnityEvent<string, GameObject, NetworkConnection, CharacterSelectMsg> { }
[Serializable] public class UnityEventCharacterDeleteMsg : UnityEvent<CharacterDeleteMsg> { }
[Serializable] public class UnityEventNetworkConnection : UnityEvent<NetworkConnection> { }

[RequireComponent(typeof(Database))]
[DisallowMultipleComponent]
public partial class NetworkManagerMMO : NetworkManager
{
    // current network manager state on client
    public NetworkState state = NetworkState.Offline;

    // <conn, account> dict for the lobby
    // (people that are still creating or selecting characters)
    public Dictionary<NetworkConnection, string> lobby = new Dictionary<NetworkConnection, string>();

    // UI components to avoid FindObjectOfType
    [Header("UI")]
    public UIPopup uiPopup;

    // we may want to add another game server if the first one gets too crowded.
    // the server list allows people to choose a server.
    //
    // note: we use one port for all servers, so that a headless server knows
    // which port to bind to. otherwise it would have to know which one to
    // choose from the list, which is far too complicated. one port for all
    // servers will do just fine for an Indie MMORPG.
    [Serializable]
    public class ServerInfo
    {
        public string name;
        public string ip;
    }

    public struct CharactersAvailableMsg : NetworkMessage
    {
        public CharacterPreview[] characters;

        public struct CharacterPreview
        {
            public string name;
            public int level;
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
                    level = player.level.current,
                    className = player.className,
                    equipment = new List<ItemSlot>(player.equipment.slots).ToArray(),
#if _iMMOUMACHARACTERS
                    umaDna = player.playerAddonsConfigurator.umaDna
#endif
                };
            }
        }
    }
    public List<ServerInfo> serverList = new List<ServerInfo>() {
        new ServerInfo{name="Local", ip="localhost"}
    };

    [Header("Logout")]
    [Tooltip("Players shouldn't be able to log out instantly to flee combat. There should be a delay.")]
    public float combatLogoutDelay = 5;

    [Header("Character Selection")]
    public int selection = -1;
    public Transform[] selectionLocations;
    public Transform selectionCameraLocation;
    [HideInInspector] public List<Player> playerClasses = new List<Player>(); // cached in Awake

    [Header("Database")]
    public int characterLimit = 4;
    public int characterNameMaxLength = 300;
    public float saveInterval = 60f; // in seconds

    // we still need OnStartClient/Server/etc. events for NetworkManager because
    // those are not regular NetworkBehaviour events that all components get.
    [Header("Events")]
    public UnityEvent onStartClient;
    public UnityEvent onStopClient;
    public UnityEvent onStartServer;
    public UnityEvent onStopServer;
    public UnityEventNetworkConnection onClientConnect;
    public UnityEventNetworkConnection onServerConnect;
    public UnityEventCharactersAvailableMsg onClientCharactersAvailable;
    public UnityEventCharacterCreateMsgPlayer onServerCharacterCreate;
    public UnityEventStringGameObjectNetworkConnectionCharacterSelectMsg onServerCharacterSelect;
    public UnityEventCharacterDeleteMsg onServerCharacterDelete;
    public UnityEventNetworkConnection onClientDisconnect;
    public UnityEventNetworkConnection onServerDisconnect;

    // store characters available message on client so that UI can access it
    [HideInInspector] public CharactersAvailableMsg charactersAvailableMsg;

    // name checks /////////////////////////////////////////////////////////////
#if !_iMMOWORDFILTER
    public virtual bool IsAllowedCharacterName(string characterName)
    {
        // not too long?
        // only contains letters, number and underscore and not empty (+)?
        // (important for database safety etc.)
        return characterName.Length <= characterNameMaxLength &&
               Regex.IsMatch(characterName, @"^[a-zA-Z0-9_]+$");
    }
#endif

    // nearest startposition ///////////////////////////////////////////////////
#if _iMMO2D
    public static Transform GetNearestStartPosition(Vector2 from) =>
    Utils.GetNearestTransform(startPositions, from);
#else
    public static Transform GetNearestStartPosition(Vector3 from) =>
        Utils.GetNearestTransform(startPositions, from);
#endif

    // player classes //////////////////////////////////////////////////////////
    public List<Player> FindPlayerClasses()
    {
        // filter out all player prefabs from spawnPrefabs
        // (avoid Linq for performance/gc. players are spawned a lot. it matters.)
        List<Player> classes = new List<Player>();
        foreach (GameObject prefab in spawnPrefabs)
        {
            Player player = prefab.GetComponent<Player>();
            if (player != null)
                classes.Add(player);
        }
        return classes;
    }

    // events //////////////////////////////////////////////////////////////////
    public override void Awake()
    {
        base.Awake();

        // cache list of player classes from spawn prefabs.
        // => we assume that this won't be changed at runtime (why would it?)
        // => this is way better than looping all prefabs in character
        //    select/create/delete each TimeLogout!
        playerClasses = FindPlayerClasses();
    }
    public float updateInterval = 0.5F;
    private double lastInterval;
    private int frames = 0;
    private float fps;
    float currentTime = 0.0f;

    public override void Update()
    {
        base.Update();

        // any valid local player? then set state to world
        if (NetworkClient.localPlayer != null)
            state = NetworkState.World;
        if (autoStartServerBuild)
        {
            ++frames;
            float timeNow = Time.realtimeSinceStartup;
            if (timeNow > lastInterval + updateInterval)
            {
                fps = (float)(frames / (timeNow - lastInterval));
#if UNITY_STANDALONE_WIN
                Console.Title = fps.ToString();
#endif
                frames = 0;
                lastInterval = timeNow;
            }
        }
    }

    // error messages //////////////////////////////////////////////////////////
    public void ServerSendError(NetworkConnection conn, string error, bool disconnect)
    {
        conn.Send(new ErrorMsg { text = error, causesDisconnect = disconnect });
    }

    void OnClientError(ErrorMsg message)
    {
        Debug.Log("OnClientError: " + message.text);

        // show a popup
        uiPopup.Show(message.text);

        // disconnect if it was an important network error
        // (this is needed because the login failure message doesn't disconnect
        //  the client immediately (only after timeout))
        if (message.causesDisconnect)
        {
            NetworkClient.connection.Disconnect();

            // also stop the host if running as host
            // (host shouldn't start server but disconnect client for invalid
            //  login, which would be pointless)
            if (NetworkServer.active) StopHost();
        }
    }

    // start & stop ////////////////////////////////////////////////////////////
    public override void OnStartClient()
    {
        // setup handlers
        NetworkClient.RegisterHandler<ErrorMsg>(OnClientError, false); // allowed before auth!
        NetworkClient.RegisterHandler<CharactersAvailableMsg>(OnClientCharactersAvailable);

        // addon system hooks
        onStartClient.Invoke();
    }

    public override void OnStartServer()
    {
        // connect to database
        Database.singleton.Connect();

        // handshake packet handlers (in OnStartServer so that reconnecting works)
        NetworkServer.RegisterHandler<CharacterCreateMsg>(OnServerCharacterCreate);
        NetworkServer.RegisterHandler<CharacterSelectMsg>(OnServerCharacterSelect);
        NetworkServer.RegisterHandler<CharacterDeleteMsg>(OnServerCharacterDelete);

        //load all player generated structures
        //(after base.OnStartServer so that we can call NetworkServer.Spawn)
        Database.singleton.LoadStructures();

        // invoke saving
        InvokeRepeating(nameof(SavePlayers), saveInterval, saveInterval);

        // addon system hooks
        onStartServer.Invoke();
    }

    public override void OnStopClient()
    {
        // addon system hooks
        onStopClient.Invoke();
    }

    public override void OnStopServer()
    {
        CancelInvoke(nameof(SavePlayers));

        // addon system hooks
        onStopServer.Invoke();
    }

    // handshake: login ////////////////////////////////////////////////////////
    // called on the client if a client connects after successful auth
    public override void OnClientConnect()
    {
        // addon system hooks
        onClientConnect.Invoke(NetworkClient.connection);

        // do NOT call base function, otherwise client becomes "ready".
        // => it should only be "ready" after selecting a character. otherwise
        //    it might receive world messages from monsters etc. already
        //base.OnClientConnect(conn);
    }

    // called on the server if a client connects after successful auth
    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        // grab the account from the lobby
        string account = lobby[conn];

        // send necessary data to client
        conn.Send(MakeCharactersAvailableMessage(account));

        // addon system hooks
        onServerConnect.Invoke(conn);
    }

    // the default OnClientSceneChanged sets the client as ready automatically,
    // which makes no sense for MMORPG situations. this was more for situations
    // where the server tells all clients to load a new scene.
    // -> setting client as ready will cause 'already set as ready' errors if
    //    we call StartClient before loading a new scene (e.g. for zones)
    // -> it's best to just overwrite this with an empty function
    public override void OnClientSceneChanged() { }

    // helper function to make a CharactersAvailableMsg from all characters in
    // an account
    CharactersAvailableMsg MakeCharactersAvailableMessage(string account)
    {
        // load from database
        // (avoid Linq for performance/gc. characters are loaded frequently!)
        List<Player> characters = new List<Player>();
        foreach (string characterName in Database.singleton.CharactersForAccount(account))
        {
            GameObject player = Database.singleton.CharacterLoad(characterName, playerClasses, true);
            if (player == null)
            {
                Debug.LogWarning("Skipping character preview because CharacterLoad failed for: " + characterName);
                continue;
            }

            Player previewPlayer = player.GetComponent<Player>();
            if (previewPlayer == null)
            {
                Debug.LogWarning("Skipping character preview because prefab has no Player component: " + characterName);
                Destroy(player);
                continue;
            }

            characters.Add(previewPlayer);
        }

        // construct the message
        CharactersAvailableMsg message = new CharactersAvailableMsg();
        message.Load(characters);


        // destroy the temporary players again and return the result
        characters.ForEach(player => Destroy(player.gameObject));
        return message;
    }

    // handshake: character selection //////////////////////////////////////////
    void LoadPreview(GameObject prefab, Transform location, int selectionIndex, CharactersAvailableMsg.CharacterPreview character)
    {
        // instantiate the prefab
        GameObject preview = Instantiate(prefab.gameObject, location.position, location.rotation);
        preview.transform.parent = location;
        Player player = preview.GetComponent<Player>();

        // Character selection previews should stay fixed on their platform and
        // never run gameplay movement/physics logic.
        Movement previewMovement = preview.GetComponent<Movement>();
        if (previewMovement != null)
            previewMovement.enabled = false;

        CharacterController previewController = preview.GetComponent<CharacterController>();
        if (previewController != null)
            previewController.enabled = false;

        // assign basic preview values like name and equipment
        player.name = character.name;
#if !_iMMO2D
        //player.isGameMaster = character.isGameMaster;
#endif
#if _iMMOUMACHARACTERS
        if (player.playerAddonsConfigurator.tmpl_UMACharacterCreation)
        {
            string decompressor = CompressUMA.Compressor.DecompressDna(character.umaDna);
            player.playerAddonsConfigurator.umaDna = character.umaDna;
            preview.GetComponentInChildren<UMA.CharacterSystem.DynamicCharacterAvatar>().LoadFromRecipeString(decompressor);
        }
#endif
        for (int i = 0; i < character.equipment.Length; ++i)
        {
            ItemSlot slot = character.equipment[i];
            player.equipment.slots.Add(slot);
            if (slot.amount > 0)
            {

                // Cmd_OnEquipmentChanged won't be called unless spawned, we
                // need to refresh manually
                ((PlayerEquipment)player.equipment).RefreshLocation(i);
                //player.RefreshLocation(i);
#if _iMMOUMACHARACTERS
                if (player.playerAddonsConfigurator)
                    player.playerAddonsConfigurator.UpdateUma();
#endif
            }
#if !_iMMO2D
            else
            {
                EquipmentInfo info = ((PlayerEquipment)player.equipment).slotInfo[i];
                if (info.nakedModel != null)
                {
                    ((PlayerEquipment)player.equipment).RefreshLocation(i);
                }
            }
#endif
        }

        // add selection script
        preview.AddComponent<SelectableCharacter>();
        preview.GetComponent<SelectableCharacter>().index = selectionIndex;
    }

    public void ClearPreviews()
    {
        selection = -1;
        foreach (Transform location in selectionLocations)
            if (location.childCount > 0)
                Destroy(location.GetChild(0).gameObject);
    }

    void OnClientCharactersAvailable(CharactersAvailableMsg message)
    {

        charactersAvailableMsg = message;
        Debug.Log("characters available:" + charactersAvailableMsg.characters.Length);

        // set state
        state = NetworkState.Lobby;

        // clear previous previews in any case
        ClearPreviews();

        // load previews for 3D character selection
        for (int i = 0; i < charactersAvailableMsg.characters.Length; ++i)
        {
            CharactersAvailableMsg.CharacterPreview character = charactersAvailableMsg.characters[i];

            // find the prefab for that class
            Player prefab = playerClasses.Find(p => p.name == character.className);
            if (prefab != null)
                LoadPreview(prefab.gameObject, selectionLocations[i], i, character);
            else
                Debug.LogWarning("Character Selection: no prefab found for class " + character.className);
        }

        // setup camera
        Camera.main.transform.position = selectionCameraLocation.position;
        Camera.main.transform.rotation = selectionCameraLocation.rotation;

        // addon system hooks
        onClientCharactersAvailable.Invoke(charactersAvailableMsg);
    }

    // handshake: character creation ///////////////////////////////////////////
    // find a NetworkStartPosition for this class, or a normal one otherwise
    // (ignore the ones with playerPrefab == null)
    public Transform GetStartPositionFor(string className)
    {
        // avoid Linq for performance/GC. players spawn frequently!
        foreach (Transform startPosition in startPositions)
        {
            NetworkStartPositionForClass spawn = startPosition.GetComponent<NetworkStartPositionForClass>();
            if (spawn != null &&
                spawn.playerPrefab != null &&
                spawn.playerPrefab.name == className)
                return spawn.transform;
        }
        // return any start position otherwise
        return GetStartPosition();
    }

    private Player CreateCharacter(GameObject classPrefab, string characterName, string account)
    {
        Player player = Instantiate(classPrefab).GetComponent<Player>();
        player.name = characterName;
        player.account = account;
        player.className = classPrefab.name;
        player.transform.position = GetStartPositionFor(player.className).position;

        for (int i = 0; i < player.inventory.size; ++i)
        {
            player.inventory.slots.Add(
                i < player.inventory.defaultItems.Length
                    ? new ItemSlot(new Item(player.inventory.defaultItems[i].item), player.inventory.defaultItems[i].amount)
                    : new ItemSlot());
        }

        for (int i = 0; i < ((PlayerEquipment)player.equipment).slotInfo.Length; ++i)
        {
            EquipmentInfo info = ((PlayerEquipment)player.equipment).slotInfo[i];
            player.equipment.slots.Add(
                info.defaultItem.item != null
                    ? new ItemSlot(new Item(info.defaultItem.item), info.defaultItem.amount)
                    : new ItemSlot());
        }

        player.health.current = player.health.max;
        player.mana.current = player.mana.max;
#if _iMMOSTAMINA
        player.stamina.current = player.stamina.max;
#endif
        return player;
    }

    void OnServerCharacterCreate(NetworkConnectionToClient conn, CharacterCreateMsg message)
    {
        if (!lobby.ContainsKey(conn))
        {
            ServerSendError(conn, "CharacterCreate: not in lobby", true);
            return;
        }

        if (!IsAllowedCharacterName(message.name))
        {
            ServerSendError(conn, "character name not allowed", false);
            return;
        }

        string account = lobby[conn];
        if (Database.singleton.CharacterExists(message.name))
        {
            ServerSendError(conn, "name already exists", false);
            return;
        }

        if (Database.singleton.CharactersForAccount(account).Count >= characterLimit)
        {
            ServerSendError(conn, "character limit reached", false);
            return;
        }

        if (message.classIndex < 0 || message.classIndex >= playerClasses.Count)
        {
            ServerSendError(conn, "character invalid class", false);
            return;
        }

        Player player = CreateCharacter(playerClasses[message.classIndex].gameObject, message.name, account);
        onServerCharacterCreate.Invoke(message, player);
        Database.singleton.CharacterSave(player, false);
        Destroy(player.gameObject);
        conn.Send(MakeCharactersAvailableMessage(account));
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Debug.LogWarning("Use the CharacterSelectMsg instead");
    }

    void OnServerCharacterSelect(NetworkConnectionToClient conn, CharacterSelectMsg message)
    {
        if (!lobby.ContainsKey(conn))
        {
            Debug.Log("CharacterSelect: not in lobby" + conn);
            ServerSendError(conn, "CharacterSelect: not in lobby", true);
            return;
        }

        string account = lobby[conn];
        List<string> characters = Database.singleton.CharactersForAccount(account);
        if (0 <= message.index && message.index < characters.Count)
        {
            GameObject go = Database.singleton.CharacterLoad(characters[message.index], playerClasses, false);
            if (go == null)
            {
                Debug.LogWarning("CharacterSelect failed to load character: " + characters[message.index]);
                ServerSendError(conn, "character failed to load", false);
                return;
            }

            NetworkServer.AddPlayerForConnection(conn, go);
            onServerCharacterSelect.Invoke(account, go, conn, message);
            lobby.Remove(conn);
        }
        else
        {
            Debug.Log("invalid character index: " + account + " " + message.index);
            ServerSendError(conn, "invalid character index", false);
        }
    }

    void OnServerCharacterDelete(NetworkConnectionToClient conn, CharacterDeleteMsg message)
    {
        if (!lobby.ContainsKey(conn))
        {
            Debug.Log("CharacterDelete: not in lobby: " + conn);
            ServerSendError(conn, "CharacterDelete: not in lobby", true);
            return;
        }

        string account = lobby[conn];
        List<string> characters = Database.singleton.CharactersForAccount(account);
        if (0 <= message.index && message.index < characters.Count)
        {
            Debug.Log("delete character: " + characters[message.index]);
            Database.singleton.CharacterDelete(characters[message.index]);
            onServerCharacterDelete.Invoke(message);
            conn.Send(MakeCharactersAvailableMessage(account));
        }
        else
        {
            Debug.Log("invalid character index: " + account + " " + message.index);
            ServerSendError(conn, "invalid character index", false);
        }
    }

    void SavePlayers()
    {
        if (Database.singleton.useThread)
        {
            ThreadManager.MThread.QueueOnMainThread(() =>
            {
                Database.singleton.CharacterSaveMany(Player.onlinePlayers.Values);
            });
        }
        else
        {
            Database.singleton.CharacterSaveMany(Player.onlinePlayers.Values);
        }

        if (Player.onlinePlayers.Count > 0)
            Debug.Log("saved " + Player.onlinePlayers.Count + " player(s)");

        Database.singleton.SaveStorages(Storage.storages.Values);
        if (Storage.storages.Count > 0)
            Debug.Log("saved " + Storage.storages.Count + " storage(s)");

        Database.singleton.SaveFurnaces(Furnace.furnaces.Values);
        if (Furnace.furnaces.Count > 0)
            Debug.Log("saved " + Furnace.furnaces.Count + " furnace(s)");

        Database.singleton.SaveStructures(Structure.structures);
        if (Structure.structures.Count > 0)
            Debug.Log("saved " + Structure.structures.Count + " structure(s)");
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        float delay = 0;
        if (conn.identity != null)
        {
            Player player = conn.identity.GetComponent<Player>();
            delay = (float)player.remainingLogoutTime;
        }

        StartCoroutine(DoServerDisconnect(conn, delay));
    }

    IEnumerator<WaitForSeconds> DoServerDisconnect(NetworkConnectionToClient conn, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (conn.identity != null && conn.identity.TryGetComponent(out Player player))
        {
            if (player.canBeSaved)
                Database.singleton.CharacterSave(player, false);
        }

        onServerDisconnect.Invoke(conn);
        lobby.Remove(conn);
        base.OnServerDisconnect(conn);
    }

    public override void OnClientDisconnect()
    {
        Debug.Log("OnClientDisconnect");

        Camera mainCamera = Camera.main;
        if (mainCamera != null && mainCamera.transform.parent != null)
            mainCamera.transform.SetParent(null);

        uiPopup.Show("Disconnected.");
        base.OnClientDisconnect();
        state = NetworkState.Offline;
        onClientDisconnect.Invoke(NetworkClient.connection);
    }

    public static void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public override void OnValidate()
    {
        base.OnValidate();

        if (!Application.isPlaying && networkAddress != "")
            networkAddress = "Use the Server List below!";

        if (selectionLocations.Length != characterLimit)
        {
            Transform[] newArray = new Transform[characterLimit];
            for (int i = 0; i < Mathf.Min(characterLimit, selectionLocations.Length); ++i)
                newArray[i] = selectionLocations[i];
            selectionLocations = newArray;
        }
    }
}