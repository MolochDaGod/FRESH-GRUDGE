using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICommandConfirmation : MonoBehaviour
{
    public static UICommandConfirmation singleton;
    public Canvas canvas;
    public TMP_Text messageText;
    public Button confirmButton;

    void Awake()
    {
        // initialize singleton
        if (singleton == null) singleton = this;
    }

    public void Show(string message, UnityAction onConfirm)
    {
        messageText.text = message;
        confirmButton.onClick.SetListener(onConfirm);
        canvas.enabled = true;
    }

    public void Hide()
    {
        canvas.enabled = false;
    }
}
