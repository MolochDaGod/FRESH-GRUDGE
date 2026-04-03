using UnityEngine;
using UnityEngine.UI;

public class MainPanel : MonoBehaviour
{
    public KeyCode hotKey = KeyCode.C;
    public GameObject panel;

    void Update()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            // hotkey (not while typing in chat, etc.)
            if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
                panel.SetActive(!panel.activeSelf);
        }
    }
}