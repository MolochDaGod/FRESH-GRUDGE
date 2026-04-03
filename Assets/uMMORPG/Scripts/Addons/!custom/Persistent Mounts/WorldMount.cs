using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMount : NetworkBehaviour
{

    public Mount mountPrefab;
	[SyncVar, HideInInspector] public GameObject Owner;


    private void OnMouseDown()
    {
        Player player = Player.localPlayer;
        if (player != null)
        {
            if (Vector3.Distance(player.transform.position, transform.position) <= player.interactionRange)
            {
                player.mountControl.startDismountTimer = true;
                player.worldMounts.CmdSetMount(this);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
			if (isServer && Owner == null)
			{
				Player player = other.GetComponentInParent<Player>();
				Owner = player.gameObject;
			}
			if (isServer)
            {
                Player player = other.GetComponentInParent<Player>();
				if (player != null && Owner == player.gameObject)
                {
                    player.worldMounts.currentWorldMount = this;
                    player.worldMounts.canMount();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            if (isServer)
            {
                Player player = other.GetComponentInParent<Player>();
                if (player != null)
                {
					Owner = null;
                    player.worldMounts.currentWorldMount = null;
                    player.worldMounts.cantMount();
                }
            }
        }
    }
}
