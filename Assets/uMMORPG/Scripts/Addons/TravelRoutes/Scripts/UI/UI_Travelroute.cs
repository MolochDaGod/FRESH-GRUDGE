using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ===================================================================================
// TRAVELROUTE UI
// ===================================================================================
public partial class UI_Travelroute : MonoBehaviour
{
    public GameObject panel;
    public Slot_Travelroute slotPrefab;
    public Transform content;

    public const string CURRENCY_LABEL = " Gold ";
    public const string TRAVEL_LABEL = "Travel ";

    // -----------------------------------------------------------------------------------
    // Show
    // @Client
    // -----------------------------------------------------------------------------------
    public void Show()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        List<Travelroute> travelroutesAvailable = player.playerTravelroute.TravelroutesVisibleForPlayer();
        UIUtils.BalancePrefabs(slotPrefab.gameObject, travelroutesAvailable.Count, content);

        for (int i = 0; i < travelroutesAvailable.Count; ++i)
        {
            bool valid = true;
            int price = 0;
            int index = i;
            Slot_Travelroute slot = content.GetChild(i).GetComponent<Slot_Travelroute>();
            string realmstring = "";
            string name = "";

#if _iMMOHONORSHOP
            valid = player.playerTravelroute.CheckTravelHonorCost(index);
#endif

            if (travelroutesAvailable[i].teleportationTarget.Valid)
            {
                price = player.playerTravelroute.GetTravelCost(i);
                name = travelroutesAvailable[i].teleportationTarget.name;
            }

            slot.descriptionText.text = name;

            if (player.gold >= price && valid)
            {
                slot.actionButton.interactable = true;
            }
            else
            {
                slot.actionButton.interactable = false;
            }

            if (price > 0)
            {
                slot.actionButton.GetComponentInChildren<Text>().text = price + CURRENCY_LABEL + realmstring;
            }
            else
            {
                slot.actionButton.GetComponentInChildren<Text>().text = TRAVEL_LABEL + realmstring;
            }

            slot.actionButton.onClick.SetListener(() =>
            {
                player.playerTravelroute.Cmd_WarpTravelroute(index);
                panel.SetActive(false);
            });
        }

        panel.SetActive(true);
    }

    // -----------------------------------------------------------------------------------
    // Hide
    // @Client
    // -----------------------------------------------------------------------------------
    public void Hide()
    {
        panel.SetActive(false);
    }

    // -----------------------------------------------------------------------------------
}
