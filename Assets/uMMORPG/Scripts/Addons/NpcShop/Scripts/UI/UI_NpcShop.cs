using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class UI_NpcShop : MonoBehaviour
{
    public GameObject panel;

    public Button categorySlotPrefab;
    public UI_NpcShopSlot itemSlotPrefab;

    public Button buttonSwitchMode;
    public Button repairButton;
    public Transform categoryContent;
    public ScrollRect scrollRect;
    public Transform itemContent;
    public Text currencyAmountText;
    public GameObject inventoryPanel;

    public GameObject uniqueAcquiredPanel;

    public string emptyCategoryLabel = "";
    public string sellText = "Sell";
    public string buyText = "Buy";
    public string buyAmountText = "How many do you want to buy?";
    public string sellAmountText = "How many do you want to sell?";

    [Header("[-=-[ Input Amout ]-=-]")]
    public GameObject inputAmount;
    public TMP_Text inputAmountMessage;
    public TMP_Text inputSliderText;
    public Slider inputSliderAmount;
    public Button inputConfirm;
    public Button inputCancel;

    private bool sellMode = false;
    private bool isActive = false;
    private string currentCategory = "";

    private List<ScriptableItem> list = new List<ScriptableItem>();
    public void Show()
    {
        Player player = Player.localPlayer;
        panel.SetActive(true);

        // use collider point(s) to also work with big entities
        if (player != null && player.target != null && player.target is Npc npc && Utils.ClosestDistance(player, player.target) <= player.interactionRange)
        {
            // On vide la liste au refresh quoi qu'il se passe
            if (npc.npcShop != null)
            {

                // --------------------------------------------------------------------------- Category button (Buy/Sell Mode)

                // quand on ouvre le panel on attribue tous les boutons et on affiche par defaut la catégorie 0 on va viré all je pense
                if (npc.npcShop.shopTrading.shopCategories.Length > 0)
                {
                    GenerateCategoryButton(npc.npcShop.shopTrading.shopCategories);

                    if (isActive)
                    {
                        ShowItemCategory(currentCategory);
                    }
                    else
                    {
                        currentCategory = npc.npcShop.shopTrading.shopCategories[0];
                        ShowItemCategory(npc.npcShop.shopTrading.shopCategories[0]);
                    }
                }
                // --------------------------------------------------------------------------- Update Overview
                currencyAmountText.text = player.gold.ToString();
            }
            isActive = true;
#if !_iMMO2D
            /**
             * Repair Service
             */
            if (npc.npcShop.shopTrading.offersRepair)
            {

                repairButton.gameObject.SetActive(true);
                repairButton.onClick.SetListener(() =>
                {

                    int costInventory = ((npc.npcShop.shopTrading.repairInventory) ? player.inventory.GetTotalMissingDurability() : 0);
                    int costEquipment = ((npc.npcShop.shopTrading.repairEquipment) ? player.equipment.GetTotalMissingDurability() : 0);
                    int missing = costInventory + costEquipment;
                    int price = missing * npc.npcShop.shopTrading.repairGoldCostPerDurabilityPoint;

                    if (price == 0)
                    {
                        UIPopup.singleton.Show("Sorry no item need repair");
                    }
                    else if (price > player.gold)
                    {
                        UIPopup.singleton.Show("sorry you don't have enough money,\r\n the repair price and " + price + " gold, \r\nand you only have " + player.gold + " gold");
                    }
                    else
                    {
                        UIConfirmation.singleton.Show("Repair all Items for: " + price + " gold?", () =>
                        {
                            player.playerNpcShop.Cmd_RepairAllItems();
                            currencyAmountText.text = player.gold.ToString(); // on update les gold car il on changer
                            ShowItemCategory(currentCategory); // On update l'interface au cas ou on ne puisse plus acheter un item
                        });
                    }
                });
            }
            else
            {
                repairButton.gameObject.SetActive(false);
            }
#else
            repairButton.gameObject.SetActive(false);
#endif
        }
        else
        {
            panel.SetActive(false);
        }
    }

    private void GenerateCategoryButton(string[] categoriesList)
    {
        UIUtils.BalancePrefabs(categorySlotPrefab.gameObject, categoriesList.Length, categoryContent);

        // --------------------------------------------------------------------------- Refresh Category Buttons
        for (int i = 0; i < categoriesList.Length; ++i)
        {
            Button button = categoryContent.GetChild(i).GetComponent<Button>();

            button.interactable = categoriesList[i] != currentCategory;

            button.GetComponentInChildren<TMP_Text>().text = categoriesList[i];

            int icopy = i; // needed for lambdas, otherwise i is Count
            string catName = categoriesList[i]; /// needed for lambdas, otherwise i is Count

            button.onClick.AddListener(() =>
            {
                currentCategory = categoriesList[icopy];
                ShowItemCategory(currentCategory);
                EnableDisableButton(categoriesList);
                ScrollToBeginning();
            });
        }
    }

    private void EnableDisableButton(string[] categoriesList)
    {
        for (int i = 0; i < categoriesList.Length; ++i)
        {
            Button button = categoryContent.GetChild(i).GetComponent<Button>();
            bool isActive = categoriesList[i] != currentCategory;
            button.interactable = isActive;
        }
    }

    private void ShowItemCategory(string category)
    {
        Player player = Player.localPlayer;
        if (player != null && player.target != null && player.target is Npc npc && Utils.ClosestDistance(player, player.target) <= player.interactionRange)
        {

            // --------------------------------------------------------------------------- Update Content (Sell Mode)
            if (sellMode)
            {
                GenSellList(player);
            }
            // --------------------------------------------------------------------------- Update Content (Buy Mode)
            else
            {
                GenBuyListItem(npc, player);
            }
            currencyAmountText.text = player.gold.ToString();
        }
    }

    private void GenBuyListItem(Npc npc, Player player)
    {
        List<ScriptableItem> list = new List<ScriptableItem>();
        ScriptableItem[] items = npc.npcShop.shopTrading.saleItems.Where(x => (x.itemCategory == currentCategory || currentCategory == "" || currentCategory == "All")).ToArray();
        list.AddRange(items);



        // Si j'ai des item unique
        if (npc.npcShop.shopTrading.saleUniqueItems.Length > 0)
        {
            foreach (var uniqueItem in npc.npcShop.shopTrading.saleUniqueItems)
            {
                // Si l'article unique appartient à la catégorie actuelle
                if (uniqueItem.saleUniqueItems.itemCategory == currentCategory || currentCategory == "" || currentCategory == "All")
                {
                    bool shouldAddUniqueItem = true;

                    // Vérifier si l'article unique est déjà vendu
                    if (uniqueItem.isUniqueWorldItem == true && npc.npcShop.shopItemsSold.Any(soldItem => soldItem.itemName == uniqueItem.saleUniqueItems.name))
                    {
                        // L'article unique est déjà vendu
                        shouldAddUniqueItem = false;
                    }
                    else if (uniqueItem.isUniqueWorldItem == false)
                    {
                        // L'article unique peut être vendu plusieurs fois, tant qu'il n'a pas déjà été acheté par le joueur
                        if (npc.npcShop.shopItemsSold.Any(soldItem => soldItem.itemName == uniqueItem.saleUniqueItems.name && soldItem.character == player.name))
                        {
                            // Le joueur a déjà acheté l'article unique
                            shouldAddUniqueItem = false;
                        }
                    }

                    // Ajouter l'article unique à la liste si nécessaire
                    if (shouldAddUniqueItem)
                    {
                        list.Add(uniqueItem.saleUniqueItems);
                    }
                }
            }
        }

        UIUtils.BalancePrefabs(itemSlotPrefab.gameObject, list.Count, itemContent);
        int totalItems = list.Count;
        // refresh all items in that category
        for (int i = 0; i < totalItems; ++i)
        {
            UI_NpcShopSlot slot = itemContent.GetChild(i).GetComponent<UI_NpcShopSlot>();
            ScriptableItem itemData = list[i];
#if _iMMOITEMRARITY
            slot.RatityOutline.color = RarityColor.SetRarityColorResult(itemData);
#endif
            slot.tooltip.text = new Item(itemData).ToolTip();
            slot.image.color = Color.white;
            slot.image.sprite = itemData.image;
            slot.nameText.text = itemData.name;
            slot.priceText.text = itemData.buyPrice.ToString();
            slot.buyText.text = buyText;
            bool isUnique = i > (items.Length - 1);
            if (isUnique)
            {
                slot.unique.SetActive(true);
            }
            slot.buyButton.interactable = player.isAlive && player.gold >= itemData.buyPrice;
            int icopy = i; // needed for lambdas, otherwise i is Count
            int maxBuy = MaxBuyAmount(player, itemData);
            slot.buyButton.onClick.SetListener(() =>
            {
                if (maxBuy == 1)
                {
                    ConfirmBuy(1, icopy, totalItems, isUnique);
                }
                else
                    ShowAmountSlider(buyAmountText, 1, maxBuy, icopy, totalItems, isUnique);
            });

        }
    }

    private void GenSellList(Player player)
    {
        List<ItemSlot> sellitems = new List<ItemSlot>();
        List<int> sellitemIndices = new List<int>();
        sellitems.Clear();
        sellitemIndices.Clear();
        for (int i = 0; i < player.inventory.slots.Count; i++)
        {
            if (player.inventory.slots[i].amount > 0 && player.inventory.slots[i].item.data.sellable && (player.inventory.slots[i].item.data.itemCategory == currentCategory || currentCategory == "" || currentCategory == "All"))
            {
                sellitems.Add(player.inventory.slots[i]);
                sellitemIndices.Add(i);
            }
        }
        int totalItems = sellitems.Count;
        UIUtils.BalancePrefabs(itemSlotPrefab.gameObject, sellitems.Count, itemContent);

        if (sellitems.Count > 0)
        {
            for (int i = 0; i < sellitems.Count; ++i)
            {
                UI_NpcShopSlot slot = itemContent.GetChild(i).GetComponent<UI_NpcShopSlot>();
                ScriptableItem itemData = sellitems[i].item.data;

#if _iMMOITEMRARITY
                slot.RatityOutline.color = RarityColor.SetRarityColorResult(itemData);
#endif
                slot.tooltip.text = new Item(itemData).ToolTip();
                slot.image.color = Color.white;
                slot.image.sprite = itemData.image;
                slot.nameText.text = itemData.name;
                slot.quantityText.text = " x" + sellitems[i].amount;
                slot.priceText.text = itemData.sellPrice.ToString();
                slot.buyText.text = sellText;
                slot.buyButton.interactable = player.isAlive && sellitems[i].amount > 0;


                int icopy = i;
                int iInventotycopy = sellitemIndices[i];
                slot.buyButton.onClick.SetListener(() =>
                {
                    //if (sellitems[icopy].amount == 1)
                    //{
                    ConfirmSell(1, iInventotycopy);
                    //}
                    //else
                    //{
                    //    ShowAmountSlider(sellAmountText, 1, sellitems[icopy].amount, totalItems, iInventotycopy);
                    //}
                    inventoryPanel.SetActive(true); // better feedback
                });
            }
        }
    }


    // -----------------------------------------------------------------------------------
    // Button Switch mode
    // -----------------------------------------------------------------------------------
    public void ChangeModeShop()
    {
        buttonSwitchMode.GetComponentInChildren<Text>().text = (sellMode) ? sellText : buyText;
        sellMode = !sellMode;
        ShowItemCategory(currentCategory);
    }

    // -----------------------------------------------------------------------------------
    // ConfirmBuy
    // -----------------------------------------------------------------------------------
    private void ConfirmBuy(int amount, int id, int totalItems, bool isUnique = false)
    {
        Player player = Player.localPlayer;
        if (!player) return;

        player.playerNpcShop.Cmd_NpcBuyItem(id, currentCategory, amount, totalItems);
        inventoryPanel.SetActive(true);
        if (isUnique)
        {
            uniqueAcquiredPanel.SetActive(true);
            Invoke(nameof(Show), 0.5f);
            Invoke(nameof(UniqueAcquired), 0.5f);
            //ShowItemCategory(currentCategory);
        }
    }

    private void UniqueAcquired()
    {
        uniqueAcquiredPanel.SetActive(false);
    }
    // -----------------------------------------------------------------------------------
    // ConfirmSell
    // -----------------------------------------------------------------------------------
    private void ConfirmSell(int amount, int id)
    {
        Player player = Player.localPlayer;
        if (!player) return;
        player.playerNpcShop.Cmd_SellItem(id, amount);
    }

    // -----------------------------------------------------------------------------------
    // MaxBuyAmount
    // -----------------------------------------------------------------------------------
    private int MaxBuyAmount(Player player, ScriptableItem item)
    {
        if (item.maxStack == 1) return 1;

        return Mathf.Min((int)(player.gold / item.buyPrice), item.maxStack);
    }



    private void ScrollToBeginning()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1;
    }


    // -----------------------------------------------------------------------------------
    // ShowAmountSlider
    // -----------------------------------------------------------------------------------
    public void ShowAmountSlider(string text, int defaultValue, int intMaxValue, int index, int totalItems, bool isUnique = false)
    {
        inputAmountMessage.text = buyAmountText;
        inputSliderText.text = defaultValue.ToString();
        inputSliderAmount.value = defaultValue;
        inputSliderAmount.minValue = defaultValue;
        inputSliderAmount.maxValue = intMaxValue;
        inputSliderText.text = inputSliderAmount.value.ToString() + "/" + inputSliderAmount.maxValue.ToString();
        inputAmount.SetActive(true);// buyAmountText, 1, maxBuy, icopy

        inputConfirm.onClick.SetListener(() =>
        {
            if (sellMode)
                ConfirmSell(intMaxValue, index);
            else
                ConfirmBuy((int)inputSliderAmount.value, index, totalItems, isUnique);
            inputAmount.SetActive(false);
        });
    }

    // -----------------------------------------------------------------------------------
    // SliderValueChanged (Use on button events for refresh UI on value changed)
    // -----------------------------------------------------------------------------------
    public void SliderValueChanged()
    {
        inputSliderText.text = inputSliderAmount.value.ToString() + "/" + inputSliderAmount.maxValue.ToString();
    }
    // -----------------------------------------------------------------------------------
}
