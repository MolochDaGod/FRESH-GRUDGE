using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// UI_BindpointPanel
public partial class UI_BindpointPanel : MonoBehaviour
{
    public static UI_BindpointPanel singleton;

    public GameObject panel;
    public Button acceptButton;
    public Button declineButton;

    public void Awake()
    {
        if (singleton == null) singleton = this;
    }

    public void btnAcceptBind()
    {
        Player player = Player.localPlayer;
        if (!player) return;
        // player.playerBindpoint.MyBindpoint.position = new Vector3(((Npc)player.target).npcBindpoint.bindpoint.position.x, ((Npc)player.target).npcBindpoint.bindpoint.position.y, ((Npc)player.target).npcBindpoint.bindpoint.position.z);
        player.playerAddonsConfigurator.MyBindpoint.position = player.transform.position;// new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z);
        player.playerAddonsConfigurator.MyBindpoint.SceneName = SceneManager.GetActiveScene().name;

        player.Tools_AddMessage("your soul is now bind here!");
        player.playerAddonsConfigurator.Cmd_SetBindpoint();
        panel.SetActive(false);
    }

    public void btnDeclineBind()
    {
        panel.SetActive(false);
    }
    // -----------------------------------------------------------------------------------
}