using UnityEngine;
using System.Collections;

public class DisableAfter: MonoBehaviour {
	public GameObject overlay;
	public bool IsActive = false;
	public float disableTime=0.0f;

	void OnEnable()
	{
		//Start the coroutine after 5 secs
		// then wait 0 secs
		StartCoroutine(Overlay(disableTime)); 
	}

	IEnumerator Overlay(float DisableTime)
	{
		IsActive = true; //For outside checking if this is active
		while(true)
		{
			yield return new WaitForSeconds(DisableTime);
			//Deactivate the overlay 
			overlay.SetActive(false);
		}
	}
}