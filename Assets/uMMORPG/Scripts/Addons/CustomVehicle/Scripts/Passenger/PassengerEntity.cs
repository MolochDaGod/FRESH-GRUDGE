
//(c8

using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PassengerEntity : Entity
{
    public PassengerStateMachine passengerStateMachine;

    protected override void Start()
    {
        // do nothing
    }

    public override bool CanAttack(Entity entity)
    {
        if (base.CanAttack(entity))
        {
            bool targetIsOwnVehicle = (passengerStateMachine.passengerPlayer.playerVehicle.enterVehicle != null && passengerStateMachine.passengerPlayer.playerVehicle.enterVehicle.vehicleEntity == entity);
            bool targetIsInsideOwnVehicle = (passengerStateMachine.passengerPlayer.playerVehicle.enterVehicle != null && passengerStateMachine.passengerPlayer.playerVehicle.enterVehicle.IsEntityPlayerIsInVehicle(entity));
            bool targetInsideAnotherVehicle = (!targetIsInsideOwnVehicle && entity is Player && ((Player)entity).playerVehicle.enterVehicle != null);
            
            if (!targetIsOwnVehicle && !targetIsInsideOwnVehicle && !targetInsideAnotherVehicle)
                return true;
        }

        return false;
    }

    public override void OnInteract()
    {
        //throw new System.NotImplementedException();
    }

    protected override void UpdateClient()
    {
        passengerStateMachine.UpdateClient();
    }

    protected override string UpdateServer()
    {
        return passengerStateMachine.UpdateServer();
    }
}
