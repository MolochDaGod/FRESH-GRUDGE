using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasController : MonoBehaviour
{
	public Canvas[] canvases;


	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.W))
		{

			HideCanvases();
		}
		else if (Input.GetKeyUp(KeyCode.W))
		{

			ShowCanvases();
		}
	}

	private void HideCanvases()
	{
		foreach (Canvas canvas in canvases)
		{
			canvas.enabled = false;
		}
	}

	private void ShowCanvases()
	{
		foreach (Canvas canvas in canvases)
		{
			canvas.enabled = true;
		}
	}
}