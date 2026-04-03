#if _SQLITE
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIInfoMessenger : MonoBehaviour
{
    public static UIInfoMessenger singleton;

    public TMP_InputField messageInput;
    public Button sendButton;
    [Space(14)]
    public Button everyoneButton;
    public TMP_Text everyoneText;
    [Space(14)]
    public Button staffButton;
    public TMP_Text staffText;
    [Space(14)]
    public Button privateButton;
    public TMP_Text privateText;
    [Space(14)]
    public Toggle showWindow;

    [HideInInspector] public MessageType messageType;
    public enum MessageType
    {
        everyone = 0,
        staff,
        individual
    }
    
    Color labelColor;   // #BCBCBCFF

    void Awake()
    {
        // initialize singleton
        if (singleton == null) singleton = this;
    }

    void Start()
    {
        ColorUtility.TryParseHtmlString("#BCBCBCFF", out labelColor);
    }

    // invoked when the value of the text field changes
    public void ValueChangeCheck()
    {
        // send button works only if you enter text
        if (string.IsNullOrWhiteSpace(messageInput.text))
        {
            //Debug.Log("message[inactive]");
            sendButton.interactable = false;
        }
        else if (!sendButton.IsInteractable())
        {
            //Debug.Log("message[active]");
            sendButton.interactable = true;
        }
    }

    // change the type of message after selecting one of the three buttons
    public void ChangeMessageType(int index)
    {
        switch (index)
        {
            case 0:
                // everyone
                everyoneButton.interactable = false;
                if (!staffButton.IsInteractable()) staffButton.interactable = true;
                if (UIManagement.singleton.currentName != "" && !privateButton.IsInteractable())
                {
                    privateButton.interactable = true;
                }

                // change the color of the button name
                everyoneText.color = labelColor;
                staffText.color = Color.grey;
                privateText.color = Color.grey;

                // set the message type to everyone
                messageType = (MessageType)index;
                //Debug.Log("message[0]");
                break;
            case 1:
                // staff
                staffButton.interactable = false;
                if (!everyoneButton.IsInteractable()) everyoneButton.interactable = true;
                if (UIManagement.singleton.currentName != "" && !privateButton.IsInteractable())
                {
                    privateButton.interactable = true;
                }

                // change the color of the button name
                everyoneText.color = Color.grey;
                staffText.color = labelColor;
                privateText.color = Color.grey;

                // set the message type to staff
                messageType = (MessageType)index;
                //Debug.Log("message[1]");
                break;
            case 2:
                // private
                privateButton.interactable = false;
                if (!everyoneButton.IsInteractable()) everyoneButton.interactable = true;
                if (!staffButton.IsInteractable()) staffButton.interactable = true;

                // change the color of the button name
                everyoneText.color = Color.grey;
                staffText.color = Color.grey;
                privateText.color = labelColor;

                // set the message type to individual
                messageType = (MessageType)index;
                //Debug.Log("message[2]");
                break;
        }
    }
    
    public void SendMessageByType()
    {
        // check whether to show the information window
        bool infoWindow = showWindow.isOn;

        switch (Convert.ToInt32(messageType))
        {
            case 0:
                UIManagement.singleton.player.CmdSendPublicMessage(messageInput.text, infoWindow);
                //Debug.Log("everyone");
                break;
            case 1:
                UIManagement.singleton.player.CmdSendStaffMessage(messageInput.text, infoWindow);
                //Debug.Log("staff");
                break;
            case 2:
                UIManagement.singleton.player.CmdSendIndividualMessage(UIManagement.singleton.currentName, messageInput.text, infoWindow);
                //Debug.Log("private");
                break;
        }
        UIManagement.singleton.DeleteText(messageInput);
    }    

    // enable the private message option if the player has been selected
    public void EnablePrivateMessageOption()
    {
        if (messageType != (MessageType)2)
        {
            if (!privateButton.interactable)
            {
                privateButton.interactable = true;
                //Debug.Log("private[active]");
            }
        }
    }

    // disable the private message option when the selected player logs out
    public void DisablePrivateMessageOption()
    {
        if (privateButton.interactable || messageType == (MessageType)2)
        {
            // set the message type to default
            ChangeMessageType(0);

            // disable the ability to select private message option
            privateButton.interactable = false;

            // clear the message input field
            UIManagement.singleton.DeleteText(messageInput);
            //Debug.Log("private, button[clear]");
        }
    }
}

#endif
