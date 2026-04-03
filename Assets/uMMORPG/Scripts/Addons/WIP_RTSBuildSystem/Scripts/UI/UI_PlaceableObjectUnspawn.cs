
using Mirror;
using UnityEngine;
using UnityEngine.UI;

// UI_PlaceableObjectUnspawn
public partial class UI_PlaceableObjectUnspawn : MonoBehaviour
{
    public GameObject panel;
    public Button acceptButton;
    public Button declineButton;
    private NetworkIdentity targetIdentity = null;

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
            // -- Unspawn Structure
            acceptButton.onClick.SetListener(() =>
            {
                //if (player.target)
                //{
                player.playerRTSBuildSystem.Cmd_UnspawnPlaceableObject(targetIdentity);
                panel.SetActive(false);
                targetIdentity = null;
                //}
                //else
                // {
                //     Debug.Log("Pas de target");
                // }
            });

            // -- Cancel Unspawn Structure
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
    public void Show(NetworkIdentity _targetIdentity)
    {
        targetIdentity = _targetIdentity;
        panel.SetActive(true);
    }

    // -----------------------------------------------------------------------------------
}
