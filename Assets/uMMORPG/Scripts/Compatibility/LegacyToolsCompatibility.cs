using Mirror;
using UnityEngine;

public partial class Player
{
#if _iMMOPRESTIGECLASSES
    public PlayerAddonsConfigurator playerPrestigeClasses => playerAddonsConfigurator;
#endif

    public int strength
    {
        get
        {
            Strength component = GetComponent<Strength>();
            return component != null ? component.value : 0;
        }
        set
        {
            Strength component = GetComponent<Strength>();
            if (component != null)
                component.value = value;
        }
    }

    public int intelligence
    {
        get
        {
            Intelligence component = GetComponent<Intelligence>();
            return component != null ? component.value : 0;
        }
        set
        {
            Intelligence component = GetComponent<Intelligence>();
            if (component != null)
                component.value = value;
        }
    }

    public bool isGameMaster { get; set; }

    public bool Tools_checkHasClass(Player[] classes)
    {
        if (classes == null || classes.Length == 0)
            return true;

        foreach (Player playerClass in classes)
            if (playerClass != null && classIcon == playerClass.classIcon)
                return true;

        return false;
    }

    public void Tools_AddItems(Player player, Tools_ItemRequirement[] items)
    {
        if (player == null || items == null)
            return;

        foreach (Tools_ItemRequirement requirement in items)
        {
            if (requirement != null && requirement.item != null && requirement.amount > 0)
                player.inventory.Add(new Item(requirement.item), requirement.amount);
        }
    }

    public void Target_Tools_ShowPopup(NetworkConnection target, string message)
    {
        Target_Tools_ShowPopup(target, message, 0, 0);
    }

    public void Tools_ClientShowPopup(string message)
    {
        Tools_ClientShowPopup(message, 0, 0);
    }
}