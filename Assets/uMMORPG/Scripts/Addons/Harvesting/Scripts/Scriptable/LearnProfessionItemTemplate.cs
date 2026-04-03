using System.Text;
using UnityEngine;

#if _iMMOHARVESTING

// LEARN PROFESSION ITEM TEMPLATE

[CreateAssetMenu(fileName = "New Learn Profession", menuName = "ADDON/Item/Learn Profession Item", order = 999)]
public class LearnProfessionItemTemplate : UsableItem
{
    [Header("[-=-=-[ Learn Profession Item ]-=-=-]")]
    public HarvestingProfessionTemplate learnProfession;

    [Tooltip("[Optional] Amount of profession experience gained when used (should never be less than 1 - otherwise the profession wont be learned).")]
    public int gainProfessionExp = 1;

    [Tooltip("[Optional] The item can only be used when the profession has not been learned yet.")]
    public bool onlyWhenLearnable;

    public string expProfessionTxt = " Profession experience gained!";
    public string learnProfessionText = "You learned a new profession: ";

    [Tooltip("Decrease amount by how many each use (can be 0)?")]
    public int decreaseAmount = 1;

#if _iMMOTITLES
    [Header("[-=-=-[ Title Reward ]-=-=-]")]
    public Tmpl_Titles rewardTitle;
#endif

    // -----------------------------------------------------------------------------------
    // CanUse
    // -----------------------------------------------------------------------------------
    public override bool CanUse(Player player, int inventoryIndex)
    {
        return (onlyWhenLearnable && !player.playerHarvesting.HasHarvestingProfession(learnProfession) || !onlyWhenLearnable) && minLevel < player.level.current;
    }

    // -----------------------------------------------------------------------------------
    // Use
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

            if (!player.playerHarvesting.HasHarvestingProfession(learnProfession))
            {
                HarvestingProfession tmpProf = new HarvestingProfession(learnProfession.name);

                tmpProf.experience = gainProfessionExp;
                player.playerHarvesting.Professions.Add(tmpProf);

                player.Tools_ShowPopup(learnProfessionText + learnProfession.name);

#if _iMMOTITLES
                if(rewardTitle != null)
                    player.playerTitles.EarnTitle(rewardTitle);
#endif
            }
            else
            {
                HarvestingProfession tmpProf = player.playerHarvesting.getHarvestingProfessionData(learnProfession);

                tmpProf.experience += gainProfessionExp;

                player.playerHarvesting.SetHarvestingProfession(tmpProf);
                player.Tools_TargetAddMessage(gainProfessionExp.ToString() + expProfessionTxt);
            }

            slot.DecreaseAmount(decreaseAmount);
            player.inventory.slots[inventoryIndex] = slot;
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