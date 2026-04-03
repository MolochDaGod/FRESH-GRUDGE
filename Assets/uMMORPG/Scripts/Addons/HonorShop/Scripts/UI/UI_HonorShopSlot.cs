using UnityEngine;
using UnityEngine.UI;

public class UI_HonorShopSlot : MonoBehaviour
{
    public UIShowToolTip tooltip;
    public Image image;
    public UIDragAndDropable dragAndDropable;
    public Text nameText;
    public Text priceText;
    public Text currencyText;
    public Button buyButton;
    public Image RatityOutline;

    private void Start()
    {
#if !_iMMOITEMRARITY
        RatityOutline.gameObject.SetActive(false);
#endif
    }
}