using System.Collections;
using System.Linq;
using UnityEngine;

public class NetworkManagerMMOCraftingExtended : MonoBehaviour
{
    public NetworkManagerMMO networkManagerMMO;

#if _SERVER && _iMMOCRAFTING
    // -----------------------------------------------------------------------------------
    // OnStartServer_Tools_DatabaseCleaner
    // -----------------------------------------------------------------------------------
    public void Start()
    {
        networkManagerMMO.onServerCharacterCreate.AddListener(OnServerCharacterCreate_CraftingExtended);
        //temp bandaid fix for loading crafting
        StartCoroutine(LoadDict());
    }
    IEnumerator LoadDict()
    {
        yield return new WaitUntil(() =>
            TemplateConfiguration.singleton != null
        );
        var item = Tmpl_Recipe.All;
    }

    private void OnServerCharacterCreate_CraftingExtended(CharacterCreateMsg message, Player player)
    {
        // Check if a scriptable is  not empty
        if (player.playerCraftingExtended && player.playerCraftingExtended.playerCraftingConfiguration)
        {
            // -- check starting craft professions
            foreach (DefaultCraftingProfession craft in player.playerCraftingExtended.playerCraftingConfiguration.startingCrafts)
            {
                if (!player.playerCraftingExtended.HasCraftingProfession(craft.craftProfession))
                {
                    CraftingProfession tmpProf = new CraftingProfession(craft.craftProfession.name)
                    {
                        experience = craft.startingExp
                    };
                    player.playerCraftingExtended.Crafts.Add(tmpProf);
                }
            }

            // -- check starting recipes
            foreach (Tmpl_Recipe recipe in player.playerCraftingExtended.playerCraftingConfiguration.startingRecipes)
            {
                if (!player.playerCraftingExtended._recipes.Any(r => r == recipe.name))
                    player.playerCraftingExtended._recipes.Add(recipe.name);
            }
        }
        else
        {
            GameLog.LogWarning("PlayerCraftingExtended Component or PlayerCraftingConfiguration is not installer on prefab :" + player.className);
        }
    }
#endif
}
