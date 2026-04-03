using System;
using UnityEngine;

// ---------------------------------------------------------------------------------------
// Emotes_Emote
// ---------------------------------------------------------------------------------------
[Serializable]
public class Emotes_Emote
{
    public GameObject emote;
    public AudioClip soundEffect;
    public KeyCode hotKey = KeyCode.C;
    public Vector3 distanceAboveHead = new Vector3(0f, 2.5f, 0f);
}

// ---------------------------------------------------------------------------------------
// Emotes_Animation
// ---------------------------------------------------------------------------------------
[Serializable]
public class Emotes_Animation
{
    public string animationName;
    public AudioClip soundEffect;
    public KeyCode hotKey = KeyCode.C;
    public float secondsBetweenEmotes;
}

[Serializable]
public struct EmotesEmoji
{
    public GameObject emote;
    public AudioClip soundEffect;
    public Vector3 distanceAboveHead;// = new Vector3(0f, 2.5f, 0f);
    public KeyCode hotKey;
}

[Serializable]
public struct EmotesAnimation
{
    public string animationName;
    public AudioClip soundEffect;
    public float secondsBetweenEmotes;
    public KeyCode hotKey;
    public float duration;  // Duration of the animation
    public float animationSpeed; // Speed of the animation
}