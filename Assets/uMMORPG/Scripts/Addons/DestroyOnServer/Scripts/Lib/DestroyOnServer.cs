using UnityEngine;
// DESTROY ON SERVER
public class DestroyOnServer : MonoBehaviour
{
    // -------------------------------------------------------------------------------
    // Start
    // -------------------------------------------------------------------------------
#if _SERVER && !_CLIENT
    private void Start()
    {
        Destroy(gameObject);
    }
#endif
    // -------------------------------------------------------------------------------
}