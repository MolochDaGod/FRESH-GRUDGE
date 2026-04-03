using UnityEngine;
using Mirror;
// ===================================================================================
// NPC RESTRICTIONS
// ===================================================================================
public class NpcRestrictions : NetworkBehaviour
{
    public Npc npc;

    [Header("[-=-=-[ NPC RESTRICTIONS ]-=-=-]")]
    public Tools_Requirements npcRestrictions;

    protected Tools_UI_NpcAccessRequirement instance;
    protected UINpcDialogue rr;

    public override void OnStartServer()
    {
        npc.onInteract.AddListener(NpcRestrictions_check);
    }
    // -----------------------------------------------------------------------------------
    // ValidateNpcRestrictions
    // -----------------------------------------------------------------------------------
    //[DevExtMethods("Awake")]
    public void OnInteract_NpcRestrictions()
    {
        if (instance == null)
            instance = FindObjectOfType<Tools_UI_NpcAccessRequirement>();
    }

    public void NpcRestrictions_check()
    {
        //player player = co.GetComponentInParent<player>();
        Player player = Player.localPlayer;
        if ( instance == null)
        {
            instance = FindObjectOfType<Tools_UI_NpcAccessRequirement>();
        }
        ValidateNpcRestrictions(player);

    }
    // -----------------------------------------------------------------------------------
    // ValidateNpcRestrictions
    // -----------------------------------------------------------------------------------
    public bool ValidateNpcRestrictions(Player player)
    {
        bool bValid = npcRestrictions.checkRequirements(player);

        if (!bValid)
        {
            //Tools_InfoBox r = new Tools_InfoBox();
            rr = FindObjectOfType<UINpcDialogue>();
            rr.panel.SetActive(false);
            if (instance != null)
            {
                instance.panel.SetActive(true);
//                instance.Show(gameObject);
            }
        }
        return bValid;
    }

    // -----------------------------------------------------------------------------------
    // ValidateNpcRestrictions
    // -----------------------------------------------------------------------------------
    public void ConfirmAccess()
    {
        UINpcDialogue.singleton.Show();
    }
    // -----------------------------------------------------------------------------------
}