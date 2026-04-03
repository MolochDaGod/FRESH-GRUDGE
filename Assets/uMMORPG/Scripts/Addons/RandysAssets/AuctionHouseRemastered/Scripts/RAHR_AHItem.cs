[System.Serializable]
public class RAHR_AHItem {
    public long id;
    public long auctionHouseId;
    public string category;
    public string itemName;
    public long level;
    public long stackSize;
    public long summonedHealth;
    public long summonedLevel;
    public long summonedExperience;
    public long currentBid;
    public long buyout;
    public long insertFee;
    public long ends;
    public string seller;
    public string bidder;

    public string claimableBy;
    public long finalValue;
    public long outbid;

    public RAHR_AHItem() {
        id = -1;
    }

    public RAHR_AHItem(RAHR_AHItem AHItem) {
        id = AHItem.id;
        auctionHouseId = AHItem.auctionHouseId;
        category = AHItem.category;
        itemName = AHItem.itemName;
        level = AHItem.level;
        stackSize = AHItem.stackSize;
        summonedHealth = AHItem.summonedHealth;
        summonedLevel = AHItem.summonedLevel;
        summonedExperience = AHItem.summonedExperience;
        currentBid = AHItem.currentBid;
        buyout = AHItem.buyout;
        insertFee = AHItem.insertFee;
        ends = AHItem.ends;
        seller = AHItem.seller;
        bidder = AHItem.bidder;

        claimableBy = AHItem.claimableBy;
        finalValue = AHItem.finalValue;
        outbid = AHItem.outbid;
    }

    public RAHR_AHItem(long id, long auctionHouseId, string category, string itemName, long level, long stackSize, long summonedHealth, long summonedLevel, long summonedExperience, long currentBid, long buyout, long insertFee, long ends, string seller, string bidder) {
        this.id = id;
        this.auctionHouseId = auctionHouseId;
        this.category = category;
        this.itemName = itemName;
        this.level = level;
        this.stackSize = stackSize;
        this.summonedHealth = summonedHealth;
        this.summonedLevel = summonedLevel;
        this.summonedExperience = summonedExperience;
        this.currentBid = currentBid;
        this.buyout = buyout;
        this.insertFee = insertFee;
        this.ends = ends;
        this.seller = seller;
        this.bidder = bidder;
    }

    public RAHR_AHItem(long id, long auctionHouseId, string claimableBy, string itemName, long stackSize, long summonedHealth, long summonedLevel, long summonedExperience, long finalValue, long insertFee, string seller, string bidder, long outbid) {
        this.id = id;
        this.auctionHouseId = auctionHouseId;
        this.claimableBy = claimableBy;
        this.itemName = itemName;
        this.stackSize = stackSize;
        this.summonedHealth = summonedHealth;
        this.summonedLevel = summonedLevel;
        this.summonedExperience = summonedExperience;
        this.finalValue = finalValue;
        this.insertFee = insertFee;
        this.seller = seller;
        this.bidder = bidder;
        this.outbid = outbid;
    }
}