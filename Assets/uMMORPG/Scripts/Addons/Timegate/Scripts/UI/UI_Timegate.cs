using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ===================================================================================
// TIMEGATE UI
// ===================================================================================
public partial class UI_Timegate : MonoBehaviour
{
    public GameObject panel;
    public Button TimegateButton;
    public Transform content;
    public ScrollRect scrollRect;
    public GameObject textTimegate;

    private const string MSG_HEADING = "Timegate available:";
    private const string MSG_TIMEGATE = "Timegate to: ";
    private const string MSG_MAXVISITS = " - Max entrances per player: ";
    private const string MSG_MAXHOURS = " - Time between entrances: ";
    private const string MSG_OPENFROM = " - Opening Day(s): ";
    private const string MSG_OPENON = " - Opening Month: ";
    private const string MSG_HOURS = " hrs";

    private bool okTimegate = false;
    private bool okDayStart = false;
    private bool okDayEnd = false;
    private bool okMonth = false;
    private bool okVisits = false;
    private bool okHours = false;

    // -----------------------------------------------------------------------------------
    // validateTimegate
    // -----------------------------------------------------------------------------------
    private void ValidateTimegate()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        Area_Timegate Timegate = player.playerAddonsConfigurator.myTimegate;

        if (Timegate)
        {
            int idx = player.playerAddonsConfigurator.GetTimegateIndexByName(Timegate.name);

            if (idx > -1 && player.playerAddonsConfigurator.timegates[idx].valid)
            {
                okVisits = (Timegate.maxVisits == 0 || player.playerAddonsConfigurator.timegates[idx].count < Timegate.maxVisits);
                okHours = (Timegate.hoursBetweenVisits == 0 || player.playerAddonsConfigurator.ValidateTimegateTime(player.playerAddonsConfigurator.timegates[idx].hours, Timegate.hoursBetweenVisits));
            }
            else
            {
                okVisits = true;
                okHours = true;
            }

            okTimegate = (Timegate.teleportationTarget.Valid);
            okDayStart = (Timegate.dayStart == 0 || Timegate.dayStart <= DateTime.UtcNow.Day);
            okDayEnd = (Timegate.dayEnd == 0 || Timegate.dayEnd >= DateTime.UtcNow.Day);
            okMonth = (Timegate.activeMonth == 0 || Timegate.activeMonth == DateTime.UtcNow.Month);

            okTimegate = okDayStart && okDayEnd && okMonth && okHours && okVisits;
        }
        else
        {
            okTimegate = false;
        }
    }

    // -----------------------------------------------------------------------------------
    // UpdateTextbox
    // -----------------------------------------------------------------------------------
    public void UpdateTextbox()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        string endDay = "";
        Area_Timegate Timegate = player.playerAddonsConfigurator.myTimegate;

        if (Timegate)
        {
            int idx = player.playerAddonsConfigurator.GetTimegateIndexByName(Timegate.name);

            AddMessage(MSG_HEADING, Color.white);
            if (idx > -1 && Timegate.maxVisits != 0)
            {
                AddMessage(MSG_MAXVISITS + player.playerAddonsConfigurator.timegates[idx].count.ToString() + " / " + Timegate.maxVisits.ToString(), okVisits ? Color.green : Color.red);
            }
            else if (Timegate.maxVisits != 0)
            {
                AddMessage(MSG_MAXVISITS + "0 / " + Timegate.maxVisits.ToString(), okVisits ? Color.green : Color.red);
            }
            if (Timegate.hoursBetweenVisits != 0) AddMessage(MSG_MAXHOURS + Timegate.hoursBetweenVisits.ToString() + MSG_HOURS, okHours ? Color.green : Color.red);
            if (Timegate.dayEnd != 0) endDay = ". - " + Timegate.dayEnd.ToString() + ".";
            if (Timegate.dayStart != 0) AddMessage(MSG_OPENFROM + Timegate.dayStart.ToString() + endDay, okDayStart && okDayEnd ? Color.green : Color.red);
            if (Timegate.activeMonth != 0) AddMessage(MSG_OPENON + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Timegate.activeMonth), okMonth ? Color.green : Color.red);
        }
    }


    public void ShowHide(bool value)
    {
        Player player = Player.localPlayer;
        if (!player) return;
        if (player.playerAddonsConfigurator.myTimegate == null)
        {
            Hide();
        }
        else
        {
            if (value)
            {
                ValidateTimegate();
                Show();
            }
            else
            {
                Hide();
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // Show
    // -----------------------------------------------------------------------------------
    public void Show()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        for (int i = 0; i < content.childCount; ++i)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        Area_Timegate Timegate = player.playerAddonsConfigurator.myTimegate;

        if (Timegate)
        {
            ValidateTimegate();
            if (okTimegate) TimegateButton.GetComponentInChildren<TMP_Text>().text = MSG_TIMEGATE + Timegate.teleportationTarget.name;

            TimegateButton.interactable = okTimegate;

            TimegateButton.onClick.SetListener(() =>
            {
                Tools_UI_Tools.FadeOutScreen();
                player.playerAddonsConfigurator.Cmd_SimpleTimegate();
            });
        }

        UpdateTextbox();
        panel.SetActive(true);
    }

    // -----------------------------------------------------------------------------------
    // Hide
    // -----------------------------------------------------------------------------------
    public void Hide()
    {
        panel.SetActive(false);
    }

    // -----------------------------------------------------------------------------------
    // AutoScroll
    // -----------------------------------------------------------------------------------
    private void AutoScroll()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0;
    }

    // -----------------------------------------------------------------------------------
    // AddMessage
    // -----------------------------------------------------------------------------------
    public void AddMessage(string msg, Color color)
    {
        GameObject go = Instantiate(textTimegate);
        go.transform.SetParent(content.transform, false);
        go.GetComponent<TMP_Text>().text = msg;
        go.GetComponent<TMP_Text>().color = color;
        AutoScroll();
    }

}
