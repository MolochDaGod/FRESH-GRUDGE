using System.Collections;
using Mirror;
using UnityEngine;

public partial class Entity
{
    [ClientRpc]
    public void RpcBackstabStartTeleport(Vector3 position, Vector3 enemyPos)
    {
#if _iMMO2D
        //movement.LookAt(enemyPos);
#else
        movement.LookAtY(enemyPos);
#endif
        movement.Warp(position);
    }
}