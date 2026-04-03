using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

// PLAYER

public partial class PlayerAddonsConfigurator
{
    [HideInInspector, SyncVar] public BindPoint MyBindpoint;

    // -----------------------------------------------------------------------------------
    // SetBindpoint
    // @Server
    // -----------------------------------------------------------------------------------
    public void SetBindpointFromArea(string name, float x, float y, float z)
    {
        if (player.isAlive)
        {
            MyBindpoint = new BindPoint();
            MyBindpoint.name = name;
            MyBindpoint.position = new Vector3(x, y, z);
            MyBindpoint.SceneName = SceneManager.GetActiveScene().name;
        }
    }

    // -----------------------------------------------------------------------------------
    // Cmd_SetBindpoint
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_SetBindpoint()
    {
#if _SERVER
        if (player.target != null && player.target.isAlive && player.isAlive && player.target is Npc npc && Utils.ClosestDistance(player, npc) <= player.interactionRange && npc.npcBindpoint.offertBindpoint)
        {
            MyBindpoint.name = npc.name;
            MyBindpoint.position = player.transform.position;
            MyBindpoint.SceneName = SceneManager.GetActiveScene().name;
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // Cmd_RespawnToBindpoint
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_RespawnToBindpoint()
    {
#if _SERVER
        if (!MyBindpoint.Valid) return;

		RespawnToLocalBindpoint();
#endif
    }

    // -----------------------------------------------------------------------------------
    // RespawnToLocalBindpoint
    // @Server
    // -----------------------------------------------------------------------------------

#if _SERVER
    [Server]
    public void RespawnToLocalBindpoint()
    {
        player.movement.Warp(MyBindpoint.position);

        if (player.health.current == 0)
        {
            player.Revive(0.5f);
        }
        player.Tools_OverrideState("IDLE");

#if _iMMOLOBBY
        if (MyBindpoint.SceneName != SceneManager.GetActiveScene().name)
        {
            player.playerNetworkLobby.OnPortal(MyBindpoint);
        }
#endif
    }
#endif
    // -----------------------------------------------------------------------------------
}