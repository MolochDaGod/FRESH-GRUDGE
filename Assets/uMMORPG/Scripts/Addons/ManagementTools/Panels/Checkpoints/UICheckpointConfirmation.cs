#if _SQLITE
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICheckpointConfirmation : MonoBehaviour
{
    public static UICheckpointConfirmation singleton;
    public Canvas canvas;

    public Button saveButton;

    public TMP_InputField nameInputField;

    void Awake()
    {
        // initialize singleton
        if (singleton == null) singleton = this;
    }

    public void Show(UnityAction onConfirm)
    {
        saveButton.onClick.SetListener(onConfirm);
        canvas.enabled = true;
    }

    // invoked when the value of the text field changes
    public void ValueChangeCheck()
    {
        // save button works only if you enter text
        if (string.IsNullOrWhiteSpace(nameInputField.text))
        {
            saveButton.interactable = false;
        }
        else if (!saveButton.IsInteractable())
        {
            saveButton.interactable = true;
        }
    }

    public void Hide()
    {
        DeleteText(nameInputField);

        canvas.enabled = false;
    }

    public void DeleteText(TMP_InputField input)
    {
        input.text = "";
    }
}

#endif
