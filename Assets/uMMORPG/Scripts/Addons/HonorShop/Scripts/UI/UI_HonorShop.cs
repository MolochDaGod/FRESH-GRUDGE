using UnityEngine;
using UnityEngine.UI;

public partial class UI_HonorShop : MonoBehaviour
{
    public static UI_HonorShop singleton;

    public GameObject panel;
    public Button categorySlotPrefab;
    public Transform categoryContent;
    public ScrollRect scrollRect;
    public UI_HonorShopSlot itemSlotPrefab;
    public Transform itemContent;
    private int currentCategory = 0;
    public Text currencyNameText;
    public Text currencyAmountText;
    public GameObject inventoryPanel;


    public void Awake()
    {
        if (singleton == null) singleton = this;
    }
    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    private void ScrollToBeginning()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1;
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    private void Update() 
    { 

        Player player = Player.localPlayer;
        if (!player) return;

        // use collider point(s) to also work with big entities
        if (player.target != null && player.target is Npc npc && Utils.ClosestDistance(player, player.target) <= player.interactionRange && npc.npcHonnorShop != null && npc.npcHonnorShop.offersShop) {


            long amount = player.playerHonorShop.GetHonorCurrency(npc.npcHonnorShop.itemShopCategories[currentCategory].honorCurrency);
            if (amount == -1) amount = 0;

            // instantiate/destroy enough category slots
            UIUtils.BalancePrefabs(categorySlotPrefab.gameObject, npc.npcHonnorShop.itemShopCategories.Length, categoryContent);

            // -- Categories
            for (int i = 0; i < npc.npcHonnorShop.itemShopCategories.Length; ++i)
            {
                Button button = categoryContent.GetChild(i).GetComponent<Button>();
                button.interactable = i != currentCategory;
                button.GetComponentInChildren<Text>().text = npc.npcHonnorShop.itemShopCategories[i].categoryName;
                int icopy = i; // needed for lambdas, otherwise i is Count
                button.onClick.SetListener(() =>
                {
                    // set new category and then scroll to the top again
                    currentCategory = icopy;
                    ScrollToBeginning();
                });
            }

            // -- Items
            if (npc.npcHonnorShop.itemShopCategories.Length > 0)
            {
                // instantiate/destroy enough item slots for that category
                ScriptableItem[] items = npc.npcHonnorShop.itemShopCategories[currentCategory].items;
                UIUtils.BalancePrefabs(itemSlotPrefab.gameObject, items.Length, itemContent);

                // refresh all items in that category
                for (int i = 0; i < items.Length; ++i)
                {
                    UI_HonorShopSlot slot = itemContent.GetChild(i).GetComponent<UI_HonorShopSlot>();
                    ScriptableItem itemData = items[i];
                    Item itm = new Item(itemData);
#if _iMMOITEMRARITY
                    slot.RatityOutline.color = RarityColor.SetRarityColorResult(itemData);
#endif
                    // refresh item
                    slot.tooltip.text = itm.ToolTip();
                    slot.image.color = Color.white;
                    slot.image.sprite = itemData.image;
                    slot.nameText.text = itemData.name;
                    slot.priceText.text = itm.GetHonorCurrency(npc.npcHonnorShop.itemShopCategories[currentCategory].honorCurrency).ToString();
                    slot.currencyText.text = npc.npcHonnorShop.itemShopCategories[currentCategory].honorCurrency.name;
                    slot.buyButton.interactable = player.isAlive && amount >= itm.GetHonorCurrency(npc.npcHonnorShop.itemShopCategories[currentCategory].honorCurrency);
                    int icopy = i; // needed for lambdas, otherwise i is Count
                    slot.buyButton.onClick.SetListener(() =>
                    {
                        player.playerHonorShop.Cmd_HonorShop(currentCategory, icopy);
                        inventoryPanel.SetActive(true); // better feedback
                    });
                }
            }

            // Currency
            currencyNameText.text = npc.npcHonnorShop.itemShopCategories[currentCategory].honorCurrency.name;
            currencyAmountText.text = amount.ToString();
        }
        else panel.SetActive(false);
    }
    // -----------------------------------------------------------------------------------
}