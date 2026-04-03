using UnityEngine;
using UnityEngine.UI;

public class SpamSkill : MonoBehaviour
{
    public Button button; // The button to spam  
    public KeyCode[] keysToSpam; // Array of KeyCodes to check for

    private bool isSpamming = false; // Flag to track whether we are currently spamming

    void Update()
    {
        // Check if any key in the keysToSpam array is being held down  
        bool anyKeyHeld = false;
        foreach (KeyCode key in keysToSpam)
        {
            if (Input.GetKey(key))
            {
                anyKeyHeld = true;
                break; // No need to check further if one key is held down  
            }
        }

        if (anyKeyHeld && !isSpamming)
        {
            // If any key is held and we're not already spamming, start invoking  
            isSpamming = true;
            InvokeRepeating("InvokeButtonPress", 0f, 0.1f); // Adjust the delay as needed  
        }
        else if (!anyKeyHeld && isSpamming)
        {
            // If no key is held and we are spamming, stop invoking  
            isSpamming = false;
            CancelInvoke("InvokeButtonPress");
        }
    }

    void InvokeButtonPress()
    {
        button.onClick.Invoke(); // Invoke the button's onClick event  
    }
}
