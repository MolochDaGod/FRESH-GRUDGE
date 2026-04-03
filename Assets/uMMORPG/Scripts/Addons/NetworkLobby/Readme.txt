0) Switch to mysql
    0.1 SWITCH TO MYSQL
    0.2 you have switch to mysql lol ?

1) Open NetworkManager.cs (Assets/uMMORPG/Plugins/Mirror/Runtime)

	1.1)
	- Search 
       public static NetworkManager singleton { get; private set; }

	- Replace to
       public static NetworkManager singleton { get; set; }

--------------------------------------------------------------------

2) Open NetworkMessages.cs
	2.1)
	 - Search
	    public partial struct CharacterPreview
    {
        public string name;
        public string className; // = the prefab name
        public bool isGameMaster; // for nameoverlay prefix in preview!
        public ItemSlot[] equipment;
    }
	 - Replace to 
    public partial struct CharacterPreview
    {
        public string name;
        public string className; // = the prefab name
        public bool isGameMaster; // for nameoverlay prefix in preview!
#if _iMMOSCENELOADER || _iMMOLOBBY
        public string startingScene;
#endif
        public ItemSlot[] equipment;
    } 

	2.2 )
	 - Search
        for (int i = 0; i < players.Count; ++i)
        {
            Player player = players[i];
            characters[i] = new CharacterPreview
            {
                name = player.name,
                className = player.className,
                isGameMaster = player.isGameMaster,
                equipment = player.equipment.slots.ToArray()
            };
        }
	
	 - Replace to
        for (int i = 0; i < players.Count; ++i)
        {
            Player player = players[i];
            characters[i] = new CharacterPreview
            {
                name = player.name,
                className = player.className,
                isGameMaster = player.isGameMaster,
#if _iMMOSCENELOADER
                startingScene = (player.PlayerSceneLoader.currentScene != null ) ? player.PlayerSceneLoader.currentScene : player.PlayerSceneLoader.startingSceneName,
#elif _iMMOLOBBY
                startingScene = (player.playerNetworkLobby.currentScene.SceneName != null) ? player.playerNetworkLobby.currentScene.SceneName : player.playerNetworkLobby.startingScene.SceneName,
#endif
                equipment = player.equipment.slots.ToArray()
            };
        }

-----------------------------------------------------

3) Open Bundle scene folder and add all scene in Build Settings-> scene in build
4) select check server for build server and buid & run (lobby only is online)
5) create new file whatyouwant.cmd
    enter this in this file :
echo start server
start buildname.exe args= -scene SceneName

    /!\ Warning buildname ans SceneName is based to real name !

6) in NetworkLobby/Resources click on scriptable and add server ip, serveur port fort and background

7) in networkManager (in scene) add NetworkManagerMMONetworkLobby and assign all value, and in component networkManagerMMO assign this component too
8) in playerPrefab add PlayerNetworkLobby and assign all value

6) if you want use World (no lobby disable this addons !!)