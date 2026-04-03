using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if _SQLITE
public class RandysAuctionHouseRemastered : MonoBehaviour {
    public static RandysAuctionHouseRemastered singleton;

    private NetworkManagerMMO networkManagerMMO;

    public float auctionHouseInitialProcess = 15F; //The initial time after starting server a check is ran to process ended auctions (15 = 15 seconds)
    public float auctionHouseProcessInterval = 900F; //The time after the initial process to repeat processing ended auctions (900 = 15 mins)

    public RandysAuctionHouseRemastered() {
        if(singleton == null) singleton = this;
    }

    void Start() {
        if(!networkManagerMMO) {
            networkManagerMMO = GameObject.Find("NetworkManager").GetComponent<NetworkManagerMMO>();
        }

        networkManagerMMO.onStartServer.AddListener(OnStartServer);
        Database.singleton.onConnected.AddListener(Database.singleton.Connect_AuctionHouse);

    }

    void OnStartServer() {
        InvokeRepeating("AuctionHouseProcessAuctions", auctionHouseInitialProcess, auctionHouseProcessInterval);
    }

    private void AuctionHouseProcessAuctions() {
        Debug.Log("Processing check for ended auctions");

        List<RAHR_AHItem> listAHItems = Database.singleton.AuctionHouse_GetEndedAuctions();

        foreach(RAHR_AHItem ahItem in listAHItems) {
            RAHR_AHItem sellerItem = new RAHR_AHItem(ahItem);
            sellerItem.claimableBy = sellerItem.seller;
            sellerItem.finalValue = string.IsNullOrWhiteSpace(sellerItem.bidder) ? 0 : sellerItem.currentBid;
            Database.singleton.AuctionHouse_InsertClaim(sellerItem);

            //update sellers auctions/claims tab if its shown
            Player.onlinePlayers.TryGetValue(sellerItem.seller, out Player seller);
            if(seller) {
                seller.Target_AuctionHouse_BidBuyoutMessage(seller.connectionToClient, RAHR_AuctionReturnStatus.SellerUpdate);
            }

            if(!string.IsNullOrWhiteSpace(ahItem.bidder)) {
                RAHR_AHItem bidderItem = new RAHR_AHItem(ahItem);
                bidderItem.finalValue = bidderItem.currentBid;
                bidderItem.claimableBy = bidderItem.bidder;
                bidderItem.outbid = 0;
                Database.singleton.AuctionHouse_InsertClaim(bidderItem);

                //update all bidders bids/claims tab if its shown
                List<string> bidderList = Database.singleton.AuctionHouse_GetBidders(bidderItem.id);

                foreach(string bidder in bidderList) {
                    Player.onlinePlayers.TryGetValue(bidder, out Player pBidder);
                    if(pBidder) {
                        pBidder.Target_AuctionHouse_BidBuyoutMessage(pBidder.connectionToClient, RAHR_AuctionReturnStatus.BidderUpdate);
                    }
                }
            }

            Database.singleton.AuctionHouse_RemoveItem(ahItem.id);
        }
    }
}

#endif