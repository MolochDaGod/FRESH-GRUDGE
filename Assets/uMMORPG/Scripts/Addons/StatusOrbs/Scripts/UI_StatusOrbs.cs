using UnityEngine;
using UnityEngine.UI;

public class UI_StatusOrbs : MonoBehaviour
{
    [Header("[-=-=-[ Status Orbs ]-=-=-]")]
    [SerializeField] private bool showOrbs = true;

    [SerializeField] private GameObject panel = null;
    [SerializeField] private Text healthOrbText = null, manaOrbText = null;
    [SerializeField] private Image healthOrbImage = null, manaOrbImage = null;

    private void Update()
    {
        // Grab our local player.
        Player player = Player.localPlayer;

        // If our player is found and we're showing orbs.
        // Then update our health and mana for text and the images.
        if (player != null && showOrbs)
        {
            panel.SetActive(true);
            healthOrbText.text = player.health.current + " / " + player.health.max;
            manaOrbText.text = player.mana.current + " / " + player.mana.max;
            healthOrbImage.fillAmount = player.health.Percent();
            manaOrbImage.fillAmount = player.mana.Percent();
        }
        else panel.SetActive(false);
    }
}