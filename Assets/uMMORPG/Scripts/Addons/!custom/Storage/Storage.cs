// used to store items
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Storage : Inventory
{
    public int size = 22;
    [HideInInspector] public bool isOver;

    public string storageName;
    // cache all storages on the server to save lots of computations
    // (otherwise we'd have to iterate NetworkServer.objects all the time)
    // -> differentiate them by name. make sure to not use the same name for
    //    two storages.
    //    (sceneId is not a good alternative because it changes when changing
    //     the hierarchy)
    public static Dictionary<string, Storage> storages = new Dictionary<string, Storage>();

    public override void OnStartServer()
    {
        storageName = Mathf.RoundToInt(transform.position.x).ToString() + Mathf.RoundToInt(transform.position.y).ToString() + Mathf.RoundToInt(transform.position.z).ToString() + Mathf.RoundToInt(transform.rotation.y).ToString();
        // add to storages if none with the same name already exists
        if (!storages.ContainsKey(storageName))
        {
            storages.Add(storageName, this);

            // load from db
            Database.singleton.LoadStorage(this);
        }
        else Debug.LogWarning("A Storage with name " + name + " already exists. Use a different name for each Storage, otherwise it won't be saved to the Database.");
    }

    public bool Detect(Item item, int amount)
    {
        for (int i = 0; i < slots.Count; ++i)
        {
            ItemSlot slot = slots[i];
            // note: .Equals because name AND dynamic variables matter (petLevel etc.)
            if (slot.amount >= amount && slot.item.Equals(item))
            {
                return true;
            }
        }

        // if we got here, then we didn't remove enough items
        return false;
    }

    void OnDestroy()
    {
        storages.Remove(name);
    }

    public void OnMouseOver()
    {
        isOver = true;
    }

    public void OnMouseExit()
    {
        isOver = false;
    }
    public void OnMouseDown() // Called when the mouse clicks on the object  
    {
        if (isOver && !Utils.IsCursorOverUserInterface()) // Check if the mouse is over the object and not over the UI  
        {
            // Check if the player is close enough to interact with the storage  
            if (Vector3.Distance(Player.localPlayer.transform.position, transform.position) <= Player.localPlayer.interactionRange)
            {
                Player.localPlayer.storage = this; // Set the player's storage reference to this storage instance  
                ActivatePanel("StoragePanel"); // Activate the storage panel UI
            }
        }
    }

    public void ActivatePanel(string panelTag)
    {
        GameObject panel = GameObject.FindGameObjectWithTag(panelTag);
        if (panel != null)
        {
            panel.SetActive(true);
            Debug.Log($"{panelTag} activated.");
        }
        else
        {
            Debug.LogWarning($"{panelTag} not found!");
        }
    }
}