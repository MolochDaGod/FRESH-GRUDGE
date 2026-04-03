
//(c8

using Mirror;
using UnityEngine;

[CreateAssetMenu(menuName= "bMMORPG Item/VehicleItem", order=999)]
public class VehicleItem : UsableItem
{
    [Header("Vehicle")]
    public GameObject vehicleToSpawn;

    // usage
    public override void Use(Player player, int inventoryIndex)
    {
        // always call base function too
        base.Use(player, inventoryIndex);

        GameObject go = Instantiate(vehicleToSpawn, player.transform.position + (player.transform.forward * 10f), Quaternion.identity);
        go.name = vehicleToSpawn.name; // avoid "(Clone)"
        NetworkServer.Spawn(go);
    }

    // usage
    //public override void UseInventory(player player, int inventoryIndex)
    //{
    //    // always call base function too
    //    base.UseInventory(player, inventoryIndex);

    //    GameObject go = Instantiate(vehicleToSpawn, player.transform.position, Quaternion.identity);
    //    go.name = vehicleToSpawn.name; // avoid "(Clone)"
    //    NetworkServer.Spawn(go);
    //}

    //public override void UseHotbar(player player, int hotbarIndex, Vector3 lookAt)
    //{
    //    // call base function to start cooldown
    //    base.UseHotbar(player, hotbarIndex, lookAt);

    //    GameObject go = Instantiate(vehicleToSpawn, player.transform.position, Quaternion.identity);
    //    go.name = vehicleToSpawn.name; // avoid "(Clone)"
    //    NetworkServer.Spawn(go);
    //}
}
