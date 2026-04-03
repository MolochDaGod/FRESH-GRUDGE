
//(c8

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BurnyDev.UMMORPG.AddonCustomVehicle
{
    public class AdjustRemoteWheelHeight : MonoBehaviour
    {
        public EnterVehicle enterVehicle;
        public Transform toAdjust;
        public LayerMask layerMask;
        public bool isGrounded = false;

        private float dist;
        private Vector3 newPosition;
        private Vector3 defaultLocalPosition;

        private void Start()
        {
            defaultLocalPosition = toAdjust.localPosition;
        }

        private void FixedUpdate()
        {
            if (Player.localPlayer != null)
            {
                if (!Player.localPlayer.playerVehicle.controlVehicle || (Player.localPlayer.playerVehicle.controlVehicle && Player.localPlayer.playerVehicle.enterVehicle != enterVehicle))
                {
                    newPosition = new Vector3(transform.position.x, transform.position.y - 0.6f, transform.position.z);

                    if (Physics.Linecast(transform.position, newPosition, out RaycastHit hitInfo, layerMask))
                    {
                        isGrounded = true;
                        dist = Vector3.Distance(hitInfo.point, transform.position);

                        toAdjust.localPosition = new Vector3(toAdjust.localPosition.x, defaultLocalPosition.y + (0.6f - dist), toAdjust.localPosition.z);
                    }
                    else
                    {
                        isGrounded = false;
                    }
                }
            }
        }
    }
}
