using UnityEngine;
using UnityEngine.UI;
using System.Linq;

// UI Character Info Attributes
public partial class UI_CharacterInfoAttributes : MonoBehaviour
{
    public KeyCode hotKey = KeyCode.C;
    public GameObject panel;
    public UI_AttributeSlot slotPrefab;
    public Transform attributeContent;
#if _iMMOELEMENTS
    public Transform elementsContent;
    public UI_ElementSlot slotElementPrefab;
#endif
    public Text damageText;
    public Text defenseText;
    public Text healthText;
    public Text manaText;
    public Text criticalChanceText;
    public Text blockChanceText;
    public Text speedText;
    public Text levelText;
    public Text currentExperienceText;
    public Text maximumExperienceText;
    public Text skillExperienceText;
    public Text attrPointsText;
    public Text accuracyText;
    public Text resistanceText;
    public Text blockFactorText;
    public Text blockBreakText;
    public Text reflectText;
    public Text defBreakText;
    public Text critFactorText;
    public Text drainHealthText;
    public Text drainManaText;
    public Text critEvasionText;

    public Text reserved1Text;
    public Text reserved2Text;

    // -----------------------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------------------
    private void Update()
    {
#if _iMMOATTRIBUTES

        Player player = Player.localPlayer;
        if (!player) return;

        if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
            panel.SetActive(!panel.activeSelf);

        if (panel.activeSelf)
        {
            // -- Update Main Stats
            damageText.text = player.combat.damage.ToString();
            defenseText.text = player.combat.defense.ToString();
            healthText.text = player.health.max.ToString();
            manaText.text = player.mana.max.ToString();
            criticalChanceText.text = (player.combat.criticalChance * 100).ToString("F0") + "%";
            blockChanceText.text = (player.combat.blockChance * 100).ToString("F0") + "%";
            speedText.text = player.speed.ToString();
            levelText.text = player.level.current.ToString();
            currentExperienceText.text = player.experience.current.ToString();
            maximumExperienceText.text = player.experience.max.ToString();
            skillExperienceText.text = ((PlayerSkills)player.skills).skillExperience.ToString();
            attrPointsText.text = player.playerAttribute.AttributesSpendable().ToString();

            // -- Update Secondary Stats
            accuracyText.text = (player.combat.accuracy * 100).ToString("F0") + "%";
            resistanceText.text = (player.combat.resistance * 100).ToString("F0") + "%";
            blockFactorText.text = (player.combat.blockFactor * 100).ToString("F0") + "%";
            blockBreakText.text = (player.combat.blockBreakFactor * 100).ToString("F0") + "%";
            reflectText.text = (player.combat.reflectDamageFactor * 100).ToString("F0") + "%";
            defBreakText.text = (player.combat.defenseBreakFactor * 100).ToString("F0") + "%";
            critFactorText.text = (player.combat.criticalFactor * 100).ToString("F0") + "%";
            drainHealthText.text = (player.combat.drainHealthFactor * 100).ToString("F0") + "%";
            drainManaText.text = (player.combat.drainManaFactor * 100).ToString("F0") + "%";
            critEvasionText.text = (player.combat.criticalEvasion * 100).ToString("F0") + "%";

#if _iMMOATTRIBUTES
            UpdateAttributes();
#endif

#if _iMMOELEMENTS
            UpdateElements();
#endif
        }

#endif
    }

    // -----------------------------------------------------------------------------------
    // UpdateAttributes
    // -----------------------------------------------------------------------------------
#if _iMMOATTRIBUTES

    protected void UpdateAttributes()
    {
        Player player = Player.localPlayer;
        if (!player) return;
        UIUtils.BalancePrefabs(slotPrefab.gameObject, player.playerAttribute.Attributes.Count, attributeContent);

        for (int i = 0; i < player.playerAttribute.Attributes.Count; ++i)
        {
            UI_AttributeSlot slot = attributeContent.GetChild(i).GetComponent<UI_AttributeSlot>();
            var attr = player.playerAttribute.Attributes[i];
            int bonus = player.playerAttribute.calculateBonusAttribute(attr);

            int slotIndex = i;

            slot.tooltip.enabled = true;
            slot.tooltip.text = attr.ToolTip();
            slot.image.sprite = attr.template.image;
            slot.label.text = attr.template.name + ":";
            slot.points.text = attr.points.ToString() + " +" + bonus.ToString();
            slot.button.interactable = player.playerAttribute.AttributesSpendable() > 0;

            slot.button.onClick.SetListener(() =>
            {
                player.playerAttribute.Cmd_IncreaseAttribute(slotIndex);
            });
        }
    }

#endif

    // -----------------------------------------------------------------------------------
    // UpdateElements
    // -----------------------------------------------------------------------------------
#if _iMMOELEMENTS

    protected void UpdateElements()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        UIUtils.BalancePrefabs(slotElementPrefab.gameObject, ElementTemplate.dict.Count, elementsContent);

        for (int i = 0; i < ElementTemplate.dict.Count; ++i)
        {
            UI_ElementSlot slot2 = elementsContent.GetChild(i).GetComponent<UI_ElementSlot>();
            ElementTemplate ele = ElementTemplate.dict.Values.ElementAt(i);
            float points = 1.0f - player.CalculateElementalResistance(ele);

            slot2.tooltip.enabled = true;
            slot2.tooltip.text = ele.toolTip;
            slot2.image.sprite = ele.image;
            slot2.label.text = ele.name + ":";
            slot2.points.text = (points * 100).ToString("F0") + "%";
        }
    }

#endif

    // -----------------------------------------------------------------------------------
}
