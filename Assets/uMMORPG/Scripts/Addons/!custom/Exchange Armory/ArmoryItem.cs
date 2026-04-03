using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ScriptableItem
{
    [Header("Research Result")]
    public ArmoryUpgradeCost ArmoryCosts;
    public ScriptableItem ArmoryResult;
}
[Serializable]
public partial class ArmoryUpgradeCost
{

    [Tooltip("Next level upgrade: Gold Cost")]
    public long gold;

    [Tooltip("Next level upgrade: Items Cost")]
    public Tools_ItemRequirement[] requiredItems;
}
