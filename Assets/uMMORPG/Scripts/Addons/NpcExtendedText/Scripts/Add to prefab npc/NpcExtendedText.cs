using Mirror;
using System.Collections.Generic;
using UnityEngine;

// ===================================================================================
// NPC EXTENDED TEXT
// ===================================================================================
public class NpcExtendedText : NetworkBehaviour
{
    public Npc npc;

    [Header("[-=-[ Npc Extended Text ]-=-]")]
    public ExtendedText[] extendedTexts;

    protected string _welcome;

    // -----------------------------------------------------------------------------------
    // UpdateNpcExtendedText
    // -----------------------------------------------------------------------------------
    [ClientCallback]
    public string UpdateNpcExtendedText(Player player)
    {
        if (_welcome == "")
            _welcome = npc.welcome;

        List<ExtendedText> welcomeTexts = new List<ExtendedText>();

        foreach (ExtendedText extendedText in extendedTexts)
        {
            bool valid = extendedText.displayRequirements.checkRequirements(player);

            if (valid)
            {
                extendedText.displayRequirements.requierementCost.PayCost(player);
                if (extendedText.displayRandomly)
                {
                    welcomeTexts.Add(extendedText);
                }
                else
                {
#if _iMMOEMOTES
                    NpcEmotesAndAnimations(extendedText);
#endif
                    return extendedText.text;
                }
            }
        }

        if (welcomeTexts.Count > 1)
        {
            System.Random rnd = new System.Random();
            int r = rnd.Next(welcomeTexts.Count);
#if _iMMOEMOTES
            NpcEmotesAndAnimations(welcomeTexts[r]);
#endif
            return welcomeTexts[r].text;
        }
        else if (welcomeTexts.Count == 1)
        {
#if _iMMOEMOTES
            NpcEmotesAndAnimations(welcomeTexts[0]);
#endif
            return welcomeTexts[0].text;
        }

        return _welcome;
    }

#if _iMMOEMOTES

    // -----------------------------------------------------------------------------------
    // NpcEmotesAndAnimations
    // -----------------------------------------------------------------------------------
    [ClientCallback]
    public void NpcEmotesAndAnimations(ExtendedText extendedText)
    {
        NpcEmotes e = GetComponent<NpcEmotes>();

        if (e == null) return;

        if (extendedText.emotesId > -1)
            e.ShowEmote(extendedText.emotesId);
        /*
		else if (extendedText.animationId > -1)
			e.ShowAnimation(extendedText.animationId);
		*/
    }

#endif

    // -----------------------------------------------------------------------------------
}