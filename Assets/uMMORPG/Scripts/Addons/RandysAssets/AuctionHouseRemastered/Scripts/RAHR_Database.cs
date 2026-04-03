// SQLite-only addon database implementation.
#if _SQLITE

using SQLite;
using System;
using System.Collections.Generic;

public partial class Database {
    class auctionHouse {
        [PrimaryKey, AutoIncrement] public int id { get; set; }
        [NotNull] public long auctionHouseId { get; set; }
        [NotNull] public string category { get; set; }
        [NotNull] public string itemName { get; set; }
        [NotNull] public long level { get; set; }
        [NotNull] public long stackSize { get; set; }
        [NotNull] public long summonedHealth { get; set; }
        [NotNull] public long summonedLevel { get; set; }
        [NotNull] public long summonedExperience { get; set; }
        [NotNull] public long currentBid { get; set; }
        [NotNull] public long buyout { get; set; }
        [NotNull] public long insertFee { get; set; }
        [NotNull] public long ends { get; set; }
        [NotNull] public string seller { get; set; }
        [NotNull] public string bidder { get; set; }
    }
    class auctionHouse_EndedAuctions {
        [PrimaryKey, AutoIncrement] public int id { get; set; }
        [NotNull] public long auctionHouseId { get; set; }
        [NotNull] public string claimableBy { get; set; }
        [NotNull] public string itemName { get; set; }
        [NotNull] public long stackSize { get; set; }
        [NotNull] public long summonedHealth { get; set; }
        [NotNull] public long summonedLevel { get; set; }
        [NotNull] public long summonedExperience { get; set; }
        [NotNull] public long finalValue { get; set; }
        [NotNull] public long insertFee { get; set; }
        [NotNull] public string seller { get; set; }
        [NotNull] public string bidder { get; set; }
        [NotNull] public long outbid { get; set; }
    }
    class auctionHouse_Bids {
        [NotNull] public long auctionHouseId { get; set; }
        [NotNull] public int bids_id { get; set; }
        [NotNull] public string bids_bidder { get; set; }
    }
    class auctionHouse_WithBids : auctionHouse {
        public int bids_id { get; set; }
        public string bids_bidder { get; set; }
    }

    public void Connect_AuctionHouse() {
        connection.CreateTable<auctionHouse>();
        connection.CreateTable<auctionHouse_EndedAuctions>();
        connection.CreateTable<auctionHouse_Bids>();
        connection.CreateIndex(nameof(auctionHouse_Bids), new[] { "bids_id", "bids_bidder" }, true);
    }

    public void AuctionHouse_Insert(long auctionHouseId, string category, string itemName, long level, long stackSize, long summonedHealth, long summonedLevel, long summonedExperience, long currentBid, long buyout, long insertFee, long ends, string seller, string bidder) {
        connection.Insert(new auctionHouse {
            auctionHouseId = auctionHouseId,
            category = category,
            itemName = itemName,
            level = level,
            stackSize = stackSize,
            summonedHealth = summonedHealth,
            summonedLevel = summonedLevel,
            summonedExperience = summonedExperience,
            currentBid = currentBid,
            buyout = buyout,
            insertFee = insertFee,
            ends = ends,
            seller = seller,
            bidder = bidder
        });
    }

    public List<RAHR_AHItem> AuctionHouse_Browse_Search(long auctionHouseId, string category, string subCategory, string itemName, long minLevelRange, long maxLevelRange, int startAt, int limit, RAHR_Columns column, bool asc) {
        List<RAHR_AHItem> listAuctionHouseItems = new List<RAHR_AHItem>();
        string query = "(";
        List<object> paramList = new List<object>();
        paramList.Add(auctionHouseId);

        if(!string.IsNullOrWhiteSpace(category) && string.IsNullOrWhiteSpace(subCategory)) {
            foreach(RAHR_SubCategory subCat in Enum.GetValues(typeof(RAHR_SubCategory))) {
                if(subCat.IsSubcategoryOf(category.ToEnum<RAHR_Category>())) {
                    if(query.Equals("(")) {
                        query += "category=?";
                    } else {
                        query += " OR category=?";
                    }

                    paramList.Add(subCat.ToString());
                }
            }
        } else if(!string.IsNullOrWhiteSpace(subCategory) && string.IsNullOrWhiteSpace(category)) {
            query += "category=?";
            paramList.Add(subCategory);
        }

        query += ")";

        if(!string.IsNullOrWhiteSpace(itemName))
            paramList.Add("%" + itemName + "%");

        paramList.Add((minLevelRange > 0 ? minLevelRange : 1).ToString());
        paramList.Add((minLevelRange > 0 ? (maxLevelRange >= minLevelRange ? maxLevelRange : minLevelRange) : 1).ToString());
        paramList.Add(startAt.ToString());
        paramList.Add(limit.ToString());

        foreach(auctionHouse row in connection.Query<auctionHouse>("SELECT * FROM auctionHouse WHERE auctionHouseId=? AND " + (!query.Equals("()") ? query + " AND " : "") + (!string.IsNullOrWhiteSpace(itemName) ? "(itemName LIKE ?) AND " : "") + "(level BETWEEN ? AND ?) ORDER BY " + column.ToString() + " COLLATE NOCASE " + (asc == true ? "ASC" : "DESC") + " LIMIT ?,?", paramList.ToArray())) {
            listAuctionHouseItems.Add(new RAHR_AHItem(row.id, row.auctionHouseId, row.category, row.itemName, row.level, row.stackSize, row.summonedHealth, row.summonedLevel, row.summonedExperience, row.currentBid, row.buyout, row.insertFee, row.ends, row.seller, row.bidder));
        }

        return listAuctionHouseItems;
    }

    public void AuctionHouse_RemoveItem(long id) {
        connection.BeginTransaction();
        connection.Execute("DELETE FROM auctionHouse WHERE id=?", id);
        connection.Execute("DELETE FROM auctionHouse_Bids WHERE bids_id=?", id);
        connection.Commit();
    }

    public List<RAHR_AHItem> AuctionHouse_GetBySeller(long auctionHouseId, string seller, int startAt, int limit, RAHR_Columns column, bool asc) {
        List<RAHR_AHItem> listAuctionHouseItems = new List<RAHR_AHItem>();

        foreach(auctionHouse row in connection.Query<auctionHouse>("SELECT * FROM auctionHouse WHERE auctionHouseId=? AND seller=? ORDER BY " + column.ToString() + " COLLATE NOCASE " + (asc == true ? "ASC" : "DESC") + " LIMIT ?,?", auctionHouseId, seller, startAt, limit)) {
            listAuctionHouseItems.Add(new RAHR_AHItem(row.id, row.auctionHouseId, row.category, row.itemName, row.level, row.stackSize, row.summonedHealth, row.summonedLevel, row.summonedExperience, row.currentBid, row.buyout, row.insertFee, row.ends, row.seller, row.bidder));
        }

        return listAuctionHouseItems;
    }

    public RAHR_AHItem AuctionHouse_GetAuctionByID(long id) {
        RAHR_AHItem ahItem = new RAHR_AHItem();

        auctionHouse row = connection.FindWithQuery<auctionHouse>("SELECT * FROM auctionHouse WHERE id=?", id);

        if(row != null) {
            ahItem = new RAHR_AHItem(row.id, row.auctionHouseId, row.category, row.itemName, row.level, row.stackSize, row.summonedHealth, row.summonedLevel, row.summonedExperience, row.currentBid, row.buyout, row.insertFee, row.ends, row.seller, row.bidder);
        }

        return ahItem;
    }

    public void AuctionHouse_Bid(long auctionHouseId, long id, string bidder, long bid) {
        connection.BeginTransaction();
        connection.Execute("UPDATE auctionHouse SET currentBid=?, bidder=? WHERE id=?", bid, bidder, id);
        connection.Execute("INSERT OR IGNORE INTO auctionHouse_Bids (auctionHouseId, bids_id, bids_bidder) VALUES (?, ?, ?)", auctionHouseId, id, bidder);
        connection.Commit();
    }

    public string AuctionHouse_GetAccountName(string character) {
        string returnVal = "-1";

        try {
            returnVal = connection.FindWithQuery<characters>("SELECT account FROM characters WHERE name=?", character).account;

            if(string.IsNullOrWhiteSpace(returnVal)) {
                returnVal = "-1";
            }
        } catch {
            returnVal = "-1";
        }

        return returnVal;
    }

    public List<RAHR_AHItem> AuctionHouse_GetBids(long auctionHouseId, string character, int startAt, int limit, RAHR_Columns column, bool asc) {
        List<RAHR_AHItem> listAuctionHouseItems = new List<RAHR_AHItem>();
        foreach(var row in connection.Query<auctionHouse_WithBids>("SELECT * FROM auctionHouse_Bids INNER JOIN auctionHouse ON auctionHouse.id = auctionHouse_Bids.bids_id WHERE bids_bidder=? AND auctionHouse_Bids.auctionHouseId=? ORDER BY " + column.ToString() + " COLLATE NOCASE " + (asc == true ? "ASC" : "DESC") + " LIMIT ?,?", character, auctionHouseId, startAt, limit)) {
            listAuctionHouseItems.Add(new RAHR_AHItem(row.id, row.auctionHouseId, row.category, row.itemName, row.level, row.stackSize, row.summonedHealth, row.summonedLevel, row.summonedExperience, row.currentBid, row.buyout, row.insertFee, row.ends, row.seller, row.bidder));
        }

        return listAuctionHouseItems;
    }

    public List<RAHR_AHItem> AuctionHouse_GetEndedAuctions() {
        List<RAHR_AHItem> listAuctionHouseItems = new List<RAHR_AHItem>();
        long currentEpochTime = RAHR_Utils.GetCurrentEpochTime();

        foreach(auctionHouse row in connection.Query<auctionHouse>("SELECT * FROM auctionHouse WHERE ends<?", currentEpochTime)) {
            listAuctionHouseItems.Add(new RAHR_AHItem(row.id, row.auctionHouseId, row.category, row.itemName, row.level, row.stackSize, row.summonedHealth, row.summonedLevel, row.summonedExperience, row.currentBid, row.buyout, row.insertFee, row.ends, row.seller, row.bidder));
        }

        return listAuctionHouseItems;
    }

    public void AuctionHouse_InsertClaim(RAHR_AHItem ahItem) {
        connection.Insert(new auctionHouse_EndedAuctions {
            auctionHouseId = ahItem.auctionHouseId,
            claimableBy = ahItem.claimableBy,
            itemName = ahItem.itemName,
            stackSize = ahItem.stackSize,
            summonedHealth = ahItem.summonedHealth,
            summonedLevel = ahItem.summonedLevel,
            summonedExperience = ahItem.summonedExperience,
            finalValue = ahItem.finalValue,
            insertFee = ahItem.insertFee,
            seller = ahItem.seller,
            bidder = ahItem.bidder,
            outbid = ahItem.outbid
        });
    }

    public RAHR_AHItem AuctionHouse_GetClaim(long id, string claimableBy, long auctionHouseId) {
        RAHR_AHItem ahItem = new RAHR_AHItem();

        auctionHouse_EndedAuctions row = connection.FindWithQuery<auctionHouse_EndedAuctions>("SELECT * FROM auctionHouse_EndedAuctions WHERE id=? AND claimableBy=? AND auctionHouseId=?", id, claimableBy, auctionHouseId);
        ahItem = new RAHR_AHItem(row.id, row.auctionHouseId, row.claimableBy, row.itemName, row.stackSize, row.summonedHealth, row.summonedLevel, row.summonedExperience, row.finalValue, row.insertFee, row.seller, row.bidder, row.outbid);

        return ahItem;
    }

    public List<RAHR_AHItem> AuctionHouse_GetClaims(long auctionHouseId, string claimableBy, int startAt, int limit, bool asc) {
        List<RAHR_AHItem> listAuctionHouseItems = new List<RAHR_AHItem>();

        foreach(auctionHouse_EndedAuctions row in connection.Query<auctionHouse_EndedAuctions>("SELECT * FROM auctionHouse_EndedAuctions WHERE claimableBy=? AND auctionHouseId=? ORDER BY itemName COLLATE NOCASE " + (asc == true ? "ASC" : "DESC") + " LIMIT ?,?", claimableBy, auctionHouseId, startAt, limit)) {
            listAuctionHouseItems.Add(new RAHR_AHItem(row.id, row.auctionHouseId, row.claimableBy, row.itemName, row.stackSize, row.summonedHealth, row.summonedLevel, row.summonedExperience, row.finalValue, row.insertFee, row.seller, row.bidder, row.outbid));
        }

        return listAuctionHouseItems;
    }

    public void AuctionHouse_RemoveClaim(long id, string claimableBy) {
        connection.Execute("DELETE FROM auctionHouse_EndedAuctions WHERE id=? AND claimableBy=?", id, claimableBy);
    }
    
    public List<string> AuctionHouse_GetBidders(long id) {
        List<string> listBidders = new List<string>();
        
        foreach(auctionHouse_Bids row in connection.Query<auctionHouse_Bids>("SELECT bids_bidder FROM auctionHouse_Bids WHERE bids_id=?", id)) {
            listBidders.Add(row.bids_bidder);
        }

        return listBidders;
    }
}

#endif