using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWorldMount : NetworkBehaviour
{
    public Player player;
    [SyncVar] public WorldMount currentWorldMount;
    [SyncVar] public bool can_mount = false;

    // Update is called once per frame
    void Update()
    {
        if (currentWorldMount != null)
        {
			if (Input.GetKeyDown(KeyCode.E) && !UIUtils.AnyInputActive())
            {
                CmdSpawnMount();
                player.mountControl.startDismountTimer = true;
            }
        }
    }

    [Command]
    public void CmdSetMount(WorldMount mount)
    {
        currentWorldMount = mount;
    }
    [Server]
    public void setMount(WorldMount mount)
    {
        currentWorldMount = mount;
    }
    [Server]
    public void canMount()
    {

        can_mount = true;
    }
    [Server]
    public void cantMount()
    {

        can_mount = false;
    }

    [Command]
    public void CmdSpawnMount()
    {
        if (player.mountControl.activeMount == null)
        {
            // summon at player position

            GameObject go = Instantiate(currentWorldMount.mountPrefab.gameObject, player.transform.position, player.transform.rotation);
            Mount mount = go.GetComponent<Mount>();
            mount.name = currentWorldMount.mountPrefab.name; // avoid "(Clone)"
            mount.owner = player;
            mount.health.current = mount.health.max;

            // spawn with owner connection so the owner can call [Command]s on it
            NetworkServer.Spawn(go, player.connectionToClient);
            player.mountControl.activeMount = mount; // set syncvar after spawning
            player.mountControl.mountIndex = mount.mountIndex;
            NetworkServer.Destroy(currentWorldMount.gameObject);
        }

    }
}
