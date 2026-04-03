using Mirror;
using System.Collections;
using UnityEngine;

// PLAYER EMOTES


public class PlayerEmotes : NetworkBehaviour
{
    [Header("[-=-[ Emotes Configuration ]-=-]")]
    public Tmpl_Emotes configurationEmotes;

    private bool emoteUp = false;

    // -----------------------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------------------
    private void Update()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        if (!emoteUp && player.isAlive && !UIUtils.AnyInputActive() && (player.state == "IDLE" || player.state == "MOVING"))
        {
            CheckEmotes();
        }
    }

    // -----------------------------------------------------------------------------------
    // emoteWait
    // -----------------------------------------------------------------------------------
    private IEnumerator emoteWait(float fWaitTime)
    {
        yield return new WaitForSeconds(fWaitTime);
        emoteUp = false;
    }

    // -----------------------------------------------------------------------------------
    // ShowEmote
    // @Client
    // -----------------------------------------------------------------------------------
    [Client]
    private void ShowEmote(int index)
    {
        if (!configurationEmotes || index >= configurationEmotes.emotes.Length) return;

        Cmd_ShowEmote(index);
        emoteUp = true;

        Player player = Player.localPlayer;

        // Play sound effect if available
        if (configurationEmotes.emotes[index].soundEffect != null && player.audioSource != null)
            player.audioSource.PlayOneShot(configurationEmotes.emotes[index].soundEffect);

        StartCoroutine(emoteWait(configurationEmotes.secondsBetweenEmotes));
    }

    // -----------------------------------------------------------------------------------
    // Cmd_ShowEmote
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    private void Cmd_ShowEmote(int nIndex)
    {
        Rpc_ShowEmote(nIndex);
    }

    [ClientRpc]
    private void Rpc_ShowEmote(int nIndex)
    {
        Player player = GetComponent<Player>();
        if (configurationEmotes.emotes.Length > nIndex && configurationEmotes.emotes[nIndex].emote != null)
        {
            GameObject emoteObject = Instantiate(configurationEmotes.emotes[nIndex].emote,
                transform.position + configurationEmotes.emotes[nIndex].distanceAboveHead, Quaternion.identity);
            emoteObject.transform.parent = player.gameObject.transform;
        }
    }

    // -----------------------------------------------------------------------------------
    // ShowAnimation
    // @Client
    // -----------------------------------------------------------------------------------
    [Client]
    private void ShowAnimation(int nIndex, bool bStart)
    {
        if (!configurationEmotes || nIndex >= configurationEmotes.animations.Length) return;

        Cmd_ShowAnimation(nIndex, bStart);
        emoteUp = true;

        Player player = Player.localPlayer;

        // Play sound effect if available
        if (bStart && configurationEmotes.animations[nIndex].soundEffect != null && player.audioSource != null)
            player.audioSource.PlayOneShot(configurationEmotes.animations[nIndex].soundEffect);

        float fWaitTime = configurationEmotes.animations[nIndex].secondsBetweenEmotes > 0 ?
            configurationEmotes.animations[nIndex].secondsBetweenEmotes : configurationEmotes.secondsBetweenEmotes;

        StartCoroutine(emoteWait(fWaitTime));
    }

    // -----------------------------------------------------------------------------------
    // Cmd_ShowAnimation
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    private void Cmd_ShowAnimation(int nIndex, bool start)
    {
        if (configurationEmotes.animations.Length > nIndex &&
            !string.IsNullOrWhiteSpace(configurationEmotes.animations[nIndex].animationName))
        {
            Rpc_ShowAnimation(nIndex, start);
        }
    }

    [ClientRpc]
    private void Rpc_ShowAnimation(int nIndex, bool start)
    {
        Player player = GetComponent<Player>();
        if (!player) return;

        // Reset all other animation booleans
        for (int i = 0; i < configurationEmotes.animations.Length; i++)
        {
            if (i != nIndex && !string.IsNullOrWhiteSpace(configurationEmotes.animations[i].animationName))
            {
                player.animator.SetBool(configurationEmotes.animations[i].animationName, false);
                Debug.Log($"Stopping animation: {configurationEmotes.animations[i].animationName}");
            }
        }

        if (configurationEmotes.animations.Length > nIndex &&
            !string.IsNullOrWhiteSpace(configurationEmotes.animations[nIndex].animationName))
        {
            string animationParameter = configurationEmotes.animations[nIndex].animationName;

            if (start)
            {
                player.animator.speed = configurationEmotes.animations[nIndex].animationSpeed; // Set the animation speed
                player.animator.SetBool(animationParameter, true);
                Debug.Log($"Starting animation: {animationParameter}");

                // Use the duration property
                StartCoroutine(WaitAndStopAnimation(animationParameter, configurationEmotes.animations[nIndex].duration));
            }
        }
    }

    private IEnumerator WaitAndStopAnimation(string animationParameter, float duration)
    {
        yield return new WaitForSeconds(duration);

        Player player = GetComponent<Player>();
        if (player != null)
        {
            player.animator.SetBool(animationParameter, false);
            player.animator.speed = 1.0f; // Reset animation speed if needed
            Debug.Log($"Stopping animation: {animationParameter}");
        }
    }

    // -----------------------------------------------------------------------------------
    // CheckEmotes
    // @Client
    // -----------------------------------------------------------------------------------
    [ClientCallback]
    private void CheckEmotes()
    {
        for (int i = 0; i < configurationEmotes.emotes.Length; ++i)
        {
            if (configurationEmotes.emotes[i].emote == null) return;

            if (Input.GetKey(configurationEmotes.emotesHotKey) && Input.GetKeyDown(configurationEmotes.emotes[i].hotKey))
                ShowEmote(i);
        }

        for (int i = 0; i < configurationEmotes.animations.Length; ++i)
        {
            if (string.IsNullOrWhiteSpace(configurationEmotes.animations[i].animationName)) return;

            if (!Input.GetKey(configurationEmotes.emotesHotKey) && Input.GetKeyDown(configurationEmotes.animations[i].hotKey))
                ShowAnimation(i, true);

            if (!Input.GetKey(configurationEmotes.emotesHotKey) && Input.GetKeyUp(configurationEmotes.animations[i].hotKey))
                ShowAnimation(i, false);
        }
    }
}