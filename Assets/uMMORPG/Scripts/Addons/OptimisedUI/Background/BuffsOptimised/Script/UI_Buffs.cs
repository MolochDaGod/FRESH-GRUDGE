using UnityEngine;

public partial class UI_Buffs : MonoBehaviour
{
    public GameObject panel;
    public UIBuffSlot slotPrefab;

    [Header("[-=-=-[ Refresh Time ]-=-=-]")]
    public float refreshSpeed = .1f;

    [Header("Game Event Updater")]
    public GameEvent gameEvent;

    public void Update()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            panel.SetActive(true);

            // instantiate/destroy enough slots
            UIUtils.BalancePrefabs(slotPrefab.gameObject, player.skills.buffs.Count, panel.transform);

            // No need to recount each loop in the same frame
            int skillCount = player.skills.buffs.Count;
            // refresh all
            for (int i = 0; i < skillCount; ++i)
            {
                Buff buff = player.skills.buffs[i];
                UIBuffSlot slot = panel.transform.GetChild(i).GetComponent<UIBuffSlot>();

                // refresh
                slot.image.color = Color.white;
                slot.image.sprite = buff.image;
                // only build tooltip while it's actually shown. this
                // avoids MASSIVE amounts of StringBuilder allocations.
                if (slot.tooltip.IsVisible())
                    slot.tooltip.text = buff.ToolTip();
                slot.slider.maxValue = buff.buffTime;
                slot.slider.value = buff.BuffTimeRemaining();
            }
        }
        else panel.SetActive(false);
    }
}
