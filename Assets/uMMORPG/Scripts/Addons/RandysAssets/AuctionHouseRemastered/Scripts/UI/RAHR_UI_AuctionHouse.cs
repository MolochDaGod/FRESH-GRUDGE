using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class RAHR_UI_AuctionHouse : MonoBehaviour {
    public static RAHR_UI_AuctionHouse singleton;
    private Player player;

    [Header("MainPanel")]
    public GameObject main_Panel;
    public Text main_PlayersGold;
    public Transform main_buttonContent;
    public Sprite main_SpriteTab;
    public Sprite main_SpriteTabSelected;
    public Button main_ButtonBrowse;
    public Button main_ButtonBids;
    public Button main_ButtonAuctions;
    public Button main_ButtonClaims;
    public Color uICategorySelectionColor;
    public Color uISelectionColor;

    [Header("Popup")]
    public GameObject popup_Panel;
    public Text popup_Text;

    [Header("BrowsePanel")]
    public GameObject browse_Panel;

    [Header("BrowsePanel - Search")]
    public InputField browse_InputFieldSearch;
    public InputField browse_InputFieldMinLevelRange;
    public long browse_MinMinLevelRange = 1L;
    public InputField browse_InputFieldMaxLevelRange;
    public long browse_MaxMaxLevelRange = 999L;
    public Button browse_ButtonSearch;
    public Button browse_ButtonReset;

    [Header("BrowsePanel - Categories")]
    public Transform browse_CategoriesContent;
    public RAHR_UI_BrowseCategoriesContentItem browse_CategoriesContentPrefab;
    private string browse_SelectedCategory = "";
    private string browse_SelectedSubCategory = "";

    [Header("BrowsePanel - BrowseItems")]
    public Button browse_ButtonPrev;
    public Button browse_ButtonNext;
    public Transform browse_ItemsContent;
    public RAHR_UI_BrowseItemsContentItem browse_ItemsContentPrefab;
    public Scrollbar browse_ScrollBar;
    private int browse_StartIndex = 0;
    private int browse_SelectedIndex = -1;
    private RAHR_Columns browse_SelectedColumn = RAHR_Columns.buyout;
    private bool browse_SortAsc = true;
    public Button browse_ColumnItemName;
    public Button browse_ColumnLevel;
    public Button browse_ColumnTimeLeft;
    public Button browse_ColumnSeller;
    public Button browse_ColumnBidder;
    public Button browse_ColumnBid;
    public Button browse_ColumnBuyout;

    [Header("BrowsePanel - BiddingAndBuyout")]
    public InputField browse_InputFieldBid;
    public Button browse_ButtonBid;
    public Button browse_ButtonBuyout;

    [Header("BidsPanel")]
    public GameObject bids_Panel;
    public InputField bids_InputFieldBid;
    public Button bids_ButtonBid;
    public Button bids_ButtonBuyout;

    [Header("BidsPanel - Items")]
    public Transform bidsItemsContent;
    public RAHR_UI_BidsContentItem bidsItemsContentPrefab;
    private int bids_SelectedIndex = -1;
    public Button bids_ButtonPrev;
    public Button bids_ButtonNext;
    public Scrollbar bids_ScrollBar;
    private int bids_StartIndex = 0;
    private RAHR_Columns bids_SelectedColumn = RAHR_Columns.buyout;
    private bool bids_SortAsc = true;
    public Button bids_ColumnItemName;
    public Button bids_ColumnLevel;
    public Button bids_ColumnTimeLeft;
    public Button bids_ColumnSeller;
    public Button bids_ColumnBidder;
    public Button bids_ColumnBid;
    public Button bids_ColumnBuyout;

    [Header("AuctionsPanel")]
    public GameObject auctions_Panel;

    [Header("AuctionsPanel - Create Auction")]
    public RAHR_UI_AuctionHouseSlot auctions_UIAuctionHouseSlot;
    public Text auctions_TextItemName;
    public InputField auctions_InputFieldStartingPrice;
    public InputField auctions_InputFieldBuyoutPrice;
    public RAHR_ExtensionsToggleGroup auctions_ToggleGroupDuration;
    public Text auctions_TextDeposit;
    public Button auctions_ButtonCreateAuction;
    private int auctions_durationFeeIndex = 0;
    public Text[] auctions_DurationTexts;

    [Header("AuctionsPanel - Items")]
    public Transform auctions_ItemsContent;
    public RAHR_UI_AuctionsContentItem auctions_ItemsContentPrefab;
    public Button auctions_ButtonCancelAuction;
    private int auctions_SelectedIndex = -1;
    public Button auctions_ButtonPrev;
    public Button auctions_ButtonNext;
    public Scrollbar auctions_ScrollBar;
    private int auctions_StartIndex = 0;
    private RAHR_Columns auctions_SelectedColumn = RAHR_Columns.buyout;
    private bool auctions_SortAsc = true;
    public Button auctions_ColumnItemName;
    public Button auctions_ColumnTimeLeft;
    public Button auctions_ColumnBidder;
    public Button auctions_ColumnBid;
    public Button auctions_ColumnBuyout;

    [Header("ClaimsPanel")]
    public GameObject claims_Panel;

    [Header("ClaimsPanel - Items")]
    public Transform claims_Content;
    public RAHR_UI_ClaimsContentItem claims_ContentItemPrefab;
    public Button claims_ButtonPrev;
    public Button claims_ButtonNext;
    public Scrollbar claims_ScrollBar;
    private int claims_StartIndex = 0;
    private bool claims_SortAsc = true;
    public Button claims_ColumnItemName;

    private List<RAHR_AHCategory> browse_CategoriesList = new List<RAHR_AHCategory>();
    private List<RAHR_AHItem> browse_ItemsList = new List<RAHR_AHItem>();
    private List<RAHR_AHItem> auctions_ItemsList = new List<RAHR_AHItem>();
    private List<RAHR_AHItem> bids_ItemsList = new List<RAHR_AHItem>();
    private List<RAHR_AHItem> claims_ItemsList = new List<RAHR_AHItem>();

    private RAHR_ExtensionsToggle CurrentSelectedDuration {
        get { return auctions_ToggleGroupDuration.ActiveToggles().FirstOrDefault(); }
    }

    private void Start() {
        singleton = this;

        main_Panel.SetActive(false);
        SetActivePanel(browse_Panel);

        UI_SetupButtons();
        UI_SetupInputFields();
        UI_SetupToggles();
        UI_Browse_Populate_Categories();
    }

    private void Update() {
        if(!CheckPlayerExists()) return;

        if(main_Panel.activeSelf) {
            main_PlayersGold.text = player.gold.ToString("#,##0");
        }

        if(browse_Panel.activeSelf) {
            if(browse_SelectedIndex > -1) {
                browse_ButtonBid.interactable = true;
                browse_ButtonBuyout.interactable = true;
                browse_InputFieldBid.interactable = true;
            } else {
                browse_ButtonBid.interactable = false;
                browse_ButtonBuyout.interactable = false;
                browse_InputFieldBid.interactable = false;
            }
        }

        if(bids_Panel.activeSelf) {
            if(bids_SelectedIndex > -1) {
                bids_ButtonBid.interactable = true;
                bids_ButtonBuyout.interactable = true;
                bids_InputFieldBid.interactable = true;
            } else {
                bids_ButtonBid.interactable = false;
                bids_ButtonBuyout.interactable = false;
                bids_InputFieldBid.interactable = false;
            }
        }

        if(auctions_Panel.activeSelf) {
            if(auctions_SelectedIndex > -1) auctions_ButtonCancelAuction.interactable = true;
            else auctions_ButtonCancelAuction.interactable = false;

            if(!string.IsNullOrWhiteSpace(auctions_TextItemName.text)) auctions_ButtonCreateAuction.interactable = true;
            else auctions_ButtonCreateAuction.interactable = false;

            if(0 <= player.auctionHouse_SellIndice && player.auctionHouse_SellIndice < player.inventory.slots.Count && player.inventory.slots[player.auctionHouse_SellIndice].amount > 0) {
                ItemSlot itemSlot = player.inventory.slots[player.auctionHouse_SellIndice];

                auctions_UIAuctionHouseSlot.tooltip.enabled = true;
                auctions_UIAuctionHouseSlot.tooltip.text = itemSlot.ToolTip();
                auctions_UIAuctionHouseSlot.image.color = Color.white;
                auctions_UIAuctionHouseSlot.image.sprite = itemSlot.item.image;
                auctions_UIAuctionHouseSlot.amountOverlay.SetActive(itemSlot.amount > 1);
                auctions_UIAuctionHouseSlot.amountText.text = itemSlot.amount.ToString();

                auctions_TextItemName.text = itemSlot.item.name;

                UpdateDeposit();
            } else {
                player.auctionHouse_SellIndice = -1;

                auctions_UIAuctionHouseSlot.tooltip.enabled = false;
                auctions_UIAuctionHouseSlot.tooltip.text = "";
                auctions_UIAuctionHouseSlot.image.color = Color.clear;
                auctions_UIAuctionHouseSlot.image.sprite = null;
                auctions_UIAuctionHouseSlot.amountOverlay.SetActive(false);
                auctions_UIAuctionHouseSlot.amountText.text = "";

                auctions_TextItemName.text = "";
                auctions_TextDeposit.text = "0";
            }

            for(int cnt = 0; cnt < player.auctionHouse_DurationFee.Length; cnt++) {
                auctions_DurationTexts[cnt].text = player.auctionHouse_DurationFee[cnt].duration + (player.auctionHouse_DurationFee[cnt].duration == 1 ? " Day" : " Days");
            }
        }
    }

    #region SetupUIElements
    private void UI_SetupButtons() {
        main_ButtonBrowse.onClick.SetListener(() => {
            SetActivePanel(browse_Panel);
            UITabSelection(main_buttonContent, main_ButtonBrowse.gameObject);
        });

        main_ButtonBids.onClick.SetListener(() => {
            SetActivePanel(bids_Panel);
            UITabSelection(main_buttonContent, main_ButtonBids.gameObject);

            Request_BidsPanelData();
        });

        main_ButtonAuctions.onClick.SetListener(() => {
            SetActivePanel(auctions_Panel);
            UITabSelection(main_buttonContent, main_ButtonAuctions.gameObject);

            Request_AuctionsPanelData();
        });

        main_ButtonClaims.onClick.SetListener(() => {
            SetActivePanel(claims_Panel);
            UITabSelection(main_buttonContent, main_ButtonClaims.gameObject);

            Request_ClaimsPanelData();
        });

        browse_ButtonSearch.onClick.SetListener(() => {
            browse_StartIndex = 0;
            browse_InputFieldBid.text = "0";

            Request_Browse_Search();
        });

        browse_ButtonReset.onClick.SetListener(() => {
            browse_InputFieldSearch.text = "";
            browse_InputFieldMinLevelRange.text = "1";
            browse_InputFieldMaxLevelRange.text = "999";
            browse_InputFieldBid.text = "0";
            browse_SelectedCategory = "";
            browse_SelectedSubCategory = "";
            browse_StartIndex = 0;

            UI_Browse_Populate_Categories();
            browse_ButtonNext.interactable = false;
            browse_ButtonPrev.interactable = false;
            browse_ItemsList.Clear();
            UI_Browse_Populate_SearchedItems();
        });

        browse_ButtonBid.onClick.SetListener(() => {
            if(browse_SelectedIndex == -1) {
                ShowPopup("You have not selected an item.");
                return;
            }

            Bid(browse_ItemsList[browse_SelectedIndex].id, browse_InputFieldBid.text.ToLong());
            browse_InputFieldBid.text = "0";
        });

        browse_ButtonBuyout.onClick.SetListener(() => {
            if(browse_SelectedIndex == -1) {
                ShowPopup("You have not selected an item.");
                return;
            }

            Buyout(browse_ItemsList[browse_SelectedIndex].id);
            browse_InputFieldBid.text = "0";
        });

        browse_ButtonPrev.onClick.SetListener(() => {
            browse_StartIndex -= player.auctionHouse_ItemsPerPage;
            if(browse_StartIndex < 0) browse_StartIndex = 0;

            Request_Browse_Search();
        });

        browse_ButtonNext.onClick.SetListener(() => {
            browse_StartIndex += player.auctionHouse_ItemsPerPage;

            Request_Browse_Search();
        });

        browse_ColumnItemName.onClick.SetListener(() => {
            if(browse_SelectedColumn == RAHR_Columns.itemName)
                browse_SortAsc = !browse_SortAsc;

            browse_SelectedColumn = RAHR_Columns.itemName;

            Request_Browse_Search();
        });

        browse_ColumnLevel.onClick.SetListener(() => {
            if(browse_SelectedColumn == RAHR_Columns.level)
                browse_SortAsc = !browse_SortAsc;

            browse_SelectedColumn = RAHR_Columns.level;

            Request_Browse_Search();
        });

        browse_ColumnTimeLeft.onClick.SetListener(() => {
            if(browse_SelectedColumn == RAHR_Columns.ends)
                browse_SortAsc = !browse_SortAsc;

            browse_SelectedColumn = RAHR_Columns.ends;

            Request_Browse_Search();
        });

        browse_ColumnSeller.onClick.SetListener(() => {
            if(browse_SelectedColumn == RAHR_Columns.seller)
                browse_SortAsc = !browse_SortAsc;

            browse_SelectedColumn = RAHR_Columns.seller;

            Request_Browse_Search();
        });

        browse_ColumnBidder.onClick.SetListener(() => {
            if(browse_SelectedColumn == RAHR_Columns.bidder)
                browse_SortAsc = !browse_SortAsc;

            browse_SelectedColumn = RAHR_Columns.bidder;

            Request_Browse_Search();
        });

        browse_ColumnBid.onClick.SetListener(() => {
            if(browse_SelectedColumn == RAHR_Columns.currentBid)
                browse_SortAsc = !browse_SortAsc;

            browse_SelectedColumn = RAHR_Columns.currentBid;

            Request_Browse_Search();
        });

        browse_ColumnBuyout.onClick.SetListener(() => {
            if(browse_SelectedColumn == RAHR_Columns.buyout)
                browse_SortAsc = !browse_SortAsc;

            browse_SelectedColumn = RAHR_Columns.buyout;

            Request_Browse_Search();
        });

        bids_ButtonBid.onClick.SetListener(() => {
            if(bids_SelectedIndex == -1) {
                ShowPopup("You have not selected an item.");
                return;
            }

            Bid(bids_ItemsList[bids_SelectedIndex].id, bids_InputFieldBid.text.ToLong());
            bids_InputFieldBid.text = "0";
        });

        bids_ButtonBuyout.onClick.SetListener(() => {
            if(bids_SelectedIndex == -1) {
                ShowPopup("You have not selected an item.");
                return;
            }

            Buyout(bids_ItemsList[bids_SelectedIndex].id);
            bids_InputFieldBid.text = "0";
        });

        bids_ButtonPrev.onClick.SetListener(() => {
            bids_StartIndex -= player.auctionHouse_ItemsPerPage;
            if(bids_StartIndex < 0) bids_StartIndex = 0;

            Request_BidsPanelData();
        });

        bids_ButtonNext.onClick.SetListener(() => {
            bids_StartIndex += player.auctionHouse_ItemsPerPage;

            Request_BidsPanelData();
        });

        bids_ColumnItemName.onClick.SetListener(() => {
            if(bids_SelectedColumn == RAHR_Columns.itemName)
                bids_SortAsc = !bids_SortAsc;

            bids_SelectedColumn = RAHR_Columns.itemName;

            Request_BidsPanelData();
        });

        bids_ColumnLevel.onClick.SetListener(() => {
            if(bids_SelectedColumn == RAHR_Columns.level)
                bids_SortAsc = !bids_SortAsc;

            bids_SelectedColumn = RAHR_Columns.level;

            Request_BidsPanelData();
        });

        bids_ColumnTimeLeft.onClick.SetListener(() => {
            if(bids_SelectedColumn == RAHR_Columns.ends)
                bids_SortAsc = !bids_SortAsc;

            bids_SelectedColumn = RAHR_Columns.ends;

            Request_BidsPanelData();
        });

        bids_ColumnSeller.onClick.SetListener(() => {
            if(bids_SelectedColumn == RAHR_Columns.seller)
                bids_SortAsc = !bids_SortAsc;

            bids_SelectedColumn = RAHR_Columns.seller;

            Request_BidsPanelData();
        });

        bids_ColumnBidder.onClick.SetListener(() => {
            if(bids_SelectedColumn == RAHR_Columns.bidder)
                bids_SortAsc = !bids_SortAsc;

            bids_SelectedColumn = RAHR_Columns.bidder;

            Request_BidsPanelData();
        });

        bids_ColumnBid.onClick.SetListener(() => {
            if(bids_SelectedColumn == RAHR_Columns.currentBid)
                bids_SortAsc = !bids_SortAsc;

            bids_SelectedColumn = RAHR_Columns.currentBid;

            Request_BidsPanelData();
        });

        bids_ColumnBuyout.onClick.SetListener(() => {
            if(bids_SelectedColumn == RAHR_Columns.buyout)
                bids_SortAsc = !bids_SortAsc;

            bids_SelectedColumn = RAHR_Columns.buyout;

            Request_BidsPanelData();
        });

        auctions_ButtonCreateAuction.onClick.SetListener(() => {
            Auction_Create();
        });

        auctions_ButtonCancelAuction.onClick.SetListener(() => {
            if(auctions_SelectedIndex == -1) {
                ShowPopup("You have not selected an item.");
                return;
            }

            Auctions_Cancel();
        });

        auctions_UIAuctionHouseSlot.button.onClick.SetListener(() => {
            player.auctionHouse_SellIndice = -1;
        });

        auctions_ButtonPrev.onClick.SetListener(() => {
            auctions_StartIndex -= player.auctionHouse_ItemsPerPage;
            if(auctions_StartIndex < 0) auctions_StartIndex = 0;

            Request_AuctionsPanelData();
        });

        auctions_ButtonNext.onClick.SetListener(() => {
            auctions_StartIndex += player.auctionHouse_ItemsPerPage;

            Request_AuctionsPanelData();
        });

        auctions_ColumnItemName.onClick.SetListener(() => {
            if(auctions_SelectedColumn == RAHR_Columns.itemName)
                auctions_SortAsc = !auctions_SortAsc;

            auctions_SelectedColumn = RAHR_Columns.itemName;

            Request_AuctionsPanelData();
        });

        auctions_ColumnTimeLeft.onClick.SetListener(() => {
            if(auctions_SelectedColumn == RAHR_Columns.ends)
                auctions_SortAsc = !auctions_SortAsc;

            auctions_SelectedColumn = RAHR_Columns.ends;

            Request_AuctionsPanelData();
        });

        auctions_ColumnBidder.onClick.SetListener(() => {
            if(auctions_SelectedColumn == RAHR_Columns.bidder)
                auctions_SortAsc = !auctions_SortAsc;

            auctions_SelectedColumn = RAHR_Columns.bidder;

            Request_AuctionsPanelData();
        });

        auctions_ColumnBid.onClick.SetListener(() => {
            if(auctions_SelectedColumn == RAHR_Columns.currentBid)
                auctions_SortAsc = !auctions_SortAsc;

            auctions_SelectedColumn = RAHR_Columns.currentBid;

            Request_AuctionsPanelData();
        });

        auctions_ColumnBuyout.onClick.SetListener(() => {
            if(auctions_SelectedColumn == RAHR_Columns.buyout)
                auctions_SortAsc = !auctions_SortAsc;

            auctions_SelectedColumn = RAHR_Columns.buyout;

            Request_AuctionsPanelData();
        });

        claims_ButtonPrev.onClick.SetListener(() => {
            claims_StartIndex -= player.auctionHouse_ItemsPerPage;
            if(claims_StartIndex < 0) claims_StartIndex = 0;

            Request_ClaimsPanelData();
        });

        claims_ButtonNext.onClick.SetListener(() => {
            claims_StartIndex += player.auctionHouse_ItemsPerPage;

            Request_ClaimsPanelData();
        });

        claims_ColumnItemName.onClick.SetListener(() => {
            claims_SortAsc = !claims_SortAsc;

            Request_ClaimsPanelData();
        });
    }

    private void UI_SetupInputFields() {
        browse_InputFieldSearch.onEndEdit.SetListener(val => {
            if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) {
                browse_ButtonSearch.onClick.Invoke();
            }
        });

        browse_InputFieldMinLevelRange.onValueChanged.SetListener(val => {
            browse_InputFieldMinLevelRange.text = Utils.Clamp(val.ToLong(), browse_MinMinLevelRange, browse_MaxMaxLevelRange).ToString();
        });

        browse_InputFieldMaxLevelRange.onValueChanged.SetListener(val => {
            browse_InputFieldMaxLevelRange.text = Utils.Clamp(val.ToLong(), browse_MinMinLevelRange, browse_MaxMaxLevelRange).ToString();
        });
    }

    public void UI_SetupToggles() {
        auctions_ToggleGroupDuration.onToggleGroupToggleChanged.SetListener(val => {
            auctions_durationFeeIndex = auctions_ToggleGroupDuration.ActiveToggles().FirstOrDefault().gameObject.GetComponent<RAHR_UI_ToggleDuration>().durationFeeIndex;
        });
    }
    #endregion

    #region Client > Server Methods
    private void Bid(long id, long bid) {
        if(!CheckPlayerExists()) return;

        player.Cmd_AuctionHouse_Bid(id, bid);
    }

    private void Buyout(long id) {
        if(!CheckPlayerExists()) return;

        player.Cmd_AuctionHouse_Buyout(id);
    }

    private void Auction_Create() {
        if(!CheckPlayerExists()) return;

        player.Cmd_AuctionHouse_PostItem(player.auctionHouse_SellIndice, auctions_InputFieldStartingPrice.text.ToLong(), auctions_InputFieldBuyoutPrice.text.ToLong(), auctions_durationFeeIndex);
        auctions_InputFieldStartingPrice.text = "0";
        auctions_InputFieldBuyoutPrice.text = "0";
    }

    private void Auctions_Cancel() {
        if(!CheckPlayerExists()) return;

        player.Cmd_AuctionHouse_CancelAuction(auctions_ItemsList[auctions_SelectedIndex].id);
    }

    private void Claims_Claim(long id) {
        if(!CheckPlayerExists()) return;

        player.Cmd_AuctionHouse_Claim(id);
    }

    private void Request_Browse_Search() {
        if(!CheckPlayerExists()) return;

        string cat = !string.IsNullOrWhiteSpace(browse_SelectedCategory) ? RAHR_ExtensionMethods.GetValueFromDescription<RAHR_Category>(browse_SelectedCategory).ToString() : "";
        string subCat = !string.IsNullOrWhiteSpace(browse_SelectedSubCategory) ? RAHR_ExtensionMethods.GetValueFromDescription<RAHR_SubCategory>(browse_SelectedSubCategory).ToString() : "";

        player.Cmd_AuctionHouse_BrowseRequestSearch(cat, subCat, browse_InputFieldSearch.text, browse_InputFieldMinLevelRange.text.ToLong(), browse_InputFieldMaxLevelRange.text.ToLong(), browse_StartIndex, browse_SelectedColumn, browse_SortAsc);
    }

    private void Request_BidsPanelData() {
        if(!CheckPlayerExists()) return;

        player.Cmd_AuctionHouse_RequestMyBids(bids_StartIndex, bids_SelectedColumn, bids_SortAsc);
    }

    private void Request_AuctionsPanelData() {
        if(!CheckPlayerExists()) return;

        player.Cmd_AuctionHouse_RequestBySeller(auctions_StartIndex, auctions_SelectedColumn, auctions_SortAsc);
    }

    private void Request_ClaimsPanelData() {
        if(!CheckPlayerExists()) return;

        player.Cmd_AuctionHouse_GetClaims(claims_StartIndex, claims_SortAsc);
    }
    #endregion

    #region Misc Methods
    private bool CheckPlayerExists() {
        if(!player) player = Player.localPlayer;
        return player;
    }

    public void SetActivePanel(GameObject panel) {
        browse_Panel.SetActive(false);
        bids_Panel.SetActive(false);
        auctions_Panel.SetActive(false);
        claims_Panel.SetActive(false);

        panel.SetActive(true);
    }

    private void ClearTransformChildren(Transform t) {
        if(t.childCount > 0) {
            for(int i = t.childCount - 1; i >= 0; i--) {
                Destroy(t.GetChild(i).gameObject);
            }
        }

        t.DetachChildren();
    }

    private void UIElementSelection(Transform content, GameObject toSelect, Color color, bool isCategory = false, bool isSubCategory = false) {
        UIElementDeselectAll(content, isCategory, isSubCategory);
        
        if(!isCategory && !isSubCategory) {
            toSelect.GetComponent<Image>().color = color;
        } else if(isCategory && !isSubCategory) {
            toSelect.GetComponent<RAHR_UI_BrowseCategoriesContentItem>().buttonCategory.gameObject.GetComponent<Image>().color = color;
        } else if(!isCategory && isSubCategory) {
            toSelect.GetComponent<RAHR_UI_BrowseCategoriesContentItem>().buttonSubCategory.gameObject.GetComponent<Image>().color = color;
        }
    }

    private void UIElementDeselectAll(Transform content, bool isCategory = false, bool isSubCategory = false) {
        for(int i = 0; i <= content.childCount - 1; i++) {
            if(!isCategory && !isSubCategory) {
                content.GetChild(i).GetComponent<Image>().color = Color.white;
            } else {
                content.GetChild(i).GetComponent<RAHR_UI_BrowseCategoriesContentItem>().buttonCategory.gameObject.GetComponent<Image>().color = Color.white;
                content.GetChild(i).GetComponent<RAHR_UI_BrowseCategoriesContentItem>().buttonSubCategory.gameObject.GetComponent<Image>().color = Color.white;
            }
        }
    }

    private void UITabSelection(Transform content, GameObject toSelect) {
        for(int i = 0; i <= content.childCount - 1; i++) {
            content.GetChild(i).GetComponent<Image>().sprite = main_SpriteTab;
        }

        toSelect.GetComponent<Image>().sprite = main_SpriteTabSelected;
    }

    private void ButtonPrevNextValidation(int listCount, int startIndex, int itemsPerPage, Button buttonPrev, Button buttonNext) {
        if(startIndex < 0) startIndex = 0;

        if(listCount < itemsPerPage) buttonNext.interactable = false;
        else buttonNext.interactable = true;

        if(startIndex == 0) buttonPrev.interactable = false;
        else buttonPrev.interactable = true;
    }

    public void Show() {
        main_Panel.SetActive(true);
    }

    public void Hide() {
        main_Panel.SetActive(false);
    }

    public void ClearAllResults() {
        browse_ItemsList.Clear();
        auctions_ItemsList.Clear();
        bids_ItemsList.Clear();
        claims_ItemsList.Clear();

        for(int i = browse_ItemsContent.childCount - 1; i >= 0; i--) {
            Destroy(browse_ItemsContent.GetChild(i).gameObject);
        }

        for(int i = bidsItemsContent.childCount - 1; i >= 0; i--) {
            Destroy(bidsItemsContent.GetChild(i).gameObject);
        }

        for(int i = auctions_ItemsContent.childCount - 1; i >= 0; i--) {
            Destroy(auctions_ItemsContent.GetChild(i).gameObject);
        }

        for(int i = claims_Content.childCount - 1; i >= 0; i--) {
            Destroy(claims_Content.GetChild(i).gameObject);
        }
    }
    #endregion

    #region Server > Client Methods
    public void Requested_Browse_SearchedItems(string json) {
        browse_ItemsList = new List<RAHR_AHItem>(RAHR_JsonHelper.FromJson<RAHR_AHItem>(json));
        UI_Browse_Populate_SearchedItems();
    }

    public void Requested_Bids_BuyersActiveBids(string json) {
        bids_ItemsList = new List<RAHR_AHItem>(RAHR_JsonHelper.FromJson<RAHR_AHItem>(json));
        UI_Bids_Populate_ActiveBids();
    }

    public void Requested_Auctions_SellersActiveAuctions(string json) {
        auctions_ItemsList = new List<RAHR_AHItem>(RAHR_JsonHelper.FromJson<RAHR_AHItem>(json));
        UI_Auctions_Populate_ActiveAuctions();
    }

    public void Requested_Claims(string json) {
        claims_ItemsList = new List<RAHR_AHItem>(RAHR_JsonHelper.FromJson<RAHR_AHItem>(json));
        UI_Claims_Populate();
    }

    public void ReturnedMessage(RAHR_AuctionReturnStatus returnedMessage) {
        switch(returnedMessage) {
            case RAHR_AuctionReturnStatus.ItemDoesntExist:
                ShowPopup("The item you have selected no longer exists.");
                if(browse_Panel.activeSelf) {
                    Request_Browse_Search();
                } else if(bids_Panel.activeSelf) {
                    Request_BidsPanelData();
                }
                break;
            case RAHR_AuctionReturnStatus.ContainsBidders:
                ShowPopup("You cannot cancel an auction that has active bids.");
                break;
            case RAHR_AuctionReturnStatus.AuctionCanceled:
                ShowPopup("You have successfully canceled the auction.");
                Request_AuctionsPanelData();
                break;
            case RAHR_AuctionReturnStatus.AuctionCreated:
                ShowPopup("Auction successfully created.");
                Request_AuctionsPanelData();
                break;
            case RAHR_AuctionReturnStatus.BidTooLow:
                ShowPopup("Error: Your bid was too low.");
                break;
            case RAHR_AuctionReturnStatus.StartingPricesInvalid:
                ShowPopup("Error: Invalid Starting/Buyout prices.");
                break;
            case RAHR_AuctionReturnStatus.NotEnoughGold:
                ShowPopup("Error: You do not have enough gold.");
                break;
            case RAHR_AuctionReturnStatus.InventoryFull:
                ShowPopup("Error: Your inventory is full.");
                break;
            case RAHR_AuctionReturnStatus.CannotBidYourAuction:
                ShowPopup("Error: You cannot bid on your own auction.");
                break;
            case RAHR_AuctionReturnStatus.CannoyBuyYourAuction:
                ShowPopup("Error: You cannot buy your own auction.");
                break;
            case RAHR_AuctionReturnStatus.BidSuccess:
                ShowPopup("Bid was successfull.");
                if(browse_Panel.activeSelf) {
                    Request_Browse_Search();
                } else if(bids_Panel.activeSelf) {
                    Request_BidsPanelData();
                }
                break;
            case RAHR_AuctionReturnStatus.BuyoutSuccess:
                ShowPopup("Buyout was successfull.");
                if(browse_Panel.activeSelf) {
                    Request_Browse_Search();
                } else if(bids_Panel.activeSelf) {
                    Request_BidsPanelData();
                }
                break;
            case RAHR_AuctionReturnStatus.ClaimSuccess:
                Request_ClaimsPanelData();
                break;
            case RAHR_AuctionReturnStatus.SellerUpdate:
                if(auctions_Panel.activeSelf) {
                    Request_AuctionsPanelData();
                } else if(claims_Panel.activeSelf) {
                    Request_ClaimsPanelData();
                }
                break;
            case RAHR_AuctionReturnStatus.BidderUpdate:
                if(bids_Panel.activeSelf) {
                    Request_BidsPanelData();
                } else if(claims_Panel.activeSelf) {
                    Request_ClaimsPanelData();
                }
                break;
            case RAHR_AuctionReturnStatus.InventoryItemInvalid:
                ShowPopup("Invalid or no item selected.");
                break;
            case RAHR_AuctionReturnStatus.NotTradable:
                ShowPopup("Item is not tradable.");
                break;
            case RAHR_AuctionReturnStatus.Summoned:
                ShowPopup("Please unsummon the pet or mount before posting.");
                break;
            case RAHR_AuctionReturnStatus.Error:
                ShowPopup("Error: Internal Auction House Error");
                break;
        }
    }

    private void ShowPopup(string message) {
        popup_Panel.SetActive(true);
        popup_Text.text = message;
    }
    #endregion

    #region Populate UI Methods
    private void UI_Browse_Populate_Categories() {
        browse_CategoriesList.Clear();
        ClearTransformChildren(browse_CategoriesContent);

        foreach(RAHR_Category ahCategory in Enum.GetValues(typeof(RAHR_Category))) {
            browse_CategoriesList.Add(new RAHR_AHCategory(ahCategory.GetDescription()));

            if(!string.IsNullOrWhiteSpace(browse_SelectedCategory) && browse_SelectedCategory.ToEnum<RAHR_Category>() == ahCategory) {
                foreach(RAHR_SubCategory subCategory in Enum.GetValues(typeof(RAHR_SubCategory))) {
                    if(subCategory.IsSubcategoryOf(ahCategory)) {
                        browse_CategoriesList.Add(new RAHR_AHCategory(subCategory.GetDescription(), true));
                    }
                }
            }
        }

        UIUtils.BalancePrefabs(browse_CategoriesContentPrefab.gameObject, browse_CategoriesList.Count, browse_CategoriesContent);

        for(int i = 0; i < browse_CategoriesContent.childCount; i++) {
            RAHR_UI_BrowseCategoriesContentItem uiAHCategory = browse_CategoriesContent.GetChild(i).GetComponent<RAHR_UI_BrowseCategoriesContentItem>();

            if(!browse_CategoriesList[i].isSubCategory) {
                uiAHCategory.category.SetActive(true);
                uiAHCategory.textCategory.text = browse_CategoriesList[i].category;

                uiAHCategory.buttonCategory.onClick.SetListener(() => {
                browse_SelectedCategory = RAHR_ExtensionMethods.GetValueFromDescription<RAHR_Category>(uiAHCategory.textCategory.text).ToString();
                    if(!CheckPlayerExists()) return;

                    browse_SelectedSubCategory = "";
                    browse_StartIndex = 0;
                    UI_Browse_Populate_Categories();

                    Request_Browse_Search();
                    browse_InputFieldBid.text = "0";
                });

                if(uiAHCategory.textCategory.text.Equals(browse_SelectedCategory)) {
                    UIElementSelection(browse_CategoriesContent, uiAHCategory.gameObject, uICategorySelectionColor, true, false);
                }
            } else {
                uiAHCategory.subCategory.SetActive(true);
                uiAHCategory.textSubCategory.text = browse_CategoriesList[i].category;

                uiAHCategory.buttonSubCategory.onClick.SetListener(() => {
                    if(!CheckPlayerExists()) return;

                    browse_SelectedCategory = "";
                    browse_StartIndex = 0;
                    browse_SelectedSubCategory = uiAHCategory.textSubCategory.text;

                    Request_Browse_Search();
                    browse_InputFieldBid.text = "0";

                    UIElementSelection(browse_CategoriesContent, uiAHCategory.gameObject, uICategorySelectionColor, false, true);
                });

                if(uiAHCategory.textSubCategory.text.Equals(browse_SelectedSubCategory)) {
                    UIElementSelection(browse_CategoriesContent, uiAHCategory.gameObject, uICategorySelectionColor, false, true);
                }
            }
        }
    }

    private void UI_Browse_Populate_SearchedItems() {
        ButtonPrevNextValidation(browse_ItemsList.Count, browse_StartIndex, player.auctionHouse_ItemsPerPage, browse_ButtonPrev, browse_ButtonNext);

        browse_SelectedIndex = -1;
        ClearTransformChildren(browse_ItemsContent);
        browse_ScrollBar.value = 1;

        UIUtils.BalancePrefabs(browse_ItemsContentPrefab.gameObject, browse_ItemsList.Count, browse_ItemsContent);

        for(int i = 0; i < browse_ItemsContent.childCount; i++) {
            RAHR_UI_BrowseItemsContentItem uiAHItem = browse_ItemsContent.GetChild(i).GetComponent<RAHR_UI_BrowseItemsContentItem>();
            RAHR_AHItem ahItem = browse_ItemsList[i];
            ScriptableItem sItem;
            ScriptableItem.All.TryGetValue(ahItem.itemName.GetStableHashCode(), out sItem);
            Item item = new Item(sItem);
            item.summonedHealth = ahItem.summonedHealth.ToInt32();
            item.summonedLevel = ahItem.summonedLevel.ToInt32();
            item.summonedExperience = ahItem.summonedExperience.ToInt32();


            uiAHItem.image.sprite = item.image;
            uiAHItem.tooltip.text = item.ToolTip();
            uiAHItem.overlay.SetActive(ahItem.stackSize > 1);
            uiAHItem.overlayAmount.text = ahItem.stackSize.ToString();
            uiAHItem.itemName.text = ahItem.itemName;
            uiAHItem.level.text = ((UsableItem)sItem).minLevel.ToString();
            uiAHItem.timeLeft.text = RAHR_Utils.TimeRemaining(ahItem.ends);
            uiAHItem.seller.text = ahItem.seller;
            uiAHItem.bid.text = ahItem.currentBid.ToString("#,##0");
            uiAHItem.buyout.text = ahItem.buyout.ToString("#,##0");
            uiAHItem.bidder.text = ahItem.bidder;

            int index = i;
            uiAHItem.button.onClick.SetListener(() => {
                browse_SelectedIndex = index;
                browse_InputFieldBid.text = (ahItem.currentBid + 1).ToString();
                UIElementSelection(browse_ItemsContent, uiAHItem.gameObject, uISelectionColor);
            });
        }
    }

    private void UI_Bids_Populate_ActiveBids() {
        ButtonPrevNextValidation(bids_ItemsList.Count, bids_StartIndex, player.auctionHouse_ItemsPerPage, bids_ButtonPrev, bids_ButtonNext);

        bids_SelectedIndex = -1;
        ClearTransformChildren(bidsItemsContent);
        bids_ScrollBar.value = 1;

        UIUtils.BalancePrefabs(bidsItemsContentPrefab.gameObject, bids_ItemsList.Count, bidsItemsContent);

        for(int i = 0; i < bidsItemsContent.childCount; i++) {
            RAHR_UI_BidsContentItem uiAHItem = bidsItemsContent.GetChild(i).GetComponent<RAHR_UI_BidsContentItem>();
            RAHR_AHItem ahItem = bids_ItemsList[i];
            ScriptableItem sItem;
            ScriptableItem.All.TryGetValue(ahItem.itemName.GetStableHashCode(), out sItem);
            Item item = new Item(sItem);
            item.summonedHealth = ahItem.summonedHealth.ToInt32();
            item.summonedLevel = ahItem.summonedLevel.ToInt32();
            item.summonedExperience = ahItem.summonedExperience.ToInt32();

            uiAHItem.image.sprite = item.image;
            uiAHItem.tooltip.text = item.ToolTip();
            uiAHItem.overlay.SetActive(ahItem.stackSize > 1);
            uiAHItem.overlayAmount.text = ahItem.stackSize.ToString();
            uiAHItem.itemName.text = ahItem.itemName;
            uiAHItem.level.text = ahItem.level.ToString();
            uiAHItem.timeLeft.text = RAHR_Utils.TimeRemaining(ahItem.ends);
            uiAHItem.bidder.text = ahItem.bidder;
            uiAHItem.seller.text = ahItem.seller;
            uiAHItem.bid.text = ahItem.currentBid.ToString("#,##0");
            uiAHItem.buyout.text = ahItem.buyout.ToString("#,##0");

            int index = i;
            uiAHItem.button.onClick.SetListener(() => {
                bids_SelectedIndex = index;
                bids_InputFieldBid.text = (ahItem.currentBid + 1).ToString();
                UIElementSelection(bidsItemsContent, uiAHItem.gameObject, uISelectionColor);
            });
        }
    }

    private void UI_Auctions_Populate_ActiveAuctions() {
        ButtonPrevNextValidation(auctions_ItemsList.Count, auctions_StartIndex, player.auctionHouse_ItemsPerPage, auctions_ButtonPrev, auctions_ButtonNext);

        auctions_SelectedIndex = -1;
        ClearTransformChildren(auctions_ItemsContent);
        auctions_ScrollBar.value = 1;

        UIUtils.BalancePrefabs(auctions_ItemsContentPrefab.gameObject, auctions_ItemsList.Count, auctions_ItemsContent);

        for(int i = 0; i < auctions_ItemsContent.childCount; i++) {
            RAHR_UI_AuctionsContentItem uiAHItem = auctions_ItemsContent.GetChild(i).GetComponent<RAHR_UI_AuctionsContentItem>();
            RAHR_AHItem ahItem = auctions_ItemsList[i];
            ScriptableItem sItem;
            ScriptableItem.All.TryGetValue(ahItem.itemName.GetStableHashCode(), out sItem);
            Item item = new Item(sItem);
            item.summonedHealth = ahItem.summonedHealth.ToInt32();
            item.summonedLevel = ahItem.summonedLevel.ToInt32();
            item.summonedExperience = ahItem.summonedExperience.ToInt32();

            uiAHItem.image.sprite = item.image;
            uiAHItem.tooltip.text = item.ToolTip();
            uiAHItem.overlay.SetActive(ahItem.stackSize > 1);
            uiAHItem.overlayAmount.text = ahItem.stackSize.ToString();
            uiAHItem.itemName.text = ahItem.itemName;
            uiAHItem.timeLeft.text = RAHR_Utils.TimeRemaining(ahItem.ends);
            uiAHItem.bidder.text = ahItem.bidder;
            uiAHItem.bid.text = ahItem.currentBid.ToString("#,##0");
            uiAHItem.buyout.text = ahItem.buyout.ToString("#,##0");

            int index = i;
            uiAHItem.button.onClick.SetListener(() => {
                auctions_SelectedIndex = index;

                UIElementSelection(auctions_ItemsContent, uiAHItem.gameObject, uISelectionColor);
            });
        }
    }

    private void UI_Claims_Populate() {
        ButtonPrevNextValidation(claims_ItemsList.Count, claims_StartIndex, player.auctionHouse_ItemsPerPage, claims_ButtonPrev, claims_ButtonNext);

        ClearTransformChildren(claims_Content);
        claims_ScrollBar.value = 1;

        UIUtils.BalancePrefabs(claims_ContentItemPrefab.gameObject, claims_ItemsList.Count, claims_Content);

        for(int i = 0; i < claims_Content.childCount; i++) {
            RAHR_UI_ClaimsContentItem uiAHItem = claims_Content.GetChild(i).GetComponent<RAHR_UI_ClaimsContentItem>();
            RAHR_AHItem ahItem = claims_ItemsList[i];
            ScriptableItem sItem;
            ScriptableItem.All.TryGetValue(ahItem.itemName.GetStableHashCode(), out sItem);
            Item item = new Item(sItem);
            item.summonedHealth = ahItem.summonedHealth.ToInt32();
            item.summonedLevel = ahItem.summonedLevel.ToInt32();
            item.summonedExperience = ahItem.summonedExperience.ToInt32();

            uiAHItem.image.sprite = item.image;
            uiAHItem.tooltip.text = item.ToolTip();
            uiAHItem.amountOverlay.SetActive(ahItem.stackSize > 1);
            uiAHItem.amountText.text = ahItem.stackSize.ToString();
            uiAHItem.itemName.text = ahItem.itemName;

            if(player.name.Equals(ahItem.seller)) {
                uiAHItem.sellerBuyer.text = ahItem.bidder;

                uiAHItem.goldDetailsSeller.SetActive(true);
                uiAHItem.finalValue.text = ahItem.finalValue.ToString("#,##0");

                if(ahItem.finalValue == 0) {
                    uiAHItem.willReveive.text = "0";
                    uiAHItem.textButtonCollect.text = "Collect Item";
                } else {
                    long finalValueFee = (ahItem.finalValue * player.auctionHouse_FinalValueFee).ToLong();
                    long willReceive = (ahItem.finalValue - finalValueFee) + ahItem.insertFee;

                    uiAHItem.willReveive.text = willReceive.ToString("#,##0");
                    uiAHItem.textButtonCollect.text = "Collect Gold";
                }
            }

            if(player.name.Equals(ahItem.bidder)) {
                uiAHItem.sellerBuyer.text = ahItem.seller;

                uiAHItem.goldDetailsBuyer.SetActive(true);

                if(ahItem.outbid == 1) {
                    uiAHItem.textFVBV.text = "Outbid Value:";
                    uiAHItem.textButtonCollect.text = "Collect Gold";
                } else {
                    uiAHItem.textButtonCollect.text = "Collect Item";
                }

                uiAHItem.textFVBV_Value.text = ahItem.finalValue.ToString("#,##0");

            }

            uiAHItem.buttonCollect.onClick.SetListener(() => {
                Claims_Claim(ahItem.id);
            });
        }
    }

    public void UpdateDeposit() {
        if(player.auctionHouse_SellIndice != -1) {
            ItemSlot itemSlot = player.inventory.slots[player.auctionHouse_SellIndice];
            long deposit = itemSlot.item.sellPrice * itemSlot.amount;
            deposit = (deposit * player.auctionHouse_DurationFee[auctions_durationFeeIndex].insertFee).ToLong();
            auctions_TextDeposit.text = deposit.ToString("#,##0");
        }
    }
    #endregion
}