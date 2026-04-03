using UnityEngine;
using Mirror;

#if _iMMOCRAFTING
using System.Collections.Generic;
using System.Linq;
#endif

[DisallowMultipleComponent]
public class CraftingProductionValidator : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoInstall()
    {
        NetworkManagerMMO manager = NetworkManager.singleton as NetworkManagerMMO;
        if (manager == null)
            manager = FindObjectOfType<NetworkManagerMMO>();

        if (manager == null)
            return;

        if (manager.GetComponent<CraftingProductionValidator>() == null)
            manager.gameObject.AddComponent<CraftingProductionValidator>();
    }

    [Tooltip("Run once at startup and log warnings for production-breaking crafting setup issues.")]
    public bool runOnStart = true;

    [Tooltip("If true, emits one summary line when validation passes.")]
    public bool logSuccessSummary = true;

    void Start()
    {
        if (!runOnStart)
            return;

        Validate();
    }

    public void Validate()
    {
#if _iMMOCRAFTING
        int warnings = 0;

        Dictionary<int, CraftingProfessionTemplate> professions = CraftingProfessionTemplate.dict;
        if (professions == null || professions.Count == 0)
        {
            warnings++;
            Debug.LogWarning("[CraftingProductionValidator] No CraftingProfessionTemplate assets were loaded. Crafting professions will fail.");
        }

        Dictionary<int, Tmpl_Recipe> recipes = Tmpl_Recipe.All;
        if (recipes == null || recipes.Count == 0)
        {
            warnings++;
            Debug.LogWarning("[CraftingProductionValidator] No crafting recipes were loaded. Crafting UI will be empty.");
        }
        else
        {
            int missingProfessionRefs = recipes.Values.Count(r => r != null && r.requiredCraft == null);
            if (missingProfessionRefs > 0)
            {
                warnings++;
                Debug.LogWarning($"[CraftingProductionValidator] {missingProfessionRefs} recipe(s) are missing requiredCraft references.");
            }
        }

        if (warnings == 0 && logSuccessSummary)
            Debug.Log($"[CraftingProductionValidator] Crafting validation passed ({professions?.Count ?? 0} professions, {recipes?.Count ?? 0} recipes).");
#else
        if (logSuccessSummary)
            Debug.Log("[CraftingProductionValidator] _iMMOCRAFTING define is disabled. Validation skipped.");
#endif
    }
}
