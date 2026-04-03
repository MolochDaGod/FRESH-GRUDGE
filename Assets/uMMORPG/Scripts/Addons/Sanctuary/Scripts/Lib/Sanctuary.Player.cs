using System;
using UnityEngine;

// PLAYER
[RequireComponent(typeof(PlayerSkills))]
public partial class Player
{
    [HideInInspector] public double SecondsPassed;
#if _SERVER
    // -----------------------------------------------------------------------------------
    // RecoverOverTime
    // -----------------------------------------------------------------------------------
    public void RecoverOverTime(Area_Sanctuary sanctuary)
    {
        if (sanctuary == null) return;

        if (SecondsPassed > 0 && 
            (sanctuary.SecondsPerHealth > 0 || 
            sanctuary.SecondsPerMana > 0 ||
#if _iMMOSTAMINA
            sanctuary.SecondsPerStamina > 0 ||
#endif
            sanctuary.SecondsPerExp > 0 || 
            sanctuary.SecondsPerSkillExp > 0 || 
            sanctuary.SecondsPerGold > 0 || 
            sanctuary.SecondsPerCoins > 0)
            )
        {
            int gainHealth      = 0;
            int gainMana        = 0;
#if _iMMOSTAMINA
            int gainStamina     = 0;
#endif
            int gainExp         = 0;
            int gainSkillExp    = 0;
            int gainGold        = 0;
            int gainCoins       = 0;

            if (sanctuary.SecondsPerHealth > 0)     gainHealth = Mathf.Max(0, Convert.ToInt32(SecondsPassed / sanctuary.SecondsPerHealth));
            if (sanctuary.SecondsPerMana > 0)       gainMana = Mathf.Max(0, Convert.ToInt32(SecondsPassed / sanctuary.SecondsPerMana));
#if _iMMOSTAMINA
            if (sanctuary.SecondsPerStamina > 0)    gainStamina = Mathf.Max(0, Convert.ToInt32(SecondsPassed / sanctuary.SecondsPerStamina));
#endif
            if (sanctuary.SecondsPerSkillExp > 0)   gainSkillExp = sanctuary.MaxSkillExp > 0 ? Mathf.Clamp(Convert.ToInt32(SecondsPassed / sanctuary.SecondsPerSkillExp), 0, sanctuary.MaxSkillExp) : Convert.ToInt32(SecondsPassed / sanctuary.SecondsPerSkillExp);
            if (sanctuary.SecondsPerExp > 0)        gainExp = sanctuary.MaxExp > 0 ? Mathf.Clamp(Convert.ToInt32(SecondsPassed / sanctuary.SecondsPerExp), 0, sanctuary.MaxExp) : Convert.ToInt32(SecondsPassed / sanctuary.SecondsPerExp);
            if (sanctuary.SecondsPerGold > 0)       gainGold = sanctuary.MaxGold > 0 ? Mathf.Clamp(Convert.ToInt32(SecondsPassed / sanctuary.SecondsPerGold), 0, sanctuary.MaxGold) : Convert.ToInt32(SecondsPassed / sanctuary.SecondsPerGold);
            if (sanctuary.SecondsPerCoins > 0)      gainCoins = sanctuary.MaxCoins > 0 ? Mathf.Clamp(Convert.ToInt32(SecondsPassed / sanctuary.SecondsPerCoins), 0, sanctuary.MaxCoins) : Convert.ToInt32(SecondsPassed / sanctuary.SecondsPerCoins);

            if (gainHealth > 0 && health.current < health.max)
            {
                health.current += gainHealth;
                Tools_TargetAddMessage(sanctuary.MSG_HEALTH + gainHealth.ToString());
            }

            if (gainMana > 0 && mana.current < mana.max)
            {
                mana.current += gainMana;
                Tools_TargetAddMessage(sanctuary.MSG_MANA + gainMana.ToString());
            }

#if _iMMOSTAMINA
            if (gainStamina > 0 && stamina.current < stamina.max)
            {
                stamina.current += gainStamina;
                Tools_TargetAddMessage(sanctuary.MSG_STAMINA + gainStamina.ToString());
            }
#endif

            if (gainExp > 0)
            {
                experience.current += gainExp;
                Tools_TargetAddMessage(sanctuary.MSG_EXP + gainExp.ToString());
            }

            if (gainSkillExp > 0)
            {
                ((PlayerSkills)skills).skillExperience
                    += gainSkillExp;
                Tools_TargetAddMessage(sanctuary.MSG_SKILLEXP + gainSkillExp.ToString());
            }

            if (gainGold > 0)
            {
                gold += gainGold;
                Tools_TargetAddMessage(sanctuary.MSG_GOLD + gainGold.ToString());
            }

            if (gainCoins > 0)
            {
                itemMall.coins += gainCoins;
                Tools_TargetAddMessage(sanctuary.MSG_COINS + gainCoins.ToString());
            }

#if _iMMOHONORSHOP
            foreach (Sanctuary_HonorCurrency currency in sanctuary.honorCurrencies)
            {
                if (currency.SecondsPerUnit > 0 && currency.honorCurrency != null)
                {
                    int gainCurrency = currency.MaxUnits > 0 ? Mathf.Clamp(Convert.ToInt32(SecondsPassed / currency.SecondsPerUnit), 0, currency.MaxUnits) : Convert.ToInt32(SecondsPassed / currency.SecondsPerUnit);

                    if (gainCurrency > 0)
                    {
                        playerHonorShop.AddHonorCurrency(currency.honorCurrency, gainCurrency);
                        Tools_TargetAddMessage(sanctuary.MSG_CURRENCY + gainCurrency.ToString() + " " + currency.honorCurrency.name);
                    }
                }
            }
#endif
            SecondsPassed = 0;
        }
    }
#endif
}