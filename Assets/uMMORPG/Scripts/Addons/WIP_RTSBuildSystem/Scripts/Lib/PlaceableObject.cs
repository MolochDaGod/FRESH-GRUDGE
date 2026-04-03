using Mirror;
using System;
using UnityEngine;

// PLACEABLE OBJECT
public partial class PlaceableObject : NetworkBehaviour
{
	[Tooltip("[Optional] Does destruction/death remove this object from the database?")]
	public bool destroyable;

	[Tooltip("[Optional] Can the object get picked-up again by its owner?")]
	public bool pickupable;

	[Tooltip("[Optional, Entity only] Each entry represents a upgrade that will increase this objects level")]
	public PlaceableObjectUpgradeCost[] upgradeable;
	[Tooltip("[Optional, Entity only] Each entry represents what the object will look like with each level. " +
		" Use only if using upgradeables" +
		" Note that the first object in this list will be what the first upgrade of this object looks like, " +
		" the current gameobject is what the very first level looks like.")]
	public Item_PlaceableObject[] objectStages;

	[HideInInspector, SyncVar] public bool permanent;
	[HideInInspector, SyncVar] public string ownerCharacter;
	[HideInInspector, SyncVar] public string ownerGuild;
	[HideInInspector, SyncVar] public string itemName;

	protected UI_PlaceableObject _UI_PlaceableObject;
	protected int _id;
	public bool claimArea;
	//server side object
	public PlaceableObject currentClaimFlag;
	[SyncVar]
	public long despawnTime;
	[SyncVar]
	public int extendedTimeInHours;
	[SyncVar]
	public int extendedTimeInHoursCap;

	// -----------------------------------------------------------------------------------
	// id
	// -----------------------------------------------------------------------------------
	public int id
	{
		get
		{
			if (_id == 0)
			{
				string sId = ownerCharacter;
				_id = sId.GetStableHashCode();
			}
			return _id;
		}
		set
		{
			_id = value;
		}
	}
	public override void OnStartServer()
	{
		base.OnStartServer();
		if (claimArea && despawnTime == 0)
			despawnTime = (DateTime.UtcNow + new TimeSpan(200, 0, 0)).Ticks;
	}
	//need to put an update method somewhere and NetworkManagerBuildSystem isn't a network item
	private void FixedUpdate()
	{
		if (isServer)
		{
			if (despawnTime != 0 && despawnTime <= DateTime.UtcNow.Ticks)
			{
				for (int i = 0; i < NetworkManagerMMO.singleton.GetComponent<NetworkManagerBuildSystem>().placeableObjects.Count; i++)
				{
					if (NetworkManagerMMO.singleton.GetComponent<NetworkManagerBuildSystem>().placeableObjects[i] == null)
						NetworkManagerMMO.singleton.GetComponent<NetworkManagerBuildSystem>().placeableObjects.RemoveAt(i);
				}
				NetworkManagerMMO.singleton.GetComponent<NetworkManagerBuildSystem>().placeableObjects.Remove(this);
				NetworkServer.Destroy(gameObject);

				foreach (var entry in Player.onlinePlayers)
					entry.Value.playerRTSBuildSystem.ReSyncPlaceableObjects();
			}
			if (currentClaimFlag == null)
			{
				if (despawnTime == 0)
				{
					despawnTime = (DateTime.UtcNow + new TimeSpan(200, 0, 0)).Ticks;
				}
			}
			if (despawnTime != 0 && currentClaimFlag != null)
			{
				//claim flag found, reset it
				despawnTime = 0;
			}
		}
	}

	// -----------------------------------------------------------------------------------
	// Update
	// @Client
	// -----------------------------------------------------------------------------------
	[ClientCallback]
	private void Update()
	{
		Player player = Player.localPlayer;
		if (!player) return;

		// -- check for click
		if (Input.GetKey(KeyCode.LeftShift) && UMMO_Tools.Tools_SelectionHandling(this.gameObject))
		{
			PlaceableObject po = this.gameObject.GetComponent<PlaceableObject>();

			if (po != null &&
				po.ownerCharacter == player.name
			)
			{
				player.playerRTSBuildSystem.myPlaceableObject = po;

				if (!_UI_PlaceableObject)
					_UI_PlaceableObject = FindObjectOfType<UI_PlaceableObject>();

				_UI_PlaceableObject.Show();
			}
		}
	}

	// -----------------------------------------------------------------------------------
	// OnDestroy
	// -----------------------------------------------------------------------------------
	[ServerCallback]
	private void OnDestroy()
	{
		if ((!destroyable && !pickupable) || (this.GetComponent<Entity>() != null && this.GetComponent<Entity>().isAlive))
			return;

		// -- gather object level if it is an entity

		int level = 0;

		Entity m = this.GetComponent<Entity>();

		if (m)
			level = m.level.current;

		int id = 0;

		PlaceableObject po = this.gameObject.GetComponent<PlaceableObject>();

		if (po)
			id = po.id;

		// -- delete from database
		Database.singleton.DeletePlaceableObject(ownerCharacter, ownerGuild, level, itemName, id);
	}

	// -----------------------------------------------------------------------------------
	// getUpgradeCost
	// -----------------------------------------------------------------------------------
	public PlaceableObjectUpgradeCost getUpgradeCost(int level)
	{
		if (upgradeable.Length > level - 1)
		{
			return upgradeable[level - 1];
		}

		return null;
	}

	// -----------------------------------------------------------------------------------
}
