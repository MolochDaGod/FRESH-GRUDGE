using System;
using UnityEngine;

// -----------------------------------------------------------------------------------
// PlaceableObjectUpgradeCost
// -----------------------------------------------------------------------------------
[Serializable]
public partial class PlaceableObjectUpgradeCost
{
    [Tooltip("Minimum level of owner in order to upgrade")]
    public int minLevel;

    [Tooltip("Next level upgrade: Duration in seconds")]
    public float duration;

    [Tooltip("Next level upgrade: Gold Cost")]
    public long gold;

    [Tooltip("Next level upgrade: Coins Cost")]
    public long coins;

    [Tooltip("Next level upgrade: Items Cost")]
    public Tools_ItemRequirement[] requiredItems;
}