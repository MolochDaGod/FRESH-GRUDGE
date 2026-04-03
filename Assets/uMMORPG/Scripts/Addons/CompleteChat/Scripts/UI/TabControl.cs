using UnityEngine;
using UnityEngine.UI;

public class TabControl : MonoBehaviour
{
    public UICompleteChat chat;
    [Header("--=[World checkBox list]=--")]
    public Toggle worldWorld;
    public Toggle worldGuild;
    public Toggle worldGroup;
    public Toggle worldPm;
    public Toggle worldInfo;
    public Toggle woldConbat;
    [Header("--=[Guild checkBox list]=--")]
    public Toggle guildWorld;
    public Toggle guildGuild;
    public Toggle guildGroup;
    public Toggle guildPm;
    public Toggle guildInfo;
    public Toggle guildConbat;
    [Header("--=[Group checkBox list]=--")]
    public Toggle groupWorld;
    public Toggle groupGuild;
    public Toggle groupGroup;
    public Toggle groupPm;
    public Toggle groupInfo;
    public Toggle groupConbat;
    [Header("--=[PM checkBox list]=--")]
    public Toggle pmWorld;
    public Toggle pmGuild;
    public Toggle pmGroup;
    public Toggle pmPm;
    public Toggle pmInfo;
    public Toggle pmConbat;
    [Header("--=[Info checkBox list]=--")]
    public Toggle infoWorld;
    public Toggle infoGuild;
    public Toggle infoGroup;
    public Toggle infoPm;
    public Toggle infoInfo;
    public Toggle infoConbat;
    [Header("--=[Combat checkBox list]=--")]
    public Toggle combatWorld;
    public Toggle combatGuild;
    public Toggle combatGroup;
    public Toggle combatPm;
    public Toggle combatInfo;
    public Toggle combatConbat;

    private void Start()
    {
        if (!PlayerPrefs.HasKey("SaveSlot_ShowWorld"))
        {
            SaveTabConfig();
        }
    }

    public void LoadTabConfig()
    {
        if (PlayerPrefs.HasKey("SaveSlot_ShowWorld"))
        {
            string[] worldToogleIsOn = PlayerPrefs.GetString("SaveSlot_ShowWorld").Split(":"[0]);
            worldWorld.GetComponent<Toggle>().isOn = ((worldToogleIsOn[0] == "0") ? false : true);
            worldGuild.GetComponent<Toggle>().isOn = ((worldToogleIsOn[1] == "0") ? false : true);
            worldGroup.GetComponent<Toggle>().isOn = ((worldToogleIsOn[2] == "0") ? false : true);
            worldPm.GetComponent<Toggle>().isOn = ((worldToogleIsOn[3] == "0") ? false : true);
            worldInfo.GetComponent<Toggle>().isOn = ((worldToogleIsOn[4] == "0") ? false : true);
            woldConbat.GetComponent<Toggle>().isOn = ((worldToogleIsOn[5] == "0") ? false : true);
        }
        if (PlayerPrefs.HasKey("SaveSlot_ShowGuild"))
        {
            string[] guildToogleIsOn = PlayerPrefs.GetString("SaveSlot_ShowGuild").Split(":"[0]);
            guildWorld.GetComponent<Toggle>().isOn = ((guildToogleIsOn[0] == "0") ? false : true);
            guildGuild.GetComponent<Toggle>().isOn = ((guildToogleIsOn[1] == "0") ? false : true);
            guildGroup.GetComponent<Toggle>().isOn = ((guildToogleIsOn[2] == "0") ? false : true);
            guildPm.GetComponent<Toggle>().isOn = ((guildToogleIsOn[3] == "0") ? false : true);
            guildInfo.GetComponent<Toggle>().isOn = ((guildToogleIsOn[4] == "0") ? false : true);
            guildConbat.GetComponent<Toggle>().isOn = ((guildToogleIsOn[5] == "0") ? false : true);
        }
        if (PlayerPrefs.HasKey("SaveSlot_ShowGroup"))
        {
            string[] groupToogleIsOn = PlayerPrefs.GetString("SaveSlot_ShowGroup").Split(":"[0]);
            groupWorld.GetComponent<Toggle>().isOn = ((groupToogleIsOn[0] == "0") ? false : true);
            groupGuild.GetComponent<Toggle>().isOn = ((groupToogleIsOn[1] == "0") ? false : true);
            groupGroup.GetComponent<Toggle>().isOn = ((groupToogleIsOn[2] == "0") ? false : true);
            groupPm.GetComponent<Toggle>().isOn = ((groupToogleIsOn[3] == "0") ? false : true);
            groupInfo.GetComponent<Toggle>().isOn = ((groupToogleIsOn[4] == "0") ? false : true);
            groupConbat.GetComponent<Toggle>().isOn = ((groupToogleIsOn[5] == "0") ? false : true);
        }
        if (PlayerPrefs.HasKey("SaveSlot_ShowPm"))
        {
            string[] pmToogleIsOn = PlayerPrefs.GetString("SaveSlot_ShowPm").Split(":"[0]);
            pmWorld.GetComponent<Toggle>().isOn = ((pmToogleIsOn[0] == "0") ? false : true);
            pmGuild.GetComponent<Toggle>().isOn = ((pmToogleIsOn[1] == "0") ? false : true);
            pmGroup.GetComponent<Toggle>().isOn = ((pmToogleIsOn[2] == "0") ? false : true);
            pmPm.GetComponent<Toggle>().isOn = ((pmToogleIsOn[3] == "0") ? false : true);
            pmInfo.GetComponent<Toggle>().isOn = ((pmToogleIsOn[4] == "0") ? false : true);
            pmConbat.GetComponent<Toggle>().isOn = ((pmToogleIsOn[5] == "0") ? false : true);
        }
        if (PlayerPrefs.HasKey("SaveSlot_ShowInfo"))
        {
            string[] infoToogleIsOn = PlayerPrefs.GetString("SaveSlot_ShowInfo").Split(":"[0]);
            infoWorld.GetComponent<Toggle>().isOn = ((infoToogleIsOn[0] == "0") ? false : true);
            infoGuild.GetComponent<Toggle>().isOn = ((infoToogleIsOn[1] == "0") ? false : true);
            infoGroup.GetComponent<Toggle>().isOn = ((infoToogleIsOn[2] == "0") ? false : true);
            infoPm.GetComponent<Toggle>().isOn = ((infoToogleIsOn[3] == "0") ? false : true);
            infoInfo.GetComponent<Toggle>().isOn = ((infoToogleIsOn[4] == "0") ? false : true);
            infoConbat.GetComponent<Toggle>().isOn = ((infoToogleIsOn[5] == "0") ? false : true);
        }
        if (PlayerPrefs.HasKey("SaveSlot_ShowCombat"))
        {
            string[] combatToogleIsOn = PlayerPrefs.GetString("SaveSlot_ShowCombat").Split(":"[0]);
            combatWorld.GetComponent<Toggle>().isOn = ((combatToogleIsOn[0] == "0") ? false : true);
            combatGuild.GetComponent<Toggle>().isOn = ((combatToogleIsOn[1] == "0") ? false : true);
            combatGroup.GetComponent<Toggle>().isOn = ((combatToogleIsOn[2] == "0") ? false : true);
            combatPm.GetComponent<Toggle>().isOn = ((combatToogleIsOn[3] == "0") ? false : true);
            combatInfo.GetComponent<Toggle>().isOn = ((combatToogleIsOn[4] == "0") ? false : true);
            combatConbat.GetComponent<Toggle>().isOn = ((combatToogleIsOn[5] == "0") ? false : true);
        }
    }
    public void SaveTabConfig(bool isDefault = false)
    {
        //Valeur world
        string worldConfig = isDefault ? "1:0:0:0:1:0" : "1:" +
                             ((!worldGuild.GetComponent<Toggle>().isOn) ? "0" : "1") + ":" +
                             ((!worldGroup.GetComponent<Toggle>().isOn) ? "0" : "1") + ":" +
                             ((!worldPm.GetComponent<Toggle>().isOn) ? "0" : "1") + ":" +
                             ((!worldInfo.GetComponent<Toggle>().isOn) ? "0" : "1") + ":" +
                             ((!woldConbat.GetComponent<Toggle>().isOn) ? "0" : "1");
        Debug.Log(worldConfig +(( isDefault) ? " defaut" : " pas defaut"));
        string guildConfig = isDefault ? "0:1:0:0:1:0" : ((!guildWorld.GetComponent<Toggle>().isOn) ? "0" : "1") + ":" +
                             "1:" +
                             ((!guildGroup.GetComponent<Toggle>().isOn) ? "0" : "1") + ":" +
                             ((!guildPm.GetComponent<Toggle>().isOn) ? "0" : "1") + ":" +
                             ((!guildInfo.GetComponent<Toggle>().isOn) ? "0" : "1") + ":" +
                             ((!guildConbat.GetComponent<Toggle>().isOn) ? "0" : "1");

        string groupConfig = isDefault ? "0:0:1:0:1:0" : ((!groupWorld.GetComponent<Toggle>().isOn) ? "0" : "1") + ":" +
                             ((!groupGuild.GetComponent<Toggle>().isOn) ? "0" : "1") + ":" +
                             "1:" +
                             ((!groupPm.GetComponent<Toggle>().isOn) ? "0" : "1") + ":" +
                             ((!groupInfo.GetComponent<Toggle>().isOn) ? "0" : "1") + ":" +
                             ((!groupConbat.GetComponent<Toggle>().isOn) ? "0" : "1");

        string pmConfig = isDefault ? "0:0:0:1:1:0" : ((!pmWorld.GetComponent<Toggle>().isOn) ? "0" : "1") + ":" +
                            ((!pmGuild.GetComponent<Toggle>().isOn) ? "0" : "1") + ":" +
                            ((!pmGroup.GetComponent<Toggle>().isOn) ? "0" : "1") + ":" +
                            "1:" +
                            ((!pmInfo.GetComponent<Toggle>().isOn) ? "0" : "1") + ":" +
                            ((!pmConbat.GetComponent<Toggle>().isOn) ? "0" : "1");

        string infoConfig = isDefault ? "0:0:0:0:1:0" : ((!infoWorld.GetComponent<Toggle>().isOn) ? "0" : "1") + ":" +
                            ((!infoGuild.GetComponent<Toggle>().isOn) ? "0" : "1") + ":" +
                            ((!infoGroup.GetComponent<Toggle>().isOn) ? "0" : "1") + ":" +
                            ((!infoPm.GetComponent<Toggle>().isOn) ? "0" : "1") + ":" +
                            "1:" +
                            ((!infoConbat.GetComponent<Toggle>().isOn) ? "0" : "1");

        string combatConfig = isDefault ? "0:0:0:0:1:1" : ((!combatWorld.GetComponent<Toggle>().isOn) ? "0" : "1") + ":" +
                            ((!combatGuild.GetComponent<Toggle>().isOn) ? "0" : "1") + ":" +
                            ((!combatGroup.GetComponent<Toggle>().isOn) ? "0" : "1") + ":" +
                            ((!combatPm.GetComponent<Toggle>().isOn) ? "0" : "1") + ":" +
                            ((!combatInfo.GetComponent<Toggle>().isOn) ? "0" : "1") + ":" +
                            "1:";

        PlayerPrefs.SetString("SaveSlot_ShowWorld", worldConfig);
        PlayerPrefs.SetString("SaveSlot_ShowGuild", guildConfig);
        PlayerPrefs.SetString("SaveSlot_ShowGroup", groupConfig);
        PlayerPrefs.SetString("SaveSlot_ShowPm", pmConfig);
        PlayerPrefs.SetString("SaveSlot_ShowInfo", infoConfig);
        PlayerPrefs.SetString("SaveSlot_ShowCombat", combatConfig);
        PlayerPrefs.Save();

        if(!isDefault) 
            chat.tabShowOptions();
    }
}