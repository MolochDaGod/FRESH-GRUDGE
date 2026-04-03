using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*public partial class PlayerInventory
{ 

    // Update is called once per frame
    /*void Update()
    {
        if (Input.GetKey(KeyCode.Mouse1))
        {
            //UIChat.singleton.itemLink = true;
            Item item = player.inventory.slots[slot].item;
            string stringToCheck = UIChat.singleton.messageInput.text;
            int start = stringToCheck.IndexOf("[");
            int stop = stringToCheck.IndexOf("]");

            if(stop > -1)
            {
                string output = stringToCheck.Substring(start + 1, stop - start -1);
                stringToCheck = stringToCheck.Replace(output, item.name);
            }
            else
            {
                stringToCheck += $"[{item.name}]";
            }
            UIChat.singleton.messageInput.text = stringToCheck;
            //UIChat.singleton.itemLink = false;
        }
        else
        {
            if (itemSlot.item.data is UsableItem usable && usable.CanUse(player, icopy))
                player.inventory.CmdUseItem(icopy);
        }
    }
}*/
