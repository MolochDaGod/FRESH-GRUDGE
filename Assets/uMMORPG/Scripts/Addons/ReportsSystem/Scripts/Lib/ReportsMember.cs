[System.Serializable]
public partial struct ReportsMember
{
    public bool readBefore;         //Was the message already read?
    public string senderAcc;        //Who was the sender?
    public string senderCharacter;  //What character was the sender on?
    public string title;            //What is the report about?
    public string message;          //What are the details about this report?
    public bool solved;             //Was this reports ticket resolved?
    public string time;             //What was the TimeLogout and date the report was made?
    public string position;         //Where was the player when the report was made?
}