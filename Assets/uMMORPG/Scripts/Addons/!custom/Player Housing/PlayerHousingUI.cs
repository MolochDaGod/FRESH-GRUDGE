using UnityEngine;
using UnityEngine.UI;

public class PlayerHousingUI : MonoBehaviour
{
    public GameObject panel; // Assign your UI panel in the inspector  
    public Button[] buildableButtons; // Assign buttons for each buildable object  
    private PlayerHousing playerHousing;

    void Start()
    {
        // Assume the PlayerHousing component is on the same GameObject or find it on the player object  
        playerHousing = FindObjectOfType<PlayerHousing>();
        panel.SetActive(false); // Hide the panel at the start

        // Assign button listeners  
        for (int i = 0; i < buildableButtons.Length; i++)
        {
            int index = i; // Capture the index  
            buildableButtons[i].onClick.AddListener(() => SelectBuildableObject(index));
        }
    }

    void Update()
    {
        // Check for hotkey press (e.g., "B" key)
        if (Input.GetKeyDown(KeyCode.B))
        {
            TogglePanel();
        }
    }

    void TogglePanel()
    {
        panel.SetActive(!panel.activeSelf);
        if (panel.activeSelf)
        {
            UpdateBuildableButtons();
        }
    }

    void UpdateBuildableButtons()
    {
        for (int i = 0; i < buildableButtons.Length; i++)
        {
            if (i < playerHousing.buildableObjects.Length)
            {
                buildableButtons[i].GetComponentInChildren<Text>().text = playerHousing.buildableObjects[i].name;
                buildableButtons[i].gameObject.SetActive(true);
            }
            else
            {
                buildableButtons[i].gameObject.SetActive(false);
            }
        }
    }

    void SelectBuildableObject(int index)
    {
        Debug.Log("Selected: " + playerHousing.buildableObjects[index].name);
        panel.SetActive(false); // Hide the panel after selection  
        // Add logic to place the selected object in the game world  
    }
}
