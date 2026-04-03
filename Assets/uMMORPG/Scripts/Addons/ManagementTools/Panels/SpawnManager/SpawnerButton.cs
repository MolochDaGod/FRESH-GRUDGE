using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpawnerButton : MonoBehaviour
{
    public RectTransform rectTransform;
    public RectTransform scrollbarRectTransform;
    public RectTransform scrollViewRectTransform;
    public GameObject content;
    public Transform groupContent;

    public TMP_Text spawnName;

    [HideInInspector] public string uniqueId;
    [HideInInspector] public int area = 1;
#if UMMORPG_3D_C || UMMORPG_3D_R
    [HideInInspector] public Vector3 position;
#else
    [HideInInspector] public Vector2 position;
#endif
    [HideInInspector] public int numSlots;
    [HideInInspector] public ToggleGroup toggleGroup;

    public Toggle m_Toggle;

    public SpawnerSlot spawnerSlot;
    public ScrollRect scrollRect;

    Vector2 targetPos;

    int contentSize = 32;

    void Start()
    {
        // set the toggle group
        m_Toggle.group = toggleGroup;

        // a brand new button
        if (groupContent.childCount == 0)
        {
            // add one slot to it
            AddSpawnGroup();
        }
    }

    public int GetIndexByName(TMP_Dropdown dropDown, string name)
    {
        if (dropDown == null) { return -1; } // or exception
        if (string.IsNullOrEmpty(name) == true) { return -1; }
        List<TMP_Dropdown.OptionData> list = dropDown.options;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].text.Equals(name)) { return i; }
        }
        return -1;
    }

    public void SpawnerSlots(Spawner button, int amount)
    {
        numSlots = amount;
        UIUtils.BalancePrefabs(spawnerSlot.gameObject, amount, groupContent);

        for (int i = 0; i < amount; ++i)
        {
            SpawnerSlot slot = groupContent.GetChild(i).GetComponent<SpawnerSlot>();
            slot.AddMonsterOptions();

            SpawnGroup monster = button.groups[i];

            // set the group by name
            int index = GetIndexByName(slot.dropDown, monster.monsterName);
            slot.dropDown.value = index;
            slot.groupSizeSlider.value = monster.amount;
            slot.parent = this;
        }
        // window size control with groups
        SetScrollViewSize();
    }

    public void AddSpawnGroup()
    {
        numSlots = groupContent.childCount + 1;
        UIUtils.BalancePrefabs(spawnerSlot.gameObject, numSlots, groupContent);

        for (int i = 0; i < numSlots; ++i)
        {
            SpawnerSlot slot = groupContent.GetChild(i).GetComponent<SpawnerSlot>();
            slot.AddMonsterOptions();
            slot.parent = this;
        }
        // window size control with groups
        SetScrollViewSize();

        if (groupContent.childCount > 3)
        {
            // after adding a new group, scroll to it
            StartCoroutine(ScrollDown());
        }
    }

    // resizing the window in which the group buttons are located
    public void SetScrollViewSize()
    {
        if (numSlots >= 3)
        {
            // three or more slots, to see other groups use the scrollbar on the right
            if (scrollViewRectTransform.sizeDelta.y != 90)
            {
                SetSizeScrollView(scrollViewRectTransform, 90);
                contentSize = 90;
            }
        }
        else if (numSlots == 2)
        {
            // two slots
            if (scrollViewRectTransform.sizeDelta.y != 61)
            {
                SetSizeScrollView(scrollViewRectTransform, 61);
                contentSize = 61;
            }
        }
        else
        {
            // one slot
            if (scrollViewRectTransform.sizeDelta.y != 32)
            {
                SetSizeScrollView(scrollViewRectTransform, 32);
                contentSize = 32;
            }
        }

        if (m_Toggle.isOn)
        {
            SetSize(rectTransform, 52 + contentSize);
        }
    }

    public void SetSizeScrollView(RectTransform rectTransform, float maximum)
    {
        StartCoroutine(ResizeScrollView(rectTransform, rectTransform.sizeDelta.y, maximum));
    }

    IEnumerator ResizeScrollView(RectTransform rectTransform, float minimum, float maximum)
    {
        float step = 0;
        while (step < 1)
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.CeilToInt(Mathf.SmoothStep(minimum, maximum, step += Time.deltaTime)));
            scrollbarRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.CeilToInt(Mathf.SmoothStep(minimum, maximum, step += Time.deltaTime)));
            yield return AddonManagement.EndOfFrame;
        }
    }

    IEnumerator ScrollDown()
    {
        yield return AddonManagement.EndOfFrame;
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void ScrollbarSizeControl()
    {
        if (numSlots == 3)
        {
            if (scrollRect.verticalScrollbar.size != 1)
            {
                StartCoroutine(SetScrollbarSize(scrollRect.verticalScrollbar.size, 1));
            }
        }
    }

    IEnumerator SetScrollbarSize(float minimum, float maximum)
    {
        float step = 0;
        while (step < 1)
        {
            scrollRect.verticalScrollbar.size = Mathf.CeilToInt(Mathf.SmoothStep(minimum, maximum, step += Time.deltaTime));
            yield return AddonManagement.EndOfFrame;
        }
    }

    public void SaveSpawner()
    {
        List<SpawnGroup> groups = new List<SpawnGroup>();

        for (int i = 0; i < groupContent.childCount; i++)
        {
            SpawnerSlot spawnerSlot = groupContent.GetChild(i).GetComponent<SpawnerSlot>();
            spawnerSlot.AddMonsterOptions();

            SpawnGroup group = new SpawnGroup
            {
                slot = i,
                monsterName = spawnerSlot.dropDown.options[spawnerSlot.dropDown.value].text,
                amount = Convert.ToInt32(spawnerSlot.groupSizeSlider.value)
            };
            groups.Add(group);
        }

        // assign a unique ID
        if (uniqueId == "")
        {
            uniqueId = Guid.NewGuid().ToString();
        }

        Spawner spawnerData = new Spawner
        {
            spawnName = spawnName.text,
            uniqueId = uniqueId,
            area = area,
            position = position,
            groups = groups.ToArray()
        };
        UIManagement.singleton.player.CmdMsgAddSpawner(spawnerData);
    }

    public void EditSpawner()
    {
        UISpawnMap.singleton.Show(spawnName.text, position, area, () =>
        {
            spawnName.text = UISpawnMap.singleton.labelInputField.text;
            area = Convert.ToInt32(UISpawnMap.singleton.areaSizeText.text);
            position = UISpawnMap.singleton.targetPin.transform.position;

            UISpawnMap.singleton.Hide();
        });
    }

    public void SpawnerDelete()
    {
        UICommandConfirmation.singleton.Show($"Delete <color=#00FA9A>{spawnName.text}</color> spawner?", () =>
        {
            // close the spawn map panel if active
            if (UISpawnMap.singleton.canvas.enabled) UISpawnMap.singleton.Hide();

            // check if the unique ID is assigned
            if (uniqueId != "")
            {
                // destroy the spawner
                UIManagement.singleton.player.CmdMsgSpawnerDelete(uniqueId);
            }
            else
            {
                // scroll to the beginning
                UISpawnManager.singleton.GetStartPosition();

                // this spawner has not been activated yet
                // destroy the button only in my panel
                Destroy(gameObject);
            }
        });
    }
    
    public void ExpandButton()
    {
        // the index is not called in Start() function because the order of buttons may change
        int index = transform.GetSiblingIndex();
        targetPos = Vector2.up * (29 * index);

        if (m_Toggle.isOn)
        {
            content.SetActive(true);

            // scroll to the target position
            UISpawnManager.singleton.MovePanel(targetPos);

            SetSize(rectTransform, 52 + contentSize);
        }
        else
        {
            // scroll to the beginning
            UISpawnManager.singleton.GetStartPosition();

            SetSize(rectTransform, 26);
        }
    }

    public void SetSize(RectTransform rectTransform, float maximum)
    {
        StartCoroutine(Resize(rectTransform, rectTransform.sizeDelta.y, maximum));
    }

    IEnumerator Resize(RectTransform rectTransform, float minimum, float maximum)
    {
        float step = 0;
        while (step < 1)
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.CeilToInt(Mathf.SmoothStep(minimum, maximum, step += Time.deltaTime)));
            yield return AddonManagement.EndOfFrame;
        }
        yield return AddonManagement.EndOfFrame;
        if (maximum == 26)
        {
            content.SetActive(false);
        }
    }
}

public struct SpawnGroup
{
    public string monsterName;
    public int amount;
    public int slot;
}

public struct Spawner
{
    public string spawnName, uniqueId;
    public int area;
#if UMMORPG_3D_C || UMMORPG_3D_R
    public Vector3 position;
    public SpawnGroup[] groups;

    public Spawner(string _spawnName, string _uniqueId, int _area, Vector3 _position, SpawnGroup[] _groups)
    {
        spawnName = _spawnName;
        uniqueId = _uniqueId;
        area = _area;
        position = _position;
        groups = _groups;
    }
#else
    public Vector2 position;
    public SpawnGroup[] groups;

    public Spawner(string _spawnName, string _uniqueId, int _area, Vector2 _position, SpawnGroup[] _groups)
    {
        spawnName = _spawnName;
        uniqueId = _uniqueId;
        area = _area;
        position = _position;
        groups = _groups;
    }
#endif
}
