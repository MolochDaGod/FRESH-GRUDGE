using UnityEngine;
// DEACTIVATE ON SERVER
public class DeactivateOnServer : MonoBehaviour
{
    // -------------------------------------------------------------------------------
    // Start
    // -------------------------------------------------------------------------------
    private void Start()
    {
#if _SERVER && !_CLIENT
        this.gameObject.SetActive(false);
#else
        this.gameObject.SetActive(true);
#endif
    }
    // -------------------------------------------------------------------------------
}