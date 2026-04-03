using Mirror;
using UnityEngine;

public partial class PlayerAddonsConfigurator
{
    public Tmpl_SettingsConfiguration settingsBinding;
    [SyncVar] public bool isBlockingTrade = false;
    [SyncVar] public bool isBlockingParty = false;
    [SyncVar] public bool isBlockingGuild = false;

#if _CLIENT && _iMMOSETTINGS
    private void OnStartLocalPlayer_GameSettings()
    {
        SetSkillbar_Hotkeys();
    }

    // Set in skillbar new hotkey.
    public void SetSkillbar_Hotkeys()
    {
        if (settingsBinding != null)
        {
            if (settingsBinding.skillBarBinding.Length > 0)
            {
                for (int i = 0; i < (settingsBinding.skillBarBinding.Length); i++)
                {
                    string key = $"SkillBar_{i}";
                    if (PlayerPrefs.HasKey(key)) player.skillbar.slots[i].hotKey = (KeyCode)PlayerPrefs.GetInt(key);
                    else player.skillbar.slots[i].hotKey = settingsBinding.skillBarBinding[i].keyCode;
                }
            }
        }
    }
#endif

    #region Commands

    [Command]
    public void CmdBlockPartyInvite(bool block)
    {
#if _SERVER
        isBlockingParty = block;
#endif
    }

    [Command]
    public void CmdBlockGuildInvite(bool block)
    {
#if _SERVER
        isBlockingGuild = block;
#endif
    }

    [Command]
    public void CmdBlockTradeRequest(bool block)
    {
#if _SERVER
        isBlockingTrade = block;
#endif
    }

    #endregion Commands
}