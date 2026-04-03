using UnityEngine;
using System.Collections;
public class EnableAfter : MonoBehaviour
{
    public GameObject objectToEnable; // Drag your UI element here in the inspector
    public float delayTime = 3f; // Time in seconds to wait before enabling the object
    private bool hasEnabled = false;

    void Awake()
    {
        // Optional: Disable the object at the start
        if (objectToEnable != null)
        {
            objectToEnable.SetActive(false);
        }
        else
        {
            Debug.LogWarning("The object to enable is not assigned!");
        }
    }

    void Start()
    {
        StartCoroutine(EnableObjectTimer());
    }

    private IEnumerator EnableObjectTimer()
    {
        yield return new WaitForSeconds(delayTime); // Wait for the specified delay time

        if (objectToEnable != null && !hasEnabled)
        {
            objectToEnable.SetActive(true); // Enable the object
            hasEnabled = true; // Set the flag to true to indicate it has been enabled
        }
    }
}
