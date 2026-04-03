using Mirror;
using System.Collections;
using UnityEngine;

public class Doors : Tools_InteractableObject
{
	public enum DoorType
    {
		classicDoor,
		doubleDoor,
		portcullisDoor,
		drawbridge 

	}

	[Tooltip("[Optional] Sound played when opening.")]
	public AudioClip soundOpen;

	[Tooltip("[Optional] Sound played when closing.")]
	public AudioClip soundClose;

	[Header("[MESSAGES]")]
	[Tooltip("[Optional] Message shown to the player when the access requirements are not met.")]
	public string lockedMessage;

	[Tooltip("[Optional] Shown while opening the door.")]
	public string accessLabel;

	public bool showProgressBar = false;
	//private float closeTimer = 0;
	//[SyncVar] private bool lastOpen = false;
	[SyncVar, HideInInspector] public bool open = false;



	[Tooltip("Base duration (in seconds) it takes for the door to auto close.")]
	public float AutoCloseAfter;


	[Header("[-=-=-[ Select type Door ]-=-=-]")]
	public DoorType doorType;
	// this is for classic door
	[StringShowConditional(conditionFieldName: "doorType", conditionValue: "classicDoor")]
	public float degreesRotate = 90;

	// this is for double door
	[StringShowConditional(conditionFieldName: "doorType", conditionValue: "doubleDoor")]
	public float degreesRotateDoor1 = 90;
	[StringShowConditional(conditionFieldName: "doorType", conditionValue: "doubleDoor")]
	public float degreesRotateDoor2 = -90;

	// this is for herse door
	[StringShowConditional(conditionFieldName: "doorType", conditionValue: "portcullisDoor")]
	public float meterUpOrDown = 3;

	[StringShowConditional(conditionFieldName: "doorType", conditionValue: "drawbridge")]
	public float degreesRotateDrawbridge = 90;

	[Header("[-=-=-[ Seconds for open/close ]-=-=-]")]
	public float secondsToOpen = 1.0f;
	public float secondsToClose = 1.0f;

	[Header("[-=-=-[ Select door GameObject]")]
	public GameObject door;
	[StringShowConditional(conditionFieldName: "doorType", conditionValue: "doubleDoor")]
	public GameObject door2;

	
	protected float degreesClosed;
	protected float degreesOpened;
	protected float degreesClosed2;
	protected float degreesOpened2;

	public enum DoorState
	{
		closed,
		opening,
		opened,
		closing
	}
	[SyncVar, HideInInspector] public DoorState hingeState;

	//protected bool isPlayerNear;
	protected float timeStartedRotation;


	[ClientCallback]
	public override void OnInteractClient(Player player)
	{
#if _iMMODOORS
		player.playerAddonsConfigurator.OnSelect_Door(this);
#endif
	}

	// -----------------------------------------------------------------------------------
	// OnInteractServer
	// @Server
	// -----------------------------------------------------------------------------------
	[ServerCallback]
	public override void OnInteractServer(Player player)
	{
		if (hingeState == DoorState.closed || hingeState == DoorState.opened)
		{
			hingeState = (hingeState == DoorState.closed) ? DoorState.opening : DoorState.closing;
			timeStartedRotation = Time.time;
		}

		else if (hingeState == Doors.DoorState.closing || hingeState == Doors.DoorState.opening)
		{
			player.Tools_AddMessage("This door is currently in moovement !");
		}
	}

	// Use this for initialization
	public override void Start()
	{
		//onSelect.AddListener(OnSelect);
		base.Start();
		if (secondsToClose <= 0)
		{
			secondsToClose = 1.0f;
		}
		if (secondsToOpen <= 0)
		{
			secondsToOpen = 1.0f;
		}


		if (doorType == DoorType.doubleDoor)
		{
			degreesClosed = door.transform.eulerAngles.x;
			degreesOpened = degreesClosed + degreesRotateDoor1;
			degreesClosed2 = door2.transform.eulerAngles.x;
			degreesOpened2 = degreesClosed2 + degreesRotateDoor2;
		}
        else
        {
			degreesClosed = (doorType == DoorType.portcullisDoor) ? door.transform.position.y :
					(doorType == DoorType.drawbridge) ? door.transform.eulerAngles.x :
					door.transform.eulerAngles.y;
			degreesOpened = degreesClosed + (
								(doorType == DoorType.portcullisDoor) ? meterUpOrDown :
								(doorType == DoorType.drawbridge) ? degreesRotateDrawbridge :
								degreesRotate);
		}
	}

	// Update is called once per frame
	public void Update()
	{
		// Update rotation if in closing or opening state
		if (hingeState == DoorState.closing)
		{
			// Classic door
			if (doorType == DoorType.classicDoor && InterpolatepRotationY(door.transform, timeStartedRotation, secondsToClose, degreesOpened, degreesClosed))
			{
				hingeState = DoorState.closed;
			}
			// Double door
			else if (doorType == DoorType.doubleDoor && InterpolatepRotationY(door.transform, timeStartedRotation, secondsToOpen, degreesOpened, degreesClosed) && InterpolatepRotationY(door2.transform, timeStartedRotation, secondsToOpen, degreesOpened2, degreesClosed2))
			{
				hingeState = DoorState.closed;
			}
			// Portcullis Door
			else if (doorType == DoorType.portcullisDoor && InterpolatePositionY(door.transform, timeStartedRotation, secondsToClose, degreesOpened, degreesClosed))
			{
				hingeState = DoorState.closed;
			}
			// Drawbridge Door
			else if (doorType == DoorType.drawbridge && InterpolateRotationX(door.transform, timeStartedRotation, secondsToClose, degreesOpened, degreesClosed))
			{
				hingeState = DoorState.closed;
			}
		}
		else if (hingeState == DoorState.opening)
		{
			// Classic door
			if (doorType == DoorType.classicDoor && InterpolatepRotationY(door.transform, timeStartedRotation, secondsToOpen, degreesClosed, degreesOpened))
			{
				hingeState = DoorState.opened;
				if(AutoCloseAfter > 0)
                {
					StartCoroutine(autoClose());
                }
			}
			// Double door
			else if (doorType == DoorType.doubleDoor && InterpolatepRotationY(door.transform, timeStartedRotation, secondsToOpen, degreesClosed, degreesOpened) && InterpolatepRotationY(door2.transform, timeStartedRotation, secondsToOpen, degreesClosed2, degreesOpened2))
			{
				hingeState = DoorState.opened;
			}
			// Portcullis Door
			else if (doorType == DoorType.portcullisDoor && InterpolatePositionY(door.transform, timeStartedRotation, secondsToOpen, degreesClosed, degreesOpened))
			{
				hingeState = DoorState.opened;
			}
			// Drawbridge Door
			else if (doorType == DoorType.drawbridge && InterpolateRotationX(door.transform, timeStartedRotation, secondsToOpen, degreesClosed, degreesOpened))
			{
				hingeState = DoorState.opened;
			}
		}
	}


	// Returns true when rotation is complete
	bool InterpolatepRotationY(Transform trans, float timeStarted, float secondsDuration, float degreesStart, float degreesEnd)
	{
		float timeElapsed = Time.time - timeStarted;
		float interp = timeElapsed / secondsDuration;
		if (interp < 1.0f)
		{
			float degreesInterp = degreesStart + (degreesEnd - degreesStart) * interp;
			trans.eulerAngles = new Vector3(trans.transform.eulerAngles.x, degreesInterp, trans.transform.eulerAngles.z);
		}
		else
		{
			trans.eulerAngles = new Vector3(trans.transform.eulerAngles.x, degreesEnd, trans.transform.eulerAngles.z);
			return true;
		}
		return false;
	}

	bool InterpolateDoubleRotationY(Transform trans, Transform trans2, float timeStarted, float secondsDuration, float degreesStart, float degreesEnd, float degreesStart2, float degreesEnd2)
	{
		float timeElapsed = Time.time - timeStarted;
		float interp = timeElapsed / secondsDuration;
		if (interp < 1.0f)
		{
			float degreesInterp = degreesStart + (degreesEnd - degreesStart) * interp;
			float degreesInterp2 = degreesStart2 + (degreesEnd2 - degreesStart2) * interp;
			trans.eulerAngles = new Vector3(door.transform.eulerAngles.x, degreesInterp, door.transform.eulerAngles.z);
			//trans.position = new Vector3(door.transform.position.x, door.transform.position.y, door.transform.position.z);
			trans2.eulerAngles = new Vector3(door2.transform.eulerAngles.x, degreesInterp2, door2.transform.eulerAngles.z);
			//trans2.position = new Vector3(door2.transform.position.x, door2.transform.position.y, door2.transform.position.z);
		}
		else
		{
			trans.eulerAngles = new Vector3(door.transform.eulerAngles.x, degreesEnd, door.transform.eulerAngles.z);
			//trans.position = new Vector3(door.transform.position.x, door.transform.position.y, door.transform.position.z);
			trans2.eulerAngles = new Vector3(door2.transform.eulerAngles.x, degreesEnd2, door2.transform.eulerAngles.z);
			//trans2.position = new Vector3(door2.transform.position.x, door2.transform.position.y, door2.transform.position.z);
			return true;
		}
		return false;
	}

	bool InterpolatePositionY(Transform trans, float timeStarted, float secondsDuration, float degreesStart, float degreesEnd)
	{
		float timeElapsed = Time.time - timeStarted;
		float interp = timeElapsed / secondsDuration;
		if (interp < 1.0f)
		{
			float degreesInterp = degreesStart + (degreesEnd - degreesStart) * interp;
			trans.position = new Vector3(door.transform.position.x, degreesInterp, door.transform.position.z);
		}
		else
		{
			trans.position = new Vector3(door.transform.position.x, degreesEnd, door.transform.position.z);
			return true;
		}
		return false;
	}

	bool InterpolateRotationX(Transform trans, float timeStarted, float secondsDuration, float degreesStart, float degreesEnd)
	{
		float timeElapsed = Time.time - timeStarted;
		float interp = timeElapsed / secondsDuration;
		if (interp < 1.0f)
		{
			float degreesInterp = degreesStart + (degreesEnd - degreesStart) * interp;
			trans.eulerAngles = new Vector3(degreesInterp, door.transform.eulerAngles.y, door.transform.eulerAngles.z);
		}
		else
		{
			trans.eulerAngles = new Vector3(degreesEnd, door.transform.eulerAngles.y, door.transform.eulerAngles.z);
			return true;
		}
		return false;
	}


	IEnumerator autoClose()
    {
		yield return new WaitForSeconds(AutoCloseAfter);

		if (hingeState == DoorState.opened)
		{
			hingeState = DoorState.closing;
			timeStartedRotation = Time.time;
		}

	}
}