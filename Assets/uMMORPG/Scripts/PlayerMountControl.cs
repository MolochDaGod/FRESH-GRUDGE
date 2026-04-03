// put all mount control code into a separate component.
// => for consistency with PetControl
// => if we need mount inventories, then we can put the Cmds in here too later
using UnityEngine;
using Mirror;

[DisallowMultipleComponent]
public class PlayerMountControl : NetworkBehaviour
{
    [Header("Mount")]
    public Transform meshToOffsetWhenMounted;
    public float seatOffsetY = -1;

    public Player player;
    public bool canDismount;
    public bool startDismountTimer = false;
    private float dismountTimer = 1f;
    private float dismountTimerReset = 1f;
    // 'Mount' can't be SyncVar so we use [SyncVar] GameObject and wrap it
    [SyncVar, HideInInspector] public Mount activeMount;

    [SyncVar] public int mountIndex = -1;
    public override void OnStartClient()
    {
        CmdSpawnMount();
        if (mountIndex != -1)
        {
            startDismountTimer = true;
        }
    }

    void LateUpdate()
    {
        // follow mount's seat position if mounted
        // (on server too, for correct collider position and calculations)
        ApplyMountSeatOffset();

        if (startDismountTimer)
        {
            dismountTimer -= Time.deltaTime;
            if (dismountTimer <= 0)
            {
                canDismount = true;
                dismountTimer = dismountTimerReset;
                startDismountTimer = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.E) && (canDismount) && !UIUtils.AnyInputActive())
        {
            PlayerCharacterControllerMovement controller = (PlayerCharacterControllerMovement)player.movement;
            if (IsMounted() && canDismount && controller.state != MoveState.MOUNTED_AIRBORNE)
            {
                CmdDismount();
            }
        }
    }

    public bool IsMounted()
    {
        return activeMount != null && activeMount.health.current > 0;
    }

    void ApplyMountSeatOffset()
    {
        if (meshToOffsetWhenMounted != null)
        {
            // apply seat offset if on mount (not a dead one), reset otherwise
            if (activeMount != null && activeMount.health.current > 0)
                meshToOffsetWhenMounted.transform.position = activeMount.seat.position + Vector3.up * seatOffsetY;
            else
                meshToOffsetWhenMounted.transform.localPosition = Vector3.zero;
        }
    }

    [Command]
    public void CmdDismount()
    {

        canDismount = false;
        GameObject go = Instantiate(activeMount.worldMount.gameObject, transform.position, player.transform.rotation);
        go.name = activeMount.worldMount.name;
        NetworkServer.Spawn(go);
        mountIndex = -1;
        NetworkServer.Destroy(activeMount.gameObject);
        player.transform.position += Vector3.up * 2;
    }

    [Command]
    public void CmdSpawnMount()
    {
        if (player.mountControl.mountIndex != -1)
        {
            GameObject mountPrefab = Instantiate(MountManager.Instance.mountPrefabs[player.mountControl.mountIndex], player.transform.position, player.transform.rotation);
            Mount mount = mountPrefab.GetComponent<Mount>();
            mount.name = mountPrefab.name; // avoid "(Clone)"
            mount.owner = player;
            mount.health.current = mount.health.max;

            // spawn with owner connection so the owner can call [Command]s on it
            NetworkServer.Spawn(mountPrefab, player.connectionToClient);

            activeMount = mount; // set syncvar after spawning
            mountIndex = mount.mountIndex;

            startDismountTimer = true;
        }
    }
}
