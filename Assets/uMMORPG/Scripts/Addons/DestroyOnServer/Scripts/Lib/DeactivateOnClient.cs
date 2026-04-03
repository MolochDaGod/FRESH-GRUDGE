using UnityEngine;
// DEACTIVATE ON CLIENT
public class DeactivateOnClient : MonoBehaviour
{
    // -------------------------------------------------------------------------------
    // Start
    // -------------------------------------------------------------------------------
    private void Start()
    {
#if _CIENT && !_SERVER
        this.gameObject.SetActive(false);
#else
        this.gameObject.SetActive(true);
#endif
    }
    // -------------------------------------------------------------------------------
}