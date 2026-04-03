using UnityEngine;

// PLAYER ATTRIBUTES
[System.Serializable]
public partial class _playerAttributes
{
    public AttributeTemplate[] AttributeTypes = { };

    [Tooltip("[Optional] Number of attribute points a new character starts with.")]
    public int startingAttributePoints = 0;

    [Tooltip("[Optional] Number of attribute points rewarded on each reward level.")]
    public int rewardPoints = 1;

    [Tooltip("[Optional] Number of levels a player must achieve between rewards.")]
    public int everyXLevels = 1;

    [Tooltip("[Optional] First level when the rewards start (not counting the initial level).")]
    public int startingRewardLevel = 1;
}

// ATTRIBUTE CACHE
[System.Serializable]
public class AttributeCache
{
    public float timer = 0f;
    public int value = 0;
}