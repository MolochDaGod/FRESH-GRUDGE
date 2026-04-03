using UnityEngine;

[System.Serializable]
public partial struct Titles
{
    public string title;
    public string displayTitle;
    [HideInInspector] public bool isDisplayed;
    [HideInInspector] public bool isLearned;
}


// TITLE
/*
[System.Serializable]
public struct Titles
{
    public string name;
    public string title;
    public bool isLearned;
    public bool isDisplayed;

    public Tmpl_Titles data
    {
        get
        {
            Tmpl_Titles _data;
            Tmpl_Titles.dict.TryGetValue(name.GetStableHashCode(), out _data);
            return _data;
        }
    }
}
*/