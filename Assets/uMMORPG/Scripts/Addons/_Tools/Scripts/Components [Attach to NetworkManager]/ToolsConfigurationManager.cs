#if _iMMOTOOLS
using UnityEngine;

public partial class ToolsConfigurationManager : MonoBehaviour
{
    [Header("Configuration")]
    public TemplateConfiguration configTemplate;

    [Header("Defines")]
    public TemplateDefines addonTemplate;

    [Header("Game Rules")]
    public TemplateGameRules rulesTemplate;
}
#endif