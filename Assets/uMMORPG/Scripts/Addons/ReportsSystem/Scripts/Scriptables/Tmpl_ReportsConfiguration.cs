using UnityEngine;

[CreateAssetMenu( fileName ="Report Configuration", menuName = "ADDON/Templates/Reports Configuration", order = 1)]
public class Tmpl_ReportsConfiguration : ScriptableObject
{
    [Header("[-=-[ General Configuration ]-=-]")]
    public bool enableReport = true;

    [Header("[-=-[ Reports Configuration ]-=-]")]
    public int timeToReport = 15;   //The wait TimeLogout between reports
    public ChannelInfo messageInfo = new ChannelInfo("", "(Reports)", "(Reports)", null);

    [Header("[-=-[ Message ]-=-]")]
    public bool editMessage = false;
    [BoolShowConditional(conditionFieldName: "editMessage", conditionValue: true)]
    public string messageCantReport = "Can't report right now; because you reported a problem less than {0} minutes ago. Time remaining: {1} minutes.";
    [BoolShowConditional(conditionFieldName: "editMessage", conditionValue: true)]
    public string messageSent = "Report sent successfully.";
    [BoolShowConditional(conditionFieldName: "editMessage", conditionValue: true)]
    public string messageReportDisabled = "Sorry, the report is temporarily disabled";
}
