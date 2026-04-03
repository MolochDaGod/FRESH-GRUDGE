using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// PLACEABLE OBJECT UI
public partial class UI_PlaceableObject : MonoBehaviour
{
	public GameObject panel;
	public UI_PlaceableObjectUpgrade upgradePanel;
	public UI_PlaceableObjectUnspawn unspawnPanel;

	public Button upgradeButton;
	public Button unspawnButton;
	public Button closeButton;
	public UIDragAndDropable extendTimeSlot;
	public Button extendDespawnTimeButton;
	public GameObject slotPrefab;
	public TextMeshProUGUI timeLeftToDespawntext;
	[HideInInspector] public int extendTimeIndex = -1;

	public static UI_PlaceableObject singleton;

	private void Start()
	{
		singleton = this;
		closeButton.onClick.SetListener(() =>
			{
				panel.SetActive(false);
			});
	}

	// -----------------------------------------------------------------------------------
	// Update
	// -----------------------------------------------------------------------------------
	private void Update()
	{
		//if (!panel.activeSelf) return;

		Player player = Player.localPlayer;
		if (!player) return;
		if (player.playerRTSBuildSystem.myPlaceableObject != null &&
			player.playerRTSBuildSystem.myPlaceableObject.ownerCharacter == player.name)
		{
			// -- Upgrade Structure
			upgradeButton.gameObject.SetActive(player.playerRTSBuildSystem.CanUpgradePlaceableObject(player.playerRTSBuildSystem.myPlaceableObject));
			upgradeButton.onClick.SetListener(() =>
				{
					upgradePanel.Show();
					panel.SetActive(false);
				});

			// -- Pickup Structure
			unspawnButton.gameObject.SetActive(player.playerRTSBuildSystem.myPlaceableObject.pickupable);
			unspawnButton.onClick.SetListener(() =>
				{
					unspawnPanel.Show(player.playerRTSBuildSystem.myPlaceableObject.netIdentity);
					panel.SetActive(false);
				});
			timeLeftToDespawntext.text = "";
			if (player.playerRTSBuildSystem.myPlaceableObject.despawnTime != 0)
				timeLeftToDespawntext.text = "Time left to despawn: " + (new DateTime(player.playerRTSBuildSystem.myPlaceableObject.despawnTime) - DateTime.UtcNow).ToString("d'd 'h'h 'm'm 's's'");
			if (extendTimeIndex != -1 && extendTimeIndex < player.inventory.slots.Count &&
				player.inventory.slots[extendTimeIndex].amount > 0)
			{
				ItemSlot itemSlot = player.inventory.slots[extendTimeIndex];

				extendTimeSlot.GetComponent<Image>().sprite = itemSlot.item.image;
				// only build tooltip while it's actually shown. this
				// avoids MASSIVE amounts of StringBuilder allocations.
				extendTimeSlot.GetComponent<UIShowToolTip>().enabled = true;
				if (extendTimeSlot.GetComponent<UIShowToolTip>().IsVisible())
					extendTimeSlot.GetComponent<UIShowToolTip>().text = itemSlot.ToolTip();
				extendTimeSlot.dragable = true;
				extendDespawnTimeButton.interactable = player.inventory.slots[extendTimeIndex].item.data is ExtendDespawnTimeItem;
				if (player.inventory.slots[extendTimeIndex].item.data is ExtendDespawnTimeItem)
				{
					extendTimeSlot.GetComponent<Image>().sprite = itemSlot.item.image;
					extendTimeSlot.GetComponent<Image>().color = Color.white;
				}
				else
					extendTimeSlot.GetComponent<Image>().color = Color.clear;
				extendDespawnTimeButton.onClick.SetListener(() =>
					{
						player.playerRTSBuildSystem.CmdExtendDespawnTime(extendTimeIndex, player.playerRTSBuildSystem.myPlaceableObject);
						extendTimeIndex = -1;
					});
			}
			else
			{
				extendTimeSlot.GetComponent<Image>().color = Color.clear;
				extendTimeSlot.GetComponent<Image>().sprite = null;
				extendTimeSlot.GetComponent<UIShowToolTip>().enabled = false;
				extendTimeSlot.dragable = false;
				extendDespawnTimeButton.interactable = false;
			}
		}
		else
		{
			extendTimeIndex = -1;
			panel.SetActive(false);
		}
	}

	// -----------------------------------------------------------------------------------
	// Show
	// -----------------------------------------------------------------------------------
	public void Show()
	{
		panel.SetActive(true);
	}

	// -----------------------------------------------------------------------------------
}
