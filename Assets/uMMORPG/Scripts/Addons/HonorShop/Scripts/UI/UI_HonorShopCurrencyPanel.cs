using UnityEngine;

[RequireComponent(typeof(GameEventListener))]
public partial class UI_HonorShopCurrencyPanel : MonoBehaviour
{
    public GameObject panel;
    public GameObject currencySlotPrefab;
    public Transform currencyContent;

    public void OnResheshList()
    {
        OnUpdate();
    }

    public void OnEnable()
    {
        OnUpdate();
    }

    private void OnUpdate()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        if (gameObject.activeSelf)
        {
            UIUtils.BalancePrefabs(currencySlotPrefab.gameObject, player.playerHonorShop.honorCurrencies.Count, currencyContent);

            // -- Currencies
            for (int i = 0; i < player.playerHonorShop.honorCurrencies.Count; ++i)
            {
                UI_HonorCurrencySlot slot = currencyContent.GetChild(i).GetComponent<UI_HonorCurrencySlot>();
                slot.currencyImage.sprite = player.playerHonorShop.honorCurrencies[i].honorCurrency.image;
                slot.nameText.text = player.playerHonorShop.honorCurrencies[i].honorCurrency.name;
                slot.valueText.text = player.playerHonorShop.honorCurrencies[i].amount.ToString();
            }
        }
    }

}