Open PlayerEquipment : 
1 ) Search  public PlayerInventory inventory;

    Add after :
    // Adding events for Addons (Trugord)
    [Header("Events")]
    public UnityEventInt OnRefreshLocation;

2 ) in RefreshLocation function just before close function add this 
        // addon system hooks
        onRefreshLocation.Invoke(index);