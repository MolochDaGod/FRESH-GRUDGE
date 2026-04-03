using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GameEventListener))]
public class UI_Respawn : MonoBehaviour
{
    public GameObject panel;
    public Button button;

    public GameEvent gameEvent;

    public void UpdateEvent()
    {
        Player player = Player.localPlayer;

        // show while player is dead
        if (player != null && player.health.current == 0)
        {
            panel.SetActive(true);
            button.onClick.SetListener(() => { player.CmdRespawn(); });
        }
        else panel.SetActive(false);
    }
}
