using Mirror;
using UnityEngine;

namespace BurnyDev.UMMORPG.AddonCustomVehicle
{
    public class VehicleControl : NetworkBehaviour
    {
        #region Components
        [Header("Components")]
        public Health health;
        public Combat combat;
        public AudioSource landAudio;
        public VehicleController vehicleController;
        #endregion

        #region State
        [Header("State")]
        public MoveState state = MoveState.IDLE;
        private MoveState lastState = MoveState.IDLE;
        [HideInInspector] public Vector3 moveDir;
        #endregion

        #region Falling
        [Header("Falling")]
        public float fallMinimumMagnitude = 6f;
        public float fallDamageMinimumMagnitude = 13f;
        public float fallDamageMultiplier = 2f;
        [HideInInspector] public Vector3 lastFall;
        private bool sprintingBeforeAirborne;
        #endregion

        #region Sounds
        [Header("Sounds")]
        public AudioClip landSound;
        #endregion

        #region Properties
        public bool IsGroundedWithinTolerance => vehicleController.IsGrounded() || vehicleController.vehicleRigidbody.velocity.y > -fallMinimumMagnitude;
        public Vector3 Velocity { get; private set; }
        #endregion

        #region Input
        private Vector2 GetInputDirection()
        {
            if (UIUtils.AnyInputActive() || !isOwned) return Vector2.zero;

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector2 input = new Vector2(horizontal, vertical);
            return input.magnitude > 1 ? input.normalized : input;
        }

        private Vector3 GetDesiredDirection(Vector2 inputDir)
        {
            return transform.TransformDirection(new Vector3(inputDir.x, 0, inputDir.y).normalized);
        }
        #endregion

        #region State Machine Events
        private bool EventDied() => health.current == 0;
        private bool EventFalling() => !IsGroundedWithinTolerance;
        private bool EventLanded() => vehicleController.IsGrounded(true);
        #endregion

        #region State Updates
        private MoveState UpdateIdle(Vector2 inputDir, Vector3 desiredDir)
        {
            HandleVehicleMovement(inputDir, desiredDir);

            if (EventDied()) return MoveState.DEAD;
            if (EventFalling()) { sprintingBeforeAirborne = false; return MoveState.AIRBORNE; }
            if (inputDir != Vector2.zero) return MoveState.DRIVING;

            return MoveState.IDLE;
        }

        private MoveState UpdateAirborne(Vector2 inputDir, Vector3 desiredDir)
        {
            HandleVehicleMovement(inputDir, desiredDir);

            if (EventDied()) return MoveState.DEAD;
            if (EventLanded()) { ApplyFallDamage(); PlayLandingSound(); return MoveState.IDLE; }

            return MoveState.AIRBORNE;
        }

        private MoveState UpdateDriving(Vector2 inputDir, Vector3 desiredDir)
        {
            HandleVehicleMovement(inputDir, desiredDir);

            if (EventDied()) return MoveState.DEAD;
            if (EventFalling()) { sprintingBeforeAirborne = false; return MoveState.AIRBORNE; }
            if (inputDir == Vector2.zero) return MoveState.IDLE;

            return MoveState.DRIVING;
        }

        private MoveState UpdateDead(Vector2 inputDir, Vector3 desiredDir)
        {
            vehicleController.SetMotor(0f);
            vehicleController.SetSteering(0f);
            vehicleController.SetPitch(0f);
            vehicleController.SetYaw(0f);
            vehicleController.SetRoll(0f);

            if (health.current > 0) return MoveState.IDLE;
            return MoveState.DEAD;
        }
        #endregion

        #region State Helpers
        private void HandleVehicleMovement(Vector2 inputDir, Vector3 desiredDir)
        {
            if (vehicleController.HasFuel())
            {
                vehicleController.SetMotor(inputDir.y);
                vehicleController.SetPitch(desiredDir.y);
                vehicleController.SetYaw(desiredDir.y);
                vehicleController.SetRoll(desiredDir.y);
            }
            else
            {
                vehicleController.SetMotor(0f);
            }

            if (vehicleController.HasFuel() || !vehicleController.steeringUseFuel)
                vehicleController.SetSteering(inputDir.x);
            else
                vehicleController.SetSteering(0f);

            Velocity = vehicleController.vehicleRigidbody.velocity;
            moveDir = Velocity;
        }

        private void ApplyFallDamage()
        {
            float fallMagnitude = Mathf.Abs(lastFall.y);
            if (fallMagnitude >= fallDamageMinimumMagnitude)
            {
                int damage = Mathf.RoundToInt(fallMagnitude * fallDamageMultiplier);
                health.current -= damage;
                // Assuming a similar combat system to PlayerCharacterControllerMovement
                if (combat != null) combat.RpcOnReceivedDamaged(damage, DamageType.Normal);
            }
        }

        private void PlayLandingSound()
        {
            if (landAudio != null && landSound != null)
            {
                landAudio.clip = landSound;
                landAudio.Play();
            }
        }
        #endregion

        #region Update Methods
        void Update()
        {
            if (!isOwned || Player.localPlayer == null) return;

            if (!UIUtils.AnyInputActive())
            {
                vehicleController.driveHandBrake = Input.GetButton("Jump") ? 1f : 0f;
                if (Input.GetButtonUp("Jump"))
                    vehicleController.driveHandBrakeUp = true;
            }
        }

        void FixedUpdate()
        {
            if (!isOwned || Player.localPlayer == null) return;

            Vector2 inputDir = GetInputDirection();
            Vector3 desiredDir = GetDesiredDirection(inputDir);
            Debug.DrawLine(transform.position, transform.position + desiredDir, Color.red);

            switch (state)
            {
                case MoveState.IDLE: state = UpdateIdle(inputDir, desiredDir); break;
                case MoveState.AIRBORNE: state = UpdateAirborne(inputDir, desiredDir); break;
                case MoveState.DRIVING: state = UpdateDriving(inputDir, desiredDir); break;
                case MoveState.DEAD: state = UpdateDead(inputDir, desiredDir); break;
                default: Debug.LogError($"Unhandled Movement State: {state}"); break;
            }

            if (!vehicleController.IsGrounded()) lastFall = Velocity;

            // Sync movement with the network (similar to PlayerCharacterControllerMovement)
            CmdSyncMovement(state, transform.position, transform.eulerAngles.y);

            lastState = state;
        }
        #endregion

        #region Network
        [Command]
        private void CmdSyncMovement(MoveState state, Vector3 position, float yRotation)
        {
            this.state = state;
            transform.position = position;
            transform.rotation = Quaternion.Euler(0, yRotation, 0);
            RpcSyncMovement(state, position, yRotation);
        }

        [ClientRpc]
        private void RpcSyncMovement(MoveState state, Vector3 position, float yRotation)
        {
            if (isLocalPlayer) return;
            this.state = state;
            transform.position = position;
            transform.rotation = Quaternion.Euler(0, yRotation, 0);
        }
        #endregion
    }
}

