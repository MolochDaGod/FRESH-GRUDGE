using UnityEngine;
// BindPoint

[System.Serializable]
public struct BindPoint
{
    public string name;
    public string SceneName;
    public Vector3 position;

    public UnityScene mapScene
    {
        set { SceneName = value.SceneName; }
    }

    public bool Valid
    {
        get
        {
            return !string.IsNullOrEmpty(SceneName);
        }
    }
}