using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "ADDON/Templates/Crafting/New PLayer Crafting", order = 998)]
public class Tmpl_PlayerCrafting : ScriptableObject
{
#if _iMMOCRAFTING
    [Header("[-=-[ CRAFTING (See Tooltips) ]-=-]")]
    [Tooltip("[Optional] Default recipes the player starts the game with.")]
    public Tmpl_Recipe[] startingRecipes;

    [Tooltip("[Optional] Default crafts the player starts the game with.")]
    public DefaultCraftingProfession[] startingCrafts;

    [Tooltip("[Optional] Popup text, sound and icons (as defined in Tools).")]
    public CraftingPopupMessages craftingPopupMessages;
#endif
}
