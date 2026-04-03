using Mirror;
using UnityEngine;

public class NetworkManagerMMOTraits : MonoBehaviour
{
    public NetworkManagerMMO networkManagerMMO;
    // -----------------------------------------------------------------------------------
    // OnServerCharacterCreate_Traits
    // -----------------------------------------------------------------------------------

#if _SERVER

    public void Start()
    {
        networkManagerMMO.onServerCharacterCreate.AddListener(OnServerCharacterCreate_Traits);
    }

    private void OnServerCharacterCreate_Traits(CharacterCreateMsg netMsg, Player player)
    {
        if (player.playerTraits)
        {
            if (netMsg.name == null || netMsg.traits == null || netMsg.traits.Length == 0) return;

            foreach (int traitId in netMsg.traits)
            {
                if (traitId != 0)
                {
                    TraitTemplate trait;
                    if (TraitTemplate.dict.TryGetValue(traitId, out trait))
                    {
                        player.playerTraits.Traits.Add(new Trait(trait));


#if _iMMOATTRIBUTES
                        foreach (AttributeTemplate template in player.playerAttribute.playerAttributes.AttributeTypes)
                        {
                            if (template == null) continue;
                            TrugordAttribute attr = new TrugordAttribute(template);
                            player.playerAttribute.Attributes.Add(attr);
                        }
#endif

                        // ----------------------------------------------------------------------
                        foreach (Tools_SkillRequirement startSkill in trait.startingSkills)
                        {
                            for (int i = 0; i < player.skills.skills.Count; ++i)
                            {
                                if (player.skills.skills[i].data == startSkill.skill)
                                {
                                    Skill skill = player.skills.skills[i];
                                    skill.level += startSkill.level;
                                    player.skills.skills[i] = skill;
                                }
                            }
                        }

                        // ----------------------------------------------------------------------
                        foreach (Tools_ItemModifier startItem in trait.startingItems)
                        {
                            player.inventory.Add(new Item(startItem.item), startItem.amount);
                        }

#if _iMMOPRESTIGECLASSES
                        if (trait.startingPrestigeClass != null)
                            player.playerAddonsConfigurator.prestigeClass = trait.startingPrestigeClass;
#endif

#if _iMMOHONORSHOP
                        foreach (HonorShopCurrencyCost currency in trait.startingHonorCurrency)
                            player.playerHonorShop.AddHonorCurrency(currency.honorCurrency, currency.amount);
#endif

#if _iMMOFACTIONS
                        foreach (FactionRating faction in trait.startingFactions)
                            player.playerFactions.AddFactionRating(faction.faction, faction.startRating);
#endif

#if _iMMOCRAFTING
                        foreach (Tools_CraftingProfessionRequirement prof in trait.startingCraftingProfession)
                        {
                            if (player.playerCraftingExtended.HasCraftingProfession(prof.template))
                            {
                                var tmpProf = player.playerCraftingExtended.getCraftingProfession(prof.template);
                                tmpProf.experience += prof.level;
                                player.playerCraftingExtended.SetCraftingProfession(tmpProf);
                            }
                            else
                            {
                                CraftingProfession tmpProf = new CraftingProfession(prof.template.name);
                                tmpProf.experience += prof.level;
                                player.playerCraftingExtended.Crafts.Add(tmpProf);
                            }
                        }
#endif

#if _iMMOHARVESTING
                        foreach (HarvestingProfessionRequirement prof in trait.startingHarvestingProfession)
                        {
                            if (player.playerHarvesting.HasHarvestingProfession(prof.template))
                            {
                                var tmpProf = player.playerHarvesting.getHarvestingProfession(prof.template);
                                tmpProf.experience += prof.level;
                                player.playerHarvesting.SetHarvestingProfession(tmpProf);
                            }
                            else
                            {
                                HarvestingProfession tmpProf = new HarvestingProfession(prof.template.name);
                                tmpProf.experience += prof.level;
                                player.playerHarvesting.Professions.Add(tmpProf);
                            }
                        }
#endif

#if _iMMOPVP
                        player.SetRealm(trait.changeRealm, trait.changeAlliedRealm);
#endif

                        // ------------ Recalculate all Maxes here again (in case of bonusses)

                        player.health.current = player.health.max;
                        player.mana.current = player.mana.max;
#if _iMMOSTAMINA
                        player.stamina.current = player.stamina.max;
#endif

                        // ----------------------------------------------------------------------

                    }
                }
            }
        }
    }

#endif 
    // -----------------------------------------------------------------------------------
}
