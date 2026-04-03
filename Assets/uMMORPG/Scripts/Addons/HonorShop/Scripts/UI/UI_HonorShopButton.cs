using UnityEngine;

public partial class UI_HonorShopButton : MonoBehaviour
{
    public GameObject honorShopCurrencyPanel;

    // -----------------------------------------------------------------------------------
    // ShowHonorShopCurrencyPanel
    // -----------------------------------------------------------------------------------
    public void ShowHonorShopCurrencyPanel()
    {
        if (honorShopCurrencyPanel.activeInHierarchy)
            honorShopCurrencyPanel.SetActive(false);
        else
            honorShopCurrencyPanel.SetActive(true);
    }
    // ----------------------------------------------------------------------------------
}