using UnityEngine;

[CreateAssetMenu(menuName = "ADDON/Other/ListScene", order = 999)]
public class ScriptableSceneList : ScriptableObject
{
    [Header("Lobby Scene")]
    [Tooltip("This scene is used for login")]
    public UnityScene LobbyScene;

    [Header("Character Selection & Creation Scene")]
    [Tooltip("This scene is used for select character and create character only")]
    public UnitySceneServer CharacterManager;

    [Header("Select scene for debug")]
    public UnityScene debugScene;

    [Header("Game Scene")]
    [Tooltip("This scene is used only for playable scene")]
    public UnitySceneServer[] serverScenes;

    [Header("Loading Tips")]
    public string[] displayTips;
}

[System.Serializable]
public class UnitySceneServer
{
    [Header("Scene")]
    public UnityScene unityScenes;

    [Header("Server IP")]
    public string ipServer;

    [Header("Server Port")]
    public int portServer;

    [Header("Loading Background")]
    public Sprite background;
}