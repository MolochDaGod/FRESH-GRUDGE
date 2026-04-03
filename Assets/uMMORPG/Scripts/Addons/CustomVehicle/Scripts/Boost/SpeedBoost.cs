
//(c8

using UnityEngine;

namespace BurnyDev.UMMORPG.AddonCustomVehicle
{
    public class SpeedBoost : MonoBehaviour
    {
        public float speedTime = 1f;
        public float speedBoost = 10f;

        private void OnTriggerEnter(Collider col)
        {
            VehicleController vehicleController = col.GetComponentInParent<VehicleController>();
            if (vehicleController != null)
                vehicleController.ChangeMaxSpeed(speedTime, speedBoost);
        }
    }
}
