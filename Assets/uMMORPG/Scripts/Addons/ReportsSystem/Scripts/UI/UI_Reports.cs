using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Reports : MonoBehaviour
{
    #region Variables
    public GameObject panel;         //The reports menu to be used.
    public TMP_Dropdown inputTitle;      //Header that will be placed on the report.
    public TMP_InputField inputDetails;  //Information that will be placed on the report.
    public Button sendReport;        //Button that will send the information to your database.
    public TMP_Text userName;            //The name of the player that's sending in the report.
    #endregion Variables

    #region Functions
#if _CLIENT
    // Use this for initialization
    private void Start()
    {
        sendReport.onClick.SetListener(() => SendBugButtonEvent());
    }

    // Update is called once per frame
    public void ActivatePanel()
    {
        var player = Player.localPlayer;
        if (!player) return;
        userName.text = player.account;
        panel.SetActive(!panel.activeSelf);
    }

    //Tells the client to send the server database the report.
    public void SendBugButtonEvent()
    {
        var player = Player.localPlayer;
        if (!player) return;

        if (!string.IsNullOrWhiteSpace(inputDetails.text) && !string.IsNullOrWhiteSpace(inputTitle.options[inputTitle.value].text)) 
        {
            player.playerAddonsConfigurator.CmdSendBugReport(inputTitle.options[inputTitle.value].text, inputDetails.text);

            inputDetails.text = "";
            panel.SetActive(false);
        }
    }
#endif
    #endregion Functions
}
