using UnityEngine;
using UnityEngine.UI;

public class UI_NpcShopSlot : MonoBehaviour
{
    public UIShowToolTip tooltip;
    public Image image;
    public UIDragAndDropable dragAndDropable;
    public Text nameText;
    public Text quantityText;
    public Text priceText;
    public Text buyText;
    public Button buyButton;
    public Image RatityOutline;
    public GameObject unique;
    public int inventorySlotIndex;

    private void Start()
    {
#if !_iMMOITEMRARITY
        RatityOutline.gameObject.SetActive(false);
#endif
    }
}
