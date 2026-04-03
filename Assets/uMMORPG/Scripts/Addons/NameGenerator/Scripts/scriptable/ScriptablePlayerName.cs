using System;
using UnityEngine;

[Serializable]
public partial struct NameListByClass
{
    public string prefabPlayerName;
    public string[] firstNamePart;
    public string[] secondNamePart;
    public string[] thirdNamePart;
}
[CreateAssetMenu(menuName = "ADDON/Templates/Name Generator", order = 999)]
public class ScriptablePlayerName : ScriptableObject
{
    [Tooltip("This is a list for generate name by class.")]
    public NameListByClass[] playerPlassName;
}