
//(c8

using Mirror;
using UnityEngine;

namespace BurnyDev.UMMORPG.AddonCustomVehicle
{
    public class DoorOpenClose : MonoBehaviour//, IAdditionalInteractable
    {
        public VehicleDoor objectToControl;

        //#region IAdditionalInteractable interface

        //public float HoldActionTimeForSecondaryInteraction => 0.0f;

        //public string GetAdditionalInteractionText()
        //{
        //    return !objectToControl.isOpen ? "Open" : "Close";
        //}

        //public void OnAdditionalInteractClient(player player) { }

        //public void OnAdditionalSecondaryInteractClient(player player) { }

        //public void OnAdditionalInteractServer(player player)
        //{
        //    objectToControl.ServerOpenCloseAdditionalInteraction(player);
        //}

        //public void OnAdditionalSecondaryInteractServer(player player) { }

        //public string GetAdditionalInfoText()
        //{
        //    return "";
        //}

        //#endregion

        // selection & interaction /////////////////////////////////////////////////
        // use Unity's OnMouseDown function. no need for raycasts.
        void OnMouseDown()
        {
            // joined world yet? (not character selection?)
            // not over UI? (avoid targeting through windows)
            // and in a state where local player can select things?
            if (Player.localPlayer != null &&
                !Utils.IsCursorOverUserInterface() &&
                (Player.localPlayer.state == "IDLE" ||
                 Player.localPlayer.state == "MOVING"))
            {
                // clear requested skill in any case because if we clicked
                // somewhere else then we don't care about it anymore
                //player.localPlayer.useSkillWhenCloser = -1;

                // alive, close enough? then talk
                // use collider point(s) to also work with big entities
                if (Player.localPlayer.health.current > 0/* &&
                    ClosestDistance(player.localPlayer.transform.position, player.localPlayer.collider, transform.position, GetComponent<Collider>()) <= player.localPlayer.interactionRange*/)
                {
                    Player.localPlayer.GetComponent<PlayerVehicle>().CmdTryOpenDoorVehicle(GetComponentInParent<NetworkIdentity>());
                }
                // otherwise walk there
                // use collider point(s) to also work with big entities
                //else
                //{
                //    Vector3 destination = ClosestPoint(transform.position, GetComponent<Collider>(), player.localPlayer.transform.position);
                //    player.localPlayer.movement.Navigate(destination, player.localPlayer.interactionRange);
                //}
            }
        }
    }
}
