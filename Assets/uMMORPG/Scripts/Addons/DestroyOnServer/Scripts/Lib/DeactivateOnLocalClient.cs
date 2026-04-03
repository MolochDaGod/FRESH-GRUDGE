using UnityEngine;
// DEACTIVATE ON lOCAL CLIENT
public class DeactivateOnLocalClient : MonoBehaviour
{
    // -------------------------------------------------------------------------------
    // Start
    // -------------------------------------------------------------------------------
    private void Start()
    {
#if _CIENT && !_SERVER
        Player player = Player.localPlayer;
        if (player)
            this.gameObject.SetActive(false);
        else
            this.gameObject.SetActive(true);
#else
        this.gameObject.SetActive(true);
#endif
    }
    // -------------------------------------------------------------------------------
}