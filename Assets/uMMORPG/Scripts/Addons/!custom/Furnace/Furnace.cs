using System.Collections.Generic;
using UnityEngine;
using Mirror;



public class Furnace : NetworkBehaviour
{
    public FurnaceType furnaceType = FurnaceType.Campfire;
    [SyncVar, HideInInspector] public ItemSlot ingredientSlot;
    [SyncVar, HideInInspector] public ItemSlot fuelSlot;
    [SyncVar, HideInInspector] public ItemSlot resultSlot;
    [HideInInspector] public bool isOver;

    public string furnaceName;
    // to save computations, we only search if slots have changed.
    // (SyncVar hooks are only called on clients, can't use those on server)
    ItemSlot lastIngredientSlot;
    ItemSlot lastFuelSlot;

    public GameObject BakeEffect;
    // cache all storages on the server to save lots of computations
    // (otherwise we'd have to iterate NetworkServer.objects all the time)
    // -> differentiate them by name. make sure to not use the same name for
    //    two storages.
    //    (sceneId is not a good alternative because it changes when changing
    //     the hierarchy)
    public static Dictionary<string, Furnace> furnaces = new Dictionary<string, Furnace>();

    // bake in progress
    // -> save the recipe during the bake so we don't have to search it again
    public FurnaceRecipe currentBake;
    [SyncVar] public double bakeTimeStart; // so we can show the progress on clients
    [SyncVar] public double bakeTimeEnd;

    public override void OnStartServer()
    {
        furnaceName = Mathf.RoundToInt(transform.position.x).ToString() + Mathf.RoundToInt(transform.position.y).ToString() + Mathf.RoundToInt(transform.position.z).ToString() + Mathf.RoundToInt(transform.rotation.y).ToString();
        // add to storages if none with the same name already exists
        if (!furnaces.ContainsKey(furnaceName))
        {
            furnaces.Add(furnaceName, this); ;

            // load from db
#if _SQLITE
            Database.singleton.LoadFurnace(this);
#endif
        }
        else Debug.LogWarning("A Furnace with name " + furnaceName + " already exists. Use a different name for each Furnace, otherwise it won't be saved to the Database.");
    }

    void OnDestroy()
    {
        furnaces.Remove(name);
    }

    public void OnMouseOver()
    {
        isOver = true;
    }

    public void OnMouseExit()
    {
        isOver = false;
    }
    public void OnMouseDown()
    {
        if (isOver)
        {
            if (Vector3.Distance(Player.localPlayer.transform.position, transform.position) <= Player.localPlayer.interactionRange)
            {
                Player.localPlayer.furnace = this;
                ActivatePanel("FurnacePanel");
            }
        }
    }

    public void ActivatePanel(string panelTag)
    {
        GameObject panel = GameObject.FindGameObjectWithTag(panelTag);
        if (panel != null)
        {
            panel.SetActive(true);
            Debug.Log($"{panelTag} activated.");
        }
        else
        {
            Debug.LogWarning($"{panelTag} not found!");
        }
    }
    // helper function to check if we are currently baking. works on server &
    // client.
    public bool IsBaking() => NetworkTime.time < bakeTimeEnd;

    // start a bake
    [Server]
    public bool StartBaking(FurnaceRecipe recipe)
    {
        // result slot needs to be empty or of same type and not maxstack
        if (resultSlot.amount == 0 ||
            (resultSlot.item.data == recipe.result &&
                resultSlot.amount < resultSlot.item.maxStack))
        {
            // start baking
            currentBake = recipe;
            bakeTimeStart = NetworkTime.time;
            bakeTimeEnd = NetworkTime.time + recipe.bakingTime;
            return true;
        }
        return false;
    }

    [Server]
    public void FinishBaking()
    {
        // only if we have a valid recipe
        if (currentBake != null)
        {
            ItemSlot tempslot = ingredientSlot;
            tempslot.DecreaseAmount(1);
            ingredientSlot = tempslot;

            if (resultSlot.amount > 0 && resultSlot.item.data == currentBake.result)
            {
                ItemSlot resSlot = resultSlot;

                resSlot.IncreaseAmount(1);
                resultSlot = resSlot;
            }
            else
                resultSlot = new ItemSlot(new Item(currentBake.result), 1);

            ItemSlot flSlot = fuelSlot;
            flSlot.DecreaseAmount(1);
            fuelSlot = flSlot;

            currentBake = null;
        }
    }

    [Server]
    public void CancelBaking()
    {
        // cancel baking if needed
        currentBake = null;
        bakeTimeEnd = NetworkTime.time;
    }

    [ServerCallback]
    void Update()
    {
        // currently baking?
        if (IsBaking())
        {
            SetBakingEffect(true);
        }
        // a bake that was in progress is now finished?
        else if (currentBake != null)
        {
            FinishBaking();
            SetBakingEffect(false);
        }
        // not baking. start a new bake if possible.
        else
        {
            SetBakingEffect(false);
            // has ingredient + fuel? and
            if (ingredientSlot.amount > 0 &&
                fuelSlot.amount > 0)
            {
                // only search if something changed to save computations!
                if (!lastIngredientSlot.Equals(ingredientSlot) ||
                    !lastFuelSlot.Equals(fuelSlot))
                {
                    // find recipe (if any)
                    FurnaceRecipe recipe = FurnaceRecipe.Find(ingredientSlot, fuelSlot, furnaceType);
                    if (recipe != null)
                    {
                        // try to start baking
                        StartBaking(recipe);
                    }

                }
            }

            // remember the last searched slot in any case (even if amount == 0)
            lastIngredientSlot = ingredientSlot;
            lastFuelSlot = fuelSlot;
        }
    }
    [ClientRpc]
    public void SetBakingEffect(bool baking)
    {
        BakeEffect.SetActive(baking);
    }
}
