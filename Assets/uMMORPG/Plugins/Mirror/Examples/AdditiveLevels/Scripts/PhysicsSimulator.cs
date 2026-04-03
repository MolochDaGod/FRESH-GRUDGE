using UnityEngine;

namespace Mirror.Examples.AdditiveLevels
{
    public class PhysicsSimulator : MonoBehaviour
    {
        PhysicsScene physicsScene;
        PhysicsScene2D physicsScene2D;

        bool simulatePhysicsScene;
        bool simulatePhysicsScene2D;

        // Variables for swaying motion
        public float swayAngle = 3f; // Maximum sway angle in degrees
        public float swaySpeed = 1f; // Speed of the sway
        public float verticalMovement = 0.1f; // Maximum vertical movement
        public float verticalSpeed = 1f; // Speed of the vertical movement

        private float initialYPosition;

        void Awake()
        {
            if (NetworkServer.active)
            {
                physicsScene = gameObject.scene.GetPhysicsScene();
                simulatePhysicsScene = physicsScene.IsValid() && physicsScene != Physics.defaultPhysicsScene;

                physicsScene2D = gameObject.scene.GetPhysicsScene2D();
                simulatePhysicsScene2D = physicsScene2D.IsValid() && physicsScene2D != Physics2D.defaultPhysicsScene;

                initialYPosition = transform.position.y;
            }
            else
            {
                enabled = false;
            }
        }

        void FixedUpdate()
        {
            if (!NetworkServer.active) return;

            if (simulatePhysicsScene)
                physicsScene.Simulate(Time.fixedDeltaTime);

            if (simulatePhysicsScene2D)
                physicsScene2D.Simulate(Time.fixedDeltaTime);

            // Apply swaying motion
            float sway = Mathf.Sin(Time.time * swaySpeed) * swayAngle;
            transform.rotation = Quaternion.Euler(0, 0, sway);

            // Apply vertical movement
            float verticalOffset = Mathf.Sin(Time.time * verticalSpeed) * verticalMovement;
            transform.position = new Vector3(transform.position.x, initialYPosition + verticalOffset, transform.position.z);
        }
    }
}
