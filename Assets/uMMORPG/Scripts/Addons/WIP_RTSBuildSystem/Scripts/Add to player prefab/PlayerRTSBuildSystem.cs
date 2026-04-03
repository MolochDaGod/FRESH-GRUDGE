using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerRTSBuildSystem : NetworkBehaviour
{

	public Player player;
	public Combat combat;



	[HideInInspector] public Item_PlaceableObject myPlaceableObjectItem;
	protected UI_PlaceableObjectSpawn _UI_PlaceableObjectSpawn;
	protected int myPlaceableObjectAreaId;
	protected bool myPlaceableObjectAreaOwner;
	protected bool myPlaceableObjectAreaGuild;
	[HideInInspector] public PlaceableObject myPlaceableObject;

	public GameObject buildSystem_PreviewObject;
	protected GameObject buildSystem_GridObject;
	float mouseWheelRotation;
	Vector3 structurePosition;
	Vector3 structureScale;
	Quaternion structureRotation;
	WaitForSeconds wfs = new WaitForSeconds(1);
	SyncList<PlaceableObject> placedClaimFlags = new SyncList<PlaceableObject>();

	private const string MSG_PLACEABLEOBJECT_UPGRADE = "Upgrading...";

    // Start is called before the first frame update
    private PanelManager panelManager; // Reference to the PanelManager

    void Start()
    {
        //        combat.onClientReceivedDamage.AddListener(OnDamageDealt_UCE_cancelStartSpawnPlaceableObject);
        //        combat.onDamageDealtTo.AddListener(OnDamageDealt_UCE_cancelStartSpawnPlaceableObject);

        // Find the PanelManager in the scene
        panelManager = FindObjectOfType<PanelManager>();

        if (panelManager == null)
        {
            Debug.LogError("PanelManager not found in the scene.");
        }
    }
	public override void OnStartServer()
	{
		base.OnStartServer();
		//need the below for claim areas since the network manager isn't a network object
		for (int i = 0; i < NetworkManagerMMO.singleton.GetComponent<NetworkManagerBuildSystem>().placeableObjects.Count; i++)
			if (NetworkManagerMMO.singleton.GetComponent<NetworkManagerBuildSystem>().placeableObjects[i].claimArea)
				placedClaimFlags.Add(NetworkManagerMMO.singleton.GetComponent<NetworkManagerBuildSystem>().placeableObjects[i]);
	}
	public void ReSyncPlaceableObjects()
	{
		StartCoroutine(ReSyncCR());
	}
	IEnumerator ReSyncCR()
	{
		yield return wfs;
		placedClaimFlags.Clear();
		for (int i = 0; i < NetworkManagerMMO.singleton.GetComponent<NetworkManagerBuildSystem>().placeableObjects.Count; i++)
			if (NetworkManagerMMO.singleton.GetComponent<NetworkManagerBuildSystem>().placeableObjects[i].claimArea)
				placedClaimFlags.Add(NetworkManagerMMO.singleton.GetComponent<NetworkManagerBuildSystem>().placeableObjects[i]);
	}

	// -----------------------------------------------------------------------------------
	// OnDestroy_UCE_RTSBuildSystem
	// -----------------------------------------------------------------------------------
	//[DevExtMethods("OnDestroy")]
	private void OnDestroy_UCE_RTSBuildSystem()
	{
		CancelSpawnPlaceableObject();
	}

	// ===================================== CANCEL ======================================

	// -----------------------------------------------------------------------------------
	// OnDamageDealt_UCE_cancelStartSpawnPlaceableObject
	// Custom Hook
	// -----------------------------------------------------------------------------------
	//[DevExtMethods("OnDamageDealt")]
	private void OnDamageDealt_UCE_cancelStartSpawnPlaceableObject()
	{
		myPlaceableObjectItem = null;
		player.Tools_removeTask();
	}

	// ===================================== LATE UPDATE =================================

	// -----------------------------------------------------------------------------------
	// LateUpdate_UCE_SpawnPlaceableObject
	// @Client
	// -----------------------------------------------------------------------------------
	[ClientCallback]
	private void LateUpdate()
	{
		if (SpawnPlaceableObjectValidation())
		{
			if ((myPlaceableObject || myPlaceableObjectItem) && player.Tools_checkTimer())
			{
				if (myPlaceableObject != null)
				{
					Cmd_FinishUpgradePlaceableObject();
				}
				else if (myPlaceableObjectItem != null)
				{
					Cmd_FinishSpawnPlaceableObject();
				}

				CancelSpawnPlaceableObject();
			}
			if (Input.GetKeyDown(player.cancelActionKey))
			{
				CancelSpawnPlaceableObject();
			}
		}
	}

	// -----------------------------------------------------------------------------------
	// SpawnPlaceableObjectValidation
	// -----------------------------------------------------------------------------------
	public bool SpawnPlaceableObjectValidation()
	{
		if ((myPlaceableObject != null || myPlaceableObjectItem != null) && player.canInteract)
		{
			return true;
		}

		if ((myPlaceableObject || myPlaceableObjectItem))
		{
			CancelSpawnPlaceableObject();
		}

		return false;
	}

	// ===================================== UNSPAWN =====================================

	// -----------------------------------------------------------------------------------
	// Cmd_UnspawnPlaceableObject
	// @Client -> @Server
	// -----------------------------------------------------------------------------------
	[Command]
	public void Cmd_UnspawnPlaceableObject(NetworkIdentity ni)
	{
		if (ni != null)
		{
			PlaceableObject po = ni.GetComponent<PlaceableObject>();
			Entity e = po.GetComponent<Entity>();

			if (po && po.pickupable && po.ownerCharacter == this.name)
			{
				player.movement.LookAtY(po.gameObject.transform.position);

				ScriptableItem item;

				if (ScriptableItem.All.TryGetValue(po.itemName.GetStableHashCode(), out item))
				{
					// -- enough inventory space?

					Item result = new Item(item);

					if (player.inventory.CanAdd(result, ((Item_PlaceableObject)item).decreaseAmount))
					{
						UnspawnPlacableObject(po);
						// -- delete object from database
						//Database.singleton.DeletePlaceableObject(player.name, player.guild.guild.name, e.level.current, po.itemName, po.id);

						// -- add it back to the inventory again
						player.inventory.Add(new Item(item), ((Item_PlaceableObject)item).decreaseAmount);

						// -- remove the object
						//NetworkServer.Destroy(po.gameObject);
					}
				}
			}
		}
	}
	[Command]
	public void CmdExtendDespawnTime(int index, PlaceableObject po)
	{
		// validate: close enough, npc alive and valid index and valid item?
		// use collider point(s) to also work with big entities
		if (player.state == "IDLE" &&
			po != null &&
			po.despawnTime > 0 &&
			0 <= index && index < player.inventory.slots.Count)
		{
			// sellable?
			ItemSlot slot = player.inventory.slots[index];
			if (slot.amount > 0)
			{
				if (po.extendedTimeInHours < po.extendedTimeInHoursCap)
				{
					po.despawnTime = (new DateTime(po.despawnTime) + new TimeSpan(200, 0, 0)).Ticks;
					po.extendedTimeInHours += 200;
					slot.DecreaseAmount(1);
					player.inventory.slots[index] = slot;
					Entity e = po.GetComponent<Entity>();

					// -- delete object from database
					Database.singleton.DeletePlaceableObject(player.name, player.guild.guild.name, ((e != null) ? e.level.current : 0), po.itemName, po.id);
					Database.singleton.SavePlaceableObject(player.name, player.guild.guild.name, po.gameObject, ((e != null) ? e.level.current : 0), po.itemName, po.id, po.despawnTime, po.extendedTimeInHours, po.extendedTimeInHoursCap);
				}
			}
		}
	}

	[Server]
	private void UnspawnPlacableObject(PlaceableObject ni)
	{
		PlaceableObject po = ni.GetComponent<PlaceableObject>();
		Entity e = po.GetComponent<Entity>();

		// -- delete object from database
		Database.singleton.DeletePlaceableObject(player.name, player.guild.guild.name, ((e != null) ? e.level.current : 0), po.itemName, po.id);
		NetworkManagerMMO.singleton.GetComponent<NetworkManagerBuildSystem>().placeableObjects.Remove(po);
		//also replace all current online accounts list
		foreach (var entry in Player.onlinePlayers)
		{
			entry.Value.playerRTSBuildSystem.placedClaimFlags.Clear();
			for (int p = 0; p < NetworkManagerMMO.singleton.GetComponent<NetworkManagerBuildSystem>().placeableObjects.Count; p++)
			{
				if (NetworkManagerMMO.singleton.GetComponent<NetworkManagerBuildSystem>().placeableObjects[p].claimArea)
					entry.Value.playerRTSBuildSystem.placedClaimFlags.Add(NetworkManagerMMO.singleton.GetComponent<NetworkManagerBuildSystem>().placeableObjects[p]);
			}

		}

		// -- remove the object
		NetworkServer.Destroy(po.gameObject);
	}


	// ===================================== UPGRADE =====================================

	// -----------------------------------------------------------------------------------
	// Cmd_StartUpgradePlaceableObject
	// @Client -> @Server
	// -----------------------------------------------------------------------------------
	[Command]
	public void Cmd_StartUpgradePlaceableObject(NetworkIdentity ni)
	{
		if (ni != null)
		{
			PlaceableObject po = ni.GetComponent<PlaceableObject>();

			if (CanUpgradePlaceableObject(po))
			{
				Entity e = po.GetComponent<Entity>();

				if (e != null)
				{
					player.Tools_addTask();
					myPlaceableObject = po;
					player.Tools_setTimer(po.getUpgradeCost(e.level.current).duration);
					Target_StartUpgradePlaceableObjectClient(player.connectionToClient, ni);
				}
			}
		}
	}

	// -----------------------------------------------------------------------------------
	// Target_StartUpgradePlaceableObjectClient
	// @Server -> @Client
	// -----------------------------------------------------------------------------------
	[TargetRpc]
	public void Target_StartUpgradePlaceableObjectClient(NetworkConnection target, NetworkIdentity ni)
	{
		if (ni != null)
		{
			PlaceableObject po = ni.GetComponent<PlaceableObject>();
			Entity e = po.GetComponent<Entity>();

			if (e != null)
			{
				player.movement.LookAtY(po.gameObject.transform.position);
				myPlaceableObject = po;
				player.Tools_setTimer(po.getUpgradeCost(e.level.current).duration);
				player.Tools_CastbarShow(MSG_PLACEABLEOBJECT_UPGRADE, po.getUpgradeCost(e.level.current).duration);
			}
		}
	}

	// -----------------------------------------------------------------------------------
	// Cmd_FinishUpgradePlaceableObject
	// @Client -> @Server
	// -----------------------------------------------------------------------------------
	[Command]
	public void Cmd_FinishUpgradePlaceableObject()
	{
		// -- Revalidate in case of anything changed while upgrading
		if (myPlaceableObject)
		{
			if (CanUpgradePlaceableObject(myPlaceableObject))
			{
				Entity e = myPlaceableObject.GetComponent<Entity>();

				if (e != null)
				{
					player.Tools_removeTask();

					// -- remove Resources & Items
					PlaceableObjectUpgradeCost cost = myPlaceableObject.getUpgradeCost(e.level.current);

					player.gold -= cost.gold;
					player.itemMall.coins -= cost.coins;

					foreach (Tools_ItemRequirement reqitem in cost.requiredItems)
					{
						if (reqitem.item == null || player.inventory.Count(new Item(reqitem.item)) >= reqitem.amount)
							player.inventory.Remove(new Item(reqitem.item), reqitem.amount);
					}

					// -- delete the old object
					if (myPlaceableObject.permanent)
					{
						Database.singleton.DeletePlaceableObject(player.name, player.guild.guild.name, e.level.current, myPlaceableObject.itemName, myPlaceableObject.id);
					}
					e.level.current++;

					// -- upgrade the Object
					GameObject go = Instantiate(myPlaceableObject.objectStages[e.level.current - 2].placementObject);//(GameObject)Instantiate(((UCE_Item_PlaceableObject)ScriptableItem.dict[itemName.GetStableHashCode()]).placementObject, v, rotation);
						go.transform.position = myPlaceableObject.transform.position;
					//GameObject go = Instantiate(UCE_myPlaceableObject.objectStages[e.level - 2].placementObject, UCE_myPlaceableObject.transform.position + UCE_myPlaceableObject.objectStages[e.level - 2].placementObject.transform.position, UCE_myPlaceableObject.objectStages[e.level - 2].placementObject.transform.rotation);
					go.GetComponent<Entity>().level = e.level;
					PlaceableObject placeableObject = go.GetComponent<PlaceableObject>();
					placeableObject.ownerCharacter = this.name;
					placeableObject.ownerGuild = this.player.guild.name;
					placeableObject.itemName = placeableObject.objectStages[e.level.current - 2].name;
					placeableObject.permanent = true;
					placeableObject.destroyable = true;
					placeableObject.id = myPlaceableObject.id;
					NetworkServer.Spawn(go);

					// -- save Object to database immediately
					if (myPlaceableObject.permanent)
					{
						Database.singleton.SavePlaceableObject(this.name, 
															   this.player.guild.name, 
															   go, 
															   e.level.current, 
															   myPlaceableObject.objectStages[e.level.current - 2].name, 
															   placeableObject.id,
															   placeableObject.despawnTime,
															   placeableObject.extendedTimeInHours,
															   placeableObject.extendedTimeInHoursCap); //UCE_myPlaceableObject.objectStages[e.level-2].name
					}

					// -- reset the Attache (if any)
#if _FHIZATTACHE
                    Monster m = UCE_myPlaceableObject.GetComponent<Monster>();
                    if (m != null)
                        m.UCE_AttacheReset();
#endif
					NetworkServer.Destroy(myPlaceableObject.gameObject);
					myPlaceableObject = null;
				}
			}
		}
	}

	// -----------------------------------------------------------------------------------
	// CanUpgradePlaceableObject
	// -----------------------------------------------------------------------------------
	public bool CanUpgradePlaceableObject(PlaceableObject po)
	{
		if (po)
		{
			Entity e = po.GetComponent<Entity>();

			if (e != null)
			{
				PlaceableObjectUpgradeCost c = po.getUpgradeCost(e.level.current);

				if (c != null)
				{
					if (player.level.current >= c.minLevel &&
						player.gold >= c.gold &&
						player.itemMall.coins >= c.coins &&
						checkHasAllItems(c)
					)
						return true;
				}
			}
		}

		return false;
	}

	// -----------------------------------------------------------------------------------
	// checkHasAllItems
	// -----------------------------------------------------------------------------------
	public bool checkHasAllItems(PlaceableObjectUpgradeCost cost)
	{
		foreach (Tools_ItemRequirement reqitem in cost.requiredItems)
		{
			if (reqitem.item == null || player.inventory.Count(new Item(reqitem.item)) < reqitem.amount)
				return false;
		}
		return true;
	}

	// ====================================== SPAWN ======================================

	// -----------------------------------------------------------------------------------
	// PrepareSpawnPlaceableObject
	// @Server
	// -----------------------------------------------------------------------------------
	public void PrepareSpawnPlaceableObject(Item_PlaceableObject item)
	{
		myPlaceableObjectItem = item;
		Target_PrepareSpawnPlaceableObjectClient(player.connectionToClient, item.name.GetStableHashCode());
	}
	public void MoveCurrentObjectToMouse()
	{

        // Calculate the center of the screen
        Vector3 centerScreenPosition = new Vector3(Screen.width / 2, Screen.height / 2, 0);

        // Create a ray from the center of the screen
        Ray ray = Camera.main.ScreenPointToRay(centerScreenPosition);



		RaycastHit hitInfo;
		LayerMask oldMask = 1 << LayerMask.NameToLayer("Default");
		if (Physics.Raycast(ray, out hitInfo, 50, oldMask))
		{
			foreach (var entry in Terrain.activeTerrains)
			{
				if (hitInfo.transform == entry.transform &&
					//hitInfo.transform.parent.GetComponent<PartBehaviour>() != null) &&
					Vector3.Distance(transform.position, hitInfo.point) <= 25)
				{
					buildSystem_PreviewObject.transform.position = hitInfo.point;
					buildSystem_PreviewObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
				}
			}
		}
		if (Input.GetMouseButton(0))
		{
			structurePosition = buildSystem_PreviewObject.transform.position;
			Destroy(buildSystem_PreviewObject);
			CmdSendToServer(structurePosition, structureRotation);
			Cmd_StartSpawnPlaceableObject();
		}
	}
	[Command]
	void CmdSendToServer(Vector3 structurePosition, Quaternion structureRotation)
	{
		this.structurePosition = structurePosition;
		this.structureRotation = structureRotation;
	}
	public void RotateFromMouseWheel()
	{
		mouseWheelRotation += Input.mouseScrollDelta.y;
		buildSystem_PreviewObject.transform.Rotate(Vector3.up, mouseWheelRotation * 10f);
		structureRotation = buildSystem_PreviewObject.transform.rotation;
	}

	// -----------------------------------------------------------------------------------
	// Target_StartSpawnPlaceableObjectClient
	// @Server -> @Client
	// -----------------------------------------------------------------------------------
	[TargetRpc]
	public void Target_PrepareSpawnPlaceableObjectClient(NetworkConnection target, int itemHash)
	{
		ScriptableItem item;

		if (ScriptableItem.All.TryGetValue(itemHash, out item))
		{
			myPlaceableObjectItem = (Item_PlaceableObject)item;

			if (CanSpawnPlaceableObject(myPlaceableObjectItem))
			{
				Vector3 vBuildPosition = GetSpawnDestination();

				player.indicator.SetViaPosition(vBuildPosition);

				if (myPlaceableObjectItem.previewObject != null)
                    panelManager?.CloseAllPanels(); // Safely call CloseAllPanels
                buildSystem_PreviewObject = (GameObject)Instantiate(myPlaceableObjectItem.previewObject, vBuildPosition, Quaternion.identity);

				if (myPlaceableObjectItem.gridObject != null)
					buildSystem_GridObject = (GameObject)Instantiate(myPlaceableObjectItem.gridObject, vBuildPosition, Quaternion.identity);

				if (!_UI_PlaceableObjectSpawn)
					_UI_PlaceableObjectSpawn = FindObjectOfType<UI_PlaceableObjectSpawn>();

				//_UI_PlaceableObjectSpawn.Show();
			}
		}
	}

	// -----------------------------------------------------------------------------------
	// Cmd_CancelSpawnPlaceableObject
	// @Client
	// -----------------------------------------------------------------------------------
	public void CancelSpawnPlaceableObject()
	{
		if (buildSystem_PreviewObject != null)
			Destroy(buildSystem_PreviewObject);

		if (buildSystem_GridObject != null)
			Destroy(buildSystem_GridObject);

		buildSystem_PreviewObject = null;
		myPlaceableObject = null;
		myPlaceableObjectItem = null;
		player.Tools_removeTask();
		player.Tools_stopTimer();
		player.Tools_CastbarHide();

		Destroy(player.indicator.indicator);
	}

	// -----------------------------------------------------------------------------------
	// Cmd_StartSpawnPlaceableObject
	// @Client -> @Server
	// -----------------------------------------------------------------------------------
	[Command]
	public void Cmd_StartSpawnPlaceableObject()
	{
		if (CanSpawnPlaceableObject())
		{
			player.Tools_addTask();
			Target_StartSpawnPlaceableObjectClient(player.connectionToClient, myPlaceableObjectItem.name.GetStableHashCode());
			player.indicator.SetViaPosition(GetSpawnDestination());
			player.Tools_setTimer(myPlaceableObjectItem.placementTime);
		}
	}

	// -----------------------------------------------------------------------------------
	// Target_StartSpawnPlaceableObjectClient
	// @Server -> @Client
	// -----------------------------------------------------------------------------------
	[TargetRpc]
	public void Target_StartSpawnPlaceableObjectClient(NetworkConnection target, int itemHash)
	{
		ScriptableItem item;

		if (ScriptableItem.All.TryGetValue(itemHash, out item))
		{
			myPlaceableObjectItem = (Item_PlaceableObject)item;
			player.Tools_setTimer(myPlaceableObjectItem.placementTime);
			player.Tools_CastbarShow(myPlaceableObjectItem.name, myPlaceableObjectItem.placementTime);
		}
	}

	// -----------------------------------------------------------------------------------
	// Cmd_FinishSpawnPlaceableObject
	// @Client -> @Server
	// -----------------------------------------------------------------------------------
	[Command]
	public void Cmd_FinishSpawnPlaceableObject()
	{
		player.Tools_removeTask();
		StartCoroutine("SpawnPlaceableObject");
	}

	// -----------------------------------------------------------------------------------
	// SpawnPlaceableObject
	// @Server
	// -----------------------------------------------------------------------------------
	[Server]
	protected IEnumerator SpawnPlaceableObject()
	{
		PlaceableObject o = null;
		int level = 0;

		Vector3 vBuildPosition = GetSpawnDestination();

		if (myPlaceableObjectItem.placementObject)
		{
			GameObject SimpleObject = (GameObject)Instantiate(myPlaceableObjectItem.placementObject, vBuildPosition, structureRotation);

			//SimpleObject.transform.position += structurePosition;
			//SimpleObject.transform.rotation *= structureRotation;
			NetworkServer.Spawn(SimpleObject);

			o = SimpleObject.GetComponent<PlaceableObject>();

			if (o)
			{
				o.permanent = myPlaceableObjectItem.isPermanent;
				o.ownerCharacter = player.name;
				o.ownerGuild = player.guild.guild.name;
				o.itemName = myPlaceableObjectItem.name;

				Entity e = SimpleObject.GetComponent<Entity>();

				if (e)
					level = e.level.current;
				//if it needs a claim area and we got this far, assign the closest one
				if (myPlaceableObjectItem)
				{
					PlaceableObject claimFlag = null;
					Debug.Log("Inside Claim Area");
					List<PlaceableObject> placeableObjects = NetworkManagerMMO.singleton.GetComponent<NetworkManagerBuildSystem>().placeableObjects;
					for (int p = 0; p < placeableObjects.Count; p++)
					{
						if (placeableObjects[p].claimArea)
						{
							if (claimFlag != null && Vector3.Distance(placeableObjects[p].transform.position, o.transform.position) <= Vector3.Distance(claimFlag.transform.position, o.transform.position))
							{
								claimFlag = placeableObjects[p];
							}
							else if (claimFlag == null)
								claimFlag = placeableObjects[p];
						}
					}
					o.currentClaimFlag = claimFlag;
				}
				if (o.claimArea)
				{
					//loop through all placeable objects and if they have the same owner and are in range, assign them
					List<PlaceableObject> placeableObjects = NetworkManagerMMO.singleton.GetComponent<NetworkManagerBuildSystem>().placeableObjects;
					SphereCollider sphere = o.GetComponent<SphereCollider>();
					for (int p2 = 0; p2 < placeableObjects.Count; p2++)
					{
						if (placeableObjects[p2].ownerCharacter == player.name &&
							sphere.bounds.Contains(placeableObjects[p2].transform.position) &&
							!placeableObjects[p2].claimArea)
						{
							placeableObjects[p2].currentClaimFlag = o;
						}
					}
				}

				NetworkManagerMMO.singleton.GetComponent<NetworkManagerBuildSystem>().placeableObjects.Add(o);
				//also replace all current online accounts list
				foreach (var entry in Player.onlinePlayers)
				{
					entry.Value.playerRTSBuildSystem.placedClaimFlags.Clear();
					for (int p = 0; p < NetworkManagerMMO.singleton.GetComponent<NetworkManagerBuildSystem>().placeableObjects.Count; p++)
					{
						if (NetworkManagerMMO.singleton.GetComponent<NetworkManagerBuildSystem>().placeableObjects[p].claimArea)
							entry.Value.playerRTSBuildSystem.placedClaimFlags.Add(NetworkManagerMMO.singleton.GetComponent<NetworkManagerBuildSystem>().placeableObjects[p]);
					}
				}
			}
		}

		if (o != null && myPlaceableObjectItem.isPermanent)
			Database.singleton.SavePlaceableObject(player.name, player.guild.guild.name, o.gameObject, level, myPlaceableObjectItem.name, o.id, o.despawnTime, o.extendedTimeInHours, o.extendedTimeInHoursCap);

		player.inventory.Remove(new Item(myPlaceableObjectItem), myPlaceableObjectItem.decreaseAmount);
		myPlaceableObjectItem = null;

		yield return new WaitForEndOfFrame();
		structurePosition = Vector3.zero;
		structureRotation = Quaternion.Euler(0, 0, 0);
	}

	// -----------------------------------------------------------------------------------
	// CanSpawnPlaceableObject
	// -----------------------------------------------------------------------------------
	public bool CanSpawnPlaceableObject(Item_PlaceableObject item)
	{
		return (item != null && player.canInteract && player.isAlive
			&& CheckSpawnDestination(item.doNotSpawnAt)
			&& CheckSpawnSpheres(item)
			&& (item.placementAreaId == 0 || (myPlaceableObjectAreaId == item.placementAreaId))
			&& (item.restrictedAreaId == 0 || (myPlaceableObjectAreaId != item.restrictedAreaId))
			&& (!item.onlyPersonalArea || (item.onlyPersonalArea && myPlaceableObjectAreaOwner))
			&& (!item.onlyGuildArea || (item.onlyGuildArea && myPlaceableObjectAreaGuild))
			&& (!item.guildRankRequired || (item.guildRankRequired && CheckGuildRank(item.guildRank)))
			#if _iMMOPVP
			&& (!item.onlyPVPZone || item.onlyPVPZone && player.GetInPvpRegion())
			#endif
		);
	}
	public bool CheckSpawnSpheres(Item_PlaceableObject item)
	{
		bool valid = false;
		if (!item.requiresClaimArea && item.placementObject.GetComponent<PlaceableObject>().claimArea)
		{
			for (int i = 0; i < placedClaimFlags.Count; i++)
			{
				Vector3 spawnPosition = Vector3.zero;
				Bounds bounds = player.collider.bounds;
				spawnPosition = transform.position + transform.forward * (bounds.size.x + 2f);
				if (placedClaimFlags[i].GetComponent<SphereCollider>().bounds.Contains(spawnPosition))
				{
					return false;
				}
			}
			return true;
		}
		else if (!item.requiresClaimArea)
			return true;
		List<PlaceableObject> pos = placedClaimFlags.ToList();
		for (int i = 0; i < pos.Count; i++)
		{
			Vector3 spawnPosition = Vector3.zero;
			Bounds bounds = player.collider.bounds;
			spawnPosition = transform.position + transform.forward * (bounds.size.x + 2f);
			if (pos[i].GetComponent<SphereCollider>().bounds.Contains(spawnPosition))
			{
				if (pos[i].ownerCharacter == name)
				{
					valid = true;
					break;
				}
			}
		}
		return valid;
	}

	// -----------------------------------------------------------------------------------
	// CanSpawnPlaceableObject
	// -----------------------------------------------------------------------------------
	public bool CanSpawnPlaceableObject()
	{
		return CanSpawnPlaceableObject(myPlaceableObjectItem);
	}

	// -----------------------------------------------------------------------------------
	// CheckGuildRank
	// -----------------------------------------------------------------------------------
	protected bool CheckGuildRank(GuildRank guildRank)
	{
		return player.guild.guild.members != null && Array.FindIndex(player.guild.guild.members, (m) => m.name == name && m.rank >= guildRank) != -1;
	}

	// -----------------------------------------------------------------------------------
	// SetPlaceableObjectArea
	// -----------------------------------------------------------------------------------
	public void SetPlaceableObjectArea(int id, string areaOwnerName, string areaGuildName, bool condition)
	{
		if (condition)
		{
			myPlaceableObjectAreaId = id;
			if (areaOwnerName == name)
			{
				myPlaceableObjectAreaOwner = true;
			}
			else
			{
				myPlaceableObjectAreaOwner = false;
			}
			if (areaGuildName == player.guild.guild.name)
			{
				myPlaceableObjectAreaGuild = true;
			}
			else
			{
				myPlaceableObjectAreaGuild = false;
			}
		}
		else
		{
			myPlaceableObjectAreaId = 0;
			myPlaceableObjectAreaOwner = false;
			myPlaceableObjectAreaGuild = false;
		}
	}

	// =================================== HELPERS =======================================

	// -----------------------------------------------------------------------------------
	// GetSpawnDestination
	// -----------------------------------------------------------------------------------
	public Vector3 GetSpawnDestination()
	{
		Vector3 spawnPosition = Vector3.zero;

		Bounds bounds = player.collider.bounds;
		spawnPosition = transform.position + transform.forward * (bounds.size.x + 1);

		bool pass = false;
		int i = 0;
		if (structurePosition != Vector3.zero)
		{
			return structurePosition;
		}

		while (!pass)
		{
			i++;

			UnityEngine.AI.NavMeshHit hit;

			if (UnityEngine.AI.NavMesh.SamplePosition(spawnPosition, out hit, 1f, UnityEngine.AI.NavMesh.AllAreas))
			{
				spawnPosition = hit.position;
			}

			if (!Physics.Raycast(spawnPosition, Vector3.down, 0))
			{
				return spawnPosition;
			}

			if (i > 100)
			{
				break;          //emergency break in case of nothing found after 100 passes
			}
		}

		return spawnPosition;
	}

	// -----------------------------------------------------------------------------------
	// CheckSpawnDestination
	// -----------------------------------------------------------------------------------
	public bool CheckSpawnDestination(LayerMask doNotSpawnAt)
	{
		Vector3 spawnPosition = Vector3.zero;
		Bounds bounds = player.collider.bounds;
		spawnPosition = transform.position + transform.forward * (bounds.size.x + 2f);

		int i = 0;
		bool pass = false;

		while (!pass)
		{
			i++;

			UnityEngine.AI.NavMeshHit hit;

			if (UnityEngine.AI.NavMesh.SamplePosition(spawnPosition, out hit, 1f, UnityEngine.AI.NavMesh.AllAreas))
				spawnPosition = hit.position;

			if (!Physics.Raycast(spawnPosition, Vector3.down, 0, doNotSpawnAt))
				return true;

			if (i > 100)
				return false;           //emergency break in case of nothing found after 100 passes
		}

		return false;
	}

	// -----------------------------------------------------------------------------------
}
