
//(c8

using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BurnyDev.UMMORPG.AddonCustomVehicle
{
    public class SpaceShipController : VehicleController
    {
        // Local variable on client controlling the vehicle
        private float backThrusterLocal = 0f;
        // Server sync this value when he receive it, use by remote client
        [SyncVar] private float backThrusterSync = 0f;
        [SyncVar] private float velocityMagnitudeSync = 0f;
        // Use to time client send to server data
        private float lastClientSendToServerTime;

        [Header("Drop on ground when no fuel?")]
        public bool noFuelDropOnGround = false;
        public LayerMask canStopFallLayerMask = Physics.DefaultRaycastLayers;
        public float transformYPosMinusRange = 0.6f;
        public float changeMassToWhenDropping = 20f;
        public float changeDragToWhenDropping = 0.2f;
        private float defaultRigidbodyMass;
        private float defaultRigidbodyDrag;

        [Header("Vehicle keys")]
        public KeyCode goUp = KeyCode.Space;
        public KeyCode goDown = KeyCode.Z;
        public KeyCode goRollLeft = KeyCode.N;
        public KeyCode goRollRight = KeyCode.M;
        public KeyCode goPitchForward = KeyCode.V;
        public KeyCode goPitchBackward = KeyCode.B;
        public KeyCode goYawLeft = KeyCode.Q;
        public KeyCode goYawRight = KeyCode.E;
        public KeyCode goAutoForward = KeyCode.O;
        // Auto move forward
        private bool autoForward = false;

        [Header("Vehicle sound")]
        public AudioSource engineSound;
        public float pitchFraction = 45f;
        private float defaultEnginePitch = 1f;

        [Header("Vehicle speed")]
        public float forwardSpeed = 10f;
        public float reverseSpeed = 5f;
        public float turnSpeed = 10f;
        public float upDownSpeed = 10f;
        [Header("Vehicle roll auto return speed")]
        public float rollAutoReturnSpeed = 10f;
        [Header("Invert pitch?")]
        public bool invertPitch = false;

        public ParticleSystem[] backThrusterEffect;

        [HideInInspector] public float speedOnKmh;
        [HideInInspector] public float speedOnMph;

        public override void Start()
        {
            base.Start();

            if (engineSound != null)
                defaultEnginePitch = engineSound.pitch;

            defaultRigidbodyMass = vehicleRigidbody.mass;
            defaultRigidbodyDrag = vehicleRigidbody.drag;
        }

        public override void Update()
        {
            // Network sync on host/client that do not control a vehicle
            if (!isActive && !isServerOnly)
            {
                // They use the SyncVar value, updated from server
                AdjustBackThruster(backThrusterSync);
            }

            base.Update();

            // Sound for everyone, except server only
            if (!isServerOnly && engineSound != null)
            {
                if (enterVehicle.isDriverSit)
                {
                    if (!engineSound.isPlaying)
                        engineSound.Play();

                    engineSound.pitch = defaultEnginePitch + (Mathf.Abs(velocityMagnitudeSync) / pitchFraction);
                }
                else
                {
                    if (engineSound.isPlaying)
                        engineSound.Stop();
                }
            }

            // For client controlling the vehicle
            if (!isActive || isServerOnly || !isOwned)
                return;

            // Key auto move forward
            if (Input.GetKeyDown(goAutoForward) && !UIUtils.AnyInputActive())
            {
                autoForward = !autoForward;
            }

            // Client controlling spaceship use the local value
            AdjustBackThruster(backThrusterLocal);

            // Client controlling spaceship send value to sync to server
            if (isOwned && Time.time - lastClientSendToServerTime >= syncInterval)
            {
                CmdReceiveClientVehicleData(motorDirection, backThrusterLocal, vehicleRigidbody.velocity.magnitude);

                lastClientSendToServerTime = Time.time;
            }
        }

        // Server receive client value
        [Command]
        private void CmdReceiveClientVehicleData(float newMotorDirection, float newBackThruster, float newVelocityMagnitude)
        {
            // Use client value to update server syncvar value, will be send to all remote client
            backThrusterSync = newBackThruster;
            velocityMagnitudeSync = newVelocityMagnitude;

            // To remove fuel only when player move the vehicle
            UseFuel(newMotorDirection != 0f);
        }

        public override void ResetSyncVar()
        {
            if (isServer)
            {
                backThrusterSync = 0f;
            }

            if (!isServerOnly)
            {
                motorDirection = 0f;
                steeringDirection = 0f;
                driveHandBrake = 0f;
                driveHandBrakeUp = true;
            }
        }

        public override float GetVehicleSpeed()
        {
            return speedOnKmh;
        }

        public void AdjustBackThruster(float backThrusterForce)
        {
            // Adjust visual of thruster
            for (int i = 0; i < backThrusterEffect.Length; i++)
            {
                ParticleSystem.MainModule psmain = backThrusterEffect[i].main;
                psmain.startLifetime = backThrusterForce;
            }
        }

        public void FixedUpdate()
        {
            // Spaceship drop on ground if no fuel and no driver
            if (isServer && noFuelDropOnGround && !HasFuel() && !driverSeated)
            {
                Vector3 newPosition = new Vector3(transform.position.x, transform.position.y - transformYPosMinusRange, transform.position.z);

                if (!Physics.Linecast(transform.position, newPosition, out RaycastHit hitInfo, canStopFallLayerMask))
                {
                    if (enterVehicle.blockRigidBody)
                    {
                        vehicleRigidbody.mass = changeMassToWhenDropping;
                        vehicleRigidbody.drag = changeDragToWhenDropping;
                        vehicleRigidbody.useGravity = true;
                        vehicleRigidbody.isKinematic = false;
                    }
                }
                else
                {
                    if (enterVehicle.blockRigidBody)
                    {
                        vehicleRigidbody.mass = defaultRigidbodyMass;
                        vehicleRigidbody.drag = defaultRigidbodyDrag;
                        vehicleRigidbody.useGravity = false;
                        vehicleRigidbody.isKinematic = true;
                    }
                }
            }

            if (!isActive || isServerOnly)
                return;

            // Spaceship drop on ground if no fuel
            if (noFuelDropOnGround && !HasFuel())
            {
                Vector3 newPosition = new Vector3(transform.position.x, transform.position.y - transformYPosMinusRange, transform.position.z);

                if (!Physics.Linecast(transform.position, newPosition, out RaycastHit hitInfo, canStopFallLayerMask))
                {
                    if (enterVehicle.blockRigidBody)
                    {
                        vehicleRigidbody.mass = changeMassToWhenDropping;
                        vehicleRigidbody.drag = changeDragToWhenDropping;
                        vehicleRigidbody.useGravity = true;
                        vehicleRigidbody.isKinematic = false;
                    }
                }
                else
                {
                    if (enterVehicle.blockRigidBody)
                    {
                        vehicleRigidbody.mass = defaultRigidbodyMass;
                        vehicleRigidbody.drag = defaultRigidbodyDrag;
                        vehicleRigidbody.useGravity = false;
                        vehicleRigidbody.isKinematic = true;
                    }
                }
            }

            speedOnKmh = vehicleRigidbody.velocity.magnitude * 3.6f;
            speedOnMph = speedOnKmh * 0.62f;

            // Auto move forward
            if (HasFuel() && autoForward)
                motorDirection = 1f;

            // To adjust thruster effect
            backThrusterLocal = motorDirection;

            // Forward/reverse
            if (motorDirection > 0f)
                vehicleRigidbody.AddRelativeForce(motorDirection * Vector3.forward * forwardSpeed);
            else if (motorDirection < 0f)
                vehicleRigidbody.AddRelativeForce(motorDirection * Vector3.forward * reverseSpeed);

            // Strafe left/right
            if (steeringDirection < 0f)
                vehicleRigidbody.AddRelativeForce(steeringDirection * Vector3.right * forwardSpeed);
            else if (steeringDirection > 0f)
                vehicleRigidbody.AddRelativeForce(steeringDirection * Vector3.right * reverseSpeed);

            // Go up
            if (HasFuel() && Input.GetKey(goUp))
                vehicleRigidbody.AddRelativeForce(Vector3.up * upDownSpeed);

            // Go down
            if (HasFuel() && Input.GetKey(goDown))
                vehicleRigidbody.AddRelativeForce(Vector3.down * upDownSpeed);

            // Roll
            if (HasFuel() && (Input.GetKey(goRollLeft) || Input.GetKey(goRollRight)))
            {
                if (Input.GetKey(goRollLeft))
                {
                    vehicleRigidbody.AddRelativeTorque(0f, 0f, 1f * turnSpeed);
                }
                if (Input.GetKey(goRollRight))
                {
                    vehicleRigidbody.AddRelativeTorque(0f, 0f, -1f * turnSpeed);
                }
            }
            else
            {
                // Auto return to level
                if (vehicleRigidbody.transform.eulerAngles.z < 359.9f && vehicleRigidbody.transform.eulerAngles.z > 180.1f)
                {
                    float farFromLevel = Mathf.Abs((vehicleRigidbody.transform.eulerAngles.z - 360f)) / 180f;
                    vehicleRigidbody.AddRelativeTorque(0f, 0f, 1f * rollAutoReturnSpeed * farFromLevel);
                }
                if (vehicleRigidbody.transform.eulerAngles.z > 0.1f && vehicleRigidbody.transform.eulerAngles.z < 179.9f)
                {
                    float farFromLevel = vehicleRigidbody.transform.eulerAngles.z / 180f;
                    vehicleRigidbody.AddRelativeTorque(0f, 0f, -1f * rollAutoReturnSpeed * farFromLevel);
                }
            }

            // Pitch
            if (!useMouseDirection)
            {
                if (HasFuel() && Input.GetKey(goPitchForward))
                    vehicleRigidbody.AddRelativeTorque((invertPitch ? -1f : 1f) * turnSpeed, 0f, 0f);
                if (HasFuel() && Input.GetKey(goPitchBackward))
                    vehicleRigidbody.AddRelativeTorque((invertPitch ? 1f : -1f) * turnSpeed, 0f, 0f);
            }
            else
            {
                if (yExtra != 0f)
                {
                    vehicleRigidbody.AddRelativeTorque((invertPitch ? yExtra : -yExtra) * turnSpeed, 0f, 0f);
                }
            }

            // Yaw
            if (!useMouseDirection)
            {
                if (HasFuel() && Input.GetKey(goYawLeft))
                    vehicleRigidbody.AddRelativeTorque(0f, -1f * turnSpeed, 0f);
                if (HasFuel() && Input.GetKey(goYawRight))
                    vehicleRigidbody.AddRelativeTorque(0f, 1f * turnSpeed, 0f);
            }
            else
            {
                if (xExtra != 0f)
                {
                    vehicleRigidbody.AddRelativeTorque(0f, xExtra * turnSpeed, 0f);
                }
            }
        }
    }
}
