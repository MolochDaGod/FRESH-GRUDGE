using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ScriptableItem
{
    [Header("Research Result")]
    public ResearchUpgradeCost researchCosts;
    public ScriptableItem researchResult;
}
[Serializable]
public partial class ResearchUpgradeCost
{

    [Tooltip("Next level upgrade: Gold Cost")]
    public long gold;

    [Tooltip("Next level upgrade: Items Cost")]
    public Tools_ItemRequirement[] requiredItems;
}
