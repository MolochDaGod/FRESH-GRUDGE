using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISupport : MonoBehaviour
{
    public Canvas canvas;

    public TMP_Dropdown dropDown;
    public TMP_InputField descriptionInputField;

    public Button sendButton;

    Player player;

    void Update()
    {
        if (Input.GetKeyDown(MToolsSettings.Settings.supportPanelKey) && !UIUtils.AnyInputActive())
        {
            Show();
        }
    }

    // invoked when the value of the text field changes
    public void ValueChangeCheck()
    {
        // send button works only if you enter text
        sendButton.interactable = string.IsNullOrWhiteSpace(descriptionInputField.text) ? false : true;
    }

    public void SendRequest()
    {
        if (player != null)
        {
            if (dropDown.value == 0)
            {
                HelpRequest helpRequest = new HelpRequest
                {
                    characterName = player.name,
                    uniqueId = Guid.NewGuid().ToString(),
                    date = DateTime.UtcNow.ToShortDateString(),
                    position = player.transform.position,
                    description = descriptionInputField.text
                };
                player.CmdMsgAddHelpRequest(helpRequest);
            }
            else
            {
                BugReport report = new BugReport
                {
                    characterName = player.name,
                    uniqueId = Guid.NewGuid().ToString(),
                    status = 0,
                    date = DateTime.UtcNow.ToShortDateString(),
                    position = player.transform.position,
                    description = descriptionInputField.text
                };
                player.CmdMsgAddBugReport(report);
            }
            Hide();
        }
    }

    public void Show()
    {
        if (!player)
        {
            player = Player.localPlayer;
        }

        if (player != null)
        {
            if (!canvas.enabled)
            {
                canvas.enabled = true;
            }
            else
            {
                Hide();
            }
        }
    }

    public void Hide()
    {
        DeleteText(descriptionInputField);

        canvas.enabled = false;
    }

    public void DeleteText(TMP_InputField input)
    {
        input.text = "";
    }
}
