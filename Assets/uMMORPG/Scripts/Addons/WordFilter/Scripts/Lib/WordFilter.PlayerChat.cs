using UnityEngine;

public partial class PlayerChat
{
    [Header("[-=-[ WORD FILTER ]-=-]")]
    public Tmpl_WordFilter wordFilter;
    public string message = "[Sys] World filter detect bad word...";

    // -----------------------------------------------------------------------------------
    // IsAllowedChatText
    // -----------------------------------------------------------------------------------
    public bool IsAllowedChatText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;

        if (!wordFilter || wordFilter.badwords.Length == 0) return true;

        return WordFilter(text.ToLower());
    }

    private void addMessage()
    {
        UIChat.singleton.AddMessage(new ChatMessage("", infoChannel.identifierIn, message, "", infoChannel.textPrefab));
    }
    // -----------------------------------------------------------------------------------
    // WordFilter
    // -----------------------------------------------------------------------------------
    public bool WordFilter(string text)
    {
        foreach (string badword in wordFilter.badwords)
        {
            if (text.Contains(" "+badword.ToLower()+" "))
            {
                 addMessage();
                return false;
            }
        }
        return true;
    }

    // -----------------------------------------------------------------------------------
}