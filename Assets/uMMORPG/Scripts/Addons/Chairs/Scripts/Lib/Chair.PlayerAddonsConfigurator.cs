using UnityEngine;
using Mirror;
using System.Collections;

public partial class PlayerAddonsConfigurator
{

    [SyncVar, HideInInspector]
    public bool isSeated = false;

    [HideInInspector]
    public ChairAddon currentChair;

    /////////////////////////////////////////////////////////
    // Used for Base Addon LateUpdate Function,            //
    // Sets Animator and If you move you'll cancel sitting //
    /////////////////////////////////////////////////////////
#if _CLIENT
    private void LateUpdate_Chair()
    {
        player.animator.SetBool("Sit", isSeated);

        if (isSeated)
        {

            if (player.state == "DEAD")
            {
                if (currentChair != null)
                {
                    Cmd_Stand(currentChair.gameObject);
                }
            }
            if (player.movement.GetVelocity().magnitude > 0)
            {
                if (currentChair != null)
                {
                    Cmd_Stand(currentChair.gameObject);
                }
            }
            if (player.movement.IsMoving())
            {
                isSeated = false;
                Cmd_Stand(currentChair.gameObject);
            }
        }
    }
#endif

    //////////////////////////////////////////////////////
    // Used to Update player Rotation to Chair Rotation //
    //////////////////////////////////////////////////////
    [Command]
    public void Cmd_rotatePlayer(Quaternion rot)
    {
#if _SERVER
        this.transform.rotation = rot;
       // movement.
        Rpc_RotatePlayer(rot);
#endif
    }

    ///////////////////////////////////////////////////////
    // Used to Update All Players Nearby of new rotationPlaceableObject //
    ///////////////////////////////////////////////////////
    [ClientRpc]
    public void Rpc_RotatePlayer(Quaternion rot)
    {
        this.transform.rotation = rot;
    }


    [Command]
    public void Cmd_Warp()
    {
#if _SERVER
        player.movement.Reset();
        player.movement.Warp(currentChair.transform.position);
        Rpc_WarpPlayer();
#endif
    }

    [ClientRpc]
    public void Rpc_WarpPlayer()
    {
        player.movement.Reset();
        //Debug.Log("player name :" + player.name);
        //player.movement.Warp(currentChair.transform.positionPlaceableObject);
    }
    ///////////////////////////////////////////////////////
    // Could be touched up in the future,                //
    // Waits 0.5f Seconds to get to desitionation then   //
    // Rotates the player to the Chairs Parent Rotation  //
    ///////////////////////////////////////////////////////
    public IEnumerator startSitting()
    {
        yield return new WaitForSeconds(0.1f);
        this.Cmd_Warp();
        yield return new WaitForSeconds(0.5f);
        this.Cmd_rotatePlayer(currentChair.transform.rotation);
        this.Cmd_Sit();
    }

    ///////////////////////////////////////////////////////////
    // Tells Server to change - isSeated - to True           //
    ///////////////////////////////////////////////////////////
    [Command]
    public void Cmd_Sit()
    {
#if _SERVER
        isSeated = true;
#endif
    }

    ////////////////////////////////////////////////////////////
    // Tells Server to change - isSeated - to False and       //
    // Tells chair to become available for other players      //
    ////////////////////////////////////////////////////////////
    [Command]
    public void Cmd_Stand(GameObject go)
    {
#if _SERVER
        isSeated = false;

        if (go.GetComponent<ChairAddon>() == null) return;

        ChairAddon temp = go.GetComponent<ChairAddon>();
        temp.inUse = false;
        temp.sitCollider.enabled = true;
        temp.gameObject.GetComponent<Collider>().enabled = true;
#endif
    }
}