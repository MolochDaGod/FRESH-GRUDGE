using Mirror;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

// ===================================================================================
// ITEM DROP CLASS
// ===================================================================================
public partial class ItemDrop : Tools_InteractableObject
{
    [Tooltip("[Required] How long it takes the drop to be deleted from server (drops are not saved in the database!)")]
    [Range(1, 9999)] public float destroyAfter = 30f;

    [Tooltip("[Optional] Effect spawned alongside the drop, when it appears (Also add a 'Destroy After' component to effect object)")]
    public GameObject spawnEffect;

    [Tooltip("[optional] item Name")]
    public TMP_Text itemName;

    public string takeMessage = "You picked up: ";
    public string goldMessage = "You got gold: ";
    public AudioClip dropGoldSound;
    public string failMessage = "You can't take that right now!";

    [HideInInspector] public ScriptableItem itemData;
    [SyncVar, HideInInspector] public int amount = 1;
    [SyncVar, HideInInspector] public Item item;
    [SyncVar, HideInInspector] public long gold = 0;

    [HideInInspector] public bool pickUpAuto = false;

    [SerializeField] private int rangeParty = 15;
    public ParticleSystem particleFuse;

    [Header("[Mesh Switcher]")]
    public GameObject defaultMesh;
    public SpriteRenderer defaultSprite;
#if _iMMOITEMRARITY
    public SpriteRenderer raritySprite;
#endif

    [Header("[Optional] change mesh if stack > 1")]
    public ItemDropMeshSwitcher[] meshSwitcher;

    protected double decayTimeEnd;

    // -----------------------------------------------------------------------------------
    // Start
    // -----------------------------------------------------------------------------------
    public override void Start()
    {
        //base.OnStartClient();//        Debug.Log("name" + this.ToString()); 
#if _SERVER

        decayTimeEnd = NetworkTime.time + destroyAfter;
        Invoke(nameof(DropItemDestroy), destroyAfter);
#endif

#if _CLIENT
        if (gold > 0)
        {
            itemName.text = "Gold x" + gold;
        }
        
        else if(item.name != null)
        {
#if _iMMOITEMRARITY
            Color color = RarityColor.SetratityColor(item.data.rarity.ToString());
            if(raritySprite != null)
                raritySprite.color = color;
#else
            Color color = Color.white;
#endif
            string prefixColor = "<color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">";
            string suffixColor = "</color>";
            itemName.text = prefixColor + item.data.name + ((amount > 1) ? " x" + amount : "") + suffixColor;
            if (particleFuse)
            {
                var main = particleFuse.main;
#if _iMMOITEMRARITY
                main.startColor = color;
#endif
            }
        }
        if(defaultSprite && amount > 0)
            defaultSprite.sprite = item.data.image;
        if (amount > 0 && meshSwitcher.Length > 0)
        {
            foreach (ItemDropMeshSwitcher mesh in meshSwitcher)
            {
                if (amount < mesh.max)
                {
                    mesh.mesh.SetActive(true);
                    break;
                }
            }
        }
#endif
#if _SERVER
        // ------ create spawn effect if any
        if (spawnEffect != null)
        {
            GameObject go = Instantiate(spawnEffect.gameObject, transform.position, transform.rotation);
            NetworkServer.Spawn(go);
        }

        if (pickUpAuto && lastAggressor is Player || lastAggressor is Summonable)
        {
            OnInteractServer((lastAggressor is Summonable summonable) ? summonable.owner : (Player)lastAggressor);
        }
#endif
    }



#if _SERVER

    // -----------------------------------------------------------------------------------
    // OnUpdateServer
    // @Server
    // -----------------------------------------------------------------------------------
    [Server]
    public void DropItemDestroy()
    {
        gold = 0;
        amount = 0;
        NetworkServer.Destroy(gameObject);
    }

    // -----------------------------------------------------------------------------------
    // OnInteractServer
    // -----------------------------------------------------------------------------------
    [ServerCallback]
    public override void OnInteractServer(Player player)
    {
        if (
            (amount > 0 || gold > 0) 
#if _iMMOLOOTRULES && _iMMOITEMDROP
            && ValidateTaggedLooting(player)
#endif
            )
        {
            if (LootVictorParty && player.party.InParty())
            {
                System.Random rand = new System.Random();
                List<Player> playersInParty = Tool_MembersPartyNear(player); // on récupère la liste des joueurs du groupe qui sont à coter du dernier agresseur

                int nbrInParty = playersInParty.Count;
                
                /**
                 * pour l'or on partage de manière équitable
                 */
                if (gold > 0)
                {

                    int goldPerMember = ((int)gold / nbrInParty);
                    int goldReste = ((int)gold % nbrInParty);
                    foreach (Player plyer in playersInParty)
                    {
                        plyer.gold += goldPerMember;
                        if (dropGoldSound)
                            plyer.playerAddonsConfigurator.PlayeSoundItemDrop(dropGoldSound);
                        plyer.Tools_TargetAddMessage(goldMessage + goldPerMember);
                    }
                }

                /**
                 * Pour les items on va les assigné au joueur aléatoirement
                 */
                if (amount > 0)
                {
                    int nbrTours = ((amount < nbrInParty) ? amount : nbrInParty);
                    int amountPerMember = (amount / nbrInParty);
                    int amountReste = (amount % nbrInParty);

                    for (int i = 0; i < nbrTours; i++)
                    {
                        int index = rand.Next(playersInParty.Count);
                        playersInParty[index].inventory.Add(item, (amountPerMember + ((i == 0 ) ? amountReste : 0)));
                        if (itemData.dropSound)
                            playersInParty[index].playerAddonsConfigurator.PlayeSoundItemDrop(itemData.dropSound);
                        playersInParty[index].Tools_TargetAddMessage(takeMessage + item.name + ((amountPerMember > 1) ? " x" + amountPerMember : ""));
                        playersInParty[index].Tools_TargetAddMessage("/p " + playersInParty[index].name + " " + takeMessage + item.name + " x" + (amountPerMember + ((i == 0) ? amountReste : 0)) + " )");

                        playersInParty.RemoveAt(index);
                    }
                }
            }
            else
            {
                if (amount > 0 && player.inventory.CanAdd(item, amount))
                {
                    player.inventory.Add(item, amount);
                    if (itemData.dropSound)
                        player.playerAddonsConfigurator.PlayeSoundItemDrop(itemData.dropSound);
                    player.Tools_TargetAddMessage(takeMessage + item.name + ((amount > 1) ? " x" + amount : ""));
                }
                if (gold > 0)
                {
                    player.gold += gold;
                    if (dropGoldSound)
                        player.playerAddonsConfigurator.PlayeSoundItemDrop(dropGoldSound);
                    player.Tools_TargetAddMessage(goldMessage + gold);
                }
            }
            // clear drop's item slot too so it can't be looted again
            // before truly destroyed
            gold = 0;
            amount = 0;

            DropItemDestroy();
            //NetworkServer.Destroy(gameObject);
        }

        else
        {
            player.Tools_TargetAddMessage(failMessage);
        }
    }
    
    public List<Player> Tool_MembersPartyNear(Player player)
    {
        List<Player> players = new List<Player>();
        foreach (string plyer in player.party.party.members)
        {
            if (Player.onlinePlayers.ContainsKey(plyer))
            {
                float distance = Utils.ClosestDistance(lastAggressor, Player.onlinePlayers[plyer]);
                if (distance <= rangeParty) {
                    players.Add(Player.onlinePlayers[plyer]);
                }
            }
        }
        return players;
    }
#endif
    // -----------------------------------------------------------------------------------
}

[Serializable]
public partial class ItemDropMeshSwitcher
{
    [Range(1, 9999)] public int max;
    public GameObject mesh;
}