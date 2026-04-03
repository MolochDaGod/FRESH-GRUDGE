using UnityEngine;
using UnityEngine.UI;

public class Compass : MonoBehaviour
{
    public GameObject panel;
    public RawImage compass;

    // Update is called once per frame
    void Update()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            panel.SetActive(true);
            float uvx = player.movement.transform.localEulerAngles.y;
            compass.uvRect = new Rect(uvx / 360f, 0, 1, 1);
        }
        else panel.SetActive(false);
    }
}
