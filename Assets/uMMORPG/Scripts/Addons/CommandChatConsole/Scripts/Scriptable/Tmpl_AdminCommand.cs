using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if _iMMOASSETBUNDLEMANAGER
using Jacovone.AssetBundleMagic;
#endif

// ADMIN COMMAND - TEMPLATE
[CreateAssetMenu(fileName = "AdminCommand", menuName = "ADDON/Templates/New AdminCommand", order = 999)]
public class Tmpl_AdminCommand : ScriptableObject
{
    [Header("[-=-[ Admin Command ]-=-]")]
    public string commandName;

    public string functionName;
    [Range(1, 100)] public int commandLevel;
    public AdminCommandArgument[] arguments;
    public string helpText;

    // -----------------------------------------------------------------------------------
    // getFormat
    // -----------------------------------------------------------------------------------
    public string getFormat()
    {
        string format = "";

        format += commandName + " ";

        foreach (AdminCommandArgument arg in arguments)
        {
            format += "<" + arg.argumentType.ToString() + "> ";
        }

        return format;
    }

    // -----------------------------------------------------------------------------------
    // Caching
    // -----------------------------------------------------------------------------------
    private static Dictionary<int, Tmpl_AdminCommand> _cache;

    public static Dictionary<int, Tmpl_AdminCommand> dict
    {
        get
        {
            if (_cache == null)
            {
                ScripableObjectEntry entry = TemplateConfiguration.singleton.GetEntry(typeof(Tmpl_AdminCommand));
                string folderName = entry != null ? entry.folderName : "";
#if _iMMOASSETBUNDLEMANAGER
                if (entry != null && entry.loadFromAssetBundle)
                    _cache = AssetBundleMagic.LoadBundle(entry.bundleName).LoadAllAssets<Tmpl_AdminCommand>().ToDictionary(x => x.name.GetStableHashCode(), x => x);
                else
                    _cache = Resources.LoadAll<Tmpl_AdminCommand>(folderName).ToDictionary(x => x.name.GetStableHashCode(), x => x);
#else
                _cache = Resources.LoadAll<Tmpl_AdminCommand>("CommandChatConsole").ToDictionary(x => x.name.GetStableHashCode(), x => x);
#endif
            }

            return _cache;

        }
    }
    // -----------------------------------------------------------------------------------
}