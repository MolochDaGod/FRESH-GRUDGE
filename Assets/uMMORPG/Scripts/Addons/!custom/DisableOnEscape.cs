using UnityEngine;

public class DisableOnEscape : MonoBehaviour
{
    public GameObject myPanel; // Assign this in the inspector

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePanel();
        }
    }

    void TogglePanel()
    {
        if (myPanel != null)
        {
            myPanel.SetActive(!myPanel.activeSelf); // Toggle the panel active state
        }
    }
}
