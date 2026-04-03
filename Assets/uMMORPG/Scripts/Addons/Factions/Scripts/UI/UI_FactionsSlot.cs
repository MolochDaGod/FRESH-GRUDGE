using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ===================================================================================
// Faction SLOT
// ===================================================================================
public class UI_FactionsSlot : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text statusText;
    public TMP_Text pointsText;
    public Image factionIcon;
    public Slider ratingSlider;
    public UIShowToolTip tooltip;

    // -----------------------------------------------------------------------------------
    // Show
    // -----------------------------------------------------------------------------------
    public void Show(Faction faction)
    {
        Tmpl_Faction data = faction.data;

        nameText.text = data.name;
        statusText.text = data.getRank(faction.rating);
        pointsText.text = faction.rating.ToString();
        factionIcon.sprite = data.image;
        ratingSlider.value = faction.rating;

        tooltip.enabled = true;
        tooltip.text = data.name + " [" + faction.rating.ToString() + " " + data.getRank(faction.rating) + "]\n" + data.description;
    }

    // -----------------------------------------------------------------------------------
}