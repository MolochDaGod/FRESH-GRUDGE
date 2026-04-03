using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ScriptableItem
{
    [Header("Research Result")]
    public StableUpgradeCost StableCosts;
    public ScriptableItem StableResult;
}
[Serializable]
public partial class StableUpgradeCost
{

    [Tooltip("Next level upgrade: Gold Cost")]
    public long gold;

    [Tooltip("Next level upgrade: Items Cost")]
    public Tools_ItemRequirement[] requiredItems;
}
