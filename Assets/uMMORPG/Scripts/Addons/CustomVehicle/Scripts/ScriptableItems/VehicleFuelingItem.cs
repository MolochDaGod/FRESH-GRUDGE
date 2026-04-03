
//(c8

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "bMMORPG Item/VehicleFuelingItem", order = 999)]
public class VehicleFuelingItem : UsableItem
{
    //[Header("Fueling")]
    //public float fuelingRange = 5f;
    //[Header("If not filling is extracting")]
    //public bool filling = false;
    //[Header("Must be the name of the gameobject to fill")]
    //public string fullingCapName = "VehicleFuel";
    [Header("Fuel added and extracted each time item use")]
    public int fuelQuantityByUse = 1;

    //[Header("Physical Interaction")]
    //public LayerMask raycastLayers = Physics.DefaultRaycastLayers;

    // usage
    //public override void UseInventory(player player, int inventoryIndex)
    //{
    //    // Do nothing
    //}

    //public override void UseHotbar(player player, int hotbarIndex, Vector3 lookAt)
    //{
    //    // Cooldown
    //    base.UseHotbar(player, hotbarIndex, lookAt);

    //    // raycast to find out what we hit
    //    if (RaycastToLookAt(player, lookAt, out RaycastHit hit))
    //    {
    //        // Hit vehicle?
    //        if (hit.transform.name == fullingCapName)
    //        {
    //            EnterVehicle enterVehicle = hit.transform.GetComponentInParent<EnterVehicle>();
    //            if (enterVehicle != null)
    //            {
    //                ItemSlot slot = player.hotbar.slots[hotbarIndex];

    //                if (filling)
    //                {
    //                    // decrease amount
    //                    if (enterVehicle.vehicleController.vehicleFuel < enterVehicle.vehicleController.vehicleMaxFuel - addedFuelQuantity && slot.DecreaseAmount(addedFuelQuantity) > 0)
    //                    {
    //                        player.hotbar.slots[hotbarIndex] = slot;

    //                        // Add fuel to vehicle
    //                        enterVehicle.vehicleController.vehicleFuel += addedFuelQuantity;
    //                    }
    //                }
    //                else
    //                {
    //                    // vehicle fuel quantity and check item use durability
    //                    if (enterVehicle.vehicleController.vehicleFuel >= extractedFuelQuantity && slot.item.CheckDurability())
    //                    {
    //                        // try to add it to the inventory, remove fuel from vehicle if it worked
    //                        if (player.inventory.Add(new Item(enterVehicle.vehicleController.fuelItemData), extractedFuelQuantity))
    //                        {
    //                            // reduce durability
    //                            slot.item.durability = Mathf.Max(slot.item.durability - 1, 0);
    //                            player.hotbar.slots[player.hotbar.selection] = slot;

    //                            // Remove fuel from vehicle
    //                            enterVehicle.vehicleController.vehicleFuel -= extractedFuelQuantity;
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}

    //protected bool RaycastToLookAt(player player, Vector3 lookAt, out RaycastHit hit)
    //{
    //    // start raycast at head bone, not at muzzle location
    //    // (starting at muzzle would be physically correct, but if we lookat
    //    //  something between head and muzzle, we'd shoot backwards from
    //    //  muzzle towards head, which would cause walls being shot from the
    //    //  other side etc.)
    //    // (also makes sure that we can never shoot through walls by sticking a
    //    //  weapon through a wall)
    //    Transform head = player.animator.GetBoneTransform(HumanBodyBones.Head);

    //    // ignore self just to be sure
    //    Vector3 direction = lookAt - head.position;
    //    Debug.DrawLine(head.position, direction, Color.yellow, 1);
    //    if (Utils.RaycastWithout(head.position, direction, out hit, fuelingRange, player.gameObject, raycastLayers))
    //    {
    //        // don't hit anything between
    //        // show ray for debugging
    //        Debug.DrawLine(head.position, hit.point, Color.red, 1);
    //        Debug.DrawLine(hit.point, hit.point + hit.normal, Color.blue, 1);
    //        return true;
    //    }

    //    hit = new RaycastHit();
    //    return false;
    //}
}
