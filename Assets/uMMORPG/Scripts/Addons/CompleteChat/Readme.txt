1) Open Player.cs 
search 
public PlayerChat chat;

comment this line
//pblic PlayerChat chat;

-----------------------------------------

2) Open PlayerExperience.cs, PlayerGuild.cs and PlayerItemMall.cs
    3.1) search
    
    public PlayerChat chat;
    
    3.2) reaplce to
    
#if _iMMOCOMPLETECHAT
    public PlayerCompleteChat chat;
#else
    public PlayerChat chat;
#endif

----------------------------------------

3) Open prefab player
    Add new Component : PlayerCompleteChat

4) Reasign component in other component
    Reasign in compoenent Player, PlayerExperience, PlayerGuild, PlayerItemMall the new chat component "PlayerCompleteChat"

5) open prefab using Interaction Requierements 
exemple : Lootcrate

6.1) extend Interaction Requierements
6.2) search Require and add prefab SlotChatMessageInfoTMP

repeat this operation for all prefab use this

3) drapg and drop prefab in canvas, unpack prefab in canvas, and delete prefab 