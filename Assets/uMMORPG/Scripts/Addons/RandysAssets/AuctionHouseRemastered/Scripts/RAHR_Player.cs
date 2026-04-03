using Mirror;
using System.Collections.Generic;
using UnityEngine;

#if _SQLITE
public partial class Player {
    [HideInInspector] public int auctionHouse_SellIndice = -1;

    [Header("~~~ Auction House Remastered ~~~")]
    public long auctionHouseId = 0;
    public RAHR_DurationFee[] auctionHouse_DurationFee = new RAHR_DurationFee[3] { new RAHR_DurationFee(1, 0.1F), new RAHR_DurationFee(3, 0.2F), new RAHR_DurationFee(5, 0.3F) };
    public float auctionHouse_FinalValueFee = 0.1F;
    public int auctionHouse_ItemsPerPage = 20;

    [TargetRpc]
    public void Target_AuctionHouse_BrowseResults(NetworkConnection target, string json) {
        RAHR_UI_AuctionHouse.singleton.Requested_Browse_SearchedItems(json);
    }

    [TargetRpc]
    public void Target_AuctionHouse_BidsResults(NetworkConnection target, string json) {
        RAHR_UI_AuctionHouse.singleton.Requested_Bids_BuyersActiveBids(json);
    }

    [TargetRpc]
    public void Target_AuctionHouse_AuctionsResults(NetworkConnection target, string json) {
        RAHR_UI_AuctionHouse.singleton.Requested_Auctions_SellersActiveAuctions(json);
    }

    [TargetRpc]
    public void Target_AuctionHouse_ClaimResults(NetworkConnection target, string json) {
        RAHR_UI_AuctionHouse.singleton.Requested_Claims(json);
    }

    [TargetRpc]
    public void Target_AuctionHouse_BidBuyoutMessage(NetworkConnection target, RAHR_AuctionReturnStatus auctionReturnStatus) {
        RAHR_UI_AuctionHouse.singleton.ReturnedMessage(auctionReturnStatus);
    }

    [Command]
    public void Cmd_AuctionHouse_BrowseRequestSearch(string category, string subCategory, string itemName, long minLevelRange, long maxLevelRange, int startAt, RAHR_Columns column, bool asc) {
        List<RAHR_AHItem> ahItems = Database.singleton.AuctionHouse_Browse_Search(auctionHouseId, category, subCategory, itemName, minLevelRange, maxLevelRange, startAt, auctionHouse_ItemsPerPage, column, asc);
        string json = RAHR_JsonHelper.ToJson<RAHR_AHItem>(ahItems.ToArray());
        Target_AuctionHouse_BrowseResults(connectionToClient, json);
    }

    [Command]
    public void Cmd_AuctionHouse_RequestMyBids(int startAt, RAHR_Columns column, bool asc) {
        List<RAHR_AHItem> ahItems = Database.singleton.AuctionHouse_GetBids(auctionHouseId, name, startAt, auctionHouse_ItemsPerPage, column, asc);
        string json = RAHR_JsonHelper.ToJson<RAHR_AHItem>(ahItems.ToArray());
        Target_AuctionHouse_BidsResults(connectionToClient, json);
    }

    [Command]
    public void Cmd_AuctionHouse_PostItem(int indice, long startingPrice, long buyoutPrice, int durationFeeIndex) {
        if(indice > -1) {
            if(startingPrice > 0 && buyoutPrice > startingPrice) {
                ItemSlot itemSlot = inventory.slots[indice];

                if(itemSlot.item.tradable) {
                    if(!itemSlot.item.summoned) {
                        ScriptableItem sItem;
                        ScriptableItem.All.TryGetValue(inventory.slots[indice].item.name.GetStableHashCode(), out sItem);

                        if(sItem) {
                            long itemLevel = ((UsableItem)sItem).minLevel;
                            long ends = RAHR_Utils.AddDaysToCurrentEpochTime(auctionHouse_DurationFee[durationFeeIndex].duration);
                            long deposit = itemSlot.item.sellPrice * itemSlot.amount;
                            deposit = (deposit * auctionHouse_DurationFee[durationFeeIndex].insertFee).ToLong();

                            if(gold >= deposit) {
                                gold -= deposit;
                                Database.singleton.AuctionHouse_Insert(auctionHouseId, sItem.auctionHouseCategory.ToString(), itemSlot.item.name, itemLevel, itemSlot.amount, itemSlot.item.summonedHealth, itemSlot.item.summonedLevel, itemSlot.item.summonedExperience, startingPrice, buyoutPrice, deposit, ends, name, "");
                                itemSlot.DecreaseAmount(itemSlot.amount);
                                inventory.slots[indice] = itemSlot;
                                Target_AuctionHouse_BidBuyoutMessage(connectionToClient, RAHR_AuctionReturnStatus.AuctionCreated);
                            } else {
                                Target_AuctionHouse_BidBuyoutMessage(connectionToClient, RAHR_AuctionReturnStatus.NotEnoughGold);
                            }
                        } else {
                            Target_AuctionHouse_BidBuyoutMessage(connectionToClient, RAHR_AuctionReturnStatus.Error);
                        }
                    } else {
                        Target_AuctionHouse_BidBuyoutMessage(connectionToClient, RAHR_AuctionReturnStatus.Summoned);
                    }
                } else {
                    Target_AuctionHouse_BidBuyoutMessage(connectionToClient, RAHR_AuctionReturnStatus.NotTradable);
                }
            } else {
                Target_AuctionHouse_BidBuyoutMessage(connectionToClient, RAHR_AuctionReturnStatus.StartingPricesInvalid);
            }
        } else {
            Target_AuctionHouse_BidBuyoutMessage(connectionToClient, RAHR_AuctionReturnStatus.InventoryItemInvalid);
        }
    }

    [Command]
    public void Cmd_AuctionHouse_RequestBySeller(int startAt, RAHR_Columns column, bool asc) {
        List<RAHR_AHItem> ahItems = Database.singleton.AuctionHouse_GetBySeller(auctionHouseId, name, startAt, auctionHouse_ItemsPerPage, column, asc);
        string json = RAHR_JsonHelper.ToJson<RAHR_AHItem>(ahItems.ToArray());
        Target_AuctionHouse_AuctionsResults(connectionToClient, json);
    }

    [Command]
    public void Cmd_AuctionHouse_Bid(long id, long myBid) {
        if(gold >= myBid) {
            RAHR_AHItem ahItem = Database.singleton.AuctionHouse_GetAuctionByID(id);

            if(ahItem.id != -1) {
                string sellersAccount = Database.singleton.AuctionHouse_GetAccountName(ahItem.seller);

                if(!sellersAccount.Equals(account)) {
                    if(myBid > ahItem.currentBid) {
                        if(!string.IsNullOrWhiteSpace(ahItem.bidder)) {
                            //GIVE PREVIOUS BIDDER BACK THEIR BID
                            RAHR_AHItem outbidAHItem = new RAHR_AHItem(ahItem);
                            outbidAHItem.claimableBy = ahItem.bidder;
                            outbidAHItem.finalValue = ahItem.currentBid;
                            outbidAHItem.outbid = 1;
                            Database.singleton.AuctionHouse_InsertClaim(outbidAHItem);
                        }

                        if(myBid < ahItem.buyout) {
                            Database.singleton.AuctionHouse_Bid(auctionHouseId, id, name, myBid);
                            gold -= myBid;
                            Target_AuctionHouse_BidBuyoutMessage(connectionToClient, RAHR_AuctionReturnStatus.BidSuccess);

                            //update sellers auctions/claims tab if its shown
                            onlinePlayers.TryGetValue(ahItem.seller, out Player seller);
                            if(seller) {
                                Target_AuctionHouse_BidBuyoutMessage(seller.connectionToClient, RAHR_AuctionReturnStatus.SellerUpdate);
                            }

                            //update all bidders bids/claims tab if its shown
                            List<string> bidderList = Database.singleton.AuctionHouse_GetBidders(ahItem.id);

                            foreach(string bidder in bidderList) {
                                onlinePlayers.TryGetValue(bidder, out Player pBidder);
                                if(pBidder) {
                                    pBidder.Target_AuctionHouse_BidBuyoutMessage(pBidder.connectionToClient, RAHR_AuctionReturnStatus.BidderUpdate);
                                }
                            }
                        } else if(myBid >= ahItem.buyout) {
                            ahItem.bidder = name;
                            ahItem.finalValue = ahItem.buyout;
                            ahItem.outbid = 0;

                            //Give Buyer Claim
                            RAHR_AHItem buyerAHItem = new RAHR_AHItem(ahItem);
                            buyerAHItem.claimableBy = name;

                            gold -= ahItem.finalValue;
                            Database.singleton.AuctionHouse_InsertClaim(buyerAHItem);

                            //update all bidders bids/claims tab if its shown
                            List<string> bidderList = Database.singleton.AuctionHouse_GetBidders(buyerAHItem.id);

                            foreach(string bidder in bidderList) {
                                onlinePlayers.TryGetValue(bidder, out Player pBidder);
                                if(pBidder) {
                                    pBidder.Target_AuctionHouse_BidBuyoutMessage(pBidder.connectionToClient, RAHR_AuctionReturnStatus.BidderUpdate);
                                }
                            }

                            //Give Seller Claim
                            RAHR_AHItem sellerAHItem = new RAHR_AHItem(ahItem);
                            sellerAHItem.claimableBy = sellerAHItem.seller;
                            Database.singleton.AuctionHouse_InsertClaim(sellerAHItem);

                            //update sellers auctions/claims tab if its shown
                            onlinePlayers.TryGetValue(sellerAHItem.seller, out Player seller);
                            if(seller) {
                                Target_AuctionHouse_BidBuyoutMessage(seller.connectionToClient, RAHR_AuctionReturnStatus.SellerUpdate);
                            }

                            //Remove Item From Auction House
                            Database.singleton.AuctionHouse_RemoveItem(ahItem.id);
                            Target_AuctionHouse_BidBuyoutMessage(connectionToClient, RAHR_AuctionReturnStatus.BuyoutSuccess);
                        }
                    } else {
                        Target_AuctionHouse_BidBuyoutMessage(connectionToClient, RAHR_AuctionReturnStatus.BidTooLow);
                    }
                } else {
                    Target_AuctionHouse_BidBuyoutMessage(connectionToClient, RAHR_AuctionReturnStatus.CannotBidYourAuction);
                }
            } else {
                Target_AuctionHouse_BidBuyoutMessage(connectionToClient, RAHR_AuctionReturnStatus.ItemDoesntExist);
            }
        } else {
            Target_AuctionHouse_BidBuyoutMessage(connectionToClient, RAHR_AuctionReturnStatus.NotEnoughGold);
        }
    }

    [Command]
    public void Cmd_AuctionHouse_Buyout(long id) {
        RAHR_AHItem ahItem = Database.singleton.AuctionHouse_GetAuctionByID(id);

        if(ahItem.id != -1) {
            string sellersAccount = Database.singleton.AuctionHouse_GetAccountName(ahItem.seller);

            if(!sellersAccount.Equals(account)) {
                if(gold >= ahItem.buyout) {
                    if(!string.IsNullOrWhiteSpace(ahItem.bidder)) {
                        //GIVE PREVIOUS BIDDER BACK THEIR BID
                        RAHR_AHItem outbidAHItem = new RAHR_AHItem(ahItem);
                        outbidAHItem.claimableBy = ahItem.bidder;
                        outbidAHItem.finalValue = ahItem.currentBid;
                        outbidAHItem.outbid = 1;
                        Database.singleton.AuctionHouse_InsertClaim(outbidAHItem);
                    }

                    ahItem.bidder = name;
                    ahItem.finalValue = ahItem.buyout;
                    ahItem.outbid = 0;

                    //Give Buyer Claim
                    RAHR_AHItem buyerAHItem = new RAHR_AHItem(ahItem);
                    buyerAHItem.claimableBy = name;

                    gold -= ahItem.finalValue;
                    Database.singleton.AuctionHouse_InsertClaim(buyerAHItem);

                    //Give Seller Claim
                    RAHR_AHItem sellerAHItem = new RAHR_AHItem(ahItem);
                    sellerAHItem.claimableBy = sellerAHItem.seller;
                    Database.singleton.AuctionHouse_InsertClaim(sellerAHItem);

                    //update sellers auctions/claims tab if its shown
                    onlinePlayers.TryGetValue(sellerAHItem.seller, out Player seller);
                    if(seller) {
                        Target_AuctionHouse_BidBuyoutMessage(seller.connectionToClient, RAHR_AuctionReturnStatus.SellerUpdate);
                    }

                    //update all bidders bids/claims tab if its shown
                    List<string> bidderList = Database.singleton.AuctionHouse_GetBidders(ahItem.id);

                    foreach(string bidder in bidderList) {
                        onlinePlayers.TryGetValue(bidder, out Player pBidder);
                        if(pBidder) {
                            pBidder.Target_AuctionHouse_BidBuyoutMessage(pBidder.connectionToClient, RAHR_AuctionReturnStatus.BidderUpdate);
                        }
                    }

                    //Remove Item From Auction House
                    Database.singleton.AuctionHouse_RemoveItem(ahItem.id);
                    Target_AuctionHouse_BidBuyoutMessage(connectionToClient, RAHR_AuctionReturnStatus.BuyoutSuccess);
                } else {
                    Target_AuctionHouse_BidBuyoutMessage(connectionToClient, RAHR_AuctionReturnStatus.NotEnoughGold);
                }
            } else {
                Target_AuctionHouse_BidBuyoutMessage(connectionToClient, RAHR_AuctionReturnStatus.CannoyBuyYourAuction);
            }
        } else {
            Target_AuctionHouse_BidBuyoutMessage(connectionToClient, RAHR_AuctionReturnStatus.ItemDoesntExist);
        }
    }

    [Command]
    public void Cmd_AuctionHouse_CancelAuction(long id) {
        RAHR_AHItem ahItem = Database.singleton.AuctionHouse_GetAuctionByID(id);

        if(ahItem.id != -1) {
            if(ahItem.seller.Equals(name)) {
                if(string.IsNullOrWhiteSpace(ahItem.bidder)) {
                    ahItem.finalValue = 0;
                    ahItem.claimableBy = ahItem.seller;
                    Database.singleton.AuctionHouse_InsertClaim(ahItem);
                    Database.singleton.AuctionHouse_RemoveItem(ahItem.id);
                    Target_AuctionHouse_BidBuyoutMessage(connectionToClient, RAHR_AuctionReturnStatus.AuctionCanceled);
                } else {
                    Target_AuctionHouse_BidBuyoutMessage(connectionToClient, RAHR_AuctionReturnStatus.ContainsBidders);
                }
            }
        } else {
            Target_AuctionHouse_BidBuyoutMessage(connectionToClient, RAHR_AuctionReturnStatus.ItemDoesntExist);
        }
    }

    [Command]
    public void Cmd_AuctionHouse_GetClaims(int startAt, bool asc) {
        List<RAHR_AHItem> ahItems = Database.singleton.AuctionHouse_GetClaims(auctionHouseId, name, startAt, auctionHouse_ItemsPerPage, asc);
        string json = RAHR_JsonHelper.ToJson<RAHR_AHItem>(ahItems.ToArray());
        Target_AuctionHouse_ClaimResults(connectionToClient, json);
    }

    [Command]
    public void Cmd_AuctionHouse_Claim(long id) {
        RAHR_AHItem ahItem = Database.singleton.AuctionHouse_GetClaim(id, name, auctionHouseId);

        if(ahItem.id != -1) {
            if(name.Equals(ahItem.seller)) {
                if(ahItem.finalValue == 0) {
                    ScriptableItem scriptableItem;
                    ScriptableItem.All.TryGetValue(ahItem.itemName.GetStableHashCode(), out scriptableItem);

                    if(scriptableItem) {
                        Item item = new Item(scriptableItem);
                        item.summonedExperience = ahItem.summonedExperience;
                        item.summonedHealth = ahItem.summonedHealth.ToInt32();
                        item.summonedLevel = ahItem.summonedLevel.ToInt32();

                        if(inventory.CanAdd(item, ahItem.stackSize.ToInt32())) {
                            inventory.Add(item, ahItem.stackSize.ToInt32());
                            Database.singleton.AuctionHouse_RemoveClaim(ahItem.id, name);
                            Target_AuctionHouse_BidBuyoutMessage(connectionToClient, RAHR_AuctionReturnStatus.ClaimSuccess);
                        } else {
                            Target_AuctionHouse_BidBuyoutMessage(connectionToClient, RAHR_AuctionReturnStatus.InventoryFull);
                        }
                    } else {
                        Target_AuctionHouse_BidBuyoutMessage(connectionToClient, RAHR_AuctionReturnStatus.Error);
                    }
                } else {
                    long finalValueFee = (ahItem.finalValue * auctionHouse_FinalValueFee).ToLong();
                    long claimableGold = (ahItem.finalValue - finalValueFee) + ahItem.insertFee;

                    gold += claimableGold;
                    Database.singleton.AuctionHouse_RemoveClaim(ahItem.id, name);
                    Target_AuctionHouse_BidBuyoutMessage(connectionToClient, RAHR_AuctionReturnStatus.ClaimSuccess);
                }
            } else if(name.Equals(ahItem.bidder)) {
                if(ahItem.outbid == 1) {
                    gold += ahItem.finalValue;
                    Database.singleton.AuctionHouse_RemoveClaim(ahItem.id, name);
                    Target_AuctionHouse_BidBuyoutMessage(connectionToClient, RAHR_AuctionReturnStatus.ClaimSuccess);
                } else {
                    ScriptableItem scriptableItem;
                    ScriptableItem.All.TryGetValue(ahItem.itemName.GetStableHashCode(), out scriptableItem);

                    if(scriptableItem) {
                        Item item = new Item(scriptableItem);
                        item.summonedExperience = ahItem.summonedExperience;
                        item.summonedHealth = ahItem.summonedHealth.ToInt32();
                        item.summonedLevel = ahItem.summonedLevel.ToInt32();

                        if(inventory.CanAdd(item, ahItem.stackSize.ToInt32())) {
                            inventory.Add(item, ahItem.stackSize.ToInt32());
                            Database.singleton.AuctionHouse_RemoveClaim(ahItem.id, name);
                            Target_AuctionHouse_BidBuyoutMessage(connectionToClient, RAHR_AuctionReturnStatus.ClaimSuccess);
                        } else {
                            Target_AuctionHouse_BidBuyoutMessage(connectionToClient, RAHR_AuctionReturnStatus.InventoryFull);
                        }
                    } else {
                        Target_AuctionHouse_BidBuyoutMessage(connectionToClient, RAHR_AuctionReturnStatus.Error);
                    }
                }
            }
        } else {
            Target_AuctionHouse_BidBuyoutMessage(connectionToClient, RAHR_AuctionReturnStatus.ItemDoesntExist);
        }
    }

    void OnDragAndDrop_InventorySlot_AuctionsSellItemSlot(int[] slotIndices) {
        if(inventory.slots[slotIndices[0]].item.tradable)
            auctionHouse_SellIndice = slotIndices[0];
    }
}

#endif