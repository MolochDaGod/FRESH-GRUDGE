using Mirror;
using System.Collections;
using System.Linq;
using UnityEngine;

#if _iMMOCHEST

// LOOTCRATE

public partial class Lootcrate : Tools_InteractableObject
{
    [Header("[-=-[ LOOTCRATE (See Tooltips) ]-=-]")]
    public Animator animator;

    public ParticleSystem lootIndicator;

    [Tooltip("Base duration (in seconds) it takes to access (open)")]
    public float accessDuration;

    [Header("[LOOT]")]
    [Tooltip("[Optional] Minimum amount of gold available")]
    public int lootGoldMin = 0;

    [Tooltip("[Optional] Maximum amount of gold available")]
    public int lootGoldMax = 10;

    [Tooltip("[Optional] Minimum amount of coins (premium currency) available")]
    public int lootCoinsMin = 0;

    [Tooltip("[Optional] Maximum amount of coins (premium currency) available")]
    public int lootCoinsMax = 5;

    [Tooltip("[Optional] Loot Items available")]
    public ItemDropChance[] dropChances;

    [Header("[RESPAWN]")]
    [Tooltip("[Optional] How long the lootcrate stays around, when it has been plundered (in seconds)")]
    public float emptyTime = 10f;

    [Tooltip("[Optional] If the lootcrate is respawned once it got plundered. Otherwise its available only once.")]
    public bool respawn = true;

    [Tooltip("[Optional] How long it takes for a lootcrate to respawn (in seconds) with loot again (0 to disable).")]
    public float respawnTime = 30f;

    [Header("[ANIMATION & SOUND]")]
    [Tooltip("[Optional] Name of player animation parameter that is played when accessing (leave empty to disable).")]
    public string playerAnimation = "";

    [Tooltip("[Optional] Name of animation parameter that is played when opening (leave empty to disable).")]
    public string crateAnimationOpen = "OPEN";

    [Tooltip("[Optional] Name of animation parameter that is played when closing (leave empty to disable).")]
    public string crateAnimationClose = "CLOSE";

    [Header("[-=-[ Player Force Look at lootcrate ]-=-]")]
    public bool playerLookAtLootCrate;

    [Header("[-=-[ 3d Model ]-=-]")]
    [Tooltip("[Optional] Display 3d Model, if true select the 3d Model.")]
    public bool hide3dModelEmpty;
    
    [Tooltip("[Optional] Displayed 3d Model.")]
    public GameObject model3d;

    [Tooltip("[Optional] Sound played when opening.")]
    public AudioClip soundOpen;

    [Tooltip("[Optional] Sound played when closing.")]
    public AudioClip soundClose;

    [Tooltip("[Optional] GameObject spawned as effect when successfully looted (see ReadMe).")]
    public GameObject lootEffect;

    [Tooltip("[Optional] Sound played when successfully looted.")]
    public AudioClip lootSound;

    [Header("[MESSAGES]")]
    [Tooltip("[Optional] Message shown to the player when opening the lootcrate.")]
    public string successMessage;

    [Tooltip("[Optional] Message shown to the player when the access requirements are not met.")]
    public string lockedMessage;

    [Tooltip("[Optional] Shown while opening the lootcrate.")]
    public string accessLabel;

    private bool lastOpen = false;
    private double emptyTimeEnd;
    private double respawnTimeEnd;

    [SyncVar, HideInInspector] public long coins;
    [SyncVar, HideInInspector] public long gold;
    [SyncVar, HideInInspector] public bool _open = false;

    public readonly SyncList<ItemSlot> inventory = new SyncList<ItemSlot>();

    // -----------------------------------------------------------------------------------
    // open
    // -----------------------------------------------------------------------------------
    public bool open
    {
        get { return _open; }
        set
        {
            lastOpen = _open;
            _open = value;
        }
    }

    // -----------------------------------------------------------------------------------
    // OnInteractClient
    // @Client
    // -----------------------------------------------------------------------------------
    [ClientCallback]
    public override void OnInteractClient(Player player)
    {

        if (checkInteractionRange(player))
        {
            player.playerAddonsConfigurator.OnSelect_Lootcrate(this);
        }
        else
        {
            Vector3 destination = collider.ClosestPointOnBounds(gameObject.transform.position);
            player.movement.Navigate(destination, player.interactionRange);
        }
    }

    // -----------------------------------------------------------------------------------
    // Start
    // -----------------------------------------------------------------------------------
    public override void Start()
    {
        base.Start();
            OnRefill();
        StartCoroutine("CR_Lootcrate");
    }

    // -----------------------------------------------------------------------------------
    // EventEmptyTimeElapsed
    // -----------------------------------------------------------------------------------
    private bool EventEmptyTimeElapsed()
    {
        return open && !HasLoot() && NetworkTime.time >= emptyTimeEnd;
    }

    // -----------------------------------------------------------------------------------
    // EventRespawnTimeElapsed
    // -----------------------------------------------------------------------------------
    private bool EventRespawnTimeElapsed()
    {
        return IsHidden() && respawn && !HasLoot() && NetworkTime.time >= respawnTimeEnd;
    }

    // -----------------------------------------------------------------------------------
    // OnOpen
    // @Client
    // -----------------------------------------------------------------------------------
    [ClientCallback]
    public void OnOpen()
    {
        if (open != lastOpen)
        {
            if (!string.IsNullOrWhiteSpace(crateAnimationClose))
                animator.SetBool(crateAnimationClose, false);
            if (!string.IsNullOrWhiteSpace(crateAnimationOpen))
                animator.SetBool(crateAnimationOpen, true);
            PlaySound(soundClose);
            lastOpen = true;
        }
    }

    // -----------------------------------------------------------------------------------
    // OnClose
    // @Client
    // -----------------------------------------------------------------------------------
    [ClientCallback]
    public void OnClose()
    {
        if (open != lastOpen)
        {
            if (!string.IsNullOrWhiteSpace(crateAnimationOpen))
                animator.SetBool(crateAnimationOpen, false);
            if (!string.IsNullOrWhiteSpace(crateAnimationClose))
                animator.SetBool(crateAnimationClose, true);
            PlaySound(soundOpen);
            lastOpen = false;
        }
    }

    // -----------------------------------------------------------------------------------
    // OnLoot
    // -----------------------------------------------------------------------------------
    public void OnLoot()
    {
        if (HasLoot())
            SpawnEffect(lootEffect, lootSound);
    }


    private IEnumerator CR_Lootcrate()
    {
        while (true)
        {
            if (IsWorthUpdating())
            {
                if (isClient)
                {
                    UpdateClient();
                }
                if (isServer)
                {
                    UpdateServer();
                }
            }

            yield return new WaitForSeconds(0.02f);
        }
    }
    // -----------------------------------------------------------------------------------
    // UpdateClient
    // @Client
    // -----------------------------------------------------------------------------------
    [Client]
    protected void UpdateClient()
    {
        if (lootIndicator != null)
        {
            bool hasLoot = HasLoot();
            if (hasLoot && !lootIndicator.isPlaying)
            {
                lootIndicator.Play();
                collider.enabled = true;
                if (hide3dModelEmpty) model3d.SetActive(true);
            }
            else if (!hasLoot && lootIndicator.isPlaying)
            {
                lootIndicator.Stop();
                collider.enabled = false;
                if (hide3dModelEmpty) model3d.SetActive(false);
            }
        }

        if (open)
        {
            OnOpen();
        }
        else if (!unlocked)
        {
            OnClose();
        }
    }

    // -----------------------------------------------------------------------------------
    // UpdateServer
    // @Server
    // -----------------------------------------------------------------------------------
    [Server]
    protected void UpdateServer()
    {
        if (EventEmptyTimeElapsed())
        {
            if (respawn)
            {
                open = false;
                Hide();
            }
            else
            {
                NetworkServer.Destroy(gameObject);
            }
        }

        if (EventRespawnTimeElapsed())
        {
            OnRefill();
            Show();
        }
    }

    // -----------------------------------------------------------------------------------
    // OnAccessed
    // @Server
    // -----------------------------------------------------------------------------------
    [ServerCallback]
    public void OnAccessed()
    {
        emptyTimeEnd = NetworkTime.time + emptyTime;
        respawnTimeEnd = emptyTimeEnd + respawnTime;
        open = true;
    }

    // -----------------------------------------------------------------------------------
    // OnRefill
    // @Server
    // -----------------------------------------------------------------------------------
    [ServerCallback]
    private void OnRefill()
    {
        inventory.Clear();

        gold = Random.Range(lootGoldMin, lootGoldMax);
        coins = Random.Range(lootCoinsMin, lootCoinsMax);

        foreach (ItemDropChance itemChance in dropChances)
            if (Random.value <= itemChance.probability)
            {
                int amount = (UnityEngine.Random.Range(itemChance.minStack, itemChance.maxStack));
                for (int i = 1; i <= amount; i++)
                    inventory.Add(new ItemSlot(new Item(itemChance.item)));
            }

    }

    // -----------------------------------------------------------------------------------
    // HasLoot
    // -----------------------------------------------------------------------------------
    public bool HasLoot()
    {
        return gold > 0 || coins > 0 || inventory.Any(item => item.amount > 0);
    }
    // -----------------------------------------------------------------------------------
}
#endif