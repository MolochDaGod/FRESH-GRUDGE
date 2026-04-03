using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableEnableOnTrigger : MonoBehaviour
{
    public GameObject targetObject; // The object to enable/disable  
    public Collider triggerCollider; // The collider that will act as the trigger

    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object has the "Player" tag  
        if (other.CompareTag("Player"))
        {
            // Disable the target object  
            if (targetObject != null)
            {
                targetObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the colliding object has the "Player" tag  
        if (other.CompareTag("Player"))
        {
            // Enable the target object  
            if (targetObject != null)
            {
                targetObject.SetActive(false);
            }
        }
    }

    private void Start()
    {
        // Ensure that the collider is set as a trigger  
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
        else
        {
            Debug.LogWarning("Trigger Collider is not assigned. Please assign a collider in the inspector.");
        }
    }
}
