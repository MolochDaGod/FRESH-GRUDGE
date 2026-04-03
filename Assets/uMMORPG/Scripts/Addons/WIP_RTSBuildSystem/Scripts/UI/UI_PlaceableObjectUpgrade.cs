
using UnityEngine;
using UnityEngine.UI;

// UI_PlaceableObjectSpawn

public partial class UI_PlaceableObjectUpgrade : MonoBehaviour
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

        if (player.playerRTSBuildSystem.myPlaceableObject != null && player.playerRTSBuildSystem.myPlaceableObject.ownerCharacter == player.name)
        {
            // -- Upgrade Structure
            acceptButton.onClick.SetListener(() =>
            {
                player.playerRTSBuildSystem.Cmd_StartUpgradePlaceableObject(player.playerRTSBuildSystem.myPlaceableObject.netIdentity);
                panel.SetActive(false);
            });

            // -- Cancel Upgrade Structure
            declineButton.onClick.SetListener(() =>
            {
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
