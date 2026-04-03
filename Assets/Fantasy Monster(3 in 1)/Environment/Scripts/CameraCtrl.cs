using UnityEngine;
using System.Collections;

public class CameraCtrl : MonoBehaviour {

	private float maxPosZf;
	private float minPosZf;

	private float minPosYf;
	private float maxPosYf;

	public Camera thisCam;
	public float CamLRSpeed = 1.0f;

	// Use this for initialization
	void Start () 
	{
		if ( thisCam )
		{
			maxPosZf = thisCam.transform.localPosition.z*2.0f;
			minPosZf = maxPosZf*0.01f;

			maxPosYf = thisCam.transform.localPosition.y*2.0f;
			minPosYf = 0.0f;
		}

	}

	// Update is called once per frame
	void Update () 
	{
		if ( Input.GetKey(KeyCode.Delete))
		{
			transform.Rotate( 0,Time.deltaTime*30 *CamLRSpeed,0f);
		}

		if (Input.GetKey(KeyCode.PageDown))
		{
			transform.Rotate(0,Time.deltaTime*-30 *CamLRSpeed,0f);
		}

		if (Input.GetKey(KeyCode.End))
		{
			if ( thisCam.gameObject.transform.localPosition.z < maxPosZf )
			{
				Vector3 tempValu = new Vector3(0.0f,0.0f,0.0f);
				tempValu.z = Time.deltaTime * 30.0f;

				thisCam.transform.localPosition += tempValu;	
			}
		}

		if (Input.GetKey(KeyCode.Home))
		{
			if ( thisCam.gameObject.transform.localPosition.z > minPosZf )
			{
				Vector3 tempValu = new Vector3(0.0f,0.0f,0.0f);
				tempValu.z = Time.deltaTime * 30.0f;

				thisCam.transform.localPosition -= tempValu;	
			}
		}

		if (Input.GetKey(KeyCode.Insert))
		{
			if ( thisCam.gameObject.transform.localPosition.y < maxPosYf )
			{
				Vector3 tempValu = new Vector3(0.0f,0.0f,0.0f);
				tempValu.y = Time.deltaTime * 6.0f;

				thisCam.transform.localPosition += tempValu;	
				thisCam.transform.Rotate(Vector3.right, 0.15f , UnityEngine.Space.Self);
			}
		}

		if (Input.GetKey(KeyCode.PageUp))
		{
			if ( thisCam.gameObject.transform.localPosition.y > minPosYf )
			{
				Vector3 tempValu = new Vector3(0.0f,0.0f,0.0f);
				tempValu.y = Time.deltaTime * 6.0f;

				thisCam.transform.localPosition -= tempValu;	
				thisCam.transform.Rotate(Vector3.right, -0.15f , UnityEngine.Space.Self);
			}
		}

	}
}
