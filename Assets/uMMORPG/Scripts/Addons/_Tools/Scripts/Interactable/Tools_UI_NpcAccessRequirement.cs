#if _iMMOTOOLS

// ===================================================================================
// Tools UI NPC ACCESS REQUIREMENT
// ===================================================================================
public partial class Tools_UI_NpcAccessRequirement : Tools_UI_Requirement
{
    protected Npc npc;

    // -----------------------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------------------
    protected override void Update()
    {
        if (!panel.activeSelf)
        {
            return;
        }
        Player player = Player.localPlayer;


        if (!player) {
            return;
        }

        if (!npc || !UMMO_Tools.Tools_CheckSelectionHandling(npc.gameObject))
        {
            npc = null;
            Hide();
        }
    }

#if _iMMONPCRESTRICTIONS

    // -----------------------------------------------------------------------------------
    // Show
    // -----------------------------------------------------------------------------------
    public void Show(Npc _npc)
    {
        Player player = Player.localPlayer;
        if (!player) return;

		npc = _npc;
        requirements = npc.npcRestrictions.npcRestrictions;

        for (int i = 0; i < content.childCount; ++i)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        UpdateTextbox();

        interactButton.interactable = requirements.checkRequirements(player);

        interactButton.onClick.SetListener(() =>
        {
            npc.npcRestrictions.ConfirmAccess();
            Hide();
        });

        panel.SetActive(true);
    }

#endif

    // -----------------------------------------------------------------------------------
}
#endif