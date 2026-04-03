using UnityEngine;
using UnityEngine.UI;

public class UI_SettingsGameplay : MonoBehaviour
{
    public UI_Settings ui_Settings;

    [Header("[-=-[ Gameplay Settings ]-=-]")]
    public Slider uiScalable;                 //Sliders for all gameplay settings.+
    public Toggle blockTradeRequest;
    public Toggle blockPartiInvit;
    public Toggle blockGuildInvit;
    public Toggle showChat;
    public Toggle showOverheads;
    public Toggle showFPS;
    public Toggle showPing;


    public void LoadSettingsGameplay()
    {
        showChat.isOn = ui_Settings.settingsBinding.GetBoolPreference("ShowChat", ui_Settings.settingsBinding.isShowChat);
        showFPS.isOn = ui_Settings.settingsBinding.GetBoolPreference("ShowFps", ui_Settings.settingsBinding.isShowFps);
        showPing.isOn = ui_Settings.settingsBinding.GetBoolPreference("ShowPing", ui_Settings.settingsBinding.isShowPing);
        showOverheads.isOn = ui_Settings.settingsBinding.GetBoolPreference("ShowOverhead", ui_Settings.settingsBinding.isShowOverhead);
        blockTradeRequest.isOn = ui_Settings.settingsBinding.GetBoolPreference("BlockTrades", ui_Settings.settingsBinding.blockTradeInvit);
        blockPartiInvit.isOn = ui_Settings.settingsBinding.GetBoolPreference("BlockParty", ui_Settings.settingsBinding.blockPartyInvit);
        blockGuildInvit.isOn = ui_Settings.settingsBinding.GetBoolPreference("BlockGuild", ui_Settings.settingsBinding.blockGuildInvit);

        for (int i = 0; i < ui_Settings.uiScalablePanel.Length; i++)
        {
            ui_Settings.uiScalablePanel[i].transform.localScale = new Vector3((PlayerPrefs.HasKey("UiScale")) ? PlayerPrefs.GetFloat("UiScale") : 1, (PlayerPrefs.HasKey("UiScale")) ? PlayerPrefs.GetFloat("UiScale") : 1, 1);
        }
        uiScalable.value = (PlayerPrefs.HasKey("UiScale")) ? PlayerPrefs.GetFloat("UiScale") : 1;
    }
    // Set block trades and save its settings.
    public void SaveBlockTrades()
    {
        Player player = Player.localPlayer;
        if (player != null)
            player.playerAddonsConfigurator.CmdBlockTradeRequest(blockTradeRequest.isOn);
        ui_Settings.settingsBinding.SaveBoolPreference("BlockTrades", blockTradeRequest.isOn);
    }

    // Set block party invites and save its settings.
    public void SaveBlockParty()
    {
        Player player = Player.localPlayer;
        if (player != null)
            player.playerAddonsConfigurator.CmdBlockPartyInvite(blockPartiInvit.isOn);
        ui_Settings.settingsBinding.SaveBoolPreference("BlockParty", blockPartiInvit.isOn);
    }

    // Set block guild invites and save its settings.
    public void SaveBlockGuild()
    {
        Player player = Player.localPlayer;
        if (player != null)
            player.playerAddonsConfigurator.CmdBlockGuildInvite(blockGuildInvit.isOn);
        ui_Settings.settingsBinding.SaveBoolPreference("BlockGuild", blockGuildInvit.isOn);
    }

    // Set show overhead health and save its settings.
    public void SaveShowOverhead()
    {
        ui_Settings.settingsBinding.SaveBoolPreference("ShowOverhead", showOverheads.isOn);
    }

    // Set show chat and save its settings.
    public void SaveShowChat()
    {
        ui_Settings.settingsBinding.SaveBoolPreference("ShowChat", showChat.isOn);
    }

    // Set show chat and save its settings.
    public void SaveShowFPS()
    {
        ui_Settings.settingsBinding.SaveBoolPreference("ShowFps", showFPS.isOn);
    }

    // Set show chat and save its settings.
    public void SaveShowPing()
    {
        ui_Settings.settingsBinding.SaveBoolPreference("ShowPing", showPing.isOn);
    }

    // Set ui scale and save its settings.
    public void SaveUiScale(Slider slider)
    {
        for (int i = 0; i < ui_Settings.uiScalablePanel.Length; i++)
        {
            ui_Settings.uiScalablePanel[i].transform.localScale = new Vector3(slider.value, slider.value, 1);
        }
        PlayerPrefs.SetFloat("UiScale", slider.value);

    }

}
