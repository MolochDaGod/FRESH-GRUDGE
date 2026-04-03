
//(c8

using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BurnyDev.UMMORPG.AddonCustomVehicle
{
    public class UIDragAndDropableVehicle : MonoBehaviour, IDropHandler
    {
        // drag options
        public PointerEventData.InputButton button = PointerEventData.InputButton.Left;

        // status
        public bool dropable = true;

        // d.pointerDrag is the object that was dragged
        public void OnDrop(PointerEventData d)
        {
            // one mouse button is enough for drag and drop
            if (dropable && d.button == button)
            {
                // was the dropped GameObject a UIDragAndDropable and was it dragable?
                // (Unity calls OnDrop even if .dragable was false)
                UIDragAndDropable dropDragable = d.pointerDrag.GetComponent<UIDragAndDropable>();
                if (dropDragable != null && dropDragable.dragable)
                {
                    // let the dragable know that it was dropped onto a slot
                    dropDragable.draggedToSlot = true;

                    // only do something if we didn't drop it on itself. this way we
                    // don't have to ignore raycasts etc.
                    // message is sent to drag and drop handler for game specifics
                    if (dropDragable != this)
                    {
                        //Debug.LogWarning("OnDragAndDrop_" + dropDragable.tag + "_" + tag);
                        // send a drag and drop message like
                        // OnDragAndDrop_Skillbar_Inventory({from, to})
                        int from = dropDragable.name.ToInt();
                        int to = name.ToInt();
                        Player.localPlayer.playerVehicle.CmdDropItemOnVehicle(GetComponentInParent<NetworkIdentity>(), from);
                        //player.localPlayer.SendMessage("OnDragAndDrop_" + dropDragable.tag + "_" + tag,
                        //                               new int[] { from, to },
                        //                               SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
        }
    }
}
