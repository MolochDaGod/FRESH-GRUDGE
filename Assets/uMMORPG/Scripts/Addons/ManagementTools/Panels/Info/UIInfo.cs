#if _SQLITE
using TMPro;
using UnityEngine;

public class UIInfo : MonoBehaviour
{
    public static UIInfo singleton;
    public Canvas canvas;

    public TMP_InputField messageInputField;

    void Awake()
    {
        // initialize singleton
        if (singleton == null) singleton = this;
    }

    public void Show(string message)
    {
        messageInputField.text = message;

        if (!canvas.enabled)
        {
            canvas.enabled = true;
        }
    }

    public void Hide()
    {
        canvas.enabled = false;
    }
}

#endif
