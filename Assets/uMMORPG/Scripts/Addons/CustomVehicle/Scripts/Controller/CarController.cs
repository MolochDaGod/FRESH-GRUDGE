
//(c8

using Mirror;
using System.Collections.Generic;
using UnityEngine;

namespace BurnyDev.UMMORPG.AddonCustomVehicle
{
    [System.Serializable]
    public class AxleInfo
    {
        public WheelCollider leftWheel;
        public WheelCollider rightWheel;
        public bool motor;
        public bool steering;
        public bool rear;
    }

    public class CarController : VehicleController
    {
        [Header("Use debug flip when holding leftshift?")]
        public bool useDebugFlip = false;

        [Header("Key to flip car")]
        public KeyCode flipCarKey = KeyCode.Y;

        [Header("Key to switch headlight")]
        public KeyCode switchHeadLightKey = KeyCode.L;

        [Header("Vehicle keys")]
        public KeyCode goAutoForward = KeyCode.O;
        // Auto move forward
        private bool autoForward = false;

        [Header("Vehicle sound")]
        public AudioSource engineSound;
        public float pitchFraction = 5f;
        private float defaultEnginePitch = 1f;
        public AudioSource skidSound;

        public GameObject headLights;
        // If you want meshrenderer
        //public MeshRenderer[] headLightsMeshRenderers;

        // Local variable on remote client
        public float steeringAngle = 0f;
        public float frontWheelSpeed = 0f;
        public float rearWheelSpeed = 0f;
        public float rearWheelHandBrake = 0f;

        // Local variable on client controlling the vehicle
        [HideInInspector] public float speedOnKmh;
        [HideInInspector] public float speedOnMph;
        public bool carHeadLightLocal = false;
        private Vector3 localVel = Vector3.zero;

        // Local variable use by everyone
        private bool showSmoke = false;
        private bool showSkid = false;

        // Server sync this value when he receive it, use by remote client
        [SyncVar] private float steeringAngleSync = 0f;
        [SyncVar] private float frontWheelSpeedSync = 0f;
        [SyncVar] private float rearWheelSpeedSync = 0f;
        [SyncVar] private float rearWheelHandBrakeSync = 0f;
        [SyncVar] private Vector3 localVelocitySync = Vector3.zero;
        [SyncVar] private bool carHeadLightSync = false;

        // Use to time client send to server data
        private float lastClientSendToServerTime;

        [Header("Car controller")]
        public List<AxleInfo> axleInfos; // the information about each individual axle
        public float maxMotorTorque = 1500f; // maximum torque the motor can apply to wheel
        public float maxSteeringAngle = 30f; // maximum steer angle the wheel can have
        public float maxBrakeTorque = 30000f; // maximum brake torque wheel can have

        public WheelCollider[] wheelColliders;
        public GameObject[] updateRemoteWheels;

        [Header("Effects")]
        public ParticleSystem frontLeftWheelSmoke;
        public ParticleSystem frontRightWheelSmoke;
        public ParticleSystem rearLeftWheelSmoke;
        public ParticleSystem rearRightWheelSmoke;
        public TrailRenderer frontLeftWheelSkid;
        public TrailRenderer frontRightWheelSkid;
        public TrailRenderer rearLeftWheelSkid;
        public TrailRenderer rearRightWheelSkid;
        [Header("Sideway local velocity to show smoke on wheel")]
        public float sidewayLocalVelocityForWheelSmoke = 2f;
        [Header("Sideway local velocity to show skid on wheel")]
        public float sidewayLocalVelocityForWheelSkid = 3.5f;

        [Header("Remote side")]
        public AdjustRemoteWheelHeight[] adjustRemoteWheelHeight = new AdjustRemoteWheelHeight[4] { null, null, null, null };

        public override void Start()
        {
            base.Start();

            if (engineSound != null)
                defaultEnginePitch = engineSound.pitch;

            axleInfos[0].leftWheel.ConfigureVehicleSubsteps(30f, 5, 1);
            axleInfos[0].rightWheel.ConfigureVehicleSubsteps(30f, 5, 1);
            axleInfos[1].leftWheel.ConfigureVehicleSubsteps(30f, 5, 1);
            axleInfos[1].rightWheel.ConfigureVehicleSubsteps(30f, 5, 1);
        }

        public override void Update()
        {
            // Network sync on host/client that do not control a vehicle
            if (!isActive && !isServerOnly)
            {
                // They use the SyncVar value, updated from server
                steeringAngle = steeringAngleSync;
                frontWheelSpeed = frontWheelSpeedSync;
                rearWheelSpeed = rearWheelSpeedSync;
                rearWheelHandBrake = rearWheelHandBrakeSync;

                if (headLights != null)
                    headLights.SetActive(carHeadLightSync);
                // If you want meshrenderer
                //for (int i = 0; i < headLightsMeshRenderers.Length; i++)
                //    headLightsMeshRenderers[i].enabled = carHeadLightSync;

                //float angle = Mathf.LerpAngle(axleInfos[0].leftWheel.transform.localEulerAngles.y, steeringAngle, Time.deltaTime);
                //axleInfos[0].leftWheel.transform.localEulerAngles = new Vector3(0f, angle, 0f);
                //axleInfos[0].rightWheel.transform.localEulerAngles = new Vector3(0f, angle, 0f);

                Quaternion qt = Quaternion.Euler(new Vector3(axleInfos[0].leftWheel.transform.localEulerAngles.x, steeringAngle, axleInfos[0].leftWheel.transform.localEulerAngles.z));
                axleInfos[0].leftWheel.transform.localRotation = Quaternion.Lerp(axleInfos[0].leftWheel.transform.localRotation, qt, Time.deltaTime * 10);
                axleInfos[0].rightWheel.transform.localRotation = axleInfos[0].leftWheel.transform.localRotation;

                if (frontWheelSpeed != 0f)
                {
                    axleInfos[0].leftWheel.transform.GetChild(0).Rotate(frontWheelSpeed * 6f * Time.deltaTime, 0f, 0f);
                    axleInfos[0].rightWheel.transform.GetChild(0).Rotate(frontWheelSpeed * 6f * Time.deltaTime, 0f, 0f);
                }

                if (rearWheelSpeed != 0f)
                {
                    axleInfos[1].leftWheel.transform.GetChild(0).Rotate(rearWheelSpeed * 6f * Time.deltaTime, 0f, 0f);
                    axleInfos[1].rightWheel.transform.GetChild(0).Rotate(rearWheelSpeed * 6f * Time.deltaTime, 0f, 0f);
                }
            }

            base.Update();

            // Sound for everyone, except server only
            if (!isServerOnly && engineSound != null)
            {
                if (enterVehicle.isDriverSit)
                {
                    if (!engineSound.isPlaying)
                        engineSound.Play();

                    engineSound.pitch = defaultEnginePitch + (Mathf.Abs(localVelocitySync.sqrMagnitude) / pitchFraction);
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

            if (Input.GetKeyDown(flipCarKey) && !UIUtils.AnyInputActive())
            {
                // Debug to flip vehicle reverse
                if (useDebugFlip && Input.GetKey(KeyCode.LeftShift))
                {
                    transform.position = new Vector3(transform.position.x, transform.position.y + 5f, transform.position.z);
                    transform.eulerAngles = new Vector3(180f, 0f, 0f);
                    return;
                }

                if ((Mathf.Abs(transform.eulerAngles.z) > 20f && Mathf.Abs(transform.eulerAngles.z) < 340f) ||
                        (Mathf.Abs(transform.eulerAngles.x) > 20f && Mathf.Abs(transform.eulerAngles.x) < 340f))
                {
                    transform.position = new Vector3(transform.position.x + UnityEngine.Random.Range(-1f, 1f), transform.position.y + 5f, transform.position.z + UnityEngine.Random.Range(-1f, 1f));
                    transform.rotation = Quaternion.identity;
                }
            }

            // Switch head light
            if (Input.GetKeyDown(switchHeadLightKey) && !UIUtils.AnyInputActive())
            {
                carHeadLightLocal = !carHeadLightLocal;
            }

            if (headLights != null)
                headLights.SetActive(carHeadLightLocal);
            // If you want meshrenderer
            //for (int i = 0; i < headLightsMeshRenderers.Length; i++)
            //    headLightsMeshRenderers[i].enabled = carHeadLightLocal;

            if (isOwned && Time.time - lastClientSendToServerTime >= syncInterval)
            {
                float frontRpm = axleInfos[0].leftWheel.rpm > 0.2f ? axleInfos[0].leftWheel.rpm : axleInfos[0].leftWheel.rpm < -0.2f ? axleInfos[0].leftWheel.rpm : 0f;
                float rearRpm = axleInfos[1].leftWheel.rpm > 0.2f ? axleInfos[1].leftWheel.rpm : axleInfos[1].leftWheel.rpm < -0.2f ? axleInfos[1].leftWheel.rpm : 0f;
                CmdReceiveVehicleData((int)axleInfos[0].leftWheel.steerAngle, frontRpm, rearRpm, driveHandBrake, motorDirection, carHeadLightLocal, localVel);

                lastClientSendToServerTime = Time.time;
            }
        }

        [Command]
        private void CmdReceiveVehicleData(
            float newSteeringAngle, float newFrontWheelSpeed, float newRearWheelSpeed,
            float newRearWheelHandBrake, float newMotorDirection, bool newCarHeadLightLocal,
            Vector3 newLocalVelocitySync)
        {
            steeringAngleSync = newSteeringAngle;
            frontWheelSpeedSync = newFrontWheelSpeed;
            rearWheelSpeedSync = newRearWheelSpeed;
            rearWheelHandBrakeSync = newRearWheelHandBrake;
            localVelocitySync = newLocalVelocitySync;

            carHeadLightSync = newCarHeadLightLocal;

            // To remove fuel only when player move the vehicle
            UseFuel(newMotorDirection != 0f);
        }

        public override void ResetSyncVar()
        {
            foreach (var item in wheelColliders)
            {
                item.steerAngle = 0f;
                item.motorTorque = 0f;
            }

            if (isServer)
            {
                steeringAngleSync = 0f;
                frontWheelSpeedSync = 0f;
                rearWheelSpeedSync = 0f;
                rearWheelHandBrakeSync = 0f;
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

        public override bool IsGrounded(bool controlled = true)
        {
            if (controlled)
            {
                // If 1 wheel touch .. we consider vehicle grounded
                for (int i = 0; i < axleInfos.Count; i++)
                {
                    if (axleInfos[i].leftWheel.isGrounded) return true;
                    if (axleInfos[i].rightWheel.isGrounded) return true;
                }
            }
            else
            {
                for (int i = 0; i < adjustRemoteWheelHeight.Length; i++)
                {
                    if (adjustRemoteWheelHeight[i].isGrounded) return true;
                }
            }

            return false;
        }

        public bool IsGroundedPrecise(int wheelId, bool controlled = true)
        {
            // If player control the vehicle, we can have axleInfos data directly from the wheel
            if (controlled)
            {
                // Front left
                if (wheelId == 0)
                {
                    if (axleInfos[0].leftWheel.isGrounded) return true;
                }
                // Front right
                if (wheelId == 1)
                {
                    if (axleInfos[0].rightWheel.isGrounded) return true;
                }
                // Rear right
                if (wheelId == 2)
                {
                    if (axleInfos[1].rightWheel.isGrounded) return true;
                }
                // Rear left
                if (wheelId == 3)
                {
                    if (axleInfos[1].leftWheel.isGrounded) return true;
                }
            }
            // If player do not control the vehicle, we get this data from the sync adjusted remote wheel height
            else
            {
                // Front left
                if (wheelId == 0)
                {
                    if (adjustRemoteWheelHeight[0].isGrounded) return true;
                }
                // Front right
                if (wheelId == 1)
                {
                    if (adjustRemoteWheelHeight[1].isGrounded) return true;
                }
                // Rear right
                if (wheelId == 2)
                {
                    if (adjustRemoteWheelHeight[2].isGrounded) return true;
                }
                // Rear left
                if (wheelId == 3)
                {
                    if (adjustRemoteWheelHeight[3].isGrounded) return true;
                }
            }

            return false;
        }

        public void FixedUpdate()
        {
            ShowSmokeSkidEffects(Player.localPlayer != null && Player.localPlayer.playerVehicle.controlVehicle && Player.localPlayer.playerVehicle.enterVehicle == enterVehicle);

            if (!isActive || isServerOnly)
                return;

            localVel = transform.InverseTransformDirection(vehicleRigidbody.velocity);

            speedOnKmh = vehicleRigidbody.velocity.magnitude * 3.6f;
            speedOnMph = speedOnKmh * 0.62f;

            // Auto move forward
            if (HasFuel() && autoForward)
                motorDirection = 1f;

            foreach (AxleInfo axleInfo in axleInfos)
            {
                if (axleInfo.steering)
                {
                    if (!useMouseDirection)
                    {
                        axleInfo.leftWheel.steerAngle = steeringDirection * maxSteeringAngle;
                        axleInfo.rightWheel.steerAngle = steeringDirection * maxSteeringAngle;
                    }
                    else
                    {
                        axleInfo.leftWheel.steerAngle = xExtra * maxSteeringAngle;
                        axleInfo.rightWheel.steerAngle = xExtra * maxSteeringAngle;
                    }

                    if (localVel.z > 0.3f && motorDirection < 0f)
                    {
                        // We're moving forward and trying go backward
                        axleInfo.leftWheel.brakeTorque += maxBrakeTorque;
                        axleInfo.rightWheel.brakeTorque += maxBrakeTorque;
                    }
                    else if (localVel.z < -0.3f && motorDirection > 0f)
                    {
                        // We're moving backward and trying go forward
                        axleInfo.leftWheel.brakeTorque += maxBrakeTorque;
                        axleInfo.rightWheel.brakeTorque += maxBrakeTorque;
                    }
                    else
                    {
                        axleInfo.leftWheel.brakeTorque = 0f;
                        axleInfo.rightWheel.brakeTorque = 0f;
                    }
                }

                if (axleInfo.motor)
                {
                    if (axleInfo.leftWheel.rpm <= 300f && axleInfo.rightWheel.rpm >= -100f)
                        axleInfo.leftWheel.motorTorque = motorDirection * maxMotorTorque;
                    else
                        axleInfo.leftWheel.motorTorque = 0f;
                    if (axleInfo.rightWheel.rpm <= 300f && axleInfo.rightWheel.rpm >= -100f)
                        axleInfo.rightWheel.motorTorque = motorDirection * maxMotorTorque;
                    else
                        axleInfo.rightWheel.motorTorque = 0f;

                    if (localVel.z > 0.3f && motorDirection < 0f)
                    {
                        // We're moving forward and trying go backward
                        axleInfo.leftWheel.motorTorque = 0f;
                        axleInfo.rightWheel.motorTorque = 0f;
                        axleInfo.leftWheel.brakeTorque += maxBrakeTorque;
                        axleInfo.rightWheel.brakeTorque += maxBrakeTorque;
                    }
                    else if (localVel.z < -0.3f && motorDirection > 0f)
                    {
                        // We're moving backward and trying go forward
                        axleInfo.leftWheel.motorTorque = 0f;
                        axleInfo.rightWheel.motorTorque = 0f;
                        axleInfo.leftWheel.brakeTorque += maxBrakeTorque;
                        axleInfo.rightWheel.brakeTorque += maxBrakeTorque;
                    }
                    else
                    {
                        axleInfo.leftWheel.brakeTorque = 0f;
                        axleInfo.rightWheel.brakeTorque = 0f;
                    }
                }

                // Rear wheels
                if (axleInfo.rear)
                {
                    if (driveHandBrake != 0f)
                    {
                        WheelFrictionCurve wheelFrictionCurve = axleInfo.leftWheel.sidewaysFriction;
                        wheelFrictionCurve.extremumValue = 0.4f;
                        axleInfo.leftWheel.sidewaysFriction = wheelFrictionCurve;
                        axleInfo.rightWheel.sidewaysFriction = wheelFrictionCurve;

                        axleInfo.leftWheel.motorTorque = 0f;
                        axleInfo.rightWheel.motorTorque = 0f;
                        axleInfo.leftWheel.brakeTorque += driveHandBrake * maxBrakeTorque * 3;
                        axleInfo.rightWheel.brakeTorque += driveHandBrake * maxBrakeTorque * 3;
                    }
                    else
                    {
                        WheelFrictionCurve wheelFrictionCurve = axleInfo.leftWheel.sidewaysFriction;
                        wheelFrictionCurve.extremumValue = 1f;
                        axleInfo.leftWheel.sidewaysFriction = wheelFrictionCurve;
                        axleInfo.rightWheel.sidewaysFriction = wheelFrictionCurve;
                    }

                    if (driveHandBrakeUp)
                    {
                        axleInfo.leftWheel.brakeTorque = 0f;
                        axleInfo.rightWheel.brakeTorque = 0f;
                    }
                }

                ApplyLocalPositionToVisuals(axleInfo.leftWheel);
                ApplyLocalPositionToVisuals(axleInfo.rightWheel);
            }

            if (driveHandBrakeUp)
            {
                driveHandBrakeUp = false;
            }

            // Boost
            vehicleRigidbody.AddForce(vehicleRigidbody.transform.forward * motorBoost, ForceMode.Acceleration);

            // Always remove force if we go really too fast (because many boost or .. physic fun)
            // Do not remove force if we receive boost
            // Do not remove force if we are not grounded
            // Do not remove force if we go under max motor speed
            if ((motorBoost == 0f && IsGrounded() && Mathf.Abs(localVel.z) > 15f) || Mathf.Abs(localVel.z) > 20f)
                vehicleRigidbody.AddForce(40f * Time.fixedDeltaTime * -vehicleRigidbody.velocity, ForceMode.Acceleration);
        }

        // finds the corresponding visual wheel
        // correctly applies the transform
        // visual wheel transform must be the first child of wheelcollider
        public void ApplyLocalPositionToVisuals(WheelCollider collider)
        {
            if (collider.transform.childCount == 0)
                return;

            Transform visualWheel = collider.transform.GetChild(0);

            Vector3 position;
            Quaternion rotation;
            collider.GetWorldPose(out position, out rotation);

            visualWheel.transform.position = position;
            visualWheel.transform.rotation = rotation;
        }

        public void ShowSmokeSkidEffects(bool controlled = true)
        {
            if (rearLeftWheelSmoke != null && rearRightWheelSmoke != null)
            {
                showSmoke = Mathf.Abs(localVelocitySync.x) > sidewayLocalVelocityForWheelSmoke;

                if (showSmoke && IsGroundedPrecise(2, controlled))
                {
                    if (!rearRightWheelSmoke.isPlaying)
                        rearRightWheelSmoke.Play();
                }
                else
                {
                    if (rearRightWheelSmoke.isPlaying)
                        rearRightWheelSmoke.Stop();
                }

                if (showSmoke && IsGroundedPrecise(3, controlled))
                {
                    if (!rearLeftWheelSmoke.isPlaying)
                        rearLeftWheelSmoke.Play();
                }
                else
                {
                    if (rearLeftWheelSmoke.isPlaying)
                        rearLeftWheelSmoke.Stop();
                }
            }

            if (frontLeftWheelSkid != null && frontRightWheelSkid != null)
            {
                showSkid = Mathf.Abs(localVelocitySync.x) > sidewayLocalVelocityForWheelSkid;

                if (showSkid && IsGroundedPrecise(0, controlled))
                    frontLeftWheelSkid.emitting = true;
                else
                    frontLeftWheelSkid.emitting = false;

                if (showSkid && IsGroundedPrecise(1, controlled))
                    frontRightWheelSkid.emitting = true;
                else
                    frontRightWheelSkid.emitting = false;
            }

            if (rearLeftWheelSkid != null && rearRightWheelSkid != null)
            {
                showSkid = Mathf.Abs(localVelocitySync.x) > sidewayLocalVelocityForWheelSkid;
                float localMovement = localVelocitySync.sqrMagnitude;

                if (((localMovement > 0.1f && (driveHandBrake != 0f || rearWheelHandBrake != 0f)) || showSkid) && IsGroundedPrecise(2, controlled))
                    rearRightWheelSkid.emitting = true;
                else
                    rearRightWheelSkid.emitting = false;

                if (((localMovement > 0.1f && (driveHandBrake != 0f || rearWheelHandBrake != 0f)) || showSkid) && IsGroundedPrecise(3, controlled))
                    rearLeftWheelSkid.emitting = true;
                else
                    rearLeftWheelSkid.emitting = false;
            }

            // Skid sound, if any wheel emitting, do sound
            if (frontLeftWheelSkid.emitting || frontRightWheelSkid.emitting || rearRightWheelSkid.emitting || rearLeftWheelSkid.emitting)
            {
                if (skidSound != null)
                {
                    if (!skidSound.isPlaying)
                        skidSound.Play();
                }
            }
            else
            {
                if (skidSound != null)
                {
                    if (skidSound.isPlaying)
                        skidSound.Stop();
                }
            }
        }

        public override void EnterVehicleAsDriver()
        {
            // Activate wheel collider only for the driver
            foreach (var item in wheelColliders)
            {
                item.enabled = true;
            }

            // Deactivate wheel remote update only for the driver
            foreach (var item in updateRemoteWheels)
            {
                item.SetActive(false);
            }
        }

        public override void ExitVehicleAsDriver()
        {
            foreach (var item in wheelColliders)
            {
                item.steerAngle = 0f;
                item.motorTorque = 0f;
                item.enabled = false;
                item.transform.GetChild(0).localRotation = Quaternion.identity;
                ResetSyncVar();
            }

            foreach (var item in updateRemoteWheels)
            {
                item.SetActive(true);
            }
        }
    }
}
