using System.Text.RegularExpressions;
using UnityEngine;

public partial class NetworkManagerMMO
{
    [Header("[Component: Word Filter]")]
    public NetworkManagerMMOWordFilter networkManagerMMOWordFilter;
    // -----------------------------------------------------------------------------------
    // IsAllowedCharacterName
    // -----------------------------------------------------------------------------------
#if _iMMOWORDFILTER
    public bool IsAllowedCharacterName(string characterName)
    {
        if (string.IsNullOrWhiteSpace(characterName)) return false;

        if (networkManagerMMOWordFilter.wordFilter == null || networkManagerMMOWordFilter.wordFilter.badwords.Length == 0) return true;

        return characterName.Length <= characterNameMaxLength &&
                Regex.IsMatch(characterName, @"^[a-zA-Z0-9_]+$") &&
                CharacterNameFilter(characterName.ToLower());
    }
#endif
    // -----------------------------------------------------------------------------------
    // WordFilter
    // -----------------------------------------------------------------------------------
    public bool CharacterNameFilter(string text)
    {
        foreach (string badName in networkManagerMMOWordFilter.wordFilter.badwords)
        {
            if (text.Contains(badName.ToLower()))
                return false;
        }

        return true;
    }
    // -----------------------------------------------------------------------------------
}