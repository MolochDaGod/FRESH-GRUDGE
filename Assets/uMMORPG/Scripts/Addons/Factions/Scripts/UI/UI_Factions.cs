using UnityEngine;

// ===================================================================================
// FACTIONS UI
// ===================================================================================
public partial class UI_Factions : MonoBehaviour
{
    public GameObject panel;
    public Transform content;
    public UI_FactionsSlot slotPrefab;

    public void OnEnable()
    {
        UpdateEvent();
    }

    public void UpdateEvent()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        if (panel.activeSelf && player.playerFactions.Factions.Count > 0)
        {
            UIUtils.BalancePrefabs(slotPrefab.gameObject, player.playerFactions.Factions.Count, content);

            for (int i = 0; i < content.childCount; i++)
            {
                content.GetChild(i).GetComponent<UI_FactionsSlot>().Show(player.playerFactions.Factions[i]);
            }
        }
    }
}