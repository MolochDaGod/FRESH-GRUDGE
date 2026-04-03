using UnityEngine;
using Mirror;
using System.Collections;

// LogoutTimer
public partial class PlayerAddonsConfigurator
{
    [Header("[-=-[ player Auto Logout Timer ]-=-]")]
    public Tmpl_LogoutTimer logoutTimerTemplate;

    protected float _logoutTimer = 0;

#if _CLIENT
    private void OnStartLocalPlayer_LogoutTimer()
    {
        StartCoroutine(TimeLogout());
    }
#endif

    [ClientCallback]
    private void LogoutTimer()
    {
        if (player.state == "IDLE")
        {
            _logoutTimer += player.cacheTimerInterval;
        }
        else
        {
            _logoutTimer = 0;

            if (UI_LogoutTimer_Popup.singleton)
                UI_LogoutTimer_Popup.singleton.Hide();
            else
                GameLog.LogWarning("You forgot to add LogoutTimer_Popup to your canvas!");
        }

        if (_logoutTimer > logoutTimerTemplate.logoutKickTime) 
            NetworkManagerMMO.Quit();
        else if (_logoutTimer > logoutTimerTemplate.logoutWarningTime)
            UI_LogoutTimer_Popup.singleton.Show();
    }
#if _CLIENT
    IEnumerator TimeLogout()
    {
        while (true)
        {
            LogoutTimer();
            yield return new WaitForSeconds(1);
        }
    }
#endif
    // -----------------------------------------------------------------------------------
}