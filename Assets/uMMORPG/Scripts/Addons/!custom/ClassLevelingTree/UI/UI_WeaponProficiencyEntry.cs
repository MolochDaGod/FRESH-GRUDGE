using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Small row entry in the weapon proficiency summary section
/// at the bottom of the Class Tree panel.
///
/// PREFAB: a row with two Text components side by side.
/// </summary>
public class UI_WeaponProficiencyEntry : MonoBehaviour
{
    public Text weaponCategoryText;
    public Text bonusText;

    public void Setup(string weaponCategory, float totalMultiplierBonus)
    {
        if (weaponCategoryText != null) weaponCategoryText.text = weaponCategory;
        if (bonusText          != null) bonusText.text = $"+{Mathf.RoundToInt(totalMultiplierBonus * 100)}% Damage";
    }
}
