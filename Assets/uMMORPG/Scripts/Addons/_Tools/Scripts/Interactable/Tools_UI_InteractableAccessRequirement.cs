#if _iMMOTOOLS
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;

// ===================================================================================
// UI INTERACTABLE ACCESS REQUIREMENT
// ===================================================================================
public partial class Tools_UI_InteractableAccessRequirement : Tools_UI_Requirement
{
    [Header("[COSTS]")]
    public string labelGoldCost = " - Gold cost per use: ";
    public string labelCoinCost = " - Coins cost per use: ";
#if _iMMOHONORSHOP
    public string labelRequiredHonorCurrency 		= " - Honor Currency cost: ";
#endif

    protected Tools_Interactable interactable;

    // -----------------------------------------------------------------------------------
    // Show
    // -----------------------------------------------------------------------------------
    public void Show(Tools_Interactable _interactable)
    {
        Player player = Player.localPlayer;
        if (!player) return;

        interactable = _interactable;
        requirements = interactable.interactionRequirements;

        for (int i = 0; i < content.childCount; ++i)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        updateTextbox();

        interactButton.interactable = interactable.interactionRequirements.checkRequirements(player) || interactable.IsUnlocked();

        if (interactable.interactionText != "")
            interactButton.GetComponentInChildren<Text>().text = interactable.interactionText;

        interactButton.onClick.SetListener(() =>
        {
            interactable.ConfirmAccess();
            Hide();
        });

        cancelButton.onClick.SetListener(() =>
        {
            Hide();
        });

        panel.SetActive(true);
    }

    // -----------------------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------------------
    protected override void Update()
    {
        if (!panel.activeSelf) return;

        Player player = Player.localPlayer;
        if (!player) return;

        if (!UMMO_Tools.Tools_CheckSelectionHandling(interactable.gameObject))
        {
            Hide();
        }
    }

    // -----------------------------------------------------------------------------------
    // updateTextbox
    // -----------------------------------------------------------------------------------
    protected override void updateTextbox()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        base.updateTextbox();

        // ------------ Costs

        Tools_InteractionRequirements ir = (Tools_InteractionRequirements)requirements;

        if (ir.goldCost > 0)
            AddMessage(labelGoldCost + ir.goldCost.ToString(), player.gold >= ir.goldCost ? textColor : errorColor);

        if (ir.coinCost > 0)
            AddMessage(labelCoinCost + ir.coinCost.ToString(), player.itemMall.coins >= ir.coinCost ? textColor : errorColor);

#if _iMMOHONORSHOP
		if (ir.honorCurrencyCost.Length > 0)
		{
			AddMessage(labelRequiredHonorCurrency, textColor);
			foreach (HonorShopCurrencyDrop currency in ir.honorCurrencyCost)
			{
				if (player.playerHonorShop.GetHonorCurrency(currency.honorCurrency) < currency.amount)
				{
					AddMessage(currency.honorCurrency.name + " x" + currency.amount.ToString(), errorColor);
				}
				else
				{
					AddMessage(currency.honorCurrency.name + " x" + currency.amount.ToString(), textColor);
				}
			}
		}
#endif
    }

    // -----------------------------------------------------------------------------------
}
#endif