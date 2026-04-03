using UnityEngine;
// SceneLocation
[System.Serializable]
public partial class SceneLocation
{
    public UnityScene mapScene;
    public Vector3 position;

    public bool Valid
    {
        get
        {
            return mapScene.IsSet();
        }
    }
}