using UnityEngine;

[System.Serializable]
public class RAHR_DurationFee {
    [Header("Duration in days")]
    public int duration = 1;

    [Header("Insertion Fee - Percentage of vendor sell value")]
    [Tooltip("Fee * VendorSellValue (Will be rounded to nearest whole number)")]
    public float insertFee = 0.1F;

    public RAHR_DurationFee() { }

    public RAHR_DurationFee(int duration, float insertFee) {
        this.duration = duration;
        this.insertFee = insertFee;
    }
}