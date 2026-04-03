using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Tmpl_Emotes", menuName = "ADDON/Templates/Emotes/Tmpl Emotes", order = 999)]
public class Tmpl_Emotes : ScriptableObject
{
    public int secondsBetweenEmotes = 2;

    [Header("[-=-[ Emote : Emoji ]-=-]")]
    public EmotesEmoji[] emotes;
    public KeyCode emotesHotKey = KeyCode.LeftAlt;

    [Header("[-=-[ Emote : Animation ]-=-]")]
    public EmotesAnimation[] animations;
    //public KeyCode animationsHotKey = KeyCode.LeftControl;
}