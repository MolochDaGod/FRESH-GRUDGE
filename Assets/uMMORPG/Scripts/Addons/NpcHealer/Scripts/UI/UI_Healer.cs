using UnityEngine;
using UnityEngine.UI;

// UI_Healer
public partial class UI_Healer : MonoBehaviour
{
    public static UI_Healer singleton;

    public GameObject panel;
    public Text descriptionText;
    public Button acceptButton;
    public Button declineButton;

    public string healerText;

    public void Awake()
    {
        if (singleton == null) singleton = this;
    }
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
            Utils.ClosestDistance(player, player.target) <= player.interactionRange&&
            npc.npcHealer != null)
        {

            descriptionText.text = healerText + npc.npcHealer.healingServices.getCost(player);

            acceptButton.interactable = npc.npcHealer.healingServices.Valid(player);

            acceptButton.onClick.SetListener(() =>
            {
                player.Cmd_Healer();
                panel.SetActive(false);
            });

            declineButton.onClick.SetListener(() =>
            {
                panel.SetActive(false);
            });
        }
        else panel.SetActive(false); // hide
    }

    // -----------------------------------------------------------------------------------
}