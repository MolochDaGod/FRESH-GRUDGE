// HAZARD BUFF
[System.Serializable]
public class HazardBuff
{
    public TargetBuffSkill buff;
    public int minBuffLevel;
    public int maxBuffLevel;
    public float chance = 1f;
    public string protectiveMessage = "You are protected against the Hazard Floor effects!";
    public Tools_ActivationRequirements protectiveRequirements;
}