using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnWindows : MonoBehaviour
{
	void Start()
	{
		// Check if the current platform is Windows
		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
		{
			// Destroy the game object this script is attached to
			Destroy(this.gameObject);
		}
	}
}