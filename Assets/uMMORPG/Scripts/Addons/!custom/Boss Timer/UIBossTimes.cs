using Mirror;
using UnityEngine;
using UnityEngine.UI;

public partial class UIBossTimes : MonoBehaviour
{
    public GameObject panel;

    public Text boss1Text;
    public Text boss2Text;
    public Text boss3Text;
    public Text boss4Text;
    public Text boss5Text;
    public Text boss6Text;
    public Text boss7Text;
    public Text boss8Text;

    public Monster boss1;
    public Monster boss2;
    public Monster boss3;
    public Monster boss4;
    public Monster boss5;
    public Monster boss6;
    public Monster boss7;
    public Monster boss8;

    void Update()
    {
        Player player = Player.localPlayer;
        if (player != null)
        {
            panel.SetActive(true);
            if (boss1.health.current > 0)
                boss1Text.text = "Active";
            else
                boss1Text.text = "Respawn - " + (boss1.respawnTimeEnd - NetworkTime.time).ToString("N0") + "s";
            if (boss2.health.current > 0)
                boss2Text.text = "Active";
            else
                boss2Text.text = "Respawns - " + (boss2.respawnTimeEnd - NetworkTime.time).ToString("N0") + "s";
            if (boss3.health.current > 0)
                boss3Text.text = "Active";
            else
                boss3Text.text = "Respawns - " + (boss3.respawnTimeEnd - NetworkTime.time).ToString("N0") + "s";
            if (boss4.health.current > 0)
                boss4Text.text = "Active";
            else
                boss4Text.text = "Respawns - " + (boss4.respawnTimeEnd - NetworkTime.time).ToString("N0") + "s";
            if (boss5.health.current > 0)
                boss5Text.text = "Active";
            else
                boss5Text.text = "Respawns - " + (boss1.respawnTimeEnd - NetworkTime.time).ToString("N0") + "s";
            if (boss6.health.current > 0)
                boss6Text.text = "Active";
            else
                boss6Text.text = "Respawns - " + (boss2.respawnTimeEnd - NetworkTime.time).ToString("N0") + "s";
            if (boss7.health.current > 0)
                boss7Text.text = "Active";
            else
                boss7Text.text = "Respawns - " + (boss3.respawnTimeEnd - NetworkTime.time).ToString("N0") + "s";
            if (boss8.health.current > 0)
                boss8Text.text = "Active";
            else
                boss8Text.text = "Respawns - " + (boss4.respawnTimeEnd - NetworkTime.time).ToString("N0") + "s";
        }
        else panel.SetActive(false);
    }
}
