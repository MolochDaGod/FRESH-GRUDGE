#if _SQLITE
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIItemManager : MonoBehaviour
{
    public static UIItemManager singleton;
    public Canvas canvas;

    public Transform categoryContent;
    public Transform itemsContent;

    public ScrollRect itemsScrollRect;
    public Button categorySlotPrefab;
    public UIItemManagerSlot itemSlotPrefab;
    public TMP_InputField nameItemInput;

    int currentCategory;
    /*
    [Serializable]    
    public struct ItemManagerCategory
    {
        public string category;
        public ScriptableItem[] items;
    }
    public ItemManagerCategory[] itemManagerCategories;
    */
    void Awake()
    {
        // initialize singleton
        if (singleton == null) singleton = this;
    }

    void Start()
    {
        // create category buttons
        ShowCategories();
    }

    // triggered by clicking the items button on the toolbar
    public void PanelActivation()
    {
        UIManagement.singleton.PanelActivation(canvas);
        AddonManagement.ShowCanvasContent(canvas);
    }

    void ShowCategories()
    {
        UIUtils.BalancePrefabs(categorySlotPrefab.gameObject, MToolsSettings.Settings.itemManager.categories.Length, categoryContent);
        
        for (int i = 0; i < MToolsSettings.Settings.itemManager.categories.Length; ++i)
        {
            UIManagementButton button = categoryContent.GetChild(i).GetComponent<UIManagementButton>();

            int icopy = i;
            string buttonName = MToolsSettings.Settings.itemManager.categories[icopy].category;
            button.name = buttonName;
            button.nameText.text = buttonName;

            button.button.onClick.SetListener(() =>
            {
                // change the category and then scroll up again
                TaskOnClick(icopy);
                UIManagement.singleton.ScrollToBeginning(itemsScrollRect);
            });
        }
        // show items from the first category
        ShowItems();
    }

    // invoked when the value of the text field changes or after choosing a category
    public void ShowItems()
    {
        if (MToolsSettings.Settings.itemManager.categories.Length > 0)
        {
            // show items from the current category
            //ScriptableItem[] items = MToolsSettings.Settings.itemManager.categories[currentCategory].items;

            // show found items in the current category
            //ScriptableItem[] items = string.IsNullOrWhiteSpace(nameItemInput.text) ? MToolsSettings.Settings.itemManager.categories[currentCategory].items : MToolsSettings.Settings.itemManager.categories[currentCategory].items.Where(i => i.name.Contains(nameItemInput.text)).ToArray();

            // show items from the current category or search for them
            ScriptableItem[] items = string.IsNullOrWhiteSpace(nameItemInput.text) ? MToolsSettings.Settings.itemManager.categories[currentCategory].items : GetItemsByName(nameItemInput.text);

            UIUtils.BalancePrefabs(itemSlotPrefab.gameObject, items.Length, itemsContent);

            for (int i = 0; i < items.Length; ++i)
            {
                UIItemManagerSlot slot = itemsContent.GetChild(i).GetComponent<UIItemManagerSlot>();
                ScriptableItem item = items[i];

                string itemName = item.name;
                slot.name = itemName;

                Item itemSlot = new Item(item);
                slot.tooltip.text = new ItemSlot(itemSlot).ToolTip();
                slot.image.color = Color.white;
                slot.image.sprite = item.image;

                // Item Drop
#if ITEM_DROP_C || ITEM_DROP_R
                slot.nameText.text = item.GetTitle();
#else
                slot.nameText.text = itemName;
#endif
                slot.addButton.onClick.SetListener(() =>
                {
                    // check whether to show the information window
                    bool infoWindow = UIInfoMessenger.singleton.showWindow.isOn;

                    // get hashCode, required to search for an item in the dictionary
                    int hashCode = itemName.GetStableHashCode();

                    // sending more than one item at once is depends by its maxStack
                    if (itemSlot.maxStack > 1)
                    {
                        UIAmountConfirmation.singleton.Show(item, () => { UIManagement.singleton.player.CmdMsgAdd(UIManagement.singleton.currentName, hashCode, UIAmountConfirmation.singleton.amount, infoWindow); });
                    }
                    else
                    {
                        UIManagement.singleton.player.CmdMsgAdd(UIManagement.singleton.currentName, hashCode, 1, infoWindow);
                    }
                });
            }
        }
    }

    void TaskOnClick(int index)
    {
        //Debug.Log(index);
        UIManagement.singleton.ClearButton(categoryContent, currentCategory);
        currentCategory = index;
        UIManagement.singleton.SetButton(categoryContent, index);

        // refresh items
        ShowItems();
    }

    // keyword searching
    public ScriptableItem[] GetItemsByName(string itemName)
    {
        List<ScriptableItem> list = new List<ScriptableItem>();
        foreach (MToolsSettings.ItemManagerCategory category in MToolsSettings.Settings.itemManager.categories)
        {
            foreach (ScriptableItem template in category.items)
            {
                if (template.name.Contains(itemName))
                    list.Add(template);
            }
        }
        return list.ToArray();
    }

    // automatically loads all items and divides them by category
    public void LoadItemTemplates()
    {
        /*
#if !UMMORPG_2D
        List<ScriptableItem> templates = (from name in ScriptableItem.dict select name.Value).ToList();
#else
        List<ScriptableItem> templates = (from name in ScriptableItem.All select name.Value).ToList();
#endif
        List<string> categories = templates.Where(go => go.name != "Gold").Select(item => item.GetType().ToString().Replace("Item", "").Trim()).Distinct().ToList();

        itemManagerCategories = new ItemManagerCategory[categories.Count];

        for (int i = 0; i < categories.Count; ++i)
        {
            for (int j = 0; j < itemManagerCategories.Length; ++j)
            {
                itemManagerCategories[j].category = categories[j];
                itemManagerCategories[j].items = templates.Select(item => item).Where(go => go.GetType().ToString().Replace("Item", "").Trim() == categories[j]).ToArray();
            }
        }
        */
    }
}

#endif
