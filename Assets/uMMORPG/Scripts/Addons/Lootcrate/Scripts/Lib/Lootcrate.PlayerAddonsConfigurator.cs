using Mirror;
using UnityEngine;

#if _iMMOCHEST

// PLAYER
public partial class PlayerAddonsConfigurator
{


    protected UI_Lootcrate lootcrateUIInstance;
    [HideInInspector] public Lootcrate selectedLootcrate;


#if _CLIENT && _iMMOCHEST
    private void OnStartLocalPlayer_Lootcrate()
    {
        combat.onClientReceivedDamage.AddListener(OnServerReceivedDamage_Lootcrate);
    }
#endif

    private void OnServerReceivedDamage_Lootcrate(int degat, DamageType type)
    {
        if (degat > 0) OnDamageDealt_cancelLootcrate();
    }
    // -----------------------------------------------------------------------------------
    // OnSelect_Lootcrate
    // @Client
    // -----------------------------------------------------------------------------------
    [Client]
    public void OnSelect_Lootcrate(Lootcrate _selectedLootcrate)
    {
        if (lootcrateUIInstance)
            lootcrateUIInstance.Hide(false);

        selectedLootcrate = _selectedLootcrate;
        Cmd_checkLootcrateAccess(selectedLootcrate.gameObject);
#if !_iMMO2D
        if(selectedLootcrate.playerLookAtLootCrate)
            player.movement.LookAtY(selectedLootcrate.gameObject.transform.position);
#endif
    }

    // -----------------------------------------------------------------------------------
    // Cmd_checkLootcrateAccess
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_checkLootcrateAccess(GameObject _selectedLootcrate)
    {
        selectedLootcrate = _selectedLootcrate.GetComponent<Lootcrate>();
        //Debug.LogError("Bien sur!...");
        if (LootcrateValidation())
        {
            Target_startLootcrateAccess(connectionToClient);
        }
        else
        {
            if (selectedLootcrate != null && selectedLootcrate.checkInteractionRange(player) && selectedLootcrate.lockedMessage != "")
            {
                player.Tools_ShowPrompt(selectedLootcrate.lockedMessage);
            }
            else
            {
                // agent.destination = this.collider.ClosestPointOnBounds(transform.positionPlaceableObject);
                //player.movement.Navigate(_selectedLootcrate.transform.positionPlaceableObject, 0.5f);

                //Vector3 destination = Utils.ClosestPoint(this, player.transform.positionPlaceableObject);
                //player.movement.Navigate(destination, player.interactionRange);
                
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // Target_startLootcrateAccess
    // @Server -> @Client
    // -----------------------------------------------------------------------------------
    [TargetRpc]
    protected void Target_startLootcrateAccess(NetworkConnection target)
    {
        if (LootcrateValidation())
        {
            player.Tools_addTask();
            player.Tools_setTimer(selectedLootcrate.accessDuration);
            player.Tools_CastbarShow(selectedLootcrate.accessLabel, selectedLootcrate.accessDuration);
            player.StartAnimation(selectedLootcrate.playerAnimation);
        }
    }

    // -----------------------------------------------------------------------------------
    // LootcrateValidation
    // -----------------------------------------------------------------------------------
    public bool LootcrateValidation()
    {
        bool bValid = (selectedLootcrate != null &&
            selectedLootcrate.checkInteractionRange(player) &&
            selectedLootcrate.interactionRequirements.checkState(player));

        if (!bValid)
            CancelLootcrate();

        return bValid;
    }

    // -----------------------------------------------------------------------------------
    // LateUpdate
    // @Client
    // -----------------------------------------------------------------------------------
    [ClientCallback]
    private void LateUpdate_LootCrate()
    {
        if (LootcrateValidation() && player.Tools_checkTimer())
        {
            player.Tools_removeTask();
            player.Tools_stopTimer();
            player.Tools_CastbarHide();

            Cmd_finishLootcrateAccess();

            if (!lootcrateUIInstance)
                
                lootcrateUIInstance = FindObjectOfType<UI_Lootcrate>();

            lootcrateUIInstance.Show();

            player.StopAnimation(selectedLootcrate.playerAnimation);

            player.Tools_AddMessage(selectedLootcrate.successMessage);
        }
    }

    // -----------------------------------------------------------------------------------
    // OnDamageDealt_cancelLootcrate
    // Custom Hook
    // -----------------------------------------------------------------------------------
    private void OnDamageDealt_cancelLootcrate()
    {
        CancelLootcrate();
    }

    // -----------------------------------------------------------------------------------
    // CancelLootcrate
    // -----------------------------------------------------------------------------------
    public void CancelLootcrate()
    {
        if (selectedLootcrate != null)
        {
            player.Tools_stopTimer();
            player.Tools_removeTask();
            player.Tools_CastbarHide();

            player.StopAnimation(selectedLootcrate.playerAnimation);

            selectedLootcrate = null;
        }
    }

    // -----------------------------------------------------------------------------------
    // Cmd_finishLootcrateAccess
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_finishLootcrateAccess()
    {
#if _SERVER
        player.Tools_removeTask();
        player.Tools_stopTimer();

#if _iMMOCHEST && _iMMOQUESTS
        player.playerExtendedQuest.IncreaseQuestLootCounterFor(selectedLootcrate.name);
#endif

        selectedLootcrate.OnLoot();
        selectedLootcrate.OnAccessed();
#endif
    }

    // -----------------------------------------------------------------------------------
    // Cmd_TakeLootcrateGold
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_TakeLootcrateGold()
    {
        if (LootcrateValidation())
        {
            player.gold += selectedLootcrate.gold;
            selectedLootcrate.gold = 0;
        }
    }

    // -----------------------------------------------------------------------------------
    // Cmd_TakeLootcrateCoins
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_TakeLootcrateCoins()
    {
        if (LootcrateValidation())
        {
            player.itemMall.coins += selectedLootcrate.coins;
            selectedLootcrate.coins = 0;
        }
    }

    // -----------------------------------------------------------------------------------
    // Cmd_TakeLootcrateItem
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    public void Cmd_TakeLootcrateItem(int index)
    {
        if (LootcrateValidation() && 0 <= index && index < selectedLootcrate.inventory.Count && selectedLootcrate.inventory[index].amount > 0)
        {
            ItemSlot slot = selectedLootcrate.inventory[index];

            if (player.inventory.Add(slot.item, slot.amount))
            {
                slot.amount = 0;
                selectedLootcrate.inventory[index] = slot;
            }
        }
    }

    // -----------------------------------------------------------------------------------
}

#endif
