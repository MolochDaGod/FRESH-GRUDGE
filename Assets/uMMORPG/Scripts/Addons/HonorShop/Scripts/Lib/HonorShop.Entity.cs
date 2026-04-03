using UnityEngine;

// HONOR SHOP ENTITY
public partial class Entity
{
    protected const string MSG_GAINED = "Gained ";

    [Header("[-=-[ Entity : Honor Rewards ]-=-]")]
    public HonorShopCurrencyDrop[] currencyDrops;
    
    // -----------------------------------------------------------------------------------
    // OnDeath_HonorShop
    // -----------------------------------------------------------------------------------
    public void OnDeath_HonorShop()
    {
        if (currencyDrops.Length > 0)
        {
            if (lastAggressor != null && lastAggressor is Player)
            {
                Player player = (Player)lastAggressor;
                long amount = 0;
                bool assigned = false;

                foreach (HonorShopCurrencyDrop currencyDrop in currencyDrops)
                {
                    if (currencyDrop.honorCurrency != null && currencyDrop.honorCurrency.dropRequirements.checkRequirements(player))
                    {
#if _iMMOPVP
                        if (currencyDrop.honorCurrency.FromHostileRealmsOnly && GetAlliedRealms(player))
                            return;
#endif
                        amount = currencyDrop.amount;
                        assigned = false;

                        if (currencyDrop.honorCurrency.perLevel)
                            amount *= level.current;

                        // -- share to party
                        if (player.party.InParty() && currencyDrop.honorCurrency.shareWithParty)
                        {
                            player.playerHonorShop.HonorCurrency_ShareToParty(currencyDrop.honorCurrency, amount);
                            assigned = true;
                        }

                        // -- share to guild
                        if (player.guild.InGuild() && currencyDrop.honorCurrency.shareWithGuild)
                        {
                            player.playerHonorShop.HonorCurrency_ShareToGuild(currencyDrop.honorCurrency, amount);
                            assigned = true;
                        }

#if _iMMOPVP
                        // -- share to realm
                        if (currencyDrop.honorCurrency.shareWithRealm)
                        {
                            player.playerHonorShop.HonorCurrency_ShareToRealm(currencyDrop.honorCurrency, amount);
                            assigned = true;
                        }
#endif

                        // -- if we came this far, we add it to the player directly
                        if (!assigned)
                        {
                            player.playerHonorShop.AddHonorCurrency(currencyDrop.honorCurrency, amount);
                            player.Tools_TargetAddMessage(MSG_GAINED + currencyDrop.honorCurrency.name + " x" + amount.ToString());
                        }
                    }
                }
            }
        }
    }

    // -----------------------------------------------------------------------------------
}