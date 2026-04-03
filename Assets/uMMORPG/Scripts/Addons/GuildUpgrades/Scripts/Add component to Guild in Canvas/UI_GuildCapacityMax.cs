using UnityEngine;
using UnityEngine.UI;

public class UI_GuildCapacityMax : MonoBehaviour
{
    // Start is called before the first frame update
    public Text maximumCapacityText;

#if _iMMOGUILDUPGRADES
    // Update is called once per frame
    private void OnEnable()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            if (player.guild.guild.name == null)
            {
                player.playerGuildUpgrades.guildLevel = 0;
            }
            maximumCapacityText.text = player.playerGuildUpgrades.guildCapacity.ToString();
        }
    }
#endif
}