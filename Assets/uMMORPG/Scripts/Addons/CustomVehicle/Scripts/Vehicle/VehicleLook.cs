
//(c8

using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BurnyDev.UMMORPG.AddonCustomVehicle
{
    public class VehicleLook : NetworkBehaviour
    {
        public NetworkIdentity vehicleNetworkIdentity;
        [Header("Only when setting up a passenger seat")]
        public PassengerFollowVehicle passengerFollowVehicle;

#pragma warning disable CS0109 // member does not hide accessible member
        new Camera camera;
#pragma warning restore CS0109 // member does not hide accessible member

        private float lastClientSendTime;

        [Header("Camera")]
        public float XSensitivity = 2;
        public float YSensitivity = 2;
        public float MinimumX = -90;
        public float MaximumX = 90;

        public Transform headPosition;

        private Vector3 originalCameraPosition;

        public LayerMask viewBlockingLayers;
        public float zoomSpeed = 0.5f;
        public float distance = 0;
        public float minDistance = 0;
        public float maxDistance = 7;

        [Header("Physical Interaction")]
        [Tooltip("Layers to use for raycasting. Check Default, Walls, player, Monster, Doors, Interactables, Item, etc. Uncheck IgnoreRaycast, AggroArea, Water, UI, etc.")]
        public LayerMask raycastLayers = Physics.DefaultRaycastLayers;

        [Header("Offsets - Standing")]
        public Vector2 firstPersonOffsetStanding = Vector2.zero;
        public Vector2 thirdPersonOffsetStanding = Vector2.up;
        public Vector2 thirdPersonOffsetStandingMultiplier = Vector2.zero;

        [SyncVar, HideInInspector] public Vector3 syncedLookDirectionFar;

        public Vector3 lookDirectionFar
        {
            get
            {
                return isLocalPlayer ? camera.transform.forward : syncedLookDirectionFar;
            }
        }

        //[SyncVar, HideInInspector] Vector3 syncedLookDirectionRaycasted; not needed atm, see lookPositionRaycasted comment
        public Vector3 lookDirectionRaycasted
        {
            get
            {
                // same for local and other players
                // (positionRaycasted uses camera || syncedDirectionRaycasted anyway)
                return (lookPositionRaycasted - headPosition.position).normalized;
            }
        }

        // the far position, directionFar projected into nirvana
        public Vector3 lookPositionFar
        {
            get
            {
                Vector3 position = isLocalPlayer ? camera.transform.position : headPosition.position;
                return position + lookDirectionFar * 9999f;
            }
        }

        // the raycasted position is needed for lookDirectionRaycasted calculation
        // and for firing, so we might as well reuse it here
        public Vector3 lookPositionRaycasted
        {
            get
            {
                if (Player.localPlayer != null && isOwned)
                {
                    // raycast based on position and direction, project into nirvana if nothing hit
                    // (not * infinity because might overflow depending on position)
                    // -> without self to ignore own body parts etc.
                    return Utils.RaycastWithout(camera.transform.position, camera.transform.forward, out RaycastHit hit, Mathf.Infinity, gameObject, raycastLayers)
                           ? hit.point
                           : lookPositionFar;
                }
                else
                {
                    // the only person to need the raycast direction is the local player (right now).
                    // we use the far direction for other player's animations and we pass a the
                    // raycast direction to the server when using items. there should be no reason to
                    // sync those 12 extra bytes over the network, so let's just show an error here
                    // => can still use the below code if necessary some day
                    //return Utils.RaycastWithout(firstPersonParent.position, syncedLookDirectionRaycasted, out hit, Mathf.Infinity, gameObject, raycastLayers)
                    //       ? hit.point
                    //       : lookPositionFar;
                    Debug.LogError("VehicleLook.lookPositionRaycasted isn't synced so you can only call it on the local vehicle right now.\n" + Environment.StackTrace);
                    return Vector3.zero;
                }
            }
        }

        private void Awake()
        {
            camera = Camera.main;
        }

        public void ReSetupCamera()
        {
            // set camera parent to player
            camera.transform.SetParent(headPosition, false);

            // look into vehicle forward direction
            camera.transform.rotation = headPosition.rotation;

            // set camera to head position
            camera.transform.position = headPosition.position;

            // remember original camera position
            originalCameraPosition = camera.transform.localPosition;
        }

        [Command]
        void CmdSetLookDirection(Vector3 lookDirectionFar)//, Vector3 directionRaycasted)
        {
            //syncedLookDirectionFar = directionFar;
            //syncedLookDirectionRaycasted = directionRaycasted; <- not needed atm, see syncPositionRaycasted comment
            syncedLookDirectionFar = lookDirectionFar;
        }

        private void Update()
        {
            //if (!isLocalPlayer) return;
            if (Player.localPlayer == null || !isOwned ||
                !Player.localPlayer.playerVehicle.IsInsideVehicle() ||
                (vehicleNetworkIdentity != null && Player.localPlayer.playerVehicle.playerVehicleId != vehicleNetworkIdentity.netId) ||
                (passengerFollowVehicle != null && passengerFollowVehicle.enterVehicle != null && Player.localPlayer.playerVehicle.playerVehicleId != passengerFollowVehicle.enterVehicle.netId))
            {
                return;
            }

            // send only each 'sendinterval', otherwise we send at whatever
            // the player's tick rate is, which is like DDOS
            // (SendInterval doesn't seem to apply to Cmd, so we have to do
            //  it manually)
            if (Time.time - lastClientSendTime >= syncInterval)
            {
                // sync direction if changed
                if (Vector3.Distance(lookDirectionFar, syncedLookDirectionFar) > Mathf.Epsilon)
                    CmdSetLookDirection(lookDirectionFar); //, lookDirectionRaycasted);

                lastClientSendTime = Time.time;
            }

            // only while alive and while cursor is locked, otherwise we are in a UI
            if (Input.GetMouseButton(1)/*Cursor.lockState == CursorLockMode.Locked*/)
            {
                // calculate horizontal and vertical rotation steps
                float xExtra = Input.GetAxis("Mouse X") * XSensitivity;
                float yExtra = Input.GetAxis("Mouse Y") * YSensitivity;

                // set to player parent already?
                //if (camera.transform.parent != transform)
                //    InitializeForcedLook();

                // rotate character for horizontal, rotate camera for vertical
                //transform.Rotate(new Vector3(0, xExtra, 0));
                camera.transform.parent.Rotate(new Vector3(0, xExtra, 0));
                camera.transform.Rotate(new Vector3(-yExtra, 0, 0));
            }
        }

        // Update camera position after everything else was updated
        private void LateUpdate()
        {
            //if (!isLocalPlayer) return;

            if (Player.localPlayer == null || !isOwned ||
                !Player.localPlayer.playerVehicle.IsInsideVehicle() ||
                (vehicleNetworkIdentity != null && Player.localPlayer.playerVehicle.playerVehicleId != vehicleNetworkIdentity.netId) ||
                (passengerFollowVehicle != null && passengerFollowVehicle.enterVehicle != null && Player.localPlayer.playerVehicle.playerVehicleId != passengerFollowVehicle.enterVehicle.netId))
            {
                return;
            }

            // clamp camera rotation automatically. this way we can rotate it to
            // whatever we like in Update, and LateUpdate will correct it.
            camera.transform.localRotation = Utils.ClampRotationAroundXAxis(camera.transform.localRotation, MinimumX, MaximumX);

            // zoom after rotating, otherwise it won't be smooth and would overwrite
            // each other.

            // zoom should only happen if not in a UI right now
            if (!Utils.IsCursorOverUserInterface())
            {
                float step = Utils.GetZoomUniversal() * zoomSpeed;
                distance = Mathf.Clamp(distance - step, minDistance, maxDistance);
            }

            // calculate target and zoomed position
            if (distance == 0) // first person
            {
                // we use the current head bone position as origin here
                // -> gets rid of the idle->run head change effect that was odd
                // -> gets rid of upper body culling issues when looking downwards
                Vector3 headLocal = transform.InverseTransformPoint(headPosition.position);
                Vector3 origin = Vector3.zero;
                Vector3 offset = Vector3.zero;

                origin = headLocal;
                offset = firstPersonOffsetStanding;

                // set final position
                Vector3 target = transform.TransformPoint(origin + offset);
                camera.transform.position = target;
            }
            else // third person
            {
                Vector3 origin = Vector3.zero;
                Vector3 offsetBase = Vector3.zero;
                Vector3 offsetMult = Vector3.zero;

                origin = originalCameraPosition;
                offsetBase = thirdPersonOffsetStanding;
                offsetMult = thirdPersonOffsetStandingMultiplier;

                Vector3 target = headPosition.TransformPoint(origin + offsetBase + offsetMult * distance);
                Vector3 newPosition = target - (camera.transform.rotation * Vector3.forward * distance);

                // avoid view blocking (only third person, pointless in first person)
                // -> always based on original distance and only overwrite if necessary
                //    so that we dont have to zoom out again after view block disappears
                // -> we cast exactly from cam to target, which is the crosshair position.
                //    if anything is inbetween then view blocking changes the distance.
                //    this works perfectly.
                float finalDistance = distance;
                Debug.DrawLine(target, camera.transform.position, Color.white);
                if (Physics.Linecast(target, newPosition, out RaycastHit hit, viewBlockingLayers))
                {
                    // calculate a better distance (with some space between it)
                    finalDistance = Vector3.Distance(target, hit.point) - 0.1f;
                    Debug.DrawLine(target, hit.point, Color.red);
                }
                else Debug.DrawLine(target, newPosition, Color.green);

                // set final position
                camera.transform.position = target - (camera.transform.rotation * Vector3.forward * finalDistance);
            }
        }

        public void InitializeForcedLook()
        {
            camera.transform.SetParent(transform, false);
        }
    }
}
