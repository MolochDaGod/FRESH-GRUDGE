using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuickLoot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public KeyCode modifier = KeyCode.LeftAlt; // Modifier key to trigger the action  
    public bool onSlot; // Flag to check if the mouse is over the slot  
    public Button button; // Button to invoke  
    public float cooldownDuration = 1f; // Cooldown duration in seconds

    private float cooldownTimer; // Timer to track cooldown

    void Update()
    {
        // Update cooldown timer  
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime; // Decrease timer by the time since last frame  
        }

        // Hold down the modifier button  
        if (Input.GetKey(modifier))
        {
            // Check if the mouse cursor is on the Slot  
            if (onSlot && cooldownTimer <= 0)
            {
                button.onClick.Invoke(); // Invoke the button's click event  
                cooldownTimer = cooldownDuration; // Reset the cooldown timer  
            }
        }
    }

    public void OnPointerEnter(PointerEventData data)
    {
        onSlot = true; // Set flag when cursor enters the slot  
    }

    public void OnPointerExit(PointerEventData data)
    {
        onSlot = false; // Reset flag when cursor exits the slot  
    }
}
