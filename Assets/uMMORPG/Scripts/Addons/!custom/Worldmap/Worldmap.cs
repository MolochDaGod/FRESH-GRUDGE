using UnityEngine;
using UnityEngine.UI;

public class Worldmap : MonoBehaviour
{
	[Header("Worldmap")]
	public KeyCode hotKey = KeyCode.M; // Key used to open the map.
	public GameObject worldmap;
	public GameObject worldmapcam;

	private void Update()
	{
		// hotkey (not while typing in chat, etc.)
		if ((Input.GetKeyDown(hotKey) || (Input.GetKeyDown(KeyCode.Escape) && worldmap.activeSelf && worldmapcam.activeSelf)) && !UIUtils.AnyInputActive())
		{
			// If the hotkey is pressed or escape key is pressed while the worldmap is active and input is not active, continue.
			worldmap.SetActive(!worldmap.activeSelf); // Open/close the worldmap.
			worldmapcam.SetActive(!worldmapcam.activeSelf); // enable/disable the worldmapcam.
		}
	}
}