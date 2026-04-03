using UnityEngine;

// Travelroute

[System.Serializable]
public class Travelroute
{
    [Header("[-=-[ TRAVELROUTE ]-=-]")]
    [Tooltip("[Required] Any on scene Transform or GameObject OR off scene coordinates (requires Network Zones AddOn)")]
    public Tools_TeleportationTarget teleportationTarget;

    [Tooltip("[Optional] Price calculated based on distance of current position and destination (or fixed price on off scene)")]
    public float GoldPricePerUnit;

#if _iMMOHONORSHOP

    [Header("[-=-[ Honor Currency Cost ]-=-]")]
    [Tooltip("[Optional] Total price is calculated based on distance of current position and destination")]
    public HonorShopCurrencyCost[] currencyCost;

#endif
}
