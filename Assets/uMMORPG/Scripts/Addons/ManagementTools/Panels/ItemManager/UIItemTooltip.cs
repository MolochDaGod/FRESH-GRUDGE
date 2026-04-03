using TMPro;
using UnityEngine;

public class UIItemTooltip : MonoBehaviour
{
    public static UIItemTooltip singleton;
    public Canvas canvas;

    public TMP_Text itemDescription;

    void Awake()
    {
        // initialize singleton
        if (singleton == null) singleton = this;
    }

    public void Show(GameObject image, string text)
    {
        if (!canvas.enabled)
        {
            itemDescription.text = text;
            canvas.enabled = true;

            // set the popup window position based by the item icon
            canvas.transform.GetChild(0).transform.position = image.transform.TransformPoint(Vector3.right * 2);
        }
    }

    public void Hide()
    {
        if (canvas.enabled)
        {
            itemDescription.text = "";
            canvas.enabled = false;
        }
    }
}
