using UnityEngine;
using UnityEngine.UI;

public class UI_MailItemSlot : MonoBehaviour
{
    public UIShowToolTip tooltip;
    public Button button;
    public Image image;
    public Image RatityOutline;

    private void Start()
    {
#if !_iMMOITEMRARITY
        RatityOutline.gameObject.SetActive(false);
#endif
    }
}