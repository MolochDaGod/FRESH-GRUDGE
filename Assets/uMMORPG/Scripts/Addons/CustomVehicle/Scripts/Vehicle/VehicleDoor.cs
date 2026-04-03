
//(c8

using Mirror;
using UnityEngine;

namespace BurnyDev.UMMORPG.AddonCustomVehicle
{
    public class VehicleDoor : NetworkBehaviour
    {
        [Header("Door")]
        public Animator animator;

        [SyncVar(hook = nameof(OnChangeDoorState))] public bool isOpen = false;

        [Header("Close after time 0 will not close door automaticly")]
        public float closeAfterTime = 0f;

        [ClientCallback]
        private void OnEnable()
        {
            // Run syncvar
            OnChangeDoorState(isOpen, isOpen);
        }

        // Run on client
        private void OnChangeDoorState(bool oldState, bool newState)
        {
            animator.SetBool("Open", newState);
        }

        [Server]
        public void OpenDoor()
        {
            CancelInvoke();
            isOpen = true;
            animator.SetBool("Open", isOpen);

            if (closeAfterTime != 0f)
                Invoke(nameof(CloseDoor), closeAfterTime);
        }

        [Server]
        public void CloseDoor()
        {
            isOpen = false;
            animator.SetBool("Open", isOpen);
        }

        // validation //////////////////////////////////////////////////////////////
        void OnValidate()
        {
            // door colliders move with the animation.
            // door meshes are hidding in host and server-only mode.
            // => door colliders only move if we animate hidden meshes too!
            if (animator != null &&
                animator.cullingMode != AnimatorCullingMode.AlwaysAnimate)
            {
                Debug.LogWarning(name + " animator cull mode needs to be set to Always, otherwise the door collider won't move in host or server-only mode.");
            }
        }

        public void ServerOpenCloseAdditionalInteraction(Player player)
        {
            if (!isOpen)
                OpenDoor();
            else
                CloseDoor();
        }
    }
}
