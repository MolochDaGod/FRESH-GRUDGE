using UnityEngine;
using Mirror;

// NETWORKED SWITCH
public partial class NetworkedSwitch : Tools_InteractableObject
{
    public GameObject[] activatedObjects;
    public bool visible = true;

    // -----------------------------------------------------------------------------------
    // OnInteractClient
    // @Client
    // -----------------------------------------------------------------------------------
    [ClientCallback]
    public override void OnInteractClient(Player player) { }

    // -----------------------------------------------------------------------------------
    // OnInteractServer
    // @Server
    // -----------------------------------------------------------------------------------
#if _SERVER
    [ServerCallback]
    public override void OnInteractServer(Player player)
    {
        visible = !visible;

        foreach (GameObject go in activatedObjects)
            go.GetComponent<ActivateableObject>().Toggle(visible);
    }
#endif
    // -----------------------------------------------------------------------------------
}