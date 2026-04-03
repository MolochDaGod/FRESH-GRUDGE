using UnityEngine;
using UnityEngine.UI;

public class UI_ShowLocation : MonoBehaviour
{
    public GameObject locationPanel;
	public Text locationText;

    void Update()
	{
		Player player = Player.localPlayer;
		if (!player)
		{
			locationPanel.SetActive(false);
			return;
		}
		locationPanel.SetActive(true);
#if _iMMO2D
		locationText.text = player.transform.position.x.ToString("F0") + " (x) / " + player.transform.position.y.ToString("F0") + " (y)";
#else
		locationText.text = player.transform.position.x.ToString("F0") + " (x) / " + player.transform.position.y.ToString("F0") + " (y) / " + player.transform.position.z.ToString("F0") + " (z) ";
#endif
	}
}