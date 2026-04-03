using UnityEngine;
using UnityEngine.UI;

public partial class UIInteractInstance : MonoBehaviour
{
    public GameObject panel;
    public Button button;
    public new Collider collider; // Use the new keyword to hide inherited member

    public static UIInteractInstance instance;

    private void Start()
    {
        instance = this;
    }

    void Update()
    {
        Player player = Player.localPlayer;
        if (player != null &&
            collider != null)
        {
            panel.SetActive(true);
            button.onClick.SetListener(() =>
            {
                //there's nothing to invoke onmousedown so we're just going to have to copypaste code from
                //all other instances
                if (collider.transform.root.GetComponent<Entity>())
                {
                    Entity colliderEntity = collider.transform.root.GetComponent<Entity>();
                    // target it
                    Player.localPlayer.CmdSetTarget(colliderEntity.netIdentity);

                    // call OnSelect + hook
                    colliderEntity.OnSelect();
                    colliderEntity.onSelect.Invoke();
                    // call OnInteract + hook
                    colliderEntity.OnInteract();
                    colliderEntity.onInteract.Invoke();
                }
                if (collider.transform.root.GetComponent<Furnace>())
                {
                    if (Vector3.Distance(Player.localPlayer.transform.position, collider.transform.position) <= Player.localPlayer.interactionRange)
                    {
                        Player.localPlayer.furnace = collider.transform.root.GetComponent<Furnace>();
                        collider.transform.root.GetComponent<Furnace>().ActivatePanel("FurnacePanel");
                    }
                }
                if (collider.transform.root.GetComponent<Storage>())
                {
                    // Check if the player is close enough to interact with the storage  
                    if (Vector3.Distance(Player.localPlayer.transform.position, collider.transform.position) <= Player.localPlayer.interactionRange)
                    {
                        Player.localPlayer.storage = collider.transform.root.GetComponent<Storage>(); // Set the player's storage reference to this storage instance  
                        collider.transform.root.GetComponent<Storage>().ActivatePanel("StoragePanel"); // Activate the storage panel UI
                    }
                }
#if _SQLITE
                if (collider.transform.root.GetComponent<RAHR_UI_AuctionHouse>())
                {

                }
#endif
                if (collider.transform.root.GetComponent<InteractableWorkbench>())
                {
                    InteractableWorkbench workBench = collider.transform.root.GetComponent<InteractableWorkbench>();
                    Player.localPlayer.movement.Reset();
                    if (workBench.checkInteractionRange(Player.localPlayer))
                    {
                        //Debug.Log("Selected " + this.name);
                    }
                    // target it
                    Player.localPlayer.CmdSetTarget(workBench.netIdentity);

                    // call OnSelect + hook
                    workBench.OnSelect(Player.localPlayer);
                    workBench.onSelect.Invoke(Player.localPlayer);
                }
                if (collider.transform.root.GetComponent<PlaceableObject>())
                {

                }
            });
        }
        else panel.SetActive(false);
    }
}
