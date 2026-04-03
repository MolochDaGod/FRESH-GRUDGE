using Mirror;
using UnityEngine;

namespace BurnyDev.UMMORPG.AddonCustomVehicle
{
    public class PlayerVehicle : NetworkBehaviour
    {
        // Only use by local player
        // To set local position inside vehicle
        // and to send it to server and remote player
        [HideInInspector] public Vector3 offsetVehicle = Vector3.zero;

        // Only use by server and remote client
        // To set local position inside vehicle
        // and calculate velocity of remote player (animation)
        [HideInInspector] public Vector3 lastPositionOffsetVehicle = Vector3.zero;

        // Reference to the localparent player is inside
        [HideInInspector] public VehicleLocalParent vehicleLocalParent;

        public Player player;

        // This string is (netId, seatId)
        [SyncVar(hook = nameof(OnVehicleStringChanged)), HideInInspector] public string vehicleString = "";
        [SyncVar, HideInInspector] public bool controlVehicle = false;
        [HideInInspector] public uint playerVehicleId = 0;
        [HideInInspector] public bool isInsideVehicle = false;
        [HideInInspector] public int seatId = 0;

        // On owning client its use to show the speed in ui
        // On server its use to exit vehicle
        [HideInInspector] public EnterVehicle enterVehicle;

        [Header("Key to exit vehicle")]
        public KeyCode exitVehicleKey = KeyCode.H;

        // Set by OnVehicleStringChanged
        private Transform positionToFollow;

        // UMMORPGR use OnMouseXXX but Camera.main.eventMask is missing for layers
        // I add it here in the VehiclePlayer as we need mouse click not be blocked by VehicleParent layer
        [Header("Layer to catch OnMouseXXX method in script")]
        [SerializeField] private LayerMask inputLayerMask;

        public bool IsInsideVehicle() => isInsideVehicle;

        private float lastClientSendTime;

        //public bool IsUsableBySeat(ScriptableItem scriptableItem)
        //{
        //    if (seatId == 0)
        //        return scriptableItem.usableInVehicleAsDriver;
        //    else if (seatId > 0)
        //        return scriptableItem.usableInVehicleAsPassenger;

        //    return false;
        //}

        public override void OnStartLocalPlayer()
        {
            // We set it only for local player
            Camera.main.eventMask = inputLayerMask;
        }

        private void Update()
        {
            if (IsInsideVehicle())
            {
                if (positionToFollow != null)
                {
                    transform.position = positionToFollow.position;
                    transform.rotation = positionToFollow.rotation;
                }

                if (Player.localPlayer != null && Player.localPlayer == player)
                {
                    if (Input.GetKeyDown(exitVehicleKey) && !UIUtils.AnyInputActive())
                    {
                        if (enterVehicle.vehicleController.vehicleMaxSpeedToExit == -1f || enterVehicle.vehicleController.GetVehicleSpeed() <= enterVehicle.vehicleController.vehicleMaxSpeedToExit)
                        {
                            if (Time.time - lastClientSendTime >= syncInterval)
                            {
                                CmdTryExitVehicle();

                                lastClientSendTime = Time.time;
                            }
                        }
                    }
                }
            }
        }

        [Command]
        public void CmdTryEnterVehicle(NetworkIdentity ni, int vehicleSeatId)
        {
            // validate
            if (ni != null)
            {
                // can directly change it, or change it after casting?
                if (player.state == "IDLE" || player.state == "MOVING")
                {
                    ni.GetComponent<EnterVehicle>().EnterVehicleServer(player, vehicleSeatId);
                }
            }
        }

        [Command]
        public void CmdTryOpenDoorVehicle(NetworkIdentity ni)
        {
            // validate
            if (ni != null)
            {
                // can directly change it, or change it after casting?
                if (player.state == "IDLE" || player.state == "MOVING")
                {
                    ni.GetComponent<VehicleDoor>().ServerOpenCloseAdditionalInteraction(player);
                }
            }
        }

        [Command]
        public void CmdDropItemOnVehicle(NetworkIdentity ni, int inventoryItemIndex)
        {
            // validate
            if (ni != null)
            {
                // can directly change it, or change it after casting?
                if (player.state == "IDLE" || player.state == "MOVING")
                {
                    VehicleController vehicleController = ni.GetComponent<VehicleController>();
                    ItemSlot itemSlot = player.inventory.slots[inventoryItemIndex];
                    VehicleFuelingItem vehicleFuelingItem = itemSlot.item.data as VehicleFuelingItem;

                    if (vehicleController != null && itemSlot.amount > 0 && vehicleFuelingItem != null)
                    {
                        // If dragged item have same name as the fuel item for this vehicle .. we fill the vehicle
                        if (vehicleController.fuelItemData != null && vehicleController.fuelItemData.name == itemSlot.item.name)
                        {
                            // decrease amount
                            if (vehicleController.vehicleFuel < vehicleController.vehicleMaxFuel - vehicleFuelingItem.fuelQuantityByUse && itemSlot.DecreaseAmount(vehicleFuelingItem.fuelQuantityByUse) > 0)
                            {
                                player.inventory.slots[inventoryItemIndex] = itemSlot;
                                // Add fuel to vehicle
                                vehicleController.vehicleFuel += vehicleFuelingItem.fuelQuantityByUse;
                            }
                        }
                        // If dragged item have same name as the extract fuel item for this vehicle .. we empty the vehicle
                        else if (vehicleController.extractFuelItemData != null && vehicleController.extractFuelItemData.name == player.inventory.slots[inventoryItemIndex].item.name)
                        {
                            // vehicle fuel quantity and check item use durability
                            if (vehicleController.vehicleFuel >= vehicleFuelingItem.fuelQuantityByUse && itemSlot.item.CheckDurability())
                            {
                                // try to add it to the inventory, remove fuel from vehicle if it worked
                                if (player.inventory.Add(new Item(vehicleController.fuelItemData), vehicleFuelingItem.fuelQuantityByUse))
                                {
                                    // reduce durability
                                    itemSlot.item.durability = Mathf.Max(itemSlot.item.durability - 1, 0);
                                    player.inventory.slots[inventoryItemIndex] = itemSlot;
                                    // Remove fuel from vehicle
                                    vehicleController.vehicleFuel -= vehicleFuelingItem.fuelQuantityByUse;
                                }
                            }
                        }
                    }
                }
            }
        }

        [Command]
        public void CmdTryExitVehicle()
        {
            if (enterVehicle.vehicleController.vehicleMaxSpeedToExit == -1f || enterVehicle.vehicleController.GetVehicleSpeed() <= enterVehicle.vehicleController.vehicleMaxSpeedToExit)
            {
                TryExitVehicle();
            }
        }

        // Added in the OnEmpty event on the player character Health component
        [Server]
        public void OnDeath()
        {
            TryExitVehicle();
        }

        [Server]
        private void TryExitVehicle()
        {
            if (player != null)
            {
                if (enterVehicle != null)
                {
                    if (seatId == 0 || !enterVehicle.vehicleController.passengerWaitDriverToUnseat)
                    {
                        enterVehicle.ExitVehicleServer(player);
                    }
                    else
                    {
                        // Check if vehicle is controlled
                        // Seat 0 should be driver .. and null playerObject should mean no driver
                        if (enterVehicle.vehicleData[0].playerObject == null)
                        {
                            enterVehicle.ExitVehicleServer(player);
                        }
                    }
                }
            }
        }

        private void Retrying()
        {
            OnVehicleStringChanged(vehicleString, vehicleString);
        }

        public void OnVehicleStringChanged(string oldVehicleString, string newVehicleString)
        {
            if (string.IsNullOrEmpty(vehicleString))
                return;

            string[] splittedString = vehicleString.Split(',');
            isInsideVehicle = bool.Parse(splittedString[0]);
            playerVehicleId = uint.Parse(splittedString[1]);
            seatId = int.Parse(splittedString[2]);

            if (isInsideVehicle)
            {
                // Disable collision on sit local player
                if (player == Player.localPlayer)
                {
                    //usurvival//player.movement.controllerCollider.isTrigger = true;
                    player.GetComponent<PlayerCharacterControllerMovement>().controllerCollider.isTrigger = true;
                }

                if (isServer)
                {
                    if (NetworkServer.spawned.ContainsKey(playerVehicleId))
                    {
                        enterVehicle = NetworkServer.spawned[playerVehicleId].GetComponent<EnterVehicle>();
                    }
                    else
                    {
                        // When you enter the visibility range of other remote player
                        // If player driving the boat appear before the boat
                        // It will retry each sec until boat is networkedspawned
                        Invoke(nameof(Retrying), 1f);
                    }
                }
                else
                {
                    if (NetworkClient.spawned.ContainsKey(playerVehicleId))
                    {
                        enterVehicle = NetworkClient.spawned[playerVehicleId].GetComponent<EnterVehicle>();
                    }
                    else
                    {
                        // When you enter the visibility range of other remote player
                        // If player driving the boat appear before the boat
                        // It will retry each sec until boat is networkedspawned
                        Invoke(nameof(Retrying), 1f);
                    }
                }

                if (enterVehicle != null)
                {
                    if (enterVehicle.vehicleData[seatId].objectToTakeControl != null)
                    {
                        if (enterVehicle.vehicleData[seatId].objectToTakeControl.GetComponent<VehicleStateMachine>() != null)
                            enterVehicle.vehicleData[seatId].objectToTakeControl.GetComponent<VehicleStateMachine>().vehiclePlayer = player;
                        if (enterVehicle.vehicleData[seatId].objectToTakeControl.GetComponent<PassengerStateMachine>() != null)
                            enterVehicle.vehicleData[seatId].objectToTakeControl.GetComponent<PassengerStateMachine>().passengerPlayer = player;
                        if (enterVehicle.vehicleData[seatId].objectToTakeControl.GetComponent<Combat>() != null)
                            enterVehicle.vehicleData[seatId].objectToTakeControl.GetComponent<Combat>().entity = enterVehicle.GetComponent<Entity>();
                    }

                    positionToFollow = enterVehicle.vehicleData[seatId].seatPosition;

                    if (player == Player.localPlayer)
                    {
                        if (seatId == 0)
                        {
                            // When we control a vehicle, remote player in our scene must not collide with vehicle
                            //enterVehicle.ChangeVehicleCollisionLayer(false);

                            // Start the control of the vehicle
                            // This activate the Update method in the VehicleController
                            enterVehicle.vehicleController.isActive = true;

                            // Make vehicle physic only for the driver
                            if (enterVehicle.blockRigidBody)
                            {
                                if (enterVehicle.useGravity)
                                    enterVehicle.vehicleController.vehicleRigidbody.useGravity = true;
                                enterVehicle.vehicleController.vehicleRigidbody.isKinematic = false;
                            }

                            enterVehicle.vehicleController.EnterVehicleAsDriver();
                        }

                        // Set camera to vehicle position
                        if (enterVehicle.vehicleData[seatId].objectToTakeControl.GetComponent<VehicleLook>() != null)
                            enterVehicle.vehicleData[seatId].objectToTakeControl.GetComponent<VehicleLook>().ReSetupCamera();
                    }
                }
            }
            else
            {
                if (enterVehicle != null)
                {
                    if (player == Player.localPlayer)
                    {
                        if (seatId == 0)
                        {
                            // Same apply on exit, we just revert layer change
                            //enterVehicle.ChangeVehicleCollisionLayer(true);

                            // Stop the control of the vehicle
                            // This stop the Update method in the VehicleController
                            enterVehicle.vehicleController.isActive = false;

                            if (enterVehicle.blockRigidBody)
                            {
                                if (enterVehicle.useGravity)
                                    enterVehicle.vehicleController.vehicleRigidbody.useGravity = false;
                                enterVehicle.vehicleController.vehicleRigidbody.isKinematic = true;
                            }

                            enterVehicle.vehicleController.ExitVehicleAsDriver();
                        }

                        // Return camera to player position
                        //usurvival//player.look.ReSetupCamera();
                        player.GetComponent<PlayerCharacterControllerMovement>().ReSetupCamera();
                    }

                    if (enterVehicle.vehicleData[seatId].objectToTakeControl != null)
                    {
                        if (enterVehicle.vehicleData[seatId].objectToTakeControl.GetComponent<VehicleStateMachine>() != null)
                            enterVehicle.vehicleData[seatId].objectToTakeControl.GetComponent<VehicleStateMachine>().vehiclePlayer = null;
                        if (enterVehicle.vehicleData[seatId].objectToTakeControl.GetComponent<PassengerStateMachine>() != null)
                            enterVehicle.vehicleData[seatId].objectToTakeControl.GetComponent<PassengerStateMachine>().passengerPlayer = null;
                        if (enterVehicle.vehicleData[seatId].objectToTakeControl.GetComponent<Combat>() != null)
                            enterVehicle.vehicleData[seatId].objectToTakeControl.GetComponent<Combat>().entity = enterVehicle.vehicleData[seatId].objectToTakeControl.GetComponent<Entity>();
                    }

                    positionToFollow = null;
                    playerVehicleId = 0;
                    isInsideVehicle = false;
                }

                // Enable collision on stand local player
                if (player == Player.localPlayer)
                {
                    //usurvival//player.movement.controllerCollider.isTrigger = false;
                    player.GetComponent<PlayerCharacterControllerMovement>().controllerCollider.isTrigger = false;
                }
            }
        }

        [Server]
        public void OnRespawn()
        {
            if (vehicleLocalParent != null)
            {
                vehicleLocalParent.ExitVehicleLocalParent(player, true);
                RpcClientOnExitVehicleLocalParent();
            }
        }

        [ClientRpc]
        public void RpcClientOnExitVehicleLocalParent()
        {
            if (vehicleLocalParent != null)
            {
                vehicleLocalParent.ExitVehicleLocalParent(player, true);
            }
        }

        public bool IsPlayerMountedMoving(Transform mount)
        {
            Vector3 localVelocity = mount.InverseTransformDirection(((PlayerCharacterControllerMovement)player.movement).velocity);
            float speed = new Vector2(localVelocity.x, localVelocity.z).magnitude;
            return speed > 0.1f;
        }

        // To lightweight the copy/paste installation code
        // Run from FixedUpdate in the PlayerCharacterControllerMovement.cs
        public Vector3 ManageReceiveDataMultipleMove(Vector3 velocity, NetworkIdentity secondNetIdVehicle, Vector3 firstPositionOffsetVehicle, Vector3 secondPositionOffsetVehicle, MoveState state)
        {
            Vector3 velocityToReturn = velocity;

            // If the secondNetIdVehicle is not the player netIdentity, its because we are in a localparent environment
            if (secondNetIdVehicle != null && netIdentity != secondNetIdVehicle)
            {
                // Enter if we are not already inside
                secondNetIdVehicle.GetComponentInChildren<VehicleLocalParent>().EnterVehicleLocalParent(player);
            }
            else
            {
                // Exit if we are inside
                if (vehicleLocalParent != null)
                    vehicleLocalParent.ExitVehicleLocalParent(player);
            }

            if (vehicleLocalParent != null)
            {
                velocityToReturn = (secondPositionOffsetVehicle - firstPositionOffsetVehicle) / Time.fixedDeltaTime;

                if (state == MoveState.DEAD)
                    velocityToReturn = Vector3.zero;

                lastPositionOffsetVehicle = secondPositionOffsetVehicle;
            }

            return velocityToReturn;
        }

        // To lightweight the copy/paste installation code
        // Run from FixedUpdate in the PlayerCharacterControllerMovement.cs
        public Vector3 ManageReceiveDataSingleMove(Vector3 velocity, NetworkIdentity nextNetIdVehicle, Vector3 nextPositionOffsetVehicle, MoveState state)
        {
            Vector3 velocityToReturn = velocity;

            // If the nextNetIdVehicle is not the player netIdentity, its because we are in a localparent environment
            if (nextNetIdVehicle != null && netIdentity != nextNetIdVehicle)
            {
                nextNetIdVehicle.GetComponentInChildren<VehicleLocalParent>().EnterVehicleLocalParent(player);
            }
            else
            {
                // Exit if we are inside
                if (vehicleLocalParent != null)
                    vehicleLocalParent.ExitVehicleLocalParent(player);
            }

            if (vehicleLocalParent != null)
            {
                velocityToReturn = (nextPositionOffsetVehicle - lastPositionOffsetVehicle) / Time.fixedDeltaTime;

                if (state == MoveState.DEAD)
                    velocityToReturn = Vector3.zero;

                lastPositionOffsetVehicle = nextPositionOffsetVehicle;
            }

            return velocityToReturn;
        }

        private void OnValidate()
        {
            if (syncMode != SyncMode.Observers)
                syncMode = SyncMode.Observers;

            if (syncInterval < 0.1f)
                syncInterval = 0.1f;
        }
    }
}
