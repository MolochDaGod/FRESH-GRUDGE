
//(c8

using UnityEngine;
using UnityEngine.UI;

namespace BurnyDev.UMMORPG.AddonCustomVehicle
{
    public class UIVehicle : MonoBehaviour
    {
        public Text vehicleText;

        private void Update()
        {
            Player player = Player.localPlayer;
            if (player != null)
            {
                PlayerVehicle playerVehicle = player.GetComponent<PlayerVehicle>();
                if (playerVehicle != null && playerVehicle.IsInsideVehicle())
                {
                    vehicleText.text = playerVehicle.enterVehicle.vehicleController.GetVehicleSpeed().ToString("0") + " km/h" + (playerVehicle.enterVehicle.vehicleController.vehicleNeedFuel ? "   Fuel: " + playerVehicle.enterVehicle.vehicleController.vehicleFuel.ToString("0") : "");
                }
                else
                    vehicleText.text = "";
            }
        }
    }
}
