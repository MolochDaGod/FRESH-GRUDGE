using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if _iMMOCRAFTING
// ===================================================================================
// CRAFTING SLOT
// ===================================================================================
public class UI_CraftingSlot : MonoBehaviour
{
    public Text nameText;
    public Image professionIcon;
    public Slider expSlider;
    public UIShowToolTip tooltip;
    public TMP_Text expText;
    public TMP_Text level;

    // -----------------------------------------------------------------------------------
    // Show
    // -----------------------------------------------------------------------------------
    public void Show(CraftingProfession p)
    {
        float value = 0;

        string lvl = " [L" + p.level.ToString() + "/" + p.maxlevel.ToString() + "]";

        value = p.experiencePercent;

        nameText.text = p.template.name + lvl;
        professionIcon.sprite = p.template.image;
        expSlider.value = value;

        tooltip.enabled = true;
        tooltip.text = ToolTip(p.template, p.level, p.maxlevel, p.experienceCurrent, p.experienceNext);
        expText.text = p.experienceCurrent + "/" + p.experienceNext;
        level.text = "Level : " + p.level.ToString() + "/" + p.maxlevel.ToString();
    }

    // -----------------------------------------------------------------------------------
    // ToolTip
    // -----------------------------------------------------------------------------------
    public string ToolTip(CraftingProfessionTemplate tpl, int level, int maxlevel, long expCurrent, long expNext)
    {
        StringBuilder tip = new StringBuilder();

        tip.Append("<b>"+tpl.name + "</b>\n");
        tip.Append(tpl.toolTip + "\n");
        tip.Append("<b>Level :</b> " + level + "/" + maxlevel+"\n");
        tip.Append("<b>Experience :</b> " + expCurrent + "/" + expNext);

        return tip.ToString();
    }
    // -----------------------------------------------------------------------------------
}
#endif