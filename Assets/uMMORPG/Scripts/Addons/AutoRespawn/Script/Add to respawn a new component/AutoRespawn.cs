using TMPro;
using UnityEngine;

// UI RESPAWN
public partial class AutoRespawn : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text AutoRespawnText;

    [Header("[-=-[ Respawn TIMER ]-=-]")]
    [Range(1, 9999)] public int AutoRespawnTime = 30;

    private int secondElasted = 0;
    private int nextUpdate;

    private void OnEnable()
    {
        secondElasted = 0;
        nextUpdate = 0;
    }

    [Header("[-=-[ Refresh Speed ]-=-]")]
    public float refreshSpeed = 1f;

    void Update()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        if (!panel.activeSelf)
        {
            secondElasted = 0;
            nextUpdate = 0;
            return;
        }
        else if (Time.time >= nextUpdate)
        {
            AutoRespawnText.text = "Auto respawn in : " + (AutoRespawnTime - secondElasted) + "s";
            nextUpdate = Mathf.FloorToInt(Time.time) + 1;
            ++secondElasted;
            if (secondElasted > AutoRespawnTime)
            {
#if _iMMOBINDPOINT
                if (player.playerAddonsConfigurator.MyBindpoint.Valid)
                    player.playerAddonsConfigurator.Cmd_RespawnToBindpoint();
                else
                    player.CmdRespawn();
#else
                player.CmdRespawn();
#endif
            }

        }
    }
    // -----------------------------------------------------------------------------------
}