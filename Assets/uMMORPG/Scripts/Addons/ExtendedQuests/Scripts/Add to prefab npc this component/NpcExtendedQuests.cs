using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// talk-to-npc quests work by adding the same quest to two npcs, one with
// accept=true and complete=false, the other with accept=false and complete=true
[Serializable]
public struct ScriptableExtendedQuestOffer
{
    public Scriptable_Quest extendedQuest;
    public bool acceptHere;
    public bool completeHere;
}

public class NpcExtendedQuests : NpcOffer
{
    public Npc npc;

    [Header("Button Extended quest")]
    public string buttonName = "Extended Quests";

    [Header("Event")]
    public GameEvent UINpcExtendedQuest;

    [Header("Text Meshes")]
    public GameObject AcceptObject;  // Reference to the GameObject for accepting quests  
    public GameObject CompleteObject; // Reference to the GameObject for completing quests

    [Header("[-=-[Overlay Update Interval]-=-]")]
#pragma warning disable CS0414
    [SerializeField] [Range(0.01f, 3f)] private float updateInterval = 0.5f;
#pragma warning restore
    protected float fInterval;
    public bool enableInterval = true;

    [Header("[-=-[Extended Quests]-=-]")]
    public ScriptableExtendedQuestOffer[] quests;

    public override bool HasOffer(Player player) =>
        QuestsVisibleFor(player).Count > 0;

    public override string GetOfferName() => buttonName;

    public override void OnSelect(Player player)
    {
        UINpcExtendedQuest.TriggerEvent();
        UINpcDialogue.singleton.panel.SetActive(false);
    }

    // helper function to find a quest index by name
    public int GetIndexByName(string questName)
    {
        // (avoid Linq because it is HEAVY(!) on GC and performance)
        for (int i = 0; i < quests.Length; ++i)
            if (quests[i].extendedQuest.name == questName)
                return i;
        return -1;
    }

    // helper function to filter the quests that are shown for a player
    // -> all quests that:
    //    - can be started by the player
    //    - or were already started but aren't completed yet
    public List<Scriptable_Quest> QuestsVisibleFor(Player player)
    {
        // search manually. Linq is HEAVY(!) on GC and performance
        List<Scriptable_Quest> visibleQuests = new List<Scriptable_Quest>();
        foreach (ScriptableExtendedQuestOffer entry in quests)
            if (entry.acceptHere && player.playerExtendedQuest.CanAcceptQuest(entry.extendedQuest) ||
                entry.completeHere && player.playerExtendedQuest.HasActiveQuest(entry.extendedQuest.name))
                visibleQuests.Add(entry.extendedQuest);
        return visibleQuests;
    }

    public bool CanPlayerCompleteAnyQuestHere(PlayerExtendedQuest playerQuests)
    {
        foreach (ScriptableExtendedQuestOffer entry in quests)
            if (entry.completeHere && playerQuests.CanCompleteQuest(entry.extendedQuest.name))
                return true;
        return false;
    }

    public bool CanPlayerAcceptAnyQuestHere(PlayerExtendedQuest playerQuests)
    {
        foreach (ScriptableExtendedQuestOffer entry in quests)
            if (entry.acceptHere && playerQuests.CanAcceptQuest(entry.extendedQuest))
                return true;
        return false;
    }

#if _CLIENT && (!UNITY_SERVER || UNITY_EDITOR )
    void FixedUpdate()
    {
        Player player = Player.localPlayer;
        if (!player) return;
        if (player.playerExtendedQuest)
        {
            if (Time.time > fInterval && enableInterval)
            {
                SlowUpdate();
                fInterval = Time.time + updateInterval;
            }
            else if (!enableInterval)
            {
                SlowUpdate();
            }
        }
        else
        {
            GameLog.LogWarning("WARNING : "+ player.className + " not have ExtendedQuest addon setup !");
        }
    }

    private void SlowUpdate()
    {
        if (Player.localPlayer.playerExtendedQuest)
        {
            // Find local player (null while in character selection)
            if (Player.localPlayer != null)
            {
                // Complete Quest  
                if (CanPlayerCompleteAnyQuestHere(Player.localPlayer.playerExtendedQuest))
                {
                    CompleteObject.SetActive(true); // Show complete object  
                    AcceptObject.SetActive(false); // Hide accept object  
                }
                // Accept Quest  
                else if (CanPlayerAcceptAnyQuestHere(Player.localPlayer.playerExtendedQuest))
                {
                    AcceptObject.SetActive(true); // Show accept object  
                    CompleteObject.SetActive(false); // Hide complete object  
                }
                // Nothing  
                else
                {
                    AcceptObject.SetActive(false); // Hide accept object  
                    CompleteObject.SetActive(false); // Hide complete object  
                }
            }
        }
    }
#endif
}
