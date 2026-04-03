
//(c8

using BurnyDev.UMMORPG.AddonCustomVehicle;
using Mirror;
using UnityEngine;

public partial class VehicleEntity : Entity
{
    public VehicleStateMachine vehicleStateMachine;

    public EnterVehicle enterVehicle;

    [Header("Vehicle explosion effect, must destroy himself after time")]
    public GameObject explosionEffect;
    [Header("Delay before remove vehicle when destroyed, 0 to disable")]
    public float delayBeforeRemoveVehicle = 5f;

    private EnterVehicle enterVehicleTemp;

    protected override void Start()
    {
        // do nothing
    }

    public override bool CanAttack(Entity entity)
    {
        if (base.CanAttack(entity))
        {
            bool targetIsOwnVehicle = (enterVehicle != null && enterVehicle.vehicleEntity == entity);
            bool targetIsInsideOwnVehicle = (enterVehicle != null && enterVehicle.IsEntityPlayerIsInVehicle(entity));
            bool targetInsideAnotherVehicle = (!targetIsInsideOwnVehicle && entity is Player && ((Player)entity).playerVehicle.enterVehicle != null);
            
            if (!targetIsOwnVehicle && !targetIsInsideOwnVehicle && !targetInsideAnotherVehicle)
                return true;
        }

        return false;
    }

    // Added in the OnEmpty event on the vehicle Health component
    [Server]
    public override void OnDeath()
    //public void OnDeath()
    {
        base.OnDeath();

        // Set vehicle destroyed to block player from interact
        enterVehicle.isDestroyed = true;

        // Exit all player
        enterVehicle.ExitAllPlayerFromVehicle();

        // Send explosion to client
        RpcClientOnDeath();

        // Delay remove of vehicle and followers
        if (delayBeforeRemoveVehicle > 0f)
        {
            Invoke(nameof(DelayedDestroy), delayBeforeRemoveVehicle);
        }
    }

    [Server]
    private void DelayedDestroy()
    {
        // Destroy all followers
        enterVehicle.DestroyAllFollowers();

        // Destroy the vehicle
        NetworkServer.Destroy(gameObject);
    }

    [ClientRpc]
    public void RpcClientOnDeath()
    {
        // Spawn explosion
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }
    }

    public override void OnInteract()
    {
        //throw new System.NotImplementedException();
    }

    protected override void UpdateClient()
    {
        if (vehicleStateMachine != null)
            vehicleStateMachine.UpdateClient();
    }

    protected override string UpdateServer()
    {
        return vehicleStateMachine != null ? vehicleStateMachine.UpdateServer() : null;
    }
}
