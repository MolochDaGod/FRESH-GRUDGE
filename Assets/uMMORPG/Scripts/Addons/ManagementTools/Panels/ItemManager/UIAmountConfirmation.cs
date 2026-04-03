#if _SQLITE
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIAmountConfirmation : MonoBehaviour
{
    public static UIAmountConfirmation singleton;
    public Canvas canvas;

    public Button confirmButton;
    public TMP_InputField amountInput;

    public UIShowItemTooltip tooltip;
    public Image image;

    [HideInInspector] public int amount = 1;
    [HideInInspector] public int maxStack = 1;

    void Awake()
    {
        // initialize singleton
        if (singleton == null) singleton = this;
    }

    // invoked when the value of the text field changes
    public void ValueChangeCheck()
    {
        // replace the minus sign
        amountInput.text = amountInput.text.Replace("-", "").Trim();
        if (amountInput.text == "")
        {
            amountInput.text = "1";
        }
        // the maximum value that can be entered is the same as in the scriptable item's template
        amountInput.text = Mathf.Clamp(int.Parse(amountInput.text), 1, maxStack).ToString();
        int.TryParse(amountInput.text, out amount);
    }

    public void Show(ScriptableItem item, UnityAction onConfirm)
    {
        Item itemSlot = new Item(item);
        tooltip.text = new ItemSlot(itemSlot, amount).ToolTip();
        image.color = Color.white;
        image.sprite = item.image;

        maxStack = item.maxStack;
        amountInput.text = Mathf.Clamp(amount, 1, maxStack).ToString();

        confirmButton.onClick.SetListener(onConfirm);
        canvas.enabled = true;
    }

    public void Hide()
    {
        canvas.enabled = false;
    }
}

#endif
