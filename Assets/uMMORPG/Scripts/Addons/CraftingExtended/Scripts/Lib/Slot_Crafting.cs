using System;
using UnityEngine;
using UnityEngine.UI;

#if _iMMOCRAFTING
// ===================================================================================
// CRAFTING SLOT
// ===================================================================================
public class Slot_Crafting : MonoBehaviour
{
    [HideInInspector] public Tmpl_Recipe recipe;

    public Image overlay;
    public Image icon;
    public Text descriptionText;
    public InputField amountInput;
    public Button actionButton;
    public Button unlearnButton;
    public Toggle boostToggle;
    public UIShowToolTip extraTooltip;

    [HideInInspector] public int amount = 1;
    [HideInInspector] public bool boost = false;

    // -----------------------------------------------------------------------------------
    // Update
    // Always check to make sure our value isn't zero or negative we also don't want to be over
    // ninety-nine due to calculation time.
    // -----------------------------------------------------------------------------------
    private void Update()
    {
        Int32.TryParse(amountInput.text, out amount);
        if (amount < 1) amountInput.text = "1";
        if (amount > 99) amountInput.text = "99";

        boost = boostToggle.isOn;
    }

    // -----------------------------------------------------------------------------------
    // Show
    // -----------------------------------------------------------------------------------
    public void Show(Tmpl_Recipe r, bool canBoost)
    {
        recipe = r;
        descriptionText.text = recipe.name;
        icon.sprite = recipe.image;
        UIShowToolTip tooltip = GetComponent<UIShowToolTip>();
        tooltip.text = ToolTip();
        extraTooltip.text = ToolTip();
        boostToggle.gameObject.SetActive(canBoost);
    }

    // -----------------------------------------------------------------------------------
    // ToolTip
    // -----------------------------------------------------------------------------------
    public string ToolTip()
    {
        return recipe.ToolTip();
    }
}
#endif