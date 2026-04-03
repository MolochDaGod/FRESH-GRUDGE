using UnityEngine;
using System.Collections.Generic;

public class PanelUse : MonoBehaviour
{
    [Header("Panel Manager Reference")]
    public PanelManager panelManager; // Reference to the PanelManager

    [Header("Panels to Activate (by index)")]
    public List<int> panelsToActivateIndices; // List of indices to activate panels in the PanelManager

    void OnMouseEnter()
    {
        Debug.Log("Mouse has entered the collider");
    }

    void OnMouseExit()
    {
        Debug.Log("Mouse has exited the collider");
    }

    private void Start()
    {
        // Attempt to find the PanelManager in the scene if it's not assigned in the inspector
        if (panelManager == null)
        {
            panelManager = FindObjectOfType<PanelManager>();
            if (panelManager == null)
            {
                Debug.LogError("No PanelManager found in the scene. Please assign it in the inspector or add one.");
            }
        }
    }

    private void OnMouseDown()
    {
        if (Vector3.Distance(Player.localPlayer.transform.position, transform.position) <= Player.localPlayer.interactionRange && !Utils.IsCursorOverUserInterface())
        {
            // Activate the specified panels when the object is clicked
            if (panelManager != null && !Utils.IsCursorOverUserInterface())
            {
                foreach (var index in panelsToActivateIndices)
                {
                    // Check if the index is within range
                    if (index >= 0 && index < panelManager.panels.Count) // Ensure you have panels in the PanelManager
                    {
                        panelManager.ShowPanel(panelManager.panels[index]); // Show the panel based on index
                    }
                }
            }
        }
    }
}