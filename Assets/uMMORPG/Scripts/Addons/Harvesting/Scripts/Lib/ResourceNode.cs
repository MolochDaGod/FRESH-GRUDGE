using Mirror;
using System.Collections;
using System.Linq;
//using System.Linq;
using UnityEngine;

// RESOURCE NODE
public partial class ResourceNode : Tools_InteractableObject
{
#if _iMMOHARVESTING
    [Header("[-=-[ RESOURCE NODE (See Tooltips) ]-=-]")]
    public ParticleSystem lootIndicator;

    [Header("[-=-[ player Force Look at Resources Node ]-=-]")]
    public bool playerLookAtResourcesNode;

    [Header("[-=-[ 3D/2D Model ]-=-]")]
    [Tooltip("[Optional] Display 3D/2D Model, if true select the 3D/2D Model.")]
    public bool hideModelEmpty;

    [Tooltip("[Optional] Displayed 3D/2D Model.")]
    public GameObject modelDisplay;

    [Tooltip("[Required] Base duration (in seconds) it takes to harvest")]
    [Range(1, 999)] public float harvestDuration = 2;

    [Tooltip("[Optional] Profession experience per harvest attempt min")]
    public int ProfessionExperienceRewardMin = 0;
    [Tooltip("[Optional] Profession experience per harvest attempt max")]
    public int ProfessionExperienceRewardMax = 2;

    [Tooltip("[Optional] Mana cost per harvest attempt")]
    public int manaCost;
#if _iMMOSTAMINA
    [Tooltip("[Optional] stamina cost per harvest attempt")]
    public int staminaCost;
#endif
    [Header("[RESOURCES]")]
    [Tooltip("[Optional] Harvested items available")]
    public HarvestingHarvestItems[] harvestItems;

    [Tooltip("[Optional] Harvested items available on critical result")]
    public HarvestingHarvestItems[] harvestCriticalItems;

    [Tooltip("[Optional] Resources node automatically unspawns if the amount of resources has been collected from it (set to 0 to disable)")]
    public int totalResourcesMin = 1;
    public int totalResourcesMax = 5;

    [Header("[RESPAWN]")]
    [Tooltip("[Optional] How long the node stays empty in the scene when it has been completely harvested (in Seconds)")]
    public float emptyTime = 5f;

    [Tooltip("[Optional] Will the node respawn once it got harvested? Otherwise its available only once.")]
    public bool respawn = true;

    [Tooltip("[Optional] How long it takes for a empty object to respawn (in Seconds) with full resources again. Set higher than 0 !")]
    public float respawnTime = 10f;

    [Header("[ANIMATION & SOUND]")]
    [Tooltip("[Optional] GameObject spawned as effect when successfully harvested (see ReadMe).")]
    public GameObject harvestEffect;

    [Tooltip("[Optional] Sound played when successfully harvested.")]
    public AudioClip harvestSound;

    [Header("[MESSAGES]")]
    public string experienceMessage = " Profession Exp gained.";

    public string levelUpMessage = "Profession level up: ";
    public string harvestMessage = "You harvested: ";
    public string requirementsMessage = "Harvesting Requirements not met!";
    public string depletedMessage = "No resources left!";
    public string successMessage = "Harvest successful!";
    public string criticalSuccessMessage = "Critical harvest success!";
    public string failedMessage = "Harvest Failed!";
    public string breakMessage = "Your tool broke!";
    public string boosterMessage = "You used a booster!";

    [Tooltip("[Optional] Shown while harvesting the resource node.")]
    public string accessLabel;

    [HideInInspector] public int baseSuccessFactor = 10;

    private double emptyTimeEnd;
    private double respawnTimeEnd;

    [SyncVar] private int currentResources;
    [SyncVar] [HideInInspector] public bool _isUsed = false;

    public GameEvent eventHarvestingLoot;

    public readonly SyncList<ItemSlot> inventory = new SyncList<ItemSlot>();

    public override void OnStartServer()
    {
        //ShowNode();
        OnRefill();
        inventory.Callback += OnHarvestingProfessionUpdated;
    }

    public override void OnStartClient()
    {
        inventory.Callback += OnHarvestingProfessionUpdated;
    }
    // -----------------------------------------------------------------------------------
    // Start
    // -----------------------------------------------------------------------------------
    //public override void Start()
   // {
        /*if (!isClient) return;
        base.Start();

        inventory.Callback += OnHarvestingProfessionUpdated;*/
    //}
    // -----------------------------------------------------------------------------------
    // OnHarvestingProfessionUpdated
    // @Client
    // -----------------------------------------------------------------------------------
    void OnHarvestingProfessionUpdated(SyncList<ItemSlot>.Operation op, int index, ItemSlot oldIvalue, ItemSlot newValue)
    {
        if (isClient)
        {
            UpdateClient();
            
        }
        if(isServer)
            UpdateServer();
    }

    // -----------------------------------------------------------------------------------
    // OnInteractClient
    // @Client
    // -----------------------------------------------------------------------------------
    [ClientCallback]
    public override void OnInteractClient(Player player)
    {
        player.movement.Reset();
        //Debug.Log("BIPP");
        if (checkInteractionRange(player))
            player.playerHarvesting.OnSelect_ResourceNode(this);
        else
        {
            Vector3 destination = collider.ClosestPointOnBounds(gameObject.transform.position);
            player.movement.Navigate(destination, player.interactionRange);
        }
    }


    // -----------------------------------------------------------------------------------
    // FiniteStateMachineEvents
    // -----------------------------------------------------------------------------------
    public bool EventDepleted()
    {
        return (totalResourcesMax > 0 && currentResources <= 0 && !HasResources());
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    private bool EventEmptyTimeElapsed()
    {
        return !IsHidden() && currentResources <= 0 && !HasResources() && NetworkTime.time >= emptyTimeEnd;
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    private bool EventRespawnTimeElapsed()
    {
        return IsHidden() && respawn && NetworkTime.time >= respawnTimeEnd;
    }

    // -----------------------------------------------------------------------------------
    // OnHarvested
    // -----------------------------------------------------------------------------------
    public void OnHarvested()
    {
        if (HasResources())
            SpawnEffect(harvestEffect, harvestSound);
    }

    // -----------------------------------------------------------------------------------
    // UpdateClient
    // -----------------------------------------------------------------------------------
    [Client]
    protected void UpdateClient()
    {/*
        Debug.Log("Current Resources" + currentResources);
        int itemRest = 0;
        foreach (var item in inventory)
        {
            itemRest += item.amount;
        }
        Debug.Log("Rest :" + itemRest);
        if ((HasResources() || !IsDepleted()))
        {
            if (hideModelEmpty) modelDisplay.SetActive(true);
            collider.enabled = true;
        }
        else if ((!HasResources() && IsDepleted()))
        {
            if (hideModelEmpty) modelDisplay.SetActive(false);
            collider.enabled = false;
        }*/
        eventHarvestingLoot.TriggerEvent();
    }

    // -----------------------------------------------------------------------------------
    // UpdateServer
    // -----------------------------------------------------------------------------------
    [Server]
    protected void UpdateServer()
    {
        //Debug.Log("Serveur call");
        if (_isUsed && !IsHidden() && !HasResources()) // si non caché et qu'il n'a plus de resources
        {
            //Debug.Log("je passe ici");
            Invoke(nameof(HideNode), emptyTime);
        }
    }

    [Server]
    public void UsedNode()
    {
        _isUsed = true;
        Invoke(nameof(HideNode),  emptyTime);
        //Debug.Log((float)(emptyTime - NetworkTime.time));
    }

    [Server]
    private void HideNode()
    {
        //Debug.Log("lol..... x2");
        inventory.Clear();
        if (respawn)
        {
            Hide();
            netIdentity.visible = Visibility.ForceHidden;
           
            Invoke(nameof(ShowNode), respawnTime);
        }
        else
            NetworkServer.Destroy(gameObject);
    }

    [Server]
    private void ShowNode()
    {
        _isUsed = false;
        currentResources = Random.Range(totalResourcesMin, totalResourcesMax);
        OnRefill();
        Show();
        netIdentity.visible = Visibility.Default;
    }


    // -----------------------------------------------------------------------------------
    // OnDepleted
    // -----------------------------------------------------------------------------------
    [Server]
    public void OnDepleted()
    {
        emptyTimeEnd = NetworkTime.time + emptyTime;
        respawnTimeEnd = emptyTimeEnd + respawnTime;
    }

    // -----------------------------------------------------------------------------------
    // CanStartHarvest
    // -----------------------------------------------------------------------------------
   /* public bool CanStartHarvest()
    {
        bool amountInInventory = false;
        foreach (ItemSlot item in inventory)
        {
            if (item.amount > 0)
            {
                amountInInventory = true;
                break;
            }
        }
        Debug.Log(amountInInventory + " <-> " + !inventory.Any(item => item.amount > 0));
        return ((totalResourcesMax <= 0 || currentResources > 0) && !inventory.Any(item => item.amount > 0));
    }*/

    // -----------------------------------------------------------------------------------
    // RefillResources
    // -----------------------------------------------------------------------------------
    [ServerCallback]
    public void OnRefill()
    {
        inventory.Clear();
        int resCount = 0;
        foreach (HarvestingHarvestItems harvestItem in harvestItems)
        {
            if (Random.value <= harvestItem.probability)
            {
                int amount = (Random.Range(harvestItem.minAmount, harvestItem.maxAmount));
                for (int i = 1; i <= amount; i++)
                    inventory.Add(new ItemSlot(new Item(harvestItem.template)));
                resCount += amount;

                if (resCount >= currentResources) break;
            }
        }

        currentResources -= resCount;
    }

    // -----------------------------------------------------------------------------------
    // OnCritical
    // -----------------------------------------------------------------------------------
    [ServerCallback]
    public void OnCritical()
    {
        int resCount = 0;

        foreach (HarvestingHarvestItems harvestItem in harvestCriticalItems)
        {
            if (UnityEngine.Random.value <= harvestItem.probability)
            {
                int amount = (Random.Range(harvestItem.minAmount, harvestItem.maxAmount));
                for (int i = 1; i <= amount; i++)
                    inventory.Add(new ItemSlot(new Item(harvestItem.template)));
                resCount += amount;

                if (resCount >= currentResources) break;
            }
        }

        currentResources -= resCount;
    }

    // -----------------------------------------------------------------------------------
    // IsDepleted
    // -----------------------------------------------------------------------------------
    public bool IsDepleted()
    {
        return currentResources <= 0 && totalResourcesMax > 0;
    }

    // -----------------------------------------------------------------------------------
    // HasResources
    // -----------------------------------------------------------------------------------
    public bool HasResources()
    {
        foreach (ItemSlot item in inventory)
        {
            if (item.amount > 0)
                return true;
        }
        return false;
    }
#else
    public bool addonNotActivated;
#endif
    // -----------------------------------------------------------------------------------
}
