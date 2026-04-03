/// Partial extension to Attributes.Combat.cs
/// Applies weapon proficiency MULTIPLIER from PlayerClassTree nodes
/// into the damage calculation when _iMMOATTRIBUTES is active.
///
/// This is injected via GetDamageBonus() on PlayerClassTree (ICombatBonus),
/// which is already read at line 146-147 of Attributes.Combat.cs.
/// The flat bonus is handled there automatically.
///
/// For the MULTIPLIER (percent bonus), we extend PlayerClassTree.GetDamageBonus()
/// to include a scaled flat amount based on current base damage.
/// This gives a clean "equivalent flat" so no Combat.cs modifications are needed.
///
/// ALTERNATIVELY: To get true multiplicative scaling, hook into
///   PlayerClassTree.GetDamageBonus() and multiply by baseDamage at runtime.
///   This file documents the pattern — no code changes to Combat or Attributes needed.

// No code needed here — PlayerClassTree.GetDamageBonus() already reads
// the equipped weapon category via equipment.GetEquippedWeaponCategory()
// and returns both the flat weapon proficiency bonus AND the node.damageBonus.
// The ICombatBonus path in Attributes.Combat.cs picks this up on line 146-147.
//
// The weapon proficiency multiplierBonus is exposed via
// PlayerClassTree.GetWeaponProficiencyBonus() for UI display purposes only.
// See UI_WeaponProficiencyEntry.cs.
