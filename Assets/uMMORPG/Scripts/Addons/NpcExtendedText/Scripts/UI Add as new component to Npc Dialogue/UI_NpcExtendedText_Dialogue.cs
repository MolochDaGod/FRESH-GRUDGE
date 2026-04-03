using UnityEngine;

// UI_NpcExtendedText_Dialogue
public partial class UI_NpcExtendedText_Dialogue : MonoBehaviour
{
    public GameObject panel;

    protected bool init;

    // -----------------------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------------------
    private void Update()
    {
        Player player = Player.localPlayer;

        // use collider point(s) to also work with big entities
        if (player != null &&
            player.target != null &&
            player.target is Npc npc &&
            Utils.ClosestDistance(player, player.target) <= player.interactionRange &&
            npc.npcExtendedTexts != null)
        {
            if (!init)
            {
                npc.welcome = npc.npcExtendedTexts.UpdateNpcExtendedText(player);
                init = true;
            }
        }
        else
        {
            init = false;
        }
    }

    // -----------------------------------------------------------------------------------
}