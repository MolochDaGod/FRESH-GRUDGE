using Mirror;
using UnityEngine;

//using UnityEditor;

[HideInInspector]
public partial class PlayerAddonsConfigurator : NetworkBehaviour
{
#if _iMMOTOOLS

    [Header("Component")]
    public Player player;
    public PlayerInventory inventory;
    public PlayerEquipment playerEquipment;
    public PlayerExperience playerExperience;
    public Health health;
    public Combat combat;

    // -----------------------------------------------------------------------------------
    // OnStartClient
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Client]
    public override void OnStartClient()
    {
#if _iMMOPRESTIGECLASSES
        OnStartClient_PrestigeClasses();
#endif
#if _iMMOGUILDWAREHOUSE
       // OnStartClient_GuildWarehouse();
#endif
    }

    // -----------------------------------------------------------------------------------
    // OnStartLocalPlayer
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
#if _CLIENT
    [Client]
    public override void OnStartLocalPlayer()
    {
#if _iMMOJUKEBOX
        OnStartLocalPlayer_Jukebox();
#endif

#if _iMMOCHEST
        OnStartLocalPlayer_Lootcrate();
#endif

#if _iMMOLOGOUTTIMER
        OnStartLocalPlayer_LogoutTimer();
#endif

#if _iMMOEQUIPABLEBUFF
        OnStartLocalPlayer_EquipableBuff();
#endif

    }
#endif

    // -----------------------------------------------------------------------------------
    // OnStartServer
    // @Server
    // -----------------------------------------------------------------------------------

#if _SERVER
    [Server]
    public override void OnStartServer()
    {
#if _iMMOLEADERBOARD
        OnStartServer_Leaderboard();
#endif

#if _iMMOEQUIPABLEBUFF
        OnStartServer_EquipableBuff();
#endif

#if _iMMOLEVELUP
        OnStartServer_LevelUp();
#endif

#if _iMMOITEMDROP && _iMMOLOOTRULES
        OnStartServer_ItemDropPlayer();
#endif
    }
#endif



    // -----------------------------------------------------------------------------------
    // Start
    // @Client && @Server
    // -----------------------------------------------------------------------------------
    public void Start()
    {

#if _iMMOEXPLORATION
        Start_Exploration();
#endif

#if _CLIENT && _iMMOJUKEBOX
        Start_JukeBox();
#endif

#if _iMMOFRIENDS
        Start_Friendship();
#endif
    }


    // -----------------------------------------------------------------------------------
    // Start
    // @Client && @Server
    // -----------------------------------------------------------------------------------
    private void Update()
    {

#if _SERVER && _iMMOFRIENDS
        Update_Friendship();
#endif
    }

    // -----------------------------------------------------------------------------------
    // Cmd_ShowAnimation
    // @Client && @Server
    // -----------------------------------------------------------------------------------
    private void LateUpdate()
    {
#if _CLIENT && _iMMOCHEST
        LateUpdate_LootCrate();
#endif

#if _CLIENT && _iMMOCHAIRS
        LateUpdate_Chair();
#endif

#if _CLIENT && _iMMODOORS
        LateUpdate_Doors();
#endif

    }

#endif
}