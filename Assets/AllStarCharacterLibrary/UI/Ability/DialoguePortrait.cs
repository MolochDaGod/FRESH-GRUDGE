using UnityEngine;
using System.Collections;

public class DialoguePortrait : MonoBehaviour
{
    public Texture2D dialoguePortrait;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnGUI()
    {
        GUI.DrawTexture(new Rect(Screen.width / 2 - dialoguePortrait.width / 2, Screen.height / 2 - dialoguePortrait.height / 2, dialoguePortrait.width, dialoguePortrait.height), dialoguePortrait);
    }
}
