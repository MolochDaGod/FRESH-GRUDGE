using System;
using UnityEngine;
using Mirror;

// objects need position offsets for some grid sizes.
// (e.g. if grid = 1 and we want to build a thin palette, it should not be in
//  the center of a unit cube, but on the sides)
[Serializable]
public struct CustomRotation
{
    public Vector3 positionOffset;
    public Vector3 rotation;
}

[CreateAssetMenu(menuName = "uSurvival Item/Structure", order = 999)]
public class StructureItem : UsableItem
{
    // shown while deciding where to build
    [Header("Structure")]
    public GameObject structurePrefab;

    // show the preview 'n' units away from player.
    // -> for example, a huge building needs to be shown at least size/2 away
    //    from player
    public float previewDistance = 2;

    // resolution 1: 1,2,3,4,...
    // resolution 2: 0.5, 1, 1.5, 2, ...
    // note: modify availableRotations[] position offsets to fit grid if needed
    [Range(1, 10)] public int gridResolution = 1;

    // rotation key switches through available rotations
    public CustomRotation[] availableRotations = { new CustomRotation() };

    // can build checks
    [Range(0, 1)] public float buildToleranceCollision = 0.1f;
    [Range(0, 1)] public float buildToleranceAir = 0.1f;

    // can't use it from inventory. need to aim where to build it
    public override bool CanUse(Player player, int inventoryIndex)
    {
        // check base usability first (cooldown etc.)
        bool baseUsable = base.CanUse(player, inventoryIndex);
        if (baseUsable != true)
        {
            return false;
        }

        Vector3 position = player.construction.CalculatePreviewPosition(this, player.transform.position);
        Quaternion rotation = player.construction.CalculatePreviewRotation();

        Vector3 prefabPosition = structurePrefab.transform.position;
        Quaternion prefabRotation = structurePrefab.transform.rotation;
        structurePrefab.transform.position = position;
        structurePrefab.transform.rotation = rotation;
        Bounds bounds = structurePrefab.GetComponentInChildren<Renderer>().bounds;
        structurePrefab.transform.position = prefabPosition;
        structurePrefab.transform.rotation = prefabRotation;
        player.construction.CurrentStructure = this;
        player.construction.inventoryIndex = inventoryIndex;
        return CanBuildThere(player.transform.position, bounds, player.construction.raycastLayers); // for empty sound. better than 'Never'.
    }

    // [Server] Use logic
    public override void Use(Player player, int inventoryIndex)
    {
        // always call base function too
        base.Use(player, inventoryIndex);


        if (Vector3.Distance(player.transform.position, player.construction.GetPosition()) < 20 && !Utils.IsCursorOverUserInterface())
        {

            GameObject go = Instantiate(structurePrefab, player.construction.GetPosition(), player.construction.CalculatePreviewRotation());
            go.name = structurePrefab.name; // avoid "(Clone)". important for saving..
            NetworkServer.Spawn(go);

            // decrease amount
            ItemSlot slot = player.inventory.slots[inventoryIndex];
            slot.DecreaseAmount(1);
            player.inventory.slots[inventoryIndex] = slot;
        }


        // spawn it into the world
    }


    // can we build the structure at this position with this rotation?
    // -> having CanBuildAt in here instead of in Construction script allows for
    //    custom structures like windows that can only be built between walls
    public virtual bool CanBuildThere(Vector3 headPosition, Bounds bounds, LayerMask raycastLayers)
    {
        // nothing at this position yet?
        // => we check 90% of size so things can at least barely touch each other
        if (Physics.CheckBox(bounds.center, bounds.extents * (1 - buildToleranceCollision), Quaternion.identity, raycastLayers))
            return false;

        // not floating in air?
        // => needs to touch anything else if 110% of collider
        if (!Physics.CheckBox(bounds.center, bounds.extents * (1 + buildToleranceAir), Quaternion.identity, raycastLayers))
            return false;

        // linecast to make sure that nothing is between us and the build preview
        return !Physics.Linecast(headPosition, bounds.center, raycastLayers);
    }
}