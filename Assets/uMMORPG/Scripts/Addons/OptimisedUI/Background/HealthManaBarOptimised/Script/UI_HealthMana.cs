using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(GameEventListener))]
public class UI_HealthMana : MonoBehaviour
{
    public GameObject panel;
    public Slider healthSlider;
    public TMP_Text healthStatus;
    public Slider manaSlider;
    public TMP_Text manaStatus;
#if _iMMOSTAMINA
    public Slider staminaSlider;
    public Text staminaStatus;
#endif
    public TMP_Text levelText;

    public float refreshSpeed = 0.1f;

    private void OnEnable()
    {
        UpdateEvent();
    }

    public void UpdateEvent()
    {
        Player player = Player.localPlayer;
        if (player != null)
        {
            panel.SetActive(true);
            levelText.text = player.level.current.ToString();

            healthSlider.value = player.health.Percent();
            healthStatus.text = player.health.current + " / " + player.health.max;

            manaSlider.value = player.mana.Percent();
            manaStatus.text = player.mana.current + " / " + player.mana.max;
#if _iMMOSTAMINA
            staminaSlider.value = player.stamina.Percent();
            staminaStatus.text = player.stamina.current + " / " + player.stamina.max;
#endif
        }
        else panel.SetActive(false);
    }
}
