using UnityEngine;
using UnityEngine.UI;
// Simple implementation example
// Not the most efficient, nor the most thorough
// Also implicitly assumes that a newly opened window
//     will be placed on top of other windows (Transform.SetAsLastSibling())

public class EscCloseMarker : MonoBehaviour
{
    // Marker Class
}

public class EscToCloseWindow : MonoBehaviour
{
    public Transform canvasT;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Check children in ascending order
            for (int i = canvasT.childCount - 1; i >= 0; i--)
            {
                Transform currentChild = canvasT.GetChild(i);
                if (currentChild.gameObject.activeInHierarchy && currentChild.GetComponent<EscCloseMarker>() != null)
                {
                    // Close selected window
                    currentChild.gameObject.SetActive(false);
                }
            }
        }
    }
}