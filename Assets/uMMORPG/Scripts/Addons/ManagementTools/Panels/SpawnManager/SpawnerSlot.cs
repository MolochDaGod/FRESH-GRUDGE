#if _SQLITE
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpawnerSlot : MonoBehaviour
{
    public TMP_Dropdown dropDown;
    public Slider groupSizeSlider;
    public TMP_Text groupSizeText;
    public GameObject deleteButton;

    [HideInInspector] public SpawnerButton parent;

    List<string> list = new List<string>();

    void Start()
    {
        // the first slot must always be active
        if (transform == parent.groupContent.GetChild(0))
        {
            // hide the delete button
            deleteButton.SetActive(false);
        }
    }

    public void AddMonsterOptions()
    {
        if (list.Count == 0)
        {
            // add options to the dropdown based by monster names
            list = AddonManagement.Monsters.Select(s => s.name).Distinct().ToList();
            dropDown.AddOptions(list);
        }
    }

    // executed when the value of the groupSizeSlider is changed
    public void GroupSizeUpdate()
    {
        // how many monsters are in the slot
        groupSizeText.text = groupSizeSlider.value.ToString();
    }

    public void SpawnGroupDelete()
    {
        parent.numSlots--;
        parent.SetScrollViewSize();
        parent.ScrollbarSizeControl();
        Destroy(gameObject);
    }
}

#endif
