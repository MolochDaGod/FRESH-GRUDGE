using UnityEngine;

[CreateAssetMenu(menuName = "ADDON/Skin/Character skin", order = 999)]
public class ScriptableSkin : ScriptableObject
{
    // Start is called before the first frame update
    [Header("Size Character")]
    [SerializeField] public float ModelScale = 1; // not public, use ToolTip()
    [SerializeField] public float minModelScale = 0.8f; // not public, use ToolTip()
    [SerializeField] public float maxModelScale = 1.2f; // not public, use ToolTip()

    [Header("Character Hair Model List")]
    public string[] hairsModel;

    [Header("Character Hair Color List")]
    public Color[] hairsColors = { Color.green, Color.red, Color.blue, Color.black, Color.cyan, Color.gray };

    [Header("Character Skin Color List")]
    public Color[] skinColors = { Color.green, Color.red, Color.blue, Color.black, Color.cyan, Color.gray };

    [Header("Character Eyes Color List")]
    public Color[] eyesColors = { Color.green, Color.red, Color.blue, Color.black, Color.cyan, Color.gray };
}
