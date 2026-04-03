// Attach to the prefab for easier component access by the UI Scripts.
// Otherwise we would need slot.GetChild(0).GetComponentInChildren<Text> etc.
using UnityEngine;
using UnityEngine.UI;

public class UIStorageSlot : MonoBehaviour
{
    public Button button;
    public UIShowToolTip tooltip;
    public UIDragAndDropable dragAndDropable;
    public Image image;
    public GameObject amountOverlay;
    public Text amountText;
    public Image RatityOutline;

    private void Start()
    {
#if !_iMMOITEMRARITY
		RatityOutline.gameObject.SetActive(false);
#endif
    }
}
