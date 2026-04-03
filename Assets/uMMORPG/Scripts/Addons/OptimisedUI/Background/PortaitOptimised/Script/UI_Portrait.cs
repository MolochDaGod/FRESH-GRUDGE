using UnityEngine;
using UnityEngine.UI;

public class UI_Portrait : MonoBehaviour
{
    public GameObject panel;
    public Image image;

    private void OnEnable()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            panel.SetActive(true);
            image.sprite = player.portraitIcon;
        }
    }
}