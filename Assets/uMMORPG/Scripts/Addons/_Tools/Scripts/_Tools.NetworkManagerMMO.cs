#if _iMMOTOOLS_LEGACY_CORE
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class NetworkManagerMMO : NetworkManager
{

    void LoadPreview(GameObject prefab, Transform location, int selectionIndex, CharactersAvailableMsg.CharacterPreview character)
    {
        // instantiate the prefab
        GameObject preview = Instantiate(prefab.gameObject, location.position, location.rotation);
        preview.transform.parent = location;
        Player player = preview.GetComponent<Player>();

        // assign basic preview values like name and equipment
        player.name = character.name;
#if !_iMMO2D
        player.isGameMaster = character.isGameMaster;
#endif
#if _iMMOUMACHARACTERS
        if (player.playerUMACharacter)
        {
            string decompressor = CompressUMA.Compressor.DecompressDna(character.umaDna);
            player.playerUMACharacter.umaDna = character.umaDna;
            preview.GetComponentInChildren<UMA.CharacterSystem.DynamicCharacterAvatar>().LoadFromRecipeString(decompressor);
        }
#endif
        for (int i = 0; i < character.equipment.Length; ++i)
        {
            ItemSlot slot = character.equipment[i];
            player.equipment.slots.Add(slot);
            if (slot.amount > 0)
            {

                // OnEquipmentChanged won't be called unless spawned, we
                // need to refresh manually
                ((PlayerEquipment)player.equipment).RefreshLocation(i);
                //player.RefreshLocation(i);
#if _iMMOUMACHARACTERS
                if (player.playerUMACharacter)
                    player.playerUMACharacter.UpdateUma();
#endif
            }
        }

        // add selection script
        preview.AddComponent<SelectableCharacter>();
        preview.GetComponent<SelectableCharacter>().index = selectionIndex;
    }

#if _iMMOUMACHARACTERS
    private Player CreateCharacter(GameObject classPrefab, string characterName, string account, bool gameMaster, string dna)
#else
    private Player CreateCharacter(GameObject classPrefab, string characterName, string account, bool gameMaster)
#endif
    {
        // create new character based on the prefab.
        // -> we also assign default items and equipment for new characters
        // -> skills are handled in Database.CharacterLoad every time. if we
        //    add new ones to a prefab, all existing players should get them
        // (instantiate temporary player)
        //print("creating character: " + message.name + " " + message.classIndex);
        Player player = Instantiate(classPrefab).GetComponent<Player>();
        player.name = characterName;
        player.account = account;
        player.className = classPrefab.name;
#if _iMMOUMACHARACTERS
        if(player.playerUMACharacter)
            player.playerUMACharacter.umaDna = dna;
#endif
        player.transform.position = GetStartPositionFor(player.className).position;
        for (int i = 0; i < player.inventory.size; ++i)
        {
            // add empty slot or default item if any
            player.inventory.slots.Add(i < player.inventory.defaultItems.Length ? new ItemSlot(new Item(player.inventory.defaultItems[i].item), player.inventory.defaultItems[i].amount) : new ItemSlot());
        }
        for (int i = 0; i < ((PlayerEquipment)player.equipment).slotInfo.Length; ++i)
        {
            // add empty slot or default item if any
            EquipmentInfo info = ((PlayerEquipment)player.equipment).slotInfo[i];
            player.equipment.slots.Add(info.defaultItem.item != null ? new ItemSlot(new Item(info.defaultItem.item), info.defaultItem.amount) : new ItemSlot());
        }
        player.health.current = player.health.max; // after equipment in case of boni
        player.mana.current = player.mana.max; // after equipment in case of boni
#if !_iMMO2D
        player.isGameMaster = gameMaster;
#endif
        return player;
    }

    void OnServerCharacterCreate(NetworkConnection conn, CharacterCreateMsg message)
    {
        //Debug.Log("OnServerCharacterCreate " + conn);

        // only while in lobby (aka after handshake and not ingame)
        if (lobby.ContainsKey(conn))
        {
            // allowed character name?
            if (IsAllowedCharacterName(message.name))
            {
                // not existent yet?
                string account = lobby[conn];
                if (!Database.singleton.CharacterExists(message.name))
                {
                    // not too may characters created yet?
                    if (Database.singleton.CharactersForAccount(account).Count < characterLimit)
                    {
                        // valid class index?
                        if (0 <= message.classIndex && message.classIndex < playerClasses.Count)
                        {
                            // game master can only be requested by the host.
                            // DO NOT allow regular connections to create GMs!
#if !_iMMO2D
                            if (message.gameMaster == false ||
                                conn == NetworkServer.localConnection)
                            {
#endif
                                // create new character based on the prefab.
#if _iMMOUMACHARACTERS
                                Player player = CreateCharacter(playerClasses[message.classIndex].gameObject, message.name, account, message.gameMaster, message.dna);
#else
#if _iMMO2D
                                Player player = CreateCharacter(playerClasses[message.classIndex].gameObject, message.name, account);
#else
                                Player player = CreateCharacter(playerClasses[message.classIndex].gameObject, message.name, account, message.gameMaster);
#endif
#endif
                                // addon system hooks
                                onServerCharacterCreate.Invoke(message, player);

                                // save the player
                                Database.singleton.CharacterSave(player, false);
                                Destroy(player.gameObject);

                                // send available characters list again, causing
                                // the client to switch to the character
                                // selection scene again
                                conn.Send(MakeCharactersAvailableMessage(account));
#if !_iMMO2D
                        }
                            else
                            {
                                //Debug.Log("character insufficient permissions for GM request: " + conn);  <- don't show on live server
                                ServerSendError(conn, "insufficient permissions", false);
                            }
#endif
                        }
                        else
                        {
                            //Debug.Log("character invalid class: " + message.classIndex);  <- don't show on live server
                            ServerSendError(conn, "character invalid class", false);
                        }
                    }
                    else
                    {
                        //Debug.Log("character limit reached: " + message.name); <- don't show on live server
                        ServerSendError(conn, "character limit reached", false);
                    }
                }
                else
                {
                    //Debug.Log("character name already exists: " + message.name); <- don't show on live server
                    ServerSendError(conn, "name already exists", false);
                }
            }
            else
            {
                //Debug.Log("character name not allowed: " + message.name); <- don't show on live server
                ServerSendError(conn, "character name not allowed", false);
            }
        }
        else
        {
            //Debug.Log("CharacterCreate: not in lobby"); <- don't show on live server
            ServerSendError(conn, "CharacterCreate: not in lobby", true);
        }
    }
}
#endif