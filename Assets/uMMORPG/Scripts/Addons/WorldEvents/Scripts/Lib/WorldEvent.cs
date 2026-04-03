#if _iMMOWORLDEVENTS
[System.Serializable]
public partial struct WorldEvent
{
    public string name;
    public bool participated;
    public int count;
#if _SERVER
// -----------------------------------------------------------------------------------
    // Modify
    //TODO Client ? server ? faut vérifier
    // -----------------------------------------------------------------------------------
    public void Modify(int value, bool showPopup = true)
    {

        bool bReset = false;

        foreach (WorldEventData data in template.thresholdData)
        {
            if (data.thresholdType == ThresholdType.Below && count > data.thresholdValue && count + value <= data.thresholdValue)
            {
                if (data.limitToThreshold)
                    count = data.thresholdValue;
                else if (data.resetOnThreshold)
                    bReset = true;

                if (!string.IsNullOrWhiteSpace(data.messageOnThreshold) && showPopup)
                    NetworkManagerMMOWorldEvents.BroadCastPopupToOnlinePlayers(template, data.messageParticipantsOnly, data.messageOnThreshold);

                if (data.stopFurtherThresholdChecks)
                    break;
            }
            else if (data.thresholdType == ThresholdType.Above && count < data.thresholdValue && count + value >= data.thresholdValue)
            {
                if (data.limitToThreshold)
                    count = data.thresholdValue;
                else if (data.resetOnThreshold)
                    bReset = true;

                if (!string.IsNullOrWhiteSpace(data.messageOnThreshold) && showPopup)
                    NetworkManagerMMOWorldEvents.BroadCastPopupToOnlinePlayers(template, data.messageParticipantsOnly, data.messageOnThreshold);

                if (data.stopFurtherThresholdChecks)
                    break;
            }
        }

        if (!bReset)
            count += value;
        else
            count = 0;

        participated = true;

    }
#endif
    // -----------------------------------------------------------------------------------
    // WorldEventTemplate (Getter)
    // -----------------------------------------------------------------------------------
    public WorldEventTemplate template
    {
        get { return WorldEventTemplate.All[name.GetStableHashCode()]; }
    }

    // -----------------------------------------------------------------------------------
}
#endif