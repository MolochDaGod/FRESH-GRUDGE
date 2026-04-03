
//(c8

using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BurnyDev.UMMORPG.AddonCustomVehicle
{
    public class VehicleController : NetworkBehaviour
    {
        [HideInInspector] public bool driverSeated = false;

        public EnterVehicle enterVehicle;

        [HideInInspector] public bool isActive = false;

        [HideInInspector] public float motorBoost = 0f;

        [Header("This is for steering control, false = keyb, true = mouse")]
        public bool useMouseDirection = false;
        [Header("Control by mouse dead center for steering")]
        public float deadCenter = 0.05f;
        [Header("Control by mouse distance from deadcenter to max. 2 = deadcenter and half way from screen.")]
        public float mouseRange = 2f;

        [HideInInspector] public float mouseX, mouseY;
        [HideInInspector] public float xExtra, yExtra;

        [HideInInspector] public float motorDirection = 0f;
        [HideInInspector] public float steeringDirection = 0f;
        [HideInInspector] public float driveHandBrake = 0f;
        [HideInInspector] public bool driveHandBrakeUp = false;

        // TODO : Not use
        [HideInInspector] public float pitch = 0f;
        [HideInInspector] public float yaw = 0f;
        [HideInInspector] public float roll = 0f;

        public Rigidbody vehicleRigidbody;
        public Transform centerOfGravity;

        [Header("Vehicle max speed km/h to be able exit, -1 to disable")]
        public float vehicleMaxSpeedToExit = 3f;
        [Header("Need driver standup before passenger?")]
        public bool passengerWaitDriverToUnseat = false;

        // Vehicle fuel system
        [Header("Vehicle use fuel system?")]
        public bool vehicleNeedFuel = false;
        [Header("Steering need fuel to work?")]
        public bool steeringUseFuel = false;
        [Header("Vehicle actual fuel in tank")]
        [SyncVar] public float vehicleFuel = 80f;
        [Header("Vehicle max fuel in tank")]
        public float vehicleMaxFuel = 85f;
        [Header("Fuel use by second for idle")]
        public float fuelUseOnIdle = 0.01f;
        [Header("Fuel use by second on move")]
        public float fuelUseOnMove = 0.05f;
        [Header("Fuel item")]
        // default itemData, can be assigned in Inspector
#pragma warning disable CS0649 // Field is never assigned to
        [SerializeField] public ScriptableItem fuelItemData;
#pragma warning restore CS0649 // Field is never assigned to
        // Use to time fuel use
        private float fuelUseInterval = 1f;
        private float lastUseFuelCheck;
        [Header("Extract fuel item")]
        // default itemData, can be assigned in Inspector
#pragma warning disable CS0649 // Field is never assigned to
        [SerializeField] public ScriptableItem extractFuelItemData;
#pragma warning restore CS0649 // Field is never assigned to

        private Vector3 oldPos = Vector3.zero;

        public virtual void Start()
        {
            if (vehicleRigidbody == null)
                vehicleRigidbody = GetComponent<Rigidbody>() ?? GetComponentInParent<Rigidbody>();

            if (vehicleRigidbody == null)
            {
                Debug.LogError($"{name}: VehicleController requires a Rigidbody reference.", this);
                enabled = false;
                return;
            }

            if (centerOfGravity != null)
                vehicleRigidbody.centerOfMass = centerOfGravity.localPosition;
        }

        public virtual void ResetSyncVar()
        {

        }

        public virtual bool IsGrounded(bool controlled = true)
        {
            return false;
        }

        public bool HasFuel()
        {
            return vehicleFuel > 0f || vehicleMaxFuel == 0f;
        }

        public void UseFuel(bool moving)
        {
            // If vehicule use fuel
            if (vehicleNeedFuel)
            {
                if (Time.time - lastUseFuelCheck >= fuelUseInterval)
                {
                    vehicleFuel -= !moving ? fuelUseOnIdle : fuelUseOnMove;

                    if (vehicleFuel < 0f)
                        vehicleFuel = 0f;

                    lastUseFuelCheck = Time.time;
                }
            }
        }

        public void ChangeMaxSpeed(float speedTime, float speedBoost)
        {
            motorBoost = speedBoost;

            Invoke(nameof(ReturnNormalMaxSpeed), speedTime);
        }

        public void ReturnNormalMaxSpeed()
        {
            motorBoost = 0f;
        }

        public void SetMotor(float value)
        {
            motorDirection = value;
        }

        public void SetSteering(float value)
        {
            steeringDirection = value;
        }

        // TODO : Not use
        public void SetPitch(float value)
        {
            pitch = value;
        }

        // TODO : Not use
        public void SetYaw(float value)
        {
            yaw = value;
        }

        // TODO : Not use
        public void SetRoll(float value)
        {
            roll = value;
        }

        public virtual void Update()
        {
            if (!isActive || isServerOnly || !isOwned)
                return;

            // When control vehicle by mouse
            if (useMouseDirection)
            {
                var mousePos = Camera.main.ScreenToViewportPoint(Input.mousePosition);

                // calculate horizontal and vertical rotation steps
                if (HasFuel() && (mousePos.x > (0.5f + deadCenter) || mousePos.x < (0.5f - deadCenter)))
                {
                    if (mousePos.x > (0.5f + deadCenter))
                        xExtra = (-((0.5f + deadCenter) - mousePos.x)) * (1f / (1f - (0.5f + deadCenter))) * mouseRange;
                    if (mousePos.x < (0.5f - deadCenter))
                        xExtra = (-((0.5f - deadCenter) - mousePos.x)) * (1f / (1f - (0.5f + deadCenter))) * mouseRange;
                    if (xExtra > 1f)
                        xExtra = 1f;
                }
                else
                {
                    xExtra = 0f;
                }

                if (HasFuel() && (mousePos.y > 0.6f || mousePos.y < 0.4f))
                {
                    if (mousePos.y > (0.5f + deadCenter))
                        yExtra = (-((0.5f + deadCenter) - mousePos.y)) * (1f / (1f - (0.5f + deadCenter))) * mouseRange;
                    if (mousePos.y < (0.5f - deadCenter))
                        yExtra = (-((0.5f - deadCenter) - mousePos.y)) * (1f / (1f - (0.5f + deadCenter))) * mouseRange;
                    if (yExtra > 1f)
                        yExtra = 1f;
                }
                else
                {
                    yExtra = 0f;
                }
            }
        }

        public virtual float GetVehicleSpeed()
        {
            return 0f;
        }

        public virtual void EnterVehicleAsDriver()
        {
            driverSeated = true;
        }

        public virtual void ExitVehicleAsDriver()
        {
            driverSeated = false;
        }
    }
}
