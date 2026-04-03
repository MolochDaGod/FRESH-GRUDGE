using UnityEngine;

public partial class UI_Harvesting : MonoBehaviour
{
#if _iMMOHARVESTING
    public GameObject panel;
    public Transform content;
    public UI_HarvestingSlot slotPrefab;

    private void OnEnable()
    {
        UpdateEvent();
    }

    public void UpdateEvent()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        if (player.playerHarvesting.Professions.Count > 0)
        {
            UIUtils.BalancePrefabs(slotPrefab.gameObject, player.playerHarvesting.Professions.Count, content);

            for (int i = 0; i < content.childCount; i++)
            {
                content.GetChild(i).GetComponent<UI_HarvestingSlot>().Show(player.playerHarvesting.Professions[i]);
            }
        }
    }
#endif
}