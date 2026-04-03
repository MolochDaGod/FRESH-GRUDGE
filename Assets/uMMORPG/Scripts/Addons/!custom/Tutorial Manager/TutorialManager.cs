using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    private GameObject popup1;
    private GameObject popup2;
    private GameObject popup3;

    private Vector3 lastPosition;

    public enum PlayerState { IDLE, CASTING }
    public PlayerState playerState; // This will be set externally

    private const string Popup3CompletedKey = "Popup3Completed";

    void Start()
    {
        // Find popups by full path if they are children of a Canvas or Panel  
        popup1 = GameObject.Find("Canvas/Panel/Popup1");
        popup2 = GameObject.Find("Canvas/Panel/Popup2");
        popup3 = GameObject.Find("Canvas/Panel/Popup3");

        // Check if Popup 3 has been completed  
        bool isPopup3Completed = PlayerPrefs.GetInt(Popup3CompletedKey, 0) == 1;

        // If Popup 3 is completed, hide all popups and exit early  
        if (isPopup3Completed)
        {
            HideAllPopups();
            return; // Exit the Start method early  
        }

        // Otherwise, initialize popups normally  
        ShowPopup(popup1);
        HidePopup(popup2);
        HidePopup(popup3);

        lastPosition = transform.position; // Assuming this script is on the player  

        // Check if player has moved since last position  
        if (Vector3.Distance(transform.position, lastPosition) > 0.1f)
        {
            HidePopup(popup1);
            ShowPopup(popup2);
            lastPosition = transform.position; // Update last position  
        }

        // Check initial player state to show popup3 if needed 
        if (playerState == PlayerState.CASTING)
        {
            HidePopup(popup2);
            ShowPopup(popup3);
        }

        // Add key listener for completing Popup 3  
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HidePopup(popup2);
            ShowPopup(popup3);
        }

        // Check if Popup 3 is active and handle completion if needed  
        if (popup3.activeSelf && Input.GetKeyDown(KeyCode.Return)) // Assuming pressing Return completes Popup 3  
        {
            CompletePopup3();
        }
    }

    void ShowPopup(GameObject popup)
    {
        if (popup != null)
        {
            popup.SetActive(true);
        }
    }

    void HidePopup(GameObject popup)
    {
        if (popup != null)
        {
            popup.SetActive(false);
        }
    }

    void HideAllPopups()
    {
        HidePopup(popup1);
        HidePopup(popup2);
        HidePopup(popup3);
    }

    void CompletePopup3()
    {
        // Mark Popup 3 as complete and save to PlayerPrefs  
        PlayerPrefs.SetInt(Popup3CompletedKey, 1);
        PlayerPrefs.Save(); // Ensure changes are saved

        // Hide all popups after completing Popup 3  
        HideAllPopups();
    }
}