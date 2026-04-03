#if _SQLITE
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPasswordChange : MonoBehaviour
{
    public Canvas canvas;

    public TMP_InputField passwordInputField;
    public TMP_InputField passwordConfirmInputField;
    public TMP_InputField passwordCurrentInputField;

    public Button saveButton;
    public Toggle passwordHidingToggle;

    Player player;

    public static UIPasswordChange singleton;
    [HideInInspector] public NetworkAuthenticatorMMO authenticator;

    void Awake()
    {
        // initialize singleton
        if (singleton == null) singleton = this;
    }

    void Start()
    {
        authenticator = ((NetworkManagerMMO)NetworkManager.singleton).GetComponent<NetworkAuthenticatorMMO>();
    }

    void Update ()
    {
        if (Input.GetKeyDown(MToolsSettings.Settings.passwordPanelKey) && !UIUtils.AnyInputActive())
        {
            Show();
        }
    }

    // hides the typed characters by showing them as asterisks characters
    public void ToggleValueChanged()
    {
        if (passwordHidingToggle.isOn)
        {
            passwordInputField.contentType = TMP_InputField.ContentType.Standard;
            passwordConfirmInputField.contentType = TMP_InputField.ContentType.Standard;
        }
        else
        {
            passwordInputField.contentType = TMP_InputField.ContentType.Password;
            passwordConfirmInputField.contentType = TMP_InputField.ContentType.Password;
        }
        passwordInputField.ForceLabelUpdate();
        passwordConfirmInputField.ForceLabelUpdate();
    }

    // invoked when the value of the new password field changes
    public void ValueChangeCheckPassword()
    {
        // check the save button
        if (string.IsNullOrWhiteSpace(passwordInputField.text))
        {
            saveButton.interactable = false;
        }
        else if (!string.IsNullOrWhiteSpace(passwordCurrentInputField.text))
        {
            if (passwordConfirmInputField.text == passwordInputField.text)
            {
                CompareLoginPassword();
            }
            else saveButton.interactable = false;
        }
    }

    // invoked when the value of the confirm password field changes
    public void ValueChangeCheckConfirmPassword()
    {
        // check the save button
        if (string.IsNullOrWhiteSpace(passwordConfirmInputField.text))
        {
            saveButton.interactable = false;
        }
        else if (!string.IsNullOrWhiteSpace(passwordCurrentInputField.text))
        {
            if (passwordConfirmInputField.text == passwordInputField.text)
            {
                CompareLoginPassword();
            }
            else saveButton.interactable = false;
        }
    }

    // invoked when the value of the current password field changes
    public void ValueChangeCheckCurrentPassword()
    {
        // check the save button
        if (string.IsNullOrWhiteSpace(passwordCurrentInputField.text))
        {
            saveButton.interactable = false;
        }
        else if (!string.IsNullOrWhiteSpace(passwordInputField.text) && !string.IsNullOrWhiteSpace(passwordConfirmInputField.text))
        {
            if (passwordConfirmInputField.text == passwordInputField.text)
            {
                CompareLoginPassword();
            }
            else saveButton.interactable = false;
        }
    }

    public void SavePassword()
    {
        if (player != null)
        {
            player.CmdMsgChangePassword(passwordInputField.text);

            // close the panel and clear all text fields
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
                // close the panel and clear all text fields
                Hide();
            }
        }
    }

    public void Hide()
    {
        DeleteText(passwordInputField);
        DeleteText(passwordConfirmInputField);
        DeleteText(passwordCurrentInputField);

        canvas.enabled = false;
    }

    public void DeleteText(TMP_InputField input)
    {
        input.text = "";
    }

    void CompareLoginPassword()
    {
        // save button works only if you enter your current password
        saveButton.interactable = authenticator.loginPassword == passwordCurrentInputField.text ? true : false;
    }
}

#endif
