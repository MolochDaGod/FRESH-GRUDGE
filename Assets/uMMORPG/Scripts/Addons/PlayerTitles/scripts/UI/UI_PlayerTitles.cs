using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public partial class UI_PlayerTitles : MonoBehaviour
{
    public GameObject panel;
    public Button categorySlotPrefab;
    public Transform categoryContent;
    public ScrollRect scrollRect;
    public UI_PlayerTitleSlot itemSlotPrefab;
    public Transform itemContent;

    public string emptyCategoryLabel = "Empty category";

    [HideInInspector] public string currentCategory = "";


#pragma warning disable
    private int chosenAmount = 0;
#pragma warning restore

    private void ScrollToBeginning()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1;
    }

    // -----------------------------------------------------------------------------------
    // ClearContent
    // -----------------------------------------------------------------------------------
    private void ClearContent()
    {
        for (int i = 0; i < itemContent.childCount; ++i)
        {
            Destroy(itemContent.GetChild(i).gameObject);
        }
    }

    private void OnEnable()
    {
        UpdateEvent();
    }

    public void UpdateEvent()
    {
        Player player = Player.localPlayer;

        if (player != null)
        {
            if (player.playerTitles.titlesConfiguration.titles != null)
            {

                // --------------------------------------------------------------------------- Refresh Sale Items
                currentCategory = (currentCategory == "") ? player.playerTitles.titlesConfiguration.category[0].name : currentCategory;
                List<Tmpl_Titles> titleList = new List<Tmpl_Titles>();

                titleList.Clear();

                int totalTitle = player.playerTitles.titlesConfiguration.titles.Length - 1;
                for (int i = 0; i <= totalTitle; ++i)
                {
                    if (player.playerTitles.titlesConfiguration.titles[i].category.name == currentCategory)
                        titleList.Add(player.playerTitles.titlesConfiguration.titles[i]);
                }

                // --------------------------------------------------------------------------- Update Content (By category)
                if (player.playerTitles.titlesConfiguration.category.Length > 0 && player.playerTitles.titlesConfiguration.titles.Length > 0)
                {
                    // instantiate/destroy enough item slots for that category
                    Tmpl_Titles[] items = titleList.Where(x => x.category.name == currentCategory || player.playerTitles.titlesConfiguration.category[0]).ToArray();
                    UIUtils.BalancePrefabs(itemSlotPrefab.gameObject, items.Length, itemContent);

                    // refresh all items in that category
                    for (int i = 0; i < items.Length; ++i)
                    {
                        UI_PlayerTitleSlot slot = itemContent.GetChild(i).GetComponent<UI_PlayerTitleSlot>();
                        Tmpl_Titles itemData = items[i];

                        int idx = player.playerTitles.EarnedTitles.FindIndex(x => x.title == itemData.name);
                        Titles f = player.playerTitles.EarnedTitles.FirstOrDefault(x => x.title == itemData.name);
                        //Debug.Log(f + " -> " + idx + " -> " + itemData.name);
                        slot.tooltip.text = itemData.description;// new Item(itemData).ToolTip();
                        slot.image.color = Color.white;
                        slot.image.sprite = itemData.image;
                        slot.nameText.text = itemData.displayName;
                        slot.descriptionText.text = itemData.description; // itemData.buyPrice.ToString();
                        slot.buttonActivateText.text = (player.playerTitles.EarnedTitles.Contains(f)) ? ((itemData.name == player.playerTitles.currentTitle) ? "Disable" : "Activate")  : "Not earned";
                        slot.Activate.interactable = player.playerTitles.EarnedTitles.Contains(f);
                        int icopy = i; // needed for lambdas, otherwise i is Count
                        //int maxBuy = MaxBuyAmount(player, itemData);
                        slot.Activate.onClick.SetListener(() =>
                        {
                            //if (maxBuy == 1)
                            //ConfirmBuy(1, icopy);
                            if (player.playerTitles.EarnedTitles.Contains(f))
                            {
                                if (itemData.name == player.playerTitles.currentTitle)
                                {
                                    ConfirmDisableTitle();
                                }
                                else
                                {
                                    ConfirmActivateTitle(itemData.name);
                                }
                                UpdateEvent();
                            }
                        });
                    }
                }

                // instantiate/destroy enough category slots
                UIUtils.BalancePrefabs(categorySlotPrefab.gameObject, player.playerTitles.titlesConfiguration.category.Length, categoryContent);


                // --------------------------------------------------------------------------- Refresh Category Buttons
                for (int i = 0; i < player.playerTitles.titlesConfiguration.category.Length; ++i)
                {
                    if (!player.playerTitles.titlesConfiguration.category[i].displayed)
                    {
                        categoryContent.GetChild(i).gameObject.SetActive(false);
                    }
                    else
                    {
                        Button button = categoryContent.GetChild(i).GetComponent<Button>();
                        button.interactable = player.playerTitles.titlesConfiguration.category[i].name != currentCategory;

                        if (player.playerTitles.titlesConfiguration.category[i].name == "")
                            button.GetComponentInChildren<Text>().text = emptyCategoryLabel;
                        else
                            button.GetComponentInChildren<Text>().text = player.playerTitles.titlesConfiguration.category[i].titleTypeName;

                        int icopy = i; // needed for lambdas, otherwise i is Count
                        button.onClick.SetListener(() =>
                        {
                            currentCategory = player.playerTitles.titlesConfiguration.category[icopy].name;
                            ScrollToBeginning();
                            UpdateEvent();
                        });
                    }

                }
                // --------------------------------------------------------------------------- Update Overview
            }

        }
        else
        {
            panel.SetActive(false);
        }
    }

    // -----------------------------------------------------------------------------------
    // ConfirmDisableTitle
    // -----------------------------------------------------------------------------------
    private void ConfirmDisableTitle()
    {
        Player player = Player.localPlayer;
        if (!player) return;
        player.playerTitles.CmdDisableTitle();
    }

    // -----------------------------------------------------------------------------------
    // ConfirmActivateTitle
    // -----------------------------------------------------------------------------------
    private void ConfirmActivateTitle(string title)
    {
        Player player = Player.localPlayer;
        if (!player) return;
        player.playerTitles.CmdUseTitle(title);
    }
    // -----------------------------------------------------------------------------------
}
