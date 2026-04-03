***1) in canvas ***
    - 1.1) add in canvas UI_CharacterInfo
    - 1.2) select a shortcut, add a ButtonCharacterInfo search, replace the old character info panel with a new panel in UI_CharacterInfo

***2) in network manager***
    - 2.1) add a new NetworkManagerMMOAttributes component and select networkmanager
    - 2.2) in the NetworkManagerMMO component, search for NetworkManagerMMOAttributes and select NetworkManagerMMOAttributes

    ***2) in the players' predabs***
    - 3.1) add a new PlayerAttributes component and select the player, select all necessary components (player, player equipment, health, etc.)
    - 3.2) add new attribute type and configure starting attribute points etc.
    - 3.3) select the component player and search for the player attributes and assigned the

****Advice:****
- 1) I advise you to create your own scriptables and not to use those present in the addons
- 2) I also advise you to create your own prefabs, this will avoid any subsequent overwriting

****Advice:****
     - copying a file to Unity is very simple, just press `Ctrl+d`