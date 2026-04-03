using UnityEngine;
using UnityEngine.UI;

public partial class UIStamina : MonoBehaviour
{
    public GameObject panel;
    public Slider staminaSlider;
    public Text staminaStatus;
#if _iMMOSTAMINA
    void Update()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            panel.SetActive(true);

            staminaSlider.value = player.stamina.Percent();
            staminaStatus.text = player.stamina.current + " / " + player.stamina.max;
        }
    }
#endif
}
