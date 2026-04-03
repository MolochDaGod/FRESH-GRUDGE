using UnityEngine;
using UnityEngine.UI;

public class CameraSensitivitySlider : MonoBehaviour
{
    public Slider sensitivitySlider;
    private PlayerCharacterControllerMovementMobile playerMovement; // Changed to the correct controller type

    public void OnSliderValueChanged()
    {
        Player player = Player.localPlayer;
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerCharacterControllerMovementMobile>();

            if (playerMovement != null)
            {
                // Update the camera sensitivity value in the PlayerMovement script
                playerMovement.XSensitivity = sensitivitySlider.value;
                playerMovement.YSensitivity = sensitivitySlider.value;
            }
        }
    }
}
