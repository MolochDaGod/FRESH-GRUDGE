using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//[CreateAssetMenu(menuName = "ADDON/ScriptableRarityColor", order = 999)]
public class ScriptableRarityColor : ScriptableObject
{
    //Couleur qualité item : "Poor" (Grey) < "Common" (White) < "Uncommon" (Green) < "Rare" (Blue) < "Epic" (Purple) < "Legendary" (Orange)

    public Color poor = Color.gray;
    public Color common = Color.white;
    public Color unCommon = Color.green;
    public Color rare = Color.blue;
    public Color Epic = Color.magenta;
    public Color Legendary = Color.yellow;

    /*static ScriptableRarityColor cache;
    public static ScriptableRarityColor Data
    {
        get
        {
            // not loaded yet?
            Debug.Log("lol");
            if (cache == null)
            {
                // get all ScriptableItems in resources
                ScriptableRarityColor raritycolor = Resources.Load<ScriptableRarityColor>("");
                //if(raritycolor.name)
                Debug.Log("r =>" + raritycolor.name);
                // check for duplicates, then add to cache
                cache = raritycolor;
            }
            return cache;
        }
    }*/
    static Dictionary<int, ScriptableRarityColor> cache;
    public static Dictionary<int, ScriptableRarityColor> All
    {
        get
        {
            // not loaded yet?
            if (cache == null)
            {
                // get all ScriptableItems in resources
                ScriptableRarityColor[] items = Resources.LoadAll<ScriptableRarityColor>("ItemRarity");

                // check for duplicates, then add to cache
                List<string> duplicates = items.ToList().FindDuplicates(item => item.name);
                if (duplicates.Count == 0)
                {
                    cache = items.ToDictionary(item => item.name.GetStableHashCode(), item => item);
                }
                else
                {
                    foreach (string duplicate in duplicates)
                        Debug.LogError("Resources folder contains multiple ScriptableItems with the name " + duplicate + ". If you are using subfolders like 'Warrior/Ring' and 'Archer/Ring', then rename them to 'Warrior/(Warrior)Ring' and 'Archer/(Archer)Ring' instead.");
                }
            }
            return cache;
        }
    }
}
