using UnityEngine;

public class panelHide : MonoBehaviour
{

    public GameObject panel;

    // -----------------------------------------------------------------------------------
    // Update
    // @Client
    // -----------------------------------------------------------------------------------
    private void Update()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        if (player.playerTravelroute.myTravelrouteArea == null)
        {
            panel.SetActive(false);
        }
    }
}
