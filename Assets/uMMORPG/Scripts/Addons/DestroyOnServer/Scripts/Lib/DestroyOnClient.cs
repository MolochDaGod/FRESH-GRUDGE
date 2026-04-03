using UnityEngine;

// DESTROY ON CLIENT
public class DestroyOnClient : MonoBehaviour
{
    // -------------------------------------------------------------------------------
    // Start
    // -------------------------------------------------------------------------------
#if !_SERVER && _CLIENT
    private void Start()
    {
        Destroy(this.gameObject);
    }
#endif
    // -------------------------------------------------------------------------------
}