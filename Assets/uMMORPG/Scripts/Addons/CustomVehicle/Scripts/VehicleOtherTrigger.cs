
//(c8

using UnityEngine;

namespace BurnyDev.UMMORPG.AddonCustomVehicle
{
    public class VehicleOtherTrigger : MonoBehaviour
    {
        public VehicleLocalParent vehicleLocalParent;

        // Run the same method on the referenced script VehicleLocalParent
        private void OnTriggerEnter(Collider other)
        {
            if (vehicleLocalParent != null && vehicleLocalParent.gameObject.activeSelf)
                vehicleLocalParent.OnTriggerEnter(other);
        }

        // Run the same method on the referenced script VehicleLocalParent
        private void OnTriggerExit(Collider other)
        {
            if (vehicleLocalParent != null && vehicleLocalParent.gameObject.activeSelf)
                vehicleLocalParent.OnTriggerExit(other);
        }
    }
}
