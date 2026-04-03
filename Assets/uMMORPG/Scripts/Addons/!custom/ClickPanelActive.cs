using UnityEngine;
using UnityEngine.UI;

public class ClickPanelActive : MonoBehaviour
{
	public GameObject Panel; // Reference to the UI element you want to activate

	private void OnMouseDown()
	{
		Panel.SetActive(true); // Activate the UI element when the game object is clicked
	}
}