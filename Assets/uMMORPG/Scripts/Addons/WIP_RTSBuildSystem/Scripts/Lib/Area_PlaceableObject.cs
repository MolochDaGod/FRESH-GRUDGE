using Mirror;
using UnityEngine;

// ===================================================================================
// PLACEABLE OBJECT AREA
// ===================================================================================
[RequireComponent(typeof(SphereCollider))]
public class Area_PlaceableObject : NetworkBehaviour
{
    [Tooltip("One click deactivation")]
    public bool isActive = true;

    [Tooltip("Only objects with the same ID can be placed in this area")]
    public int areaId;

    // -------------------------------------------------------------------------------
    // OnTriggerEnter
    // -------------------------------------------------------------------------------
    private void OnTriggerEnter(Collider co)
    {
        string areaOwnerName = "";
        string areaGuildName = "";

        Player e = co.GetComponentInParent<Player>();

        if (e != null && isActive)
        {
            PlaceableObject o = GetComponent<PlaceableObject>();

            if (o && o.ownerCharacter != "")
                areaOwnerName = o.ownerCharacter;

            if (o && o.ownerGuild != "")
                areaGuildName = o.ownerGuild;

            e.playerRTSBuildSystem.SetPlaceableObjectArea(areaId, areaOwnerName, areaGuildName, true);
        }
    }

    // -------------------------------------------------------------------------------
    // OnTriggerExit
    // -------------------------------------------------------------------------------
    private void OnTriggerExit(Collider co)
    {
        string areaOwnerName = "";
        string areaGuildName = "";

        Player e = co.GetComponentInParent<Player>();

        if (e != null && isActive)
            e.playerRTSBuildSystem.SetPlaceableObjectArea(areaId, areaOwnerName, areaGuildName, false);
    }
}
