using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PanelManager : MonoBehaviour
{
    [Header("Panels")]
    public List<GameObject> panels; // List to hold references to UI panels

    public void ShowPanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(true); // Activate the specified panel
            Debug.Log($"Panel {panel.name} is now shown.");
        }
        else
        {
            Debug.LogWarning("Attempted to show a null panel.");
        }
    }

    /// <summary>
    /// Shows multiple panels based on their indices.
    /// </summary>
    /// <param name="indices">Array of indices corresponding to the panels to show.</param>
    public void ShowMultiplePanels(int[] indices)
    {
        foreach (var index in indices)
        {
            if (index >= 0 && index < panels.Count)
            {
                ShowPanel(panels[index]); // Show the panel at the given index
            }
            else
            {
                Debug.LogWarning($"Index {index} is out of bounds for panels list.");
            }
        }
    }

    public void CloseAllPanels()
    {
        foreach (var panel in panels)
        {
            if (panel.activeSelf) // Check if the panel is active
            {
                panel.SetActive(false); // Deactivate the panel
                Debug.Log($"Panel {panel.name} is now closed.");
            }
        }
    }
}