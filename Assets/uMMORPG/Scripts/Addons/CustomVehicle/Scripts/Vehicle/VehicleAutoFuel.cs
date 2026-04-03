
//(c8

using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BurnyDev.UMMORPG.AddonCustomVehicle
{
    public class VehicleAutoFuel : MonoBehaviour
    {
        // Quantity filled in vehicle each time
        [SerializeReference] private float addedFuelQuantity = 1f;
        // 0 = unlimited quantity in tank
        [SerializeReference, Header("0 = unlimited quantity")] private float tankFuelMaxQuantity = 0f;
        // Add fuel each time in sec
        [SerializeReference] private float fuelWaitTime = 2f;

        // Internal running state
        private bool isRunning = false;
        // Internal list
        private List<EnterVehicle> enterVehicles = new List<EnterVehicle>();

        [ServerCallback]
        private void OnTriggerEnter(Collider other)
        {
            EnterVehicle enterVehicle = other.transform.GetComponentInParent<EnterVehicle>();

            if (enterVehicle != null)
            {
                if (!enterVehicles.Contains(enterVehicle))
                {
                    enterVehicles.Add(enterVehicle);
                }

                if (!isRunning)
                {
                    StartCoroutine(LoopVehicle());
                }
            }
        }

        [ServerCallback]
        private void OnTriggerExit(Collider other)
        {
            EnterVehicle enterVehicle = other.transform.GetComponentInParent<EnterVehicle>();

            if (enterVehicle != null)
            {
                if (enterVehicles.Contains(enterVehicle))
                {
                    enterVehicles.Remove(enterVehicle);
                }

                if (enterVehicles.Count == 0)
                {
                    isRunning = false;
                }
            }
        }

        [Server]
        private IEnumerator LoopVehicle()
        {
            isRunning = true;

            while (isRunning)
            {
                yield return new WaitForSeconds(fuelWaitTime);

                for (int i = 0; i < enterVehicles.Count; i++)
                {
                    if (enterVehicles[i] != null)
                    {
                        if ((tankFuelMaxQuantity == 0f || tankFuelMaxQuantity > addedFuelQuantity) && enterVehicles[i].vehicleController.vehicleFuel < enterVehicles[i].vehicleController.vehicleMaxFuel - addedFuelQuantity)
                        {
                            // Remove fuel from tank
                            if (tankFuelMaxQuantity > 0f)
                                tankFuelMaxQuantity -= addedFuelQuantity;

                            // Add fuel to vehicle
                            enterVehicles[i].vehicleController.vehicleFuel += addedFuelQuantity;
                        }
                    }
                }
            }
        }
    }
}
