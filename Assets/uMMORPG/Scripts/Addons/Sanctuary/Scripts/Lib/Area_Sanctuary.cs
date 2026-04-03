
using Mirror;
using UnityEngine;

// SIMPLE SANCTUARY
#if _iMMO2D
[RequireComponent(typeof(BoxCollider2D))]
#else
[RequireComponent(typeof(BoxCollider))]
#endif
public class Area_Sanctuary : NetworkBehaviour
{
    [Header("[-=-=-[ SANCTUARY ]-=-=-]")]
    [Tooltip("One click deactivation")]
    public bool isActive = true;

    [Tooltip("[Optional] Seconds spent offline to recover 1 Health")]
    public int SecondsPerHealth = 60;

    [Tooltip("[Optional] Seconds spent offline to recover 1 Mana")]
    public int SecondsPerMana = 60;

#if _iMMOSTAMINA
    [Tooltip("[Optional] Seconds spent offline to recover 1 stamina")]
    public int SecondsPerStamina = 60;
#endif

    [Tooltip("[Optional] Seconds spent offline to gain 1 Experience")]
    public int SecondsPerExp = 60;

    [Tooltip("[Optional] Seconds spent offline to gain 1 Skill Experience")]
    public int SecondsPerSkillExp = 60;

    [Tooltip("[Optional] Seconds spent offline to gain 1 Gold")]
    public int SecondsPerGold = 60;

    [Tooltip("[Optional] Seconds spent offline to gain 1 Coin")]
    public int SecondsPerCoins = 60;

    [Tooltip("[Optional] Max. Exp gain cap offline per session (set 0 to disable)")]
    public int MaxExp = 5;

    [Tooltip("[Optional] Max. Skill Exp gain cap offline per session (set 0 to disable)")]
    public int MaxSkillExp = 5;

    [Tooltip("[Optional] Max. Gold gain cap offline per session (set 0 to disable)")]
    public int MaxGold = 5;

    [Tooltip("[Optional] Max. Coins gain cap offline per session (set 0 to disable)")]
    public int MaxCoins = 5;

#if _iMMOHONORSHOP
    public Sanctuary_HonorCurrency[] honorCurrencies;
#endif

    [Header("[-=-=-[ Messages ]-=-=-]")]
    public string messageOnEnter = "You just entered a Sanctuary!";

    public string MSG_HEALTH = "[Sanctuary] Recovered health while being offline: ";
    public string MSG_MANA = "[Sanctuary] Recovered mana while being offline: ";
#if _iMMOSTAMINA
    public string MSG_STAMINA = "[Sanctuary] Recovered stamina while being offline: ";
#endif
    public string MSG_EXP = "[Sanctuary] Gained experience while being offline: ";
    public string MSG_SKILLEXP = "[Sanctuary] Gained skill experience while being offline: ";
    public string MSG_GOLD = "[Sanctuary] Earned gold while being offline: ";
    public string MSG_COINS = "[Sanctuary] Earned Coins while being offline: ";
#if _iMMOHONORSHOP
    public string MSG_CURRENCY = "[Sanctuary] Earned while being offline: ";
#endif

#if _SERVER
    // -----------------------------------------------------------------------------------
    // OnTriggerEnter
    // -----------------------------------------------------------------------------------
#if _iMMO2D
    private void OnTriggerStay2D(Collider2D co)
#else
    private void OnTriggerStay(Collider co)
#endif
    {
        Player playerConnect =  co.GetComponentInParent<Player>();
        if (playerConnect != null && Player.onlinePlayers.TryGetValue(playerConnect.name, out Player player) && isActive)
        {
            if (playerConnect.SecondsPassed > 0)
            {
                player.RecoverOverTime(this);
#if _iMMOTOOLS
                player.Tools_ShowPopup(messageOnEnter);
#endif
            }
        }
    }
#endif

}