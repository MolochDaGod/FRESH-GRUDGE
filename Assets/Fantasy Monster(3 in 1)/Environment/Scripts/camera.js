
private var maxPosZf:float;
private var minPosZf:float;

private var minPosYf:float;
private var maxPosYf:float;

public var thisCam:Camera;
public var CamLRSpeed:float = 1.0f;
	
function Start () 
	{
		if ( thisCam )
		{
			maxPosZf = thisCam.transform.localPosition.z*1.5f;
			minPosZf = maxPosZf*0.35f;
			
			maxPosYf = thisCam.transform.localPosition.y*2.0f;
			minPosYf = 0.0f;
		}
	}

function Update () 
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
					thisCam.transform.localPosition.z += Time.deltaTime*30.0f;	
				}
			}
			
		if (Input.GetKey(KeyCode.Home))
		{
			if ( thisCam.gameObject.transform.localPosition.z > minPosZf )
				{
					thisCam.transform.localPosition.z -= Time.deltaTime*30.0f;	
				}
		}
		
		if (Input.GetKey(KeyCode.Insert))
		{
			if ( thisCam.gameObject.transform.localPosition.y < maxPosYf )
				{
					thisCam.transform.localPosition.y += Time.deltaTime*5.0f;	
					thisCam.transform.Rotate(Vector3.right, 0.1f , UnityEngine.Space.Self);
				}
		}
		
		if (Input.GetKey(KeyCode.PageUp))
		{
			if ( thisCam.gameObject.transform.localPosition.y > minPosYf )
				{
					thisCam.transform.localPosition.y -= Time.deltaTime*5.0f;	
					thisCam.transform.Rotate(Vector3.right, -0.1f , UnityEngine.Space.Self);
				}
		}
	}
	

