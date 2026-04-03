
//(c8

using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BurnyDev.UMMORPG.AddonCustomVehicle
{
    [Serializable]
    public class VehicleData
    {
        public Transform seatPosition;
        public Transform seatExitPosition;
        [Header("Driver do not need follower, leave it empty for Element 0")]
        public GameObject followerToSpawn;

        // playerObject are use server side only
        [HideInInspector] public GameObject playerObject;
        [HideInInspector] public Transform objectToTakeControl;
    }

    public class EnterVehicle : NetworkBehaviour
    {
        // To know if driver is sitted in vehicle
        [SyncVar, HideInInspector] public bool isDriverSit = false;

        [Header("Save vehicle in databse?, need unique gameobject name")]
        public bool saveVehicleInDatabase = false;

        public VehicleEntity vehicleEntity;
        public VehicleLook vehicleLook;
        public VehicleController vehicleController;

        [Header("Block rigidbody when driver exit? (Set kinematic to true)")]
        public bool blockRigidBody = false;
        [Header("Use rigidbody gravity when driver enter?")]
        public bool useGravity = true;

        public bool isDestroyed = false;

        [Header("Damage vehicle when too much rotation?")]
        public bool damageWhenRotated = false;
        [Header("Damageinterval in sec")]
        public float intervalToDamageWhenRev = 1f;
        private float actualIntervalTime = 0f;
        [Header("Damage to do each interval")]
        public int damageToDoWhenRev = 200;
        // Max angle - Min angle
        //         360-0
        // 340 --\   o   /-- 20
        //         /   \
        // 270 -  |     |  - 90
        [Header("Vehicle angle setup")]
        public float dmgAngleMinX = 10f;
        public float dmgAngleMaxX = 350f;
        public float dmgAngleMinZ = 10f;
        public float dmgAngleMaxZ = 350f;

        //[Header("Vehicle collider setup, add all collider.\n - Collider on layer Default will be change to layer VehicleParent\n    to not collide with other player locally")]
        //public GameObject[] toChangeLayer;

        [Header("Need at least 1 for the driver")]
        public VehicleData[] vehicleData = new VehicleData[1] { null };

        public static Dictionary<string, EnterVehicle> allVehicles = new Dictionary<string, EnterVehicle>();

        public override void OnStartServer()
        {
            if (saveVehicleInDatabase)
            {
                // add to allVehicles if none with the same name already exists
                if (!allVehicles.ContainsKey(name))
                {
                    allVehicles[name] = this;

                    // load from db
#if _SQLITE
                    Database.singleton.LoadCustomVehicle(this);
#endif
                }
                else Debug.LogWarning("A Custom vehicle with name " + name + " already exists. Use a different name for each CustomVehicle, otherwise it won't be saved to the Database.");
            }

            // Next spawn and setup each follower
            for (int i = 0; i < vehicleData.Length; i++)
            {
                if (i == 0)
                {
                    // First object is this vehicle
                    // No need to spawn it, its already spawned and setup
                    vehicleData[i].objectToTakeControl = transform;
                }
                else if (vehicleData[i].followerToSpawn != null)
                {
                    GameObject go = Instantiate(vehicleData[i].followerToSpawn, vehicleData[i].seatPosition.position, Quaternion.identity);
                    vehicleData[i].objectToTakeControl = go.transform;
                    NetworkServer.Spawn(go);
                    go.GetComponent<PassengerFollowVehicle>().niToFollow = netId + "," + i;

                    // Because dedicated do not run syncvar hook
                    if (isServerOnly)
                        go.GetComponent<PassengerFollowVehicle>().OnSetFollowChange(netId + "," + i, netId + "," + i);
                }
            }
        }

        public override void OnStartClient()
        {
            // First object is this vehicle
            // No need to spawn it, its already spawned and setup
            vehicleData[0].objectToTakeControl = transform;
        }

        private void Update()
        {
            if (damageWhenRotated && isServer)
            {
                if (Time.time - actualIntervalTime >= intervalToDamageWhenRev)
                {
                    actualIntervalTime = Time.time;

                    if ((Mathf.Abs(transform.eulerAngles.x) > dmgAngleMinX && Mathf.Abs(transform.eulerAngles.x) < dmgAngleMaxX) ||
                        (Mathf.Abs(transform.eulerAngles.z) > dmgAngleMinZ && Mathf.Abs(transform.eulerAngles.z) < dmgAngleMinZ))
                    {
                        //usurvival//vehicleEntity.combat.DealDamageAt(vehicleEntity, damageToDoWhenRev, Vector3.zero, Vector3.one, vehicleEntity.collider);
                        vehicleEntity.combat.DealDamageAt(vehicleEntity, damageToDoWhenRev);
                    }
                }
            }
        }

        public bool IsEntityPlayerIsInVehicle(Entity entity)
        {
            foreach (var item in vehicleData)
            {
                if (item != null && item.playerObject != null && item.playerObject.GetComponent<Entity>() == entity)
                    return true;
            }

            return false;
        }

        [Server]
        public bool IsAnyPlayerInsideVehicle()
        {
            if (GetAllPlayerCountInVehicle() > 0)
                return true;

            return false;
        }

        [Server]
        public bool IsDriverPlayerInsideVehicle()
        {
            if (vehicleData[0].playerObject != null)
                return true;

            return false;
        }

        [Server]
        private int GetAllPlayerCountInVehicle()
        {
            int passengerCount = 0;

            foreach (var item in vehicleData)
            {
                if (item.playerObject != null)
                    passengerCount++;
            }

            return passengerCount;
        }

        [Server]
        public void ExitAllPlayerFromVehicle()
        {
            foreach (var item in vehicleData)
            {
                if (item.playerObject != null)
                {
                    ExitVehicleServer(item.playerObject.GetComponent<Player>());
                }
            }
        }

        [Server]
        public void DestroyAllFollowers()
        {
            foreach (var item in vehicleData)
            {
                if (item.objectToTakeControl != null)
                {
                    NetworkServer.Destroy(item.objectToTakeControl.gameObject);
                }
            }
        }

        public void EnterVehicleServer(Player player, int vehicleSeatId)
        {
            // Check if vehicle is destroyed
            if (isDestroyed)
            {
                return;
            }

            // Check if player have pet or mounted
            if (player.petControl.activePet != null || player.mountControl.activeMount != null)
            {
                return;
            }

            // Check if player already inside a vehicle
            if (player.playerVehicle.IsInsideVehicle())
            {
                return;
            }

            // Check if the selected seat is free, seat without player is free
            if (vehicleData[vehicleSeatId].playerObject != null)
            {
                return;
            }

            // Remove the actual player target
            player.target = null;

            // Occupy the seat
            vehicleData[vehicleSeatId].playerObject = player.gameObject;
            isDriverSit = true;

            vehicleData[vehicleSeatId].objectToTakeControl.GetComponent<NetworkIdentity>().AssignClientAuthority(player.connectionToClient);

            PlayerVehicle playerVehicle = player.playerVehicle;

            playerVehicle.enterVehicle = this;

            if (vehicleSeatId == 0)
            {
                playerVehicle.controlVehicle = true;

                GetComponent<VehicleNetwork>().Warp(transform.position, transform.eulerAngles);
            }

            playerVehicle.vehicleString = "true" + "," + netId + "," + vehicleSeatId;

            // Because dedicated do not run syncvar hook
            if (isServerOnly)
            {
                playerVehicle.OnVehicleStringChanged(playerVehicle.vehicleString, playerVehicle.vehicleString);
            }
        }

        [Server]
        public void ExitVehicleServer(Player player)
        {
            // Check if player control this vehicle
            PlayerVehicle playerVehicle = player.playerVehicle;
            if (playerVehicle.playerVehicleId != netId)
            {
                return;
            }

            // Find the seat of this player
            int actualSeat = -1;
            for (int i = 0; i < vehicleData.Length; i++)
            {
                if (vehicleData[i].playerObject != null && vehicleData[i].playerObject.name == player.name)
                {
                    actualSeat = i;
                    break;
                }
            }

            if (actualSeat == -1)
            {
                Debug.LogWarning("player already exited vehicle id: " + netId);
                return;
            }

            // UnOccupy the seat
            vehicleData[actualSeat].playerObject = null;
            isDriverSit = false;

            playerVehicle.vehicleString = "false" + "," + netId + "," + actualSeat;

            // Because dedicated do not run syncvar hook
            if (isServerOnly)
            {
                playerVehicle.OnVehicleStringChanged(playerVehicle.vehicleString, playerVehicle.vehicleString);
            }

            if (actualSeat == 0)
            {
                playerVehicle.controlVehicle = false;

                GetComponent<VehicleNetwork>().Warp(transform.position, transform.eulerAngles);
            }

            // To have player not keep the vehicle rotation
            player.transform.rotation = Quaternion.identity;

            if (vehicleData[actualSeat].objectToTakeControl != null)
            {
                vehicleData[actualSeat].objectToTakeControl.GetComponent<NetworkIdentity>().RemoveClientAuthority();
            }

            if (actualSeat == 0)
            {
                // Reset SyncVar
                vehicleController.ResetSyncVar();
            }
        }

        //public void ChangeVehicleCollisionLayer(bool otherPlayerCollide)
        //{
        //    if (!otherPlayerCollide)
        //    {
        //        // When we control a vehicle, remote player in our scene must not collide with vehicle
        //        // They will do their own collision locally in their scene
        //        // To do this we change layer of some collider on the vehicle to VehicleParent
        //        // As VehicleParent not colliding with layer use in the default player prefab (IgnoreRaycast, player)
        //        if (toChangeLayer != null && toChangeLayer.Length > 0)
        //        {
        //            for (int i = 0; i < toChangeLayer.Length; i++)
        //            {
        //                toChangeLayer[i].layer = LayerMask.NameToLayer("VehicleParent");
        //            }
        //        }
        //    }
        //    else
        //    {
        //        // Same apply on exit, we just revert layer change
        //        // So we change layer player to layer Default
        //        if (toChangeLayer != null && toChangeLayer.Length > 0)
        //        {
        //            for (int i = 0; i < toChangeLayer.Length; i++)
        //            {
        //                toChangeLayer[i].layer = LayerMask.NameToLayer("Default");
        //            }
        //        }
        //    }
        //}

        //public void Fill()
        //{
        //    EnterVehicle enterVehicle = hit.transform.GetComponentInParent<EnterVehicle>();
        //    if (enterVehicle != null)
        //    {
        //        ItemSlot slot = player.hotbar.slots[hotbarIndex];

        //        if (filling)
        //        {
        //            // decrease amount
        //            if (enterVehicle.vehicleController.vehicleFuel < enterVehicle.vehicleController.vehicleMaxFuel - addedFuelQuantity && slot.DecreaseAmount(addedFuelQuantity) > 0)
        //            {
        //                player.hotbar.slots[hotbarIndex] = slot;

        //                // Add fuel to vehicle
        //                enterVehicle.vehicleController.vehicleFuel += addedFuelQuantity;
        //            }
        //        }
        //        else
        //        {
        //            // vehicle fuel quantity and check item use durability
        //            if (enterVehicle.vehicleController.vehicleFuel >= extractedFuelQuantity && slot.item.CheckDurability())
        //            {
        //                // try to add it to the inventory, remove fuel from vehicle if it worked
        //                if (player.inventory.Add(new Item(enterVehicle.vehicleController.fuelItemData), extractedFuelQuantity))
        //                {
        //                    // reduce durability
        //                    slot.item.durability = Mathf.Max(slot.item.durability - 1, 0);
        //                    player.hotbar.slots[player.hotbar.selection] = slot;

        //                    // Remove fuel from vehicle
        //                    enterVehicle.vehicleController.vehicleFuel -= extractedFuelQuantity;
        //                }
        //            }
        //        }
        //    }
        //}
    }
}
