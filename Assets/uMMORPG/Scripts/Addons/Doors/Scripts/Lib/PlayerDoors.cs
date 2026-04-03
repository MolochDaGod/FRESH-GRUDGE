using UnityEngine;
using Mirror;

#if _iMMODOORS
// PLAYER
public partial class PlayerAddonsConfigurator
{
    [HideInInspector] public Doors selectedDoor;

    // -----------------------------------------------------------------------------------
    // OnSelect_Door
    // @Client
    // -----------------------------------------------------------------------------------
    [Client]
    public void OnSelect_Door(Doors _selectedDoors)
    {
        selectedDoor = _selectedDoors;
        // Check for PlaceableObject component
        if (selectedDoor.TryGetComponent<PlaceableObject>(out PlaceableObject placeableObject))
        {
            if (placeableObject.ownerCharacter != player.name)
            {
                player.Tools_ShowPrompt("You do not have access to this door.");
            }
        }
        Cmd_checkDoorAccess(selectedDoor.gameObject);
    }

    // -----------------------------------------------------------------------------------
    // Cmd_checkDoorAccess
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    protected void Cmd_checkDoorAccess(GameObject _selectedDoor)
    {
        selectedDoor = _selectedDoor.GetComponent<Doors>();

        if (DoorValidation())
        {
            Debug.Log(selectedDoor.hingeState);
            if (selectedDoor.hingeState == Doors.DoorState.closed || selectedDoor.hingeState == Doors.DoorState.opened)
                Target_startDoorAccess(player.connectionToClient);
        }
        else
        {
            if (selectedDoor != null && selectedDoor.checkInteractionRange(player) && selectedDoor.lockedMessage != "")
            {
                player.Tools_ShowPrompt(selectedDoor.lockedMessage);
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // Target_startDoorAccess
    // @Server -> @Client
    // -----------------------------------------------------------------------------------
    [TargetRpc]
    protected void Target_startDoorAccess(NetworkConnection target)
    {
        if (DoorValidation())
        {
            player.Tools_addTask();
            player.Tools_setTimer(selectedDoor.secondsToOpen);
            if (selectedDoor.showProgressBar)
                player.Tools_CastbarShow(selectedDoor.accessLabel, selectedDoor.secondsToOpen);
        }
    }

    // -----------------------------------------------------------------------------------
    // DoorValidation
    // -----------------------------------------------------------------------------------
    public bool DoorValidation()
    {
        // Check if selectedDoor is null
        if (selectedDoor == null)
        {
            return false; // Early exit if there's no door selected
        }

        // Basic validation checks for interaction range and state
        bool bValid = (selectedDoor.checkInteractionRange(player) &&
            selectedDoor.interactionRequirements.checkState(player));

        // Check for PlaceableObject component
        if (selectedDoor.TryGetComponent<PlaceableObject>(out PlaceableObject placeableObject))
        {
            if (placeableObject.ownerCharacter != player.name)
            {
                bValid = false; // Player cannot access the door if they are not the owner
                if (isServer)
                    player.Tools_ShowPrompt("You do not have access to this door.");
            }
        }

        // If the door is not valid, cancel any ongoing tasks
        if (!bValid)
        {
            CancelDoor();
        }

        return bValid;
    }

    // -----------------------------------------------------------------------------------
    // @Client
    // -----------------------------------------------------------------------------------
    [ClientCallback]
    private void LateUpdate_Doors()
    {
        if (DoorValidation() && player.Tools_checkTimer())
        {
            player.Tools_removeTask();
            player.Tools_stopTimer();
            if (selectedDoor.showProgressBar)
                player.Tools_CastbarHide();

            Cmd_finishDoorAccess();
        }
    }

    // -----------------------------------------------------------------------------------
    // cancelDoor
    // -----------------------------------------------------------------------------------
    public void CancelDoor()
    {
        if (selectedDoor != null)
        {
            player.Tools_stopTimer();
            player.Tools_removeTask();
            player.Tools_CastbarHide();
            selectedDoor = null;
        }
    }

    // -----------------------------------------------------------------------------------
    // Cmd_finishDoorAccess
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_finishDoorAccess()
    {
        player.Tools_removeTask();
        player.Tools_stopTimer();
    }
}
#endif