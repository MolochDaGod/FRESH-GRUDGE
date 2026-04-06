#if _iMMOTOOLS
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;

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
       // Debug.Log("ding");
        if (!panel.activeSelf)
        {
        //    Debug.Log("if (!panel.activeSelf) if");
            return;
        }
        Player player = Player.localPlayer;


        if (!player) {
         //   Debug.Log("   if (!player) if");
            return;
        }
        else
        {
        //    Debug.Log("   if (!player) else");
        }

        if (!npc || !UMMO_Tools.Tools_CheckSelectionHandling(npc.gameObject))
        {
            //     Debug.Log("if (!npc || !Tools.Tools_CheckSelectionHandling(npc.gameObject)) if");
            //Debug.Log("pas un npc ou Tools_CheckSelectionHandling retourne false ");
            npc = null;
            Hide();
        }
        else
        {
        //    Debug.Log("if (!npc || !Tools.Tools_CheckSelectionHandling(npc.gameObject)) else");
        }
        
    }

#if _iMMONPCRESTRICTIONS

    // -----------------------------------------------------------------------------------
    // Show
    // -----------------------------------------------------------------------------------
    public void Show(Npc _npc)
    {
        Debug.Log("on teste le joueur");
        Player player = Player.localPlayer;
        if (!player) return;

		npc = _npc;
        requirements = npc.npcRestrictions.npcRestrictions;

        for (int i = 0; i < content.childCount; ++i)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        updateTextbox();

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