using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

[Serializable]
public struct SkillbarEntry
{
    public string reference;
    public KeyCode hotKey;
}

[RequireComponent(typeof(PlayerEquipment))]
[RequireComponent(typeof(PlayerInventory))]
[RequireComponent(typeof(PlayerSkills))]
public class PlayerSkillbar : NetworkBehaviour
{
    [Header("Components")]
    public PlayerEquipment equipment;
    public PlayerInventory inventory;
    public PlayerSkills skills;
    private Player _player;

    [Header("Skillbar")]
    public SkillbarEntry[] slots =
    {
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha1},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha2},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha3},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha4},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha5},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha6},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha7},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha8},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha9},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha0},
    };

    public override void OnStartLocalPlayer()
    {
        // load skillbar after player data was loaded
        Load();
    }

    public override void OnStopClient()
    {
        if (isLocalPlayer)
            Save();
    }

    // skillbar ////////////////////////////////////////////////////////////////
    //[Client] <- disabled while UNET OnDestroy isLocalPlayer bug exists
    void Save()
    {
        // save skillbar to player prefs (based on player name, so that
        // each character can have a different skillbar)
        for (int i = 0; i < slots.Length; ++i)
            PlayerPrefs.SetString(name + "_skillbar_" + i, slots[i].reference);

        // force saving playerprefs, otherwise they aren't saved for some reason
        PlayerPrefs.Save();
    }

    [Client]
    void Load()
    {
        _player = GetComponent<Player>();

        // Get initial skill list
        for (int nc = 0; nc < _player.equipment.slots.Count; nc++)
        {
            ScriptableItem slotItem = _player.equipment.slots[nc].amount > 0 ? _player.equipment.slots[nc].item.data : null;
            // New item contains data
            var newEquipmentItemData = slotItem as WeaponItem; // only one cast
            if (newEquipmentItemData != null)
            {
                if (newEquipmentItemData.scriptableWeaponSkillList != null)
                {
                    // Clear the SkillBar of existing skills
                    for (int nc2 = 0; nc2 < _player.skillbar.slots.Length; nc2++)
                    {
                        _player.skillbar.slots[nc2].reference = "";
                    }

                    int skillIndex = 0;
                    // Add each skill from the weaponSkills list to the skillbar
                    foreach (ScriptableSkill skill in newEquipmentItemData.scriptableWeaponSkillList.weaponSkills)
                    {
                        // TODO: Check if player meets skill requirements. If not, don't add skill to Skillbar. 
                        _player.skillbar.slots[skillIndex].reference = skill.name;
                        skillIndex++;
                        //  Debug.Log(skill.name);
                    }
                }
            }
        }
    }
}
