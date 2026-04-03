/// LobbySceneCameraManager
/// Single source-of-truth for lobby scene camera positioning.
///
/// Flow:
///   Offline / Handshake  →  loginCameraLocation
///   Lobby, NOT creating  →  selectionCameraLocation (from NetworkManagerMMO)
///   Lobby, IS creating   →  creationCameraLocation  (from UI_CharacterCreation)
///
/// HOW TO HOOK UP IN THE SCENE:
///   1. Create an empty GameObject in the lobby scene, add this component.
///   2. Assign the three Transform fields in the Inspector.
///      - loginCameraLocation    : a Transform placed where the camera sits on the login screen
///      - uiCharacterCreation    : drag the UI_CharacterCreation GameObject here
///      - manager                : drag the NetworkManager here
///   3. Remove Camera.main moves from UI_CharacterCreation.Show()/Hide() if you want this
///      to be the ONLY driver — or leave them; this manager will win each frame anyway.
///
/// SMOOTH TRANSITIONS: set transitionSpeed > 0. Set to 0 for instant snap.

using UnityEngine;
using Mirror;

public class LobbySceneCameraManager : MonoBehaviour
{
    [Header("References")]
    public NetworkManagerMMO manager;
    public UI_CharacterCreation uiCharacterCreation;

    [Header("Camera Positions")]
    [Tooltip("Where the camera sits on the Login screen (before connecting).")]
    public Transform loginCameraLocation;

    [Header("Transition")]
    [Tooltip("Lerp speed for smooth camera movement. 0 = instant snap.")]
    [Range(0f, 20f)]
    public float transitionSpeed = 8f;

    // ── Internal state ────────────────────────────────────────────────────

    private Transform targetPosition;   // the transform we're moving toward this frame
    private bool isActive = true;       // disabled automatically once the player enters the world

    void Update()
    {
        if (!isActive) return;

        // Stop driving the camera once we leave the lobby (player has entered the world)
        if (manager != null &&
            manager.state != NetworkState.Offline &&
            manager.state != NetworkState.Handshake &&
            manager.state != NetworkState.Lobby)
        {
            isActive = false;
            return;
        }

        Transform desired = ResolveDesiredPosition();
        if (desired == null) return;

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[LobbySceneCameraManager] Camera.main is null — ensure the camera is tagged 'MainCamera'.");
            return;
        }

        if (transitionSpeed <= 0f)
        {
            // Instant snap
            cam.transform.SetPositionAndRotation(desired.position, desired.rotation);
        }
        else
        {
            cam.transform.position = Vector3.Lerp(cam.transform.position, desired.position, Time.deltaTime * transitionSpeed);
            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, desired.rotation, Time.deltaTime * transitionSpeed);
        }
    }

    // ── State machine ─────────────────────────────────────────────────────

    private Transform ResolveDesiredPosition()
    {
        if (manager == null) return loginCameraLocation;

        NetworkState state = manager.state;

        // Offline / connecting / handshaking → login screen
        if (state == NetworkState.Offline || state == NetworkState.Handshake)
            return loginCameraLocation;

        // In lobby
        if (state == NetworkState.Lobby)
        {
            // If the creation panel is open → creation camera
            if (uiCharacterCreation != null && uiCharacterCreation.IsVisible())
                return uiCharacterCreation.creationCameraLocation;

            // Otherwise → selection camera
            return manager.selectionCameraLocation;
        }

        return null; // world is active — do nothing
    }

    // ── Public helpers (for other scripts to call) ────────────────────────

    /// <summary>Re-enables the manager after it was deactivated (e.g. on disconnect).</summary>
    public void ReActivate() => isActive = true;
}
