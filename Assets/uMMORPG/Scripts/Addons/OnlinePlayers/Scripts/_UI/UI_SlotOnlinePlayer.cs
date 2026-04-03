using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_SlotOnlinePlayer : MonoBehaviour
{
#if _iMMOPVP
    public Image factionImage;
#else
    public Image classStatusImage;
#endif
    public TMP_Text nameText;
    public TMP_Text levelText;
    public TMP_Text className;
#if _iMMOFRIENDS
    public Button friendButton;
#endif
    public Button tradingButton;
    public Button partyButton;
    public Button guildButton;
}
