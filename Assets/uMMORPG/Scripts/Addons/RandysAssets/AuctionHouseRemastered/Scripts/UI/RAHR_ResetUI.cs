using UnityEngine;

public class RAHR_ResetUI : MonoBehaviour {
    public void OnWindowClose() {
        RAHR_UI_AuctionHouse.singleton.ClearAllResults();
        RAHR_UI_AuctionHouse.singleton.SetActivePanel(RAHR_UI_AuctionHouse.singleton.browse_Panel);
        RAHR_UI_AuctionHouse.singleton.Hide();
    }
}