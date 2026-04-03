using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnMobile : MonoBehaviour
{
	void Start()
	{
		// Check if the current platform is Android
		if (Application.platform == RuntimePlatform.Android)
		{
			// Destroy the game object this script is attached to
			Destroy(this.gameObject);
		}
	}
}