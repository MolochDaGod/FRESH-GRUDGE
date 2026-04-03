using UnityEngine;

public abstract partial class ScriptableSkill
{
    [Header("[-=-=-[ IA Conditional Skills (Monster, Pet, Npc, Mount]-=-=-]")]
    [Tooltip("all settings applied there only affect the skill activation of of Pets, Monsters, Npcs and Mounts.")]
    public ConditionalSkill skillConditions;
}