
//(c8

using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BurnyDev.UMMORPG.AddonCustomVehicle
{
    public class WaterLevelCheck : MonoBehaviour
    {
        public BoatController boatController;

        [ClientCallback] // client authoritative movement, don't do this on Server
        //[ServerCallback] <- disabled for now, since movement is client authoritative
        void OnTriggerEnter(Collider co)
        {
            // touching water? then set water collider
            // use CompareTag for performance
            if (co.CompareTag("Water"))
                boatController.waterCollider = co;
        }

        [ClientCallback] // client authoritative movement, don't do this on Server
        void OnTriggerExit(Collider co)
        {
            // use CompareTag for performance
            if (co.CompareTag("Water"))
                boatController.waterCollider = null;
        }
    }
}
