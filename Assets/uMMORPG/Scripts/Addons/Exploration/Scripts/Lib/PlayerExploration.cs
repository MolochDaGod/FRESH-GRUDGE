using UnityEngine;
using Mirror;

// PLAYER
public partial class PlayerAddonsConfigurator
{
    [HideInInspector] public Exploration_Area myExploration;

    public readonly SyncList<string> exploredAreas = new SyncList<string>();

    protected double fTimerCache;
    protected double fTimerInterval = 10f;

    private void Start_Exploration()
    {
        if (!isServer && !isClient) return;
#if _SERVER
        // player.combat.onKilledEnemy.AddListener(QuestsOnKilled);
#endif

#if _CLIENT
        //player.inventory.onInventoryChanged.AddListener(CheckCompletionQuest); //! For auto quest gather problem numer refresh
        exploredAreas.Callback += OnExploredAreaUpdated;
#endif
    }


#if _CLIENT
    // -----------------------------------------------------------------------------------
    // OnHarvestingProfessionUpdated
    // @Client
    // -----------------------------------------------------------------------------------
    void OnExploredAreaUpdated(SyncList<string>.Operation op, int index, string oldIvalue, string newValue)
    {
        //UIQuests.TriggerEvent();
        //UIQuestTracker.TriggerEvent();
    }
#endif

// -----------------------------------------------------------------------------------
// ExploreArea
// -----------------------------------------------------------------------------------
#if _SERVER
    [ServerCallback]
    public void ExploreArea()
    {
        if (myExploration && myExploration.explorationRequirements.isActive && myExploration.explorationRequirements.checkRequirements(player))
        {
            // -- explore the area
            if (!exploredAreas.Contains(myExploration.name))
            {
                exploredAreas.Add(myExploration.name);

                myExploration.explorationRewards.gainRewards(player);

                var msg = myExploration.explorePopup.message + myExploration.name;

                player.Tools_MinimapSceneText(myExploration.name);
                fTimerCache = NetworkTime.time + fTimerInterval;

            }
            // -- show notice if already explored
            else if (myExploration.noticeOnEnter)
            {
                if (NetworkTime.time <= fTimerCache) return;
                var msg = myExploration.enterPopup.message + myExploration.name;

                player.Tools_MinimapSceneText(myExploration.name);
                fTimerCache = NetworkTime.time + fTimerInterval;
            }


#if _iMMOQUESTS
            // Require visited Area for update Quests
            player.playerExtendedQuest.IncreaseExplorationCounterFor(myExploration.name);
#endif
        }
    }
#endif
    // -----------------------------------------------------------------------------------
    // HasExploredArea
    // -----------------------------------------------------------------------------------
    public bool HasExploredArea(Exploration_Area simpleExplorationArea)
    {
        return exploredAreas.Contains(simpleExplorationArea.name);
    }
}