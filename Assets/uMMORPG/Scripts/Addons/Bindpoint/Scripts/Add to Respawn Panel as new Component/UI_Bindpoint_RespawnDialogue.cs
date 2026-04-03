using UnityEngine;
using UnityEngine.UI;

// UI RESPAWN
public partial class UI_Bindpoint_RespawnDialogue : MonoBehaviour
{
    public GameObject panel;
    public Button RespawnToBindpointButton;

    // -----------------------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------------------
    private void Update()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        if (!panel.activeSelf) return;

        RespawnToBindpointButton.gameObject.SetActive(player.playerAddonsConfigurator.MyBindpoint.Valid);
        RespawnToBindpointButton.onClick.SetListener(() => { player.playerAddonsConfigurator.Cmd_RespawnToBindpoint(); });
    }
    // -----------------------------------------------------------------------------------
}