using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_NpcDialogue : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text welcomeText;
    public Transform offerPanel;
    public GameObject offerButtonPrefab;

    public void UpdateEvent()
    {
        Player player = Player.localPlayer;

        // use collider point(s) to also work with big entities
        if (player != null &&
            panel.activeSelf &&
            player.target != null &&
            player.target is Npc npc &&
            Utils.ClosestDistance(player, player.target) <= player.interactionRange)
        {
            // welcome text
            welcomeText.text = npc.welcome;

            // count amount of valid offers
            int validOffers = 0;
            foreach (NpcOffer offer in npc.offers)
                if (offer.HasOffer(player))
                    ++validOffers;

            // instantiate enough buttons
            UIUtils.BalancePrefabs(offerButtonPrefab, validOffers, offerPanel);

            // show a button for each valid offer
            int index = 0;
            foreach (NpcOffer offer in npc.offers)
            {
                if (offer.HasOffer(player))
                {
                    Button button = offerPanel.GetChild(index).GetComponent<Button>();
                    button.GetComponentInChildren<TMP_Text>().text = offer.GetOfferName();
                    button.onClick.SetListener(() => {
                        offer.OnSelect(player);
                    });
                    ++index;
                }
            }
        }
        else panel.SetActive(false);
    }

    public void Show() { panel.SetActive(true); }
}