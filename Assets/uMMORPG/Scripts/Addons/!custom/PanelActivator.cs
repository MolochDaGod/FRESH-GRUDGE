using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelActivator : MonoBehaviour
{
    public string[] panelNames; // Array of panel names to activate

    private void OnMouseDown() // This will be called when the object is clicked
    {
        ActivatePanels();
    }

    private void ActivatePanels()
    {
        foreach (string panelName in panelNames)
        {
            GameObject panel = GameObject.Find(panelName);
            if (panel != null)
            {
                panel.SetActive(true); // Activate the panel
            }
            else
            {
                Debug.LogWarning($"Panel named '{panelName}' not found!");
            }
        }
    }
}
