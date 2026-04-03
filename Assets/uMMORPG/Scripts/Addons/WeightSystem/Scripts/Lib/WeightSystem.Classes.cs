// WEIGHT SYSTEM
[System.Serializable]
public partial struct WeightSystem
{
    public TargetBuffSkill burdenedBuff;
    public int maxWeight;
    public int carryPerPoint;
#if _iMMOATTRIBUTES
    public AttributeTemplate weightAttribute;
#else
#endif
    public int maxBurdenLevel;
    public bool equipmentNotIncluded;

}
