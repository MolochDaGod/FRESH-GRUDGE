using Mirror;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

// ===================================================================================
// ITEM DROP CLASS
// ===================================================================================
public partial class DisableItemDrop : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Area_PVPZone>() != null)
        {
            Player player = GetComponent<Player>();
            Monster monster = GetComponent<Monster>();
            if (player)
            {
                player.playerAddonsConfigurator.itemDropConfiguration.isActive = false;
            }
            if (monster)
            {
                monster.monsterItemDrop.itemDropConfiguration.isActive = false;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Area_PVPZone>() != null)
        {
            Player player = GetComponent<Player>();
            Monster monster = GetComponent<Monster>();
            if (player)
            {
                player.playerAddonsConfigurator.itemDropConfiguration.isActive = true;
            }
            if (monster)
            {
                monster.monsterItemDrop.itemDropConfiguration.isActive = true;
            }
        }
    }
}