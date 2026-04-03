
//(c8

using Mirror;
using UnityEngine;

namespace BurnyDev.UMMORPG.AddonCustomVehicle
{
    public class BoatController : VehicleController
    {
        // Server sync this value when he receive it, use by remote client
        [SyncVar] private float backThrusterSync = 0f;
        [SyncVar] private float motorDirectionSync = 0f;
        [SyncVar] private float steeringDirectionSync = 0f;
        [SyncVar] private float velocityMagnitudeSync = 0f;
        // Use to time client send to server data
        private float lastClientSendToServerTime = 0f;

        [Header("No air freeze boat")]
        public bool noAirFreezeBoat = false;

        [Header("Vehicle keys")]
        public KeyCode goAutoForward = KeyCode.O;
        // Auto move forward
        private bool autoForward = false;

        [Header("Vehicle sound")]
        public AudioSource engineSound;
        public bool stopSoundWhenNoThrottle = false;
        public float pitchFraction = 5f;
        private float defaultEnginePitch = 1f;

        [Header("Vehicle setup")]
        public bool useStepSpeed = false;
        private float motorDirectionUse = 0f;
        private float steppedSpeed = 0f;
        private float lastSteppedSpeedTime = 0f;
        public GameObject[] sails;
        private float backThruster = 0f;

        [Header("Vehicle speed")]
        public float forwardSpeed = 2600f;
        public float reverseSpeed = 800f;
        public float steeringSpeed = 400f;
        public float stayLevelSpeed = 1500f;

        [Header("When go forward, tilt boat with force")]
        public float forwardTiltTorque = 120f;
        [Header("When turn, tilt boat with force")]
        public float sidewayTiltTorque = 50f;
        [Header("When boat go sideway, apply counter force")]
        public float sidewayForce = 10f;
        [Header("When the boat is considered underwater")]
        public float underwaterLevel = 1.5f;
        [Header("If boat is underwater\nmultiply gravitate to surface force by this percent, 1 = 100% no change")]
        public float underwaterStopVelocity = 0.96f;
        [Header("In water add some drag to slow down boat when no force apply")]
        public float inWaterDrag = 0.6f;
        [Header("Out of the water 0 = no drag\ncould be very low to have max jump/falling speed")]
        public float outWaterDrag = 0f;

        [Header("Distance from water level to apply force to return to surface")]
        public float swimSurfaceOffset = -5f;
        [Header("Force to apply when returning to surface")]
        public float floatTowardSurfaceSpeed = 750f;

        [Header("Max rotation X (front/back) before clamp and reverse force, 0 = not use it")]
        public float maxRotationX = 10f;
        [Header("Max rotation Z (left/right) before clamp and reverse force, 0 = not use it")]
        public float maxRotationZ = 10f;
        [Header("Oposite force power")]
        public float opositeForcePower = 500f;

        [HideInInspector] public Collider waterCollider;
        private bool inWater => waterCollider != null; // standing in water / touching it?

        [HideInInspector] public float speedOnKmh;
        [HideInInspector] public float speedOnMph;

        private Vector3 localVel = Vector3.zero;

        public ParticleSystem[] backThrusterEffect;

        [Header("Wind System")]
        [Tooltip("Enable wind force on the boat")]
        public bool useWind = true;
        [Tooltip("Global wind direction (world space). Normalised automatically.")]
        public Vector3 windDirection = new Vector3(1f, 0f, 0f);
        [Tooltip("Wind force strength applied to the boat when in water")]
        public float windForce = 120f;
        [Tooltip("How much sails contribute to catching wind (0 = no effect, 1 = full)")]
        [Range(0f, 1f)] public float sailWindMultiplier = 1f;
        [Tooltip("Wind pushes boat even without a driver")]
        public bool windAffectsUncrewed = true;
        // Allows runtime wind changes (e.g. from a world weather manager)
        [HideInInspector] public static Vector3 GlobalWindDirection = new Vector3(1f, 0f, 0f);
        [HideInInspector] public static float GlobalWindForce = 120f;
        private static bool globalWindInitialised = false;

        public override void Start()
        {
            base.Start();

            if (engineSound != null)
                defaultEnginePitch = engineSound.pitch;

            // Seed global wind from first boat if not yet set
            if (!globalWindInitialised)
            {
                GlobalWindDirection = windDirection.normalized;
                GlobalWindForce = windForce;
                globalWindInitialised = true;
            }
        }

        public override void Update()
        {
            // Network sync on host/client that do not control a vehicle
            if (!isActive && !isServerOnly)
            {
                // They use the SyncVar value, updated from server
                AdjustBackThruster(backThrusterSync);

                for (int i = 0; i < sails.Length; i++)
                {
                    if (sails[i] != null)
                    {
                        sails[i].transform.localScale = new Vector3(1f, motorDirectionSync < 0.1f ? 0.1f : motorDirectionSync, 1f);
                        sails[i].transform.localRotation = Quaternion.Euler(0f, transform.localRotation.y + steeringDirectionSync * 20f, 0f);
                    }
                }
            }

            base.Update();

            // Sound for everyone, except server only
            if (!isServerOnly && engineSound != null)
            {
                if (enterVehicle.isDriverSit && (stopSoundWhenNoThrottle ? motorDirectionSync > 0.1f : true))
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

            // Stepped speed to adjust speed each 0.3 sec and hold forward automatically but reverse return to 0 automatically
            if (useStepSpeed && Time.time - lastSteppedSpeedTime >= 0.3f)
            {
                if (motorDirection > 0f)
                {
                    if (steppedSpeed < 0f)
                        steppedSpeed = 0f;
                    steppedSpeed += 0.1f;
                }
                else if (motorDirection < 0f)
                    steppedSpeed -= 0.1f;
                else if (motorDirection == 0f && steppedSpeed < 0f)
                    steppedSpeed = 0f;

                if (steppedSpeed > 1f)
                    steppedSpeed = 1f;
                else if (steppedSpeed < -1f)
                    steppedSpeed = -1f;

                lastSteppedSpeedTime = Time.time;
            }

            // Client controlling spaceship use the local value
            AdjustBackThruster(backThruster);

            for (int i = 0; i < sails.Length; i++)
            {
                if (sails[i] != null)
                {
                    sails[i].transform.localScale = new Vector3(1f, motorDirectionUse < 0.1f ? 0.1f : motorDirectionUse, 1f);
                    sails[i].transform.localRotation = Quaternion.Euler(0f, transform.localRotation.y + steeringDirection * 20f, 0f);
                }
            }

            // Client controlling spaceship send value to sync to server
            if (Time.time - lastClientSendToServerTime >= syncInterval)
            {
                CmdReceiveClientVehicleData(motorDirectionUse, steeringDirection, backThruster, vehicleRigidbody.velocity.magnitude);

                lastClientSendToServerTime = Time.time;
            }
        }

        // Server receive client value
        [Command]
        private void CmdReceiveClientVehicleData(float newMotorDirection, float newSteeringDirection, float newBackThruster, float newVelocityMagnitude)
        {
            // Use client value to update server syncvar value, will be send to all remote client
            backThrusterSync = newBackThruster;
            motorDirectionSync = newMotorDirection;
            steeringDirectionSync = newSteeringDirection;
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
                steppedSpeed = 0f;
                motorDirection = 0f;
                motorDirectionUse = 0f;
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
            // To make boat drop in water without driver
            if (isServer && noAirFreezeBoat && !driverSeated && enterVehicle != null)
            {
                if (!inWater)
                {
                    // Make vehicle physic only for the driver
                    if (enterVehicle.blockRigidBody)
                    {
                        if (enterVehicle.useGravity)
                            enterVehicle.vehicleController.vehicleRigidbody.useGravity = true;
                        enterVehicle.vehicleController.vehicleRigidbody.isKinematic = false;
                    }
                }
                else
                {
                    if (enterVehicle.blockRigidBody)
                    {
                        if (enterVehicle.useGravity)
                            enterVehicle.vehicleController.vehicleRigidbody.useGravity = false;
                        enterVehicle.vehicleController.vehicleRigidbody.isKinematic = true;
                    }
                }
            }

            if (!isActive || isServerOnly)
                return;

            localVel = transform.InverseTransformDirection(vehicleRigidbody.velocity);

            speedOnKmh = vehicleRigidbody.velocity.magnitude * 3.6f;
            speedOnMph = speedOnKmh * 0.62f;

            // Auto move forward
            if (HasFuel() && autoForward)
                motorDirection = 1f;

            if (useStepSpeed)
                motorDirectionUse = steppedSpeed;
            else
                motorDirectionUse = motorDirection;

            // To adjust thruster effect
            backThruster = motorDirectionUse;

            if (inWater)
            {
                vehicleRigidbody.drag = inWaterDrag;

                // gravitate toward surface
                float surfaceRange = waterCollider.bounds.max.y - transform.position.y;
                float distanceToBoatUnderwaterLevel = surfaceRange - swimSurfaceOffset;
                //Debug.LogWarning("name: " + transform.name + " surfaceRange: " + surfaceRange + " distanceToBoatUnderwaterLevel: " + distanceToBoatUnderwaterLevel + " vect: " + Vector3.up * floatTowardSurfaceSpeed * distanceToBoatUnderwaterLevel);
                vehicleRigidbody.AddRelativeForce(floatTowardSurfaceSpeed * distanceToBoatUnderwaterLevel * Vector3.up);

                // Slow gravitate to surface when go underwater
                if (surfaceRange > underwaterLevel)
                    vehicleRigidbody.velocity *= underwaterStopVelocity;

                // stay level
                //var rot = Quaternion.FromToRotation(transform.up, Vector3.up);
                //vehicleRigidbody.AddTorque(new Vector3(rot.x, rot.y, rot.z) * stayLevelSpeed);

                // Auto return to level
                if (vehicleRigidbody.transform.eulerAngles.x < 359.9f && vehicleRigidbody.transform.eulerAngles.x > 180.1f)
                {
                    float farFromLevel = Mathf.Abs((vehicleRigidbody.transform.eulerAngles.x - 360f)) / 180f;
                    vehicleRigidbody.AddRelativeTorque(1f * stayLevelSpeed * farFromLevel, 0f, 0f);
                }
                if (vehicleRigidbody.transform.eulerAngles.x > 0.1f && vehicleRigidbody.transform.eulerAngles.x < 179.9f)
                {
                    float farFromLevel = vehicleRigidbody.transform.eulerAngles.x / 180f;
                    vehicleRigidbody.AddRelativeTorque(-1f * stayLevelSpeed * farFromLevel, 0f, 0f);
                }
                if (vehicleRigidbody.transform.eulerAngles.z < 359.9f && vehicleRigidbody.transform.eulerAngles.z > 180.1f)
                {
                    float farFromLevel = Mathf.Abs((vehicleRigidbody.transform.eulerAngles.z - 360f)) / 180f;
                    vehicleRigidbody.AddRelativeTorque(0f, 0f, 1f * stayLevelSpeed * farFromLevel);
                }
                if (vehicleRigidbody.transform.eulerAngles.z > 0.1f && vehicleRigidbody.transform.eulerAngles.z < 179.9f)
                {
                    float farFromLevel = vehicleRigidbody.transform.eulerAngles.z / 180f;
                    vehicleRigidbody.AddRelativeTorque(0f, 0f, -1f * stayLevelSpeed * farFromLevel);
                }

                // Forward/reverse
                if (motorDirectionUse > 0f)
                    vehicleRigidbody.AddRelativeForce(forwardSpeed * motorDirectionUse * Vector3.forward);
                else if (motorDirectionUse < 0f)
                    vehicleRigidbody.AddRelativeForce(motorDirectionUse * reverseSpeed * Vector3.forward);

                // Turn left/right
                if (!useMouseDirection)
                    vehicleRigidbody.AddRelativeTorque(motorDirectionUse < 0f ? -steeringDirection * steeringSpeed * Vector3.up : steeringDirection * steeringSpeed * Vector3.up);
                else
                    vehicleRigidbody.AddRelativeTorque(motorDirectionUse < 0f ? steeringSpeed * -xExtra * Vector3.up : steeringSpeed * xExtra * Vector3.up);

                // Add forward tilt torque when going forward
                if (motorDirectionUse > 0f)
                    vehicleRigidbody.AddRelativeTorque(new Vector3(-1f, 0f, 0f) * forwardTiltTorque);

                // Add rotation tilt torque when turning
                if (steeringDirection > 0f)
                    vehicleRigidbody.AddRelativeTorque(new Vector3(0f, 0f, -1f) * sidewayTiltTorque);
                if (steeringDirection < 0f)
                    vehicleRigidbody.AddRelativeTorque(new Vector3(0f, 0f, 1f) * sidewayTiltTorque);

                // If boat go sideway add force
                if (localVel.x > 0.03f)
                    vehicleRigidbody.AddForce(new Vector3(-1f, 0f, 0f) * sidewayForce);
                if (localVel.x < -0.03f)
                    vehicleRigidbody.AddForce(new Vector3(1f, 0f, 0f) * sidewayForce);

                // Wind force — pushes boat in wind direction when in water
                if (useWind && (driverSeated || windAffectsUncrewed))
                {
                    Vector3 wDir = GlobalWindDirection.sqrMagnitude > 0f
                        ? GlobalWindDirection.normalized
                        : windDirection.normalized;
                    float wStrength = GlobalWindForce > 0f ? GlobalWindForce : windForce;
                    float sailFactor = sails.Length > 0 ? sailWindMultiplier : 1f;
                    vehicleRigidbody.AddForce(wDir * wStrength * sailFactor);
                }
            }
            else
            {
                vehicleRigidbody.drag = outWaterDrag;

                // To adjust thruster effect, no effect if outside water (could be calculated on each remote client ?)
                backThruster = 0f;
            }

            // Force clamp rotation
            if (maxRotationX > 0f)
            {
                float rotationX = transform.localEulerAngles.x;
                if (rotationX > 180f && rotationX < (360f - maxRotationX))
                {
                    transform.localEulerAngles = new Vector3((360f - maxRotationX), transform.localEulerAngles.y, transform.localEulerAngles.z);
                    vehicleRigidbody.AddRelativeForce(-vehicleRigidbody.velocity * opositeForcePower);
                }
                else if (rotationX < 180f && rotationX > maxRotationX)
                {
                    transform.localEulerAngles = new Vector3(maxRotationX, transform.localEulerAngles.y, transform.localEulerAngles.z);
                    vehicleRigidbody.AddRelativeForce(-vehicleRigidbody.velocity * opositeForcePower);
                }
            }
            if (maxRotationZ > 0f)
            {
                float rotationZ = transform.localEulerAngles.z;
                if (rotationZ > 180f && rotationZ < (360f - maxRotationZ))
                {
                    transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, (360f - maxRotationZ));
                    vehicleRigidbody.AddRelativeForce(-vehicleRigidbody.velocity * opositeForcePower);
                }
                else if (rotationZ < 180f && rotationZ > maxRotationZ)
                {
                    transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, maxRotationZ);
                    vehicleRigidbody.AddRelativeForce(-vehicleRigidbody.velocity * opositeForcePower);
                }
            }
        }
    }
}
