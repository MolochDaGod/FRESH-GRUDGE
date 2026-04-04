using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

// ─────────────────────────────────────────────────────────────────────────────
// PlayerGrudgeProfessions
//
// Attach to the Player prefab. Tracks XP and tier for the 5 Grudge harvesting
// professions. Other systems (harvest nodes, crafting, fishing) call AddXp()
// server-side to progress a profession.
//
// Tiers 1-5, milestone every 100 XP per tier (configurable).
// ─────────────────────────────────────────────────────────────────────────────
[DisallowMultipleComponent]
public partial class PlayerGrudgeProfessions : NetworkBehaviour
{
    // ── Constants ─────────────────────────────────────────────────────────────
    public const int ProfessionCount = 5;
    public const int MaxTier         = 5;

    public static readonly string[] ProfessionNames = {
        "Harvesting",
        "Mining",
        "Woodcutting",
        "Fishing",
        "Crafting"
    };

    // XP required to advance FROM tier n to tier n+1 (index 0 = Tier 1 → 2)
    public static readonly long[] XpPerTier = { 100, 300, 600, 1000, 1500 };

    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Components")]
    public Player player;

    // ── SyncVars (server → clients) ───────────────────────────────────────────
    // Flat array: index 0-4 = profession XP, 5-9 = profession tier
    // Using individual SyncVars rather than SyncList for simplicity and
    // compatibility with older Mirror snapshot builds.
    [SyncVar] public long harvXp;   [SyncVar] public int harvTier  = 1;
    [SyncVar] public long mineXp;   [SyncVar] public int mineTier  = 1;
    [SyncVar] public long woodXp;   [SyncVar] public int woodTier  = 1;
    [SyncVar] public long fishXp;   [SyncVar] public int fishTier  = 1;
    [SyncVar] public long craftXp;  [SyncVar] public int craftTier = 1;

    // ── Public read helpers ───────────────────────────────────────────────────
    public long GetXp(int profession)
    {
        switch (profession)
        {
            case 0: return harvXp;
            case 1: return mineXp;
            case 2: return woodXp;
            case 3: return fishXp;
            case 4: return craftXp;
            default: return 0;
        }
    }

    public int GetTier(int profession)
    {
        switch (profession)
        {
            case 0: return harvTier;
            case 1: return mineTier;
            case 2: return woodTier;
            case 3: return fishTier;
            case 4: return craftTier;
            default: return 1;
        }
    }

    /// <summary>
    /// XP required for the next tier from the current tier.
    /// Returns 0 if already at max tier.
    /// </summary>
    public long XpToNextTier(int profession)
    {
        int tier = GetTier(profession);
        if (tier >= MaxTier) return 0;
        return XpPerTier[tier - 1];
    }

    /// <summary>Progress 0..1 toward the next tier.</summary>
    public float TierProgress(int profession)
    {
        int tier      = GetTier(profession);
        if (tier >= MaxTier) return 1f;
        long needed   = XpPerTier[tier - 1];
        long xp       = GetXp(profession);
        return needed > 0 ? Mathf.Clamp01((float)xp / needed) : 0f;
    }

    // ── Server: add XP ────────────────────────────────────────────────────────
    [Server]
    public void AddXp(int profession, long amount)
    {
        if (profession < 0 || profession >= ProfessionCount) return;
        if (amount <= 0) return;

        SetXp(profession, GetXp(profession) + amount);
        CheckTierAdvance(profession);
    }

    [Server]
    void SetXp(int profession, long value)
    {
        switch (profession)
        {
            case 0: harvXp  = value; break;
            case 1: mineXp  = value; break;
            case 2: woodXp  = value; break;
            case 3: fishXp  = value; break;
            case 4: craftXp = value; break;
        }
    }

    [Server]
    void SetTier(int profession, int value)
    {
        switch (profession)
        {
            case 0: harvTier  = value; break;
            case 1: mineTier  = value; break;
            case 2: woodTier  = value; break;
            case 3: fishTier  = value; break;
            case 4: craftTier = value; break;
        }
    }

    [Server]
    void CheckTierAdvance(int profession)
    {
        int  tier   = GetTier(profession);
        if (tier >= MaxTier) return;

        long needed = XpPerTier[tier - 1];
        long xp     = GetXp(profession);
        if (xp >= needed)
        {
            SetXp(profession, xp - needed); // carry over
            SetTier(profession, tier + 1);
            CheckTierAdvance(profession);   // chain for rare multi-advance
        }
    }

    // ── Milestone check helpers (for UI / other systems) ─────────────────────
    /// <summary>
    /// Returns true if this profession's tier can harvest/craft/fish the
    /// specified resource tier (resource tiers 1-5 match profession tiers).
    /// </summary>
    public bool CanAccessResourceTier(int profession, int resourceTier)
    {
        return GetTier(profession) >= resourceTier;
    }
}
