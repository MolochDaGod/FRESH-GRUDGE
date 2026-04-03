using System.Collections.Generic;
using UnityEngine;

// ===================================================================================
// SHARED INSTANCE UI
// ===================================================================================
public partial class UI_LimitedZones : MonoBehaviour
{
	
    public GameObject panel;
    public Slot_LimitedZones slotPrefab;
    public Transform content;
	
	public LimitedZonesManager sharedInstanceManager;
	
	protected int instanceCategory;
	
    // -----------------------------------------------------------------------------------
    // Show
    // @Client
    // -----------------------------------------------------------------------------------
    public void Show(int _instanceCategory)
    {
    
        Player player = Player.localPlayer;
        if (!player) return;
        
        instanceCategory = _instanceCategory;
        
        if (!sharedInstanceManager)
			sharedInstanceManager = FindObjectOfType<LimitedZonesManager>();
        
        List<LimitedZonesEntry> instancesAvailable = sharedInstanceManager.getAvailableSharedInstances(player, instanceCategory);
        
        if (instancesAvailable.Count == 0)
        	return;
        
        UIUtils.BalancePrefabs(slotPrefab.gameObject, instancesAvailable.Count, content);
		
        for (int i = 0; i < instancesAvailable.Count; ++i)
        {
            
            Slot_LimitedZones slot = content.GetChild(i).GetComponent<Slot_LimitedZones>();
            
            int index = i;
            
            slot.image.sprite				= instancesAvailable[i].image;
            slot.titleText.text 			= instancesAvailable[i].title;
            slot.descriptionText.text 		= instancesAvailable[i].description;
			slot.groupText.text				= instancesAvailable[i].getGroupType();
#if _iMMOHONORSHOP
            if (instancesAvailable[i].entranceCost.honorCurrency != null)
            { 
            slot.costImage.sprite			= instancesAvailable[i].entranceCost.honorCurrency.image;
            slot.costText.text				= instancesAvailable[i].entranceCost.amount.ToString();
            }
#endif
            slot.groupCountText.text 		= instancesAvailable[i].getGroupCountText();
			slot.countText.text				= instancesAvailable[i].getPlayerCountText();
			
			slot.actionButton.interactable 	= instancesAvailable[i].canPlayerEnter(player);
			
            slot.actionButton.onClick.SetListener(() =>
            {
                if(player.isAlive)
                    instancesAvailable[index].teleportPlayerToInstance(player, instanceCategory, index);
                panel.SetActive(false);
            });
            
            
        }

        panel.SetActive(true);
    }
	
    // -----------------------------------------------------------------------------------
    // Hide
    // @Client
    // -----------------------------------------------------------------------------------
    public void Hide()
    {
        panel.SetActive(false);
    }
    
   	// -----------------------------------------------------------------------------------
    // Update : This function is not needed, is player is out of collider is cloded
    // @Client
    // -----------------------------------------------------------------------------------
    /*void Update()
    {
    	
    	player player = player.localPlayer;
    	
        if (!player)
        	Hide();
        
        if (panel.activeSelf)
        	Show(instanceCategory);
    }*/
    // -----------------------------------------------------------------------------------
}