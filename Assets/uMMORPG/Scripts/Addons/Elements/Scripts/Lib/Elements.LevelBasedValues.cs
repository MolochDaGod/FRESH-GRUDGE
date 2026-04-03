using System;

[Serializable]
public struct LevelBasedElement
{
    public ElementTemplate template;
    public float baseValue;
    public float bonusPerLevel;

    public float Get(int level)
    {
        return baseValue + bonusPerLevel * (level - 1);
    }
}