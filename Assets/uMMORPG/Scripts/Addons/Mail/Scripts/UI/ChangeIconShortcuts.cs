using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// SHORTCUTS
public partial class ChangeIconShortcuts : MonoBehaviour
{
    public Image mailButtonImage;

    public Sprite mailRead;
    public Sprite mailUnread;


    [Header("[-=-[ Refresh Time ]-=-]")]
    public float refreshSpeed = .2f;

    public void Start()
    {
        StartCoroutine(CR_SMailShortcuts());
    }
    private IEnumerator CR_SMailShortcuts()
    {
        while (true)
        {
            UpdateMailShortcuts();
            yield return new WaitForSeconds(refreshSpeed);

        }
    }
    // -----------------------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------------------
    public void UpdateMailShortcuts()
    {
        Player player = Player.localPlayer;
        if (player == null) return;
        if (player.playerAddonsConfigurator && player.playerAddonsConfigurator.mailSettings != null)
        {
            if (player.playerAddonsConfigurator.mailSettings.forceUseNpc) return;

            if (player.playerAddonsConfigurator.UnreadMailCount() > 0)
                mailButtonImage.sprite = mailUnread;
            else
                mailButtonImage.sprite = mailRead;
        }
    }

    // -----------------------------------------------------------------------------------
}