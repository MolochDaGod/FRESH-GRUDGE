using UnityEngine;
using UnityEngine.UI;
// UI_PlaceableObjectSpawn
public partial class UI_PlaceableObjectSpawn : MonoBehaviour
{
    public GameObject panel;
    public Button acceptButton;
    public Button declineButton;

    // -----------------------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------------------
    private void Update()
    {
        if (!panel.activeSelf) return;

        Player player = Player.localPlayer;
        if (!player) return;

        if (panel.activeSelf && player.isAlive /*&& player.state == "IDLE"*/)
        {
            // -- Spawn Structure
            acceptButton.gameObject.SetActive(player.playerRTSBuildSystem.CanSpawnPlaceableObject());
            acceptButton.onClick.SetListener(() =>
            {
                player.playerRTSBuildSystem.Cmd_StartSpawnPlaceableObject();
                panel.SetActive(false);
            });

            // -- Cancel Spawn Structure
            declineButton.onClick.SetListener(() =>
            {
                player.playerRTSBuildSystem.CancelSpawnPlaceableObject();
                panel.SetActive(false);
            });
        }
        else
        {
            panel.SetActive(false);
        }
    }

    // -----------------------------------------------------------------------------------
    // Show
    // -----------------------------------------------------------------------------------
    public void Show()
    {
        panel.SetActive(true);
    }

    // -----------------------------------------------------------------------------------
}
