using UnityEngine;
#if _iMMOCRAFTING
// ===================================================================================
// CRAFTING UI
// ===================================================================================
public partial class UI_CraftingProfessions : MonoBehaviour
{
    public GameObject panel;
    public Transform content;
    public UI_CraftingSlot slotPrefab;
    public KeyCode hotKey = KeyCode.H;

    // -----------------------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------------------
    private void Update()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        // hotkey (not while typing in chat, etc.)
        if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
            panel.SetActive(!panel.activeSelf);

        if (panel.activeSelf && player.playerCraftingExtended.Crafts.Count > 0)
        {
            UIUtils.BalancePrefabs(slotPrefab.gameObject, player.playerCraftingExtended.Crafts.Count, content);

            for (int i = 0; i < content.childCount; i++)
            {
                content.GetChild(i).GetComponent<UI_CraftingSlot>().Show(player.playerCraftingExtended.Crafts[i]);
            }
        }
    }
}
#endif