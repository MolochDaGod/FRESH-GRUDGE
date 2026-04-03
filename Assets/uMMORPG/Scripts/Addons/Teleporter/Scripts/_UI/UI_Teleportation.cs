using UnityEngine;
using UnityEngine.UI;

// ===================================================================================
// TELEPORTATION UI
// ===================================================================================
#if _iMMOTELEPORTER
[RequireComponent(typeof(GameEventListener))]
public partial class UI_Teleportation : MonoBehaviour
{
    public GameObject panel;
    public Slot_Teleportation slotPrefab;
    public Transform content;
    
    public string labelTeleport = "Teleport to: ";
    public string teleporterOutOfRangeMessage = "Sorry you're out of teleporter range!";

#if _CLIENT
    private void OnEnable()
    {
        UpdateEvent();
    }
    // -----------------------------------------------------------------------------------
    // Update
    // @Client
    // -----------------------------------------------------------------------------------

    public void UpdateEvent()
    {
        Player player = Player.localPlayer;
        if (!player) return;
        panel.SetActive(true);
        if (player.target != null && player.target is Npc npc && Utils.ClosestDistance(player, player.target) <= player.interactionRange)
        {
            UIUtils.BalancePrefabs(slotPrefab.gameObject, npc.npcTeleporter.teleportationDestinations.Length, content);

            for (int i = 0; i < npc.npcTeleporter.teleportationDestinations.Length; ++i)
            {
                int index = i;

                Slot_Teleportation slot = content.GetChild(index).GetComponent<Slot_Teleportation>();

                slot.actionButton.interactable = npc.npcTeleporter.teleportationDestinations[index].teleportationRequirement.checkRequirements(player);

                slot.actionButton.GetComponentInChildren<Text>().text = labelTeleport + npc.npcTeleporter.teleportationDestinations[index].teleportationTarget.name;

                slot.actionButton.onClick.SetListener(() =>
                {
                    if (player.target != null && player.target is Npc npc && Utils.ClosestDistance(player, player.target) <= player.interactionRange)
                    {
                        player.Cmd_NpcWarp(index);
                        panel.SetActive(false);
                    }
                    else
                    {
                        player.Tools_AddMessage(teleporterOutOfRangeMessage);
                        panel.SetActive(false);
                    }
                });
            }

            
        }
        else
        {
            panel.SetActive(false);
        }
    }
#endif
    // -----------------------------------------------------------------------------------
}

#endif
