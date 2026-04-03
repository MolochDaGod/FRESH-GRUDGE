using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

public class UI_GuildWarehouseLog : MonoBehaviour
{

    public TMP_InputField inputField;

    public float refreshTime = 30f;
    private string logData = "";
    private float tempsDerniereExecution = 0.0f;
    // ok donc faut récupéré la liste des logs (disont les 100 derniers) 
    // ensuite les ajouter dans inputfield
    // et la prochaine mise à jours après X secondes (30 secondes par defaut) 
    // on va colorier suivant le added de la base de donnée

    //  levelup > 0 (colorisation vert guild levelup)

    //  ajout gold > jaunes
    // retrait gold > rouge

    // ajout item > jaunes
    // retrait item > rouge

    // deplacement item (movement de slot) > orange
    // split item (division use maintenant 2 slot) > orange
    // fusion de 2 slot > orange

    // Update is called once per frame
    public string prefix = "<b>{0} :</b> <i>{1}</i> ";
    public string groupedItem = "<color=blue>to grouped {0} {1} (Slot {2})</color>";
    public string separateItem = "<color=blue>to separate {0} (Slot {1})</color>";
    public string moveItem = "<color=blue>mooved {0}  {1} (Slot {2})</color>";
    public string upgrade = "<color=green>upgraded the GuildWarehouse to level {0}</color>";
    public string addedItem = "<color=green>added {0}  {1} (Slot {2})</color>";
    public string addedGold = "<color=green>added {0} gold in the account</color>";
    public string removeItem = "<color=red>removed {0}  {1} (Slot {2})</color>";
    public string withdrawgold = "<color=red>withdraw {0} gold from the account</color>";

    /*public void RefreshUI()
    {
        player player = player.localPlayer;
        if (!player) return;

        player.playerAddonsConfigurator.Cmd_LoadGuildWarehouseLogs();
    }*/

    void Update()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        tempsDerniereExecution += Time.fixedDeltaTime;  // ajoute a chaque update le temps écoulé depuis le dernier Update		
        if (tempsDerniereExecution > refreshTime || logData == "")
        {
            player.playerAddonsConfigurator.Cmd_LoadGuildWarehouseLogs();
            UpdateEvent();
            tempsDerniereExecution = 0;
        }
    }

    public void UpdateEvent()
    {
        Player player = Player.localPlayer;

        if (player)
        {
            //player.playerAddonsConfigurator.Cmd_LoadGuildWarehouseLogs();
            logData = "";
            Debug.Log(">" + player.playerAddonsConfigurator.listLogGuildWarehouse.Count);
            if (player.playerAddonsConfigurator.listLogGuildWarehouse.Count > 0)
            {
                foreach (GuildWarehouseLog row in player.playerAddonsConfigurator.listLogGuildWarehouse)
                {

                    logData  += string.Format(prefix, row.logActionDate, row.logPlayerName);
                    // le joueur à upgrade le warehouse
                    if (!string.IsNullOrEmpty(row.logLevelUp) && Convert.ToInt32(row.logLevelUp) > 0)
                    {
                        logData += string.Format(upgrade, row.logLevelUp);
                    }
                    // Ajout / retrait d'or
                    else if (!string.IsNullOrEmpty(row.logGold) && Convert.ToInt32(row.logGold) > 0)
                    {
                        if (Convert.ToInt32(row.logAdded) == 1)
                            logData += string.Format(addedGold, row.logGold);
                        else
                            logData += string.Format(withdrawgold, row.logGold);
                    }
                    // Mouvement des items
                    else if (!string.IsNullOrEmpty(row.logItemName))
                    {
                        // retire un item
                        if (Convert.ToInt32(Convert.ToInt32(row.logAdded)) == 0)
                            logData += string.Format(removeItem, row.logAmout,row.logItemName, row.logSlot);
                        // ajoute un item
                        else if (Convert.ToInt32(Convert.ToInt32(row.logAdded)) == 1)
                            logData += string.Format(addedItem, row.logAmout, row.logItemName, row.logSlot);
                        // deplace un item dans le coffre
                        else if (Convert.ToInt32(Convert.ToInt32(row.logAdded)) == 2)
                            logData += string.Format(moveItem, row.logAmout, row.logItemName, row.logSlot);
                        // split un item dans le coffre
                        else if (Convert.ToInt32(Convert.ToInt32(row.logAdded)) == 3)
                            logData += string.Format(separateItem, row.logAmout, row.logItemName, row.logSlot);
                        // regroupe un item dans le coffre
                        else if (Convert.ToInt32(Convert.ToInt32(row.logAdded)) == 4)
                            logData += string.Format(groupedItem, row.logAmout, row.logItemName, row.logSlot);
                    }
                    else
                        logData += " not find category log";

                    logData += "\r\n";
                }
            }
            else
                logData = "No log or Waiting new update logs (Refresh : "+ refreshTime+"seconds)";
            inputField.text = logData;

        }
    }
}
