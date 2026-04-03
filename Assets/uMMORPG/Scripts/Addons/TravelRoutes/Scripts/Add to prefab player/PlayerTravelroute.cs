using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// PLAYER

public class PlayerTravelroute : NetworkBehaviour
{
    public Player player;

    [Header("[-=-[ TRAVELROUTES ]-=-]")]
    public Tools_PopupClass travelroutePopup;

    [HideInInspector] public AreaBox_Travelroute myTravelrouteArea;
    public readonly SyncList<TravelrouteClass> travelroutes = new SyncList<TravelrouteClass>();

    protected UI_Travelroute instance;

    private void Start()
    {
        //instance = FindFirstObjectByType<UI_Travelroute>();
        instance = FindObjectOfType<UI_Travelroute>();
    }


    public void ShowTravelRoute()
    {
        instance.Show();
    }
    public void HideTravelRoute()
    {
        instance.Hide();
    }


    // -----------------------------------------------------------------------------------
    // UnlockTravelroutes
    // @Server
    // -----------------------------------------------------------------------------------
    [ServerCallback]
    public void UnlockTravelroutes()
    {
        foreach (Unlockroute route in myTravelrouteArea.Unlockroutes)
            UnlockTravelroute(route);
    }

    // -----------------------------------------------------------------------------------
    // UnlockTravelroute
    // @Server
    // -----------------------------------------------------------------------------------
    public void UnlockTravelroute(Unlockroute unlockroute)
    {
        if (unlockroute == null) return;

        string name = "";
        bool pass = true;

        // ------------------------------------------------------------------ get Name

        if (unlockroute.teleportationTarget.Valid)
        {
            name = unlockroute.teleportationTarget.name;
        }

        // ------------------------------------------------------------------ Validate Name
        if (!travelroutes.Any(t => t.name == name))
        {
            // -- validate and unlock
            if (pass && !string.IsNullOrWhiteSpace(name))
            {
                TravelrouteClass tRoute = new TravelrouteClass(name);
                travelroutes.Add(tRoute);
                player.experience.current += unlockroute.ExpGain;
                string msg = travelroutePopup.message + " " + name + travelroutePopup.suffix;
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // Cmd_WarpTravelroute
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_WarpTravelroute(int index)
    {
        if (myTravelrouteArea)
        {
            int price = 0;
            List<Travelroute> travelroutesAvailable = TravelroutesVisibleForPlayer();
            Travelroute targetTravelroute = travelroutesAvailable[index];
            string name = "";

            if (targetTravelroute.teleportationTarget.Valid)
                name = targetTravelroute.teleportationTarget.name;

            if (!string.IsNullOrWhiteSpace(name) && travelroutes.Any(t => t.name == name))
            {
#if _iMMOHONORSHOP
                if (!CheckTravelHonorCost(index)) return;
                PayTravelHonorCost(index);
#endif
                price = GetTravelCost(index);

                if (player.gold >= price)
                {
                    player.gold -= price;

                    targetTravelroute.teleportationTarget.OnTeleport(player);
                }
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // CheckTravelHonorCost
    // -----------------------------------------------------------------------------------
#if _iMMOHONORSHOP

    public bool CheckTravelHonorCost(int index)
    {
        bool valid = false;

        if (myTravelrouteArea)
        {
            List<Travelroute> travelroutesAvailable = TravelroutesVisibleForPlayer();
            Travelroute targetTravelroute = travelroutesAvailable[index];

            if (targetTravelroute != null)
            {
                if (targetTravelroute.currencyCost.Length == 0) return true;

                foreach (HonorShopCurrencyCost currency in targetTravelroute.currencyCost)
                {
                    int price = (int)(targetTravelroute.teleportationTarget.getDistance(myTravelrouteArea.transform) * currency.amount);

                    if (player.playerHonorShop.GetHonorCurrency(currency.honorCurrency) >= price)
                    {
                        valid = true;
                    }
                    else
                    {
                        valid = false;
                        break;
                    }
                }
            }
        }

        return valid;
    }

#endif

    // -----------------------------------------------------------------------------------
    // PayTravelHonorCost
    // -----------------------------------------------------------------------------------
#if _iMMOHONORSHOP

    public void PayTravelHonorCost(int index)
    {
        if (myTravelrouteArea)
        {
            List<Travelroute> travelroutesAvailable = TravelroutesVisibleForPlayer();
            Travelroute targetTravelroute = travelroutesAvailable[index];

            if (targetTravelroute != null)
            {
                foreach (HonorShopCurrencyCost currency in targetTravelroute.currencyCost)
                {
                    int price = (int)(targetTravelroute.teleportationTarget.getDistance(myTravelrouteArea.transform) * currency.amount);

                    player.playerHonorShop.AddHonorCurrency(currency.honorCurrency, price * -1);
                }
            }
        }
    }

#endif

    // -----------------------------------------------------------------------------------
    // GetTravelCost
    // -----------------------------------------------------------------------------------
    public int GetTravelCost(int index)
    {
        int price = 0;

        if (myTravelrouteArea)
        {
            List<Travelroute> travelroutesAvailable = TravelroutesVisibleForPlayer();
            Travelroute targetTravelroute = travelroutesAvailable[index];

            if (targetTravelroute != null && targetTravelroute.GoldPricePerUnit > 0)
                price = (int)(targetTravelroute.teleportationTarget.getDistance(myTravelrouteArea.transform) * targetTravelroute.GoldPricePerUnit);
        }

        return price;
    }

    // -----------------------------------------------------------------------------------
    // TravelroutesVisibleForPlayer
    // -----------------------------------------------------------------------------------
    public List<Travelroute> TravelroutesVisibleForPlayer()
    {
        return (from travelroute in myTravelrouteArea.Travelroutes
                where travelroutes.Any(t => travelroute.teleportationTarget.Valid && t.name == travelroute.teleportationTarget.name) // || t.name == travelroute.routeName)
                select travelroute).ToList();
    }

    // -----------------------------------------------------------------------------------
}
