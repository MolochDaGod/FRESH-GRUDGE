using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ===================================================================================
// Leaderboard
// ===================================================================================
public class Leaderboard : MonoBehaviour
{
    [SerializeField] private KeyCode hotkey = KeyCode.L;
    [SerializeField] private Transform trnGrid = null;
    [SerializeField] private GameObject gobEntry = null;
    [SerializeField] private GameObject gobLeaderboardPanel = null;
   // private List<LeaderboardPlayer> leaderboardPlayer = new List<LeaderboardPlayer>();
    private float updateRate = 30f, updateNext = 0f;
    private Player player;

    private int sortCategory = 0;           // 0 = rank / 1 = level / 2 = gold
    private bool sortMode = true;               // 0 = up / 1 = down
    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    private void Update()
    {
        if (player == null) player = Player.localPlayer;
        if (player == null) return;
        
        if (Input.GetKeyDown(hotkey) && !UIUtils.AnyInputActive())
            gobLeaderboardPanel.SetActive(!gobLeaderboardPanel.activeSelf);

        if (Time.time > updateNext && gobLeaderboardPanel.activeSelf)
        {
            player.playerAddonsConfigurator.Cmd_AllPlayersOnline();
            updateNext = Time.time + updateRate;
        }

        if (gobLeaderboardPanel.activeSelf)
        {
            if (player.playerAddonsConfigurator.currentOnlinePlayers.Count <= 0) return;

            if (sortMode)
            {
                //Debug.Log("-> " + sortCategory + " ---> (" + sortMode +", plus grand au plus petit) ");
                if (sortCategory == 6)
                {
                    //Debug.Log("order par name -> " + sortCategory + " ---> (" + sortMode + ", plus grand au plus petit) ");
                    player.playerAddonsConfigurator.currentOnlinePlayers.OrderBy(x => x.name);
                }
                else
                {
                    //Debug.Log("toutes les autres category -> " + sortCategory + " ---> (" + sortMode + ", plus grand au plus petit) ");
                    player.playerAddonsConfigurator.currentOnlinePlayers.OrderBy(x => (
                                    (sortCategory == 0) ? x.rank :
                                    (sortCategory == 1) ? x.level :
                                    (sortCategory == 2) ? x.gold :
                                    (sortCategory == 3) ? x.playerkill :
                                    (sortCategory == 4) ? x.monsterkill :
                                    (sortCategory == 5) ? x.death :
                                    // (sortCategory == 6) ? x.rank :
                                    x.rank));
                }
            }
            else
            {
                //Debug.Log("-> " + sortCategory + " ---> (" + sortMode + ", plus petit au plus grand) ");
                if (sortCategory == 6)
                {
                    //Debug.Log("order par name -> " + sortCategory + " ---> (" + sortMode + ", plus petit au plus grand) ");
                    player.playerAddonsConfigurator.currentOnlinePlayers.OrderByDescending(x => x.name);
                }
                else
                {
                    //Debug.Log("toutes les autres category-> " + sortCategory + " ---> (" + sortMode + ", plus petit au plus grand) ");
                    player.playerAddonsConfigurator.currentOnlinePlayers.OrderByDescending(x => (
                                    (sortCategory == 0) ? x.rank :
                                    (sortCategory == 1) ? x.level :
                                    (sortCategory == 2) ? x.gold :
                                    (sortCategory == 3) ? x.playerkill :
                                    (sortCategory == 4) ? x.monsterkill :
                                    (sortCategory == 5) ? x.death :
                                    // (sortCategory == 6) ? x.rank :
                                    x.rank));
                }
            }


            // Destroy all entries before adding new entries, otherwise endless building list.
            foreach (Transform child in trnGrid)
                Destroy(child.gameObject);

            // Add each player to our leaderboard and set their information.
            int i2 = 1;
            foreach(LeaderboardPlayer entryPlayer in player.playerAddonsConfigurator.currentOnlinePlayers.OrderByDescending(x => (
                                    (sortCategory == 0) ? x.rank :
                                    (sortCategory == 1) ? x.level :
                                    (sortCategory == 2) ? x.gold :
                                    (sortCategory == 3) ? x.playerkill :
                                    (sortCategory == 4) ? x.monsterkill :
                                    (sortCategory == 5) ? x.death :
                                    // (sortCategory == 6) ? x.rank :
                                    x.rank)))
            {
                GameObject entry = Instantiate(gobEntry, trnGrid);
                entry.name = "Entry: Place " + i2;
                LeaderboardEntry lEntry = entry.GetComponent<LeaderboardEntry>();
                lEntry.txtRank.text = i2.ToString();
                lEntry.txtName.text = entryPlayer.name;
                lEntry.txtLevel.text = entryPlayer.level.ToString();
                lEntry.txtGold.text = entryPlayer.gold.ToString();

                lEntry.txtStatistcOne.text = entryPlayer.monsterkill.ToString();
                lEntry.txtStatisticTwo.text = entryPlayer.playerkill.ToString();
                lEntry.txtStatisticThree.text = entryPlayer.death.ToString();
                ++i2;
            }
        }
    }

    // -----------------------------------------------------------------------------------
    public void ChangeCategoryOrder(string categoryName)
    {
        if (categoryName == "rank")
            sortCategory = 0;
        else if (categoryName == "level")
            sortCategory = 1;
        else if(categoryName == "gold")
            sortCategory = 2;
        else if (categoryName == "monsterkill")
            sortCategory = 3;
        else if (categoryName == "playerkill")
            sortCategory = 4;
        else if (categoryName == "death")
            sortCategory = 5;
        else if (categoryName == "name")
            sortCategory = 6;
        else
            sortCategory = 0;
    }
    public void sortingMode()
    {
        sortMode = !sortMode;
    }

    public enum categoryName
    {
        name,
        level,
        gold,
        monsterKill,
        playerkill,
        death
    }
}