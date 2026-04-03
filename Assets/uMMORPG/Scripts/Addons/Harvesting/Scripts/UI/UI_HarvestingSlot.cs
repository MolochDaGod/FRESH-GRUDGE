using UnityEngine;
using UnityEngine.UI;

#if _iMMOHARVESTING

// HARVESTING SLOT
public class UI_HarvestingSlot : MonoBehaviour
{
    public Text nameText;
    public Image professionIcon;
    public Slider expSlider;
    public UIShowToolTip tooltip;

    // -----------------------------------------------------------------------------------
    // Show
    // -----------------------------------------------------------------------------------
    public void Show(HarvestingProfession p)
    {
        float value = 0;

        string lvl = " [L" + p.level.ToString() + "/" + p.maxlevel.ToString() + "]";

        value = p.experiencePercent;

        nameText.text = p.template.name + lvl;
        professionIcon.sprite = p.template.image;
        expSlider.value = value;

        tooltip.enabled = true;
        tooltip.text = ToolTip(p.template);
    }

    // -----------------------------------------------------------------------------------
    // ToolTip
    // -----------------------------------------------------------------------------------
    public string ToolTip(HarvestingProfessionTemplate tpl)
    {
        return tpl.ToolTip();
    }
    // -----------------------------------------------------------------------------------
}
#endif