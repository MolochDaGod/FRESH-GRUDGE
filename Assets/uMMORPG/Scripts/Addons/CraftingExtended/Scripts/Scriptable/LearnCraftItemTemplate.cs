using System.Text;
using UnityEngine;
#if _iMMOCRAFTING
// LEARN CRAFT ITEM
[CreateAssetMenu(menuName = "ADDON/Crafting/Learn Craft Item", order = 999)]
public class LearnCraftItemTemplate : UsableItem
{
    [Header("Usage")]
    public CraftingProfessionTemplate learnProfession;

    [Tooltip("[Optional] Amount of profession experience gained when used (should never be less than 1 - otherwise the profession wont be learned).")]
    public int gainProfessionExp = 1;

    [Tooltip("[Optional] The item can only be used when the profession has not been learned yet.")]
    public bool onlyWhenLearnable;

    public string expProfessionTxt = " Profession experience gained!";
    public string learnProfessionText = "You learned a new craft: ";

    [Tooltip("Decrease amount by how many each use (can be 0)?")]
    public int decreaseAmount = 1;

#if _iMMOTITLES
    [Header("[-=-[ Earn Title ]-=-]")]
    public Tmpl_Titles earnTitle;
#endif
    // -----------------------------------------------------------------------------------
    // CanUse
    // -----------------------------------------------------------------------------------
    public override bool CanUse(Player player, int inventoryIndex)
    {
        return (onlyWhenLearnable && !player.playerCraftingExtended.HasCraftingProfession(learnProfession) || !onlyWhenLearnable) && minLevel < player.level.current;
    }

    // -----------------------------------------------------------------------------------
    //  Use
    // -----------------------------------------------------------------------------------
    public override void Use(Player player, int inventoryIndex)
    {
#if _SERVER
        ItemSlot slot = player.inventory.slots[inventoryIndex];

        // -- Only activate if enough charges left
        if (decreaseAmount == 0 || slot.amount >= decreaseAmount)
        {
            // always call base function too
            base.Use(player, inventoryIndex);

            if (!player.playerCraftingExtended.HasCraftingProfession(learnProfession))
            {
                Debug.Log("learnProfession.name = " + learnProfession.name);
                CraftingProfession tmpProf = new CraftingProfession(learnProfession.name);

                tmpProf.experience = gainProfessionExp;
                player.playerCraftingExtended.Crafts.Add(tmpProf);

                player.Tools_ShowPopup(learnProfessionText + learnProfession.name);
#if _iMMOTITLES
                player.playerTitles.EarnTitle(earnTitle);
#endif
                slot.DecreaseAmount(decreaseAmount);
                player.inventory.slots[inventoryIndex] = slot;
            }
            /*else
            {
                CraftingProfession tmpProf = player.playerCraftingExtended.getCraftingProfession(learnProfession);

                tmpProf.experience += gainProfessionExp;

                player.playerCraftingExtended.setCraftingProfession(tmpProf);
                player.Tools_TargetAddMessage(gainProfessionExp.ToString() + expProfessionTxt);
            }*/


        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // ToolTip
    // -----------------------------------------------------------------------------------
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{MINLEVEL}", minLevel.ToString());
        return tip.ToString();
    }
    // -----------------------------------------------------------------------------------
}
#endif