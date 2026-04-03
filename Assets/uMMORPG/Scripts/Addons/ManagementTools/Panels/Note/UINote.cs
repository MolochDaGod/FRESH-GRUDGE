using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UINote : MonoBehaviour
{
    public static UINote singleton;
    public Canvas canvas;

    public TMP_Text playerNameText;
    public TMP_InputField noteInputField;

    public Button saveButton;
    public Toggle editToggle;

    void Awake()
    {
        // initialize singleton
        if (singleton == null) singleton = this;
    }

    // triggered by clicking the note button on the toolbar
    public void PanelActivation()
    {
        if (!canvas.enabled)
        {
            // show info on the basis of data loaded from the database
            UIManagement.singleton.player.CmdShowNote(UIManagement.singleton.currentName);

            // show the selected player's name
            playerNameText.text = UIManagement.singleton.currentName;
        }
        UIManagement.singleton.PanelActivation(canvas);
        AddonManagement.ShowCanvasContent(canvas);
    }

    public void SaveNote()
    {
        UIManagement.singleton.player.CmdMsgChangeNote(UIManagement.singleton.currentName, noteInputField.text);
        saveButton.interactable = false;

        // turn off text edit mode
        if (editToggle.isOn)
        {
            editToggle.isOn = false;
        }
    }

    public void ToggleValueChanged()
    {
        // enable text edit mode
        if (editToggle.isOn)
        {
            noteInputField.readOnly = false;
            noteInputField.interactable = true;
        }
        else
        {
            noteInputField.readOnly = true;
            noteInputField.interactable = false;
        }
    }

    // invoked when the value of the text field changes
    public void ValueChangeCheck()
    {
        // save button works only if you enter text
        if (string.IsNullOrWhiteSpace(noteInputField.text))
        {
            saveButton.interactable = false;
        }
        else if (editToggle.isOn)
        {
            saveButton.interactable = true;
        }
    }

    public void Hide()
    {
        canvas.enabled = false;
    }
}
