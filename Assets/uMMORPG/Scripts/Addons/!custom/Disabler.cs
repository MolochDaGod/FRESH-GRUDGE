using UnityEngine;
using System.Collections;

public class Disabler : MonoBehaviour
{

	public Canvas[] canvases;


    void Update()
    {
		{
			if (Input.GetKeyDown(KeyCode.BackQuote))
			{
				Cursor.visible = false;
				HideCanvases();
			}
			else if (Input.GetKeyUp(KeyCode.BackQuote))
			{
				Cursor.visible = true;
				ShowCanvases();
			}
		}

		 void HideCanvases()
		{
			foreach (Canvas canvas in canvases)
			{
				canvas.enabled = false;
			}
		}

		 void ShowCanvases()
		{
			foreach (Canvas canvas in canvases)
			{
				canvas.enabled = true;
			}
		}
    }
}