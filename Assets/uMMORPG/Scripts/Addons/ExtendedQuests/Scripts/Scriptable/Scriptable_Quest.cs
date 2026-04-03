using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if _iMMOASSETBUNDLEMANAGER
using Jacovone.AssetBundleMagic;
#endif

// SCRIPTABLE QUEST TEMPLATE

[CreateAssetMenu(fileName = "New Quest", menuName = "ADDON/Templates/New Quest", order = 999)]
public partial class Scriptable_Quest : ScriptableObject
{
    [Header("[-=-[ QUEST EXTENTED ]-=-]")]
    [Tooltip("Enter hours between quest attempts, set to 0 to disable")]
    public int repeatable;

    [Tooltip("Show the first reward in tooltip (uncheck if you have more)?")]
    public bool showRewards;

    [TextArea(1, 10)] public string toolTip;

    [Header("[-=-[ QUEST ACCESS REQUIREMENTS ]-=-]")]
    public Tools_Requirements questRequirements;

    [Header("[-=-[ QUEST ACCEPTANCE ]-=-]")]
    [Tooltip("[Optional] Players will receive these items as soon as they accept the quest (removed when quest is cancelled).")]
    public RewardItem[] acceptItems;

    [Tooltip("[Optional] The items gained when accepting the quest will be removed as soon as the quest is complete.")]
    public bool removeAtCompletion;

    [Header("[-=-[ QUEST REWARDS ]-=-]")]
    public QuestReward[] questRewards;

#if _iMMOFACTIONS
    public FactionModifier[] factionModifiers;
#endif

#if _iMMOWORLDEVENTS
    public WorldEventTemplate worldEvent;
    [Range(-99999, 99999)] public int worldEventModifier;
#endif

    [Header("[-=-[ QUEST FULFILLMENT ]-=-]")]
#if _iMMOEXPLORATION
    [Tooltip("Add Exploration Areas from Prefabs [Limit 10]")]
    public Exploration_Area[] exploreTarget;
#endif

#if _iMMOHARVESTING
    [Tooltip("Add Harvest Nodes from Prefabs [Limit 10]")]
    public HarvestTarget[] harvestTarget;
#endif

#if _iMMOCRAFTING
    [Tooltip("Add Crafting Recipes from Resources [Limit 10]")]
    public CraftTarget[] craftTarget;
#endif

#if _iMMOCHEST
    [Tooltip("Add Lootcrate's from Prefabs [Limit 10]")]
    public LootTarget[] lootTarget;
#endif

#if _iMMOFACTIONS
    [Tooltip("Minimum required faction rating")]
    public Quest_Faction factionRequirement;
#endif

    [Tooltip("Add Npc's from Prefabs [Limit 10]")]
    public Npc[] visitTarget;
    public bool visitInOrder;

#if _iMMOPVP
    [Tooltip("Add PVP Targets [Limit 10]")]
    public PvpTarget[] pvpTarget;
#endif

    [Tooltip("Add Monsters from Prefabs [Limit 10]")]
    public KillTarget[] killTarget;

    [Tooltip("Add Items from Resources [Limit 10]")]
    public GatherTarget[] gatherTarget;

    [Tooltip("When checked, the gathered Items wont be removed on quest completion.")]
    public bool DontDestroyGathered;

    [Tooltip("When checked, the Quest will be automatically completed, without the need to return to the Questgiver.")]
    public bool autoCompleteQuest;

    [Header("[-=-[ DESCRIPTION HEADERS ]-=-]")]
    public string headerPvpTarget = "* Kill Players ";
    public string headerKillTarget = "* Kill Monsters ";
    public string headerGatherTarget = "* Gather Items ";
    public string headerVisitTarget = "* Visit Npc's ";
    public string headerExploreTarget = "* Explore Areas ";
    public string headerHarvestTarget = "* Harvest Resources ";
    public string headerCraftTarget = "* Craft Items ";
    public string headerLootTarget = "* Loot Crates ";
    public string headerFactionTarget = "* Faction Requirement ";

#if _iMMOPVP
    [Header("[-=-[ PVP DESCRIPTIONs ]-=-]")]
    public PvpDescription pvpDescription;
#endif

    // -----------------------------------------------------------------------------------
    // Caching
    // -----------------------------------------------------------------------------------
    private static Dictionary<int, Scriptable_Quest> _cache;

    public static Dictionary<int, Scriptable_Quest> All
    {
        get
        {
            if (_cache == null)
            {
                ScripableObjectEntry entry = TemplateConfiguration.singleton.GetEntry(typeof(Scriptable_Quest));
                string folderName = entry != null ? entry.folderName : "";
#if _iMMOASSETBUNDLEMANAGER
                if (entry != null && entry.loadFromAssetBundle)
                    _cache = AssetBundleMagic.LoadBundle(entry.bundleName).LoadAllAssets<Scriptable_Quest>().ToDictionary(x => x.name.GetStableHashCode(), x => x);
                else
                    _cache = Resources.LoadAll<Scriptable_Quest>(folderName).ToDictionary(x => x.name.GetStableHashCode(), x => x);
#else
                _cache = Resources.LoadAll<Scriptable_Quest>("ExtendedQuests").ToDictionary(x => x.name.GetStableHashCode(), x => x);
#endif
            }
            return _cache;
        }
    }

    // -----------------------------------------------------------------------------------
}