
//(c8

using System.Collections.Generic;
using UnityEngine;

namespace BurnyDev.UMMORPG.AddonCustomVehicle
{
    public class VehicleLocalParent : MonoBehaviour
    {
        // Ref to vehicle entity
        // To have access at component on the vehicle
        public Entity entity;

        // List of player inside
        private List<Player> playerInside;

        // To be able use multiple collider for the VehicleLocalParent area
        // Use VehicleOtherTrigger script
        // Each collider must intersect others in a way player trigger
        // the new collider before exit the one is int it
        private List<int> playerInsideCount;

        private Vector3 lastParentRotation;

        private void Awake()
        {
            // Initialize some list and rotation
            playerInside = new List<Player>();
            playerInsideCount = new List<int>();
            lastParentRotation = transform.eulerAngles;
        }

        // If this gameobject get disable in some way
        // and network dint exit it properly
        // Force exit VehicleLocalParent for each player inside
        // local/server/remote
        private void OnDisable()
        {
            for (int i = 0; i < playerInside.Count; i++)
            {
                ExitVehicleLocalParent(playerInside[i], true);
            }
        }

        // If this gameobject get destroy in some way
        // and network dint exit it properly
        // Force exit VehicleLocalParent for each player inside
        // local/server/remote
        private void OnDestroy()
        {
            for (int i = 0; i < playerInside.Count; i++)
            {
                ExitVehicleLocalParent(playerInside[i], true);
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            Player player = other.GetComponent<Player>();
            if (player == null)
                player = other.GetComponentInParent<Player>();

            // Do nothing if not local player      
            if (player != Player.localPlayer)
                return;

            EnterVehicleLocalParent(player);
        }

        public void OnTriggerExit(Collider other)
        {
            Player player = other.GetComponent<Player>();
            if (player == null)
                player = other.GetComponentInParent<Player>();

            // Do nothing if not local player      
            if (player != Player.localPlayer)
                return;

            ExitVehicleLocalParent(player);
        }

        private void Update()
        {
            for (int i = 0; i < playerInside.Count; i++)
            {
                // If we are not the driver
                if (playerInside[i] != null && ((PlayerCharacterControllerMovement)playerInside[i].movement).state != MoveState.DRIVING)
                {
                    if (playerInside[i] == Player.localPlayer)
                    {
                        // This for local player
                        // Set world position with offsetVehicle
                        playerInside[i].transform.position = transform.TransformPoint(playerInside[i].playerVehicle.offsetVehicle);
                    }
                    else
                    {
                        // This for server and remote player
                        // Set world position with lastPositionOffsetVehicle
                        playerInside[i].transform.position = transform.TransformPoint(playerInside[i].playerVehicle.lastPositionOffsetVehicle);
                    }

                    // Set rotation Y
                    playerInside[i].transform.eulerAngles = new Vector3(playerInside[i].transform.eulerAngles.x, playerInside[i].transform.eulerAngles.y - (lastParentRotation - transform.eulerAngles).y, playerInside[i].transform.eulerAngles.z);
                }
            }

            lastParentRotation = transform.eulerAngles;
        }

        public void EnterVehicleLocalParent(Player player)
        {
            if (player != null)
            {
                if (!playerInside.Contains(player))
                {
                    // If we enter a new VehicleLocalParent and we are in another one (another vehicle)
                    // Here is not about connection/intersection of same vehicle
                    // Force exit from last one before continue
                    if (player.playerVehicle.vehicleLocalParent != null)
                        ExitVehicleLocalParent(player, true);

                    if (player == Player.localPlayer)
                    {
                        // This for local player
                        // To set local position inside vehicle
                        // and to send it to server and remote player
                        player.playerVehicle.offsetVehicle = transform.InverseTransformPoint(player.transform.position);
                    }
                    else
                    {
                        // This for server and remote player
                        // To set local position inside vehicle
                        // and calculate velocity of remote player (animation)
                        player.playerVehicle.lastPositionOffsetVehicle = transform.InverseTransformPoint(player.transform.position);
                        // When remote player enter vehicle area, turn collision off
                        ((PlayerCharacterControllerMovement)player.movement).controllerCollider.isTrigger = true;
                    }

                    // Set variable to ref this VehicleLocalParent
                    // and create list of players inside
                    player.playerVehicle.vehicleLocalParent = this;
                    playerInside.Add(player);
                    playerInsideCount.Add(0);
                }
                // If VehicleLocalParent have multiple collider and player change collider
                // Add 1 to the inside count when we enter the new collider
                // as we was in another collider connected to this one
                else if (player == Player.localPlayer)
                {
                    int index = playerInside.IndexOf(player);
                    playerInsideCount[index] += 1;
                }
            }
        }

        public void ExitVehicleLocalParent(Player player, bool forceExit = false)
        {
            if (player != null && playerInside.Contains(player))
            {
                int index = playerInside.IndexOf(player);

                // If VehicleLocalParent have multiple collider and player change collider
                // Remove 1 to the inside count when we exit the last collider and stop
                // as we are in another collider connected to this one
                if (player == Player.localPlayer && !forceExit && playerInsideCount[index] > 0)
                {
                    playerInsideCount[index] -= 1;
                    return;
                }

                if (player != Player.localPlayer)
                {
                    // When remote player exit vehicle area, return collision on
                    ((PlayerCharacterControllerMovement)player.movement).controllerCollider.isTrigger = false;
                }

                // If we exit a VehicleLocalParent collider and we are not in a new one
                // Clean ref and variable
                player.playerVehicle.vehicleLocalParent = null;
                playerInside.RemoveAt(index);
                playerInsideCount.RemoveAt(index);
            }
        }
    }
}
