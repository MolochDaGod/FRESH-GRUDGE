using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UISpawnManager : MonoBehaviour
{
    public static UISpawnManager singleton;
    public Canvas canvas;

    public Transform content;
    public SpawnerButton spawnerButton;

    RectTransform rectTransform;

    void Awake()
    {
        // initialize singleton
        if (singleton == null) singleton = this;
    }

    void Start()
    {
        rectTransform = content.GetComponent<RectTransform>();
    }

    // triggered by clicking the spawn button on the toolbar
    public void PanelActivation()
    {
        canvas.enabled = !canvas.enabled;

        if (canvas.enabled)
        {
            // show a list of buttons created on the basis of data loaded from the database
            UIManagement.singleton.player.CmdShowSpawners();
        }
        // find and close the previously opened panel
        UIManagement.singleton.ClosePanelsExceptOne(canvas);
        AddonManagement.ShowCanvasContent(canvas);
    }

    public void AddSpawner()
    {
#if UMMORPG_3D_C || UMMORPG_3D_R
        UISpawnMap.singleton.Show("", Vector3.zero, 1, () =>
#else
        UISpawnMap.singleton.Show("", Vector2.zero, 1, () =>
#endif
        {
            SpawnerButton go = Instantiate(spawnerButton, content);
            go.spawnName.text = UISpawnMap.singleton.labelInputField.text;
            go.area = Convert.ToInt32(UISpawnMap.singleton.areaSizeText.text);
            go.position = UISpawnMap.singleton.targetPin.transform.position;

            ToggleGroup group = content.GetComponent<ToggleGroup>();
            go.toggleGroup = group;

            UISpawnMap.singleton.Hide();
        });
    }

    public void AddNewSpawner(Spawner spawnerData)
    {
        ToggleGroup toggleGroup = content.GetComponent<ToggleGroup>();

        SpawnerButton button = Instantiate(spawnerButton, content);
        button.spawnName.text = spawnerData.spawnName;
        button.uniqueId = spawnerData.uniqueId;
        button.area = spawnerData.area;
        button.position = spawnerData.position;
        button.SpawnerSlots(spawnerData, spawnerData.groups.Length);
        button.toggleGroup = toggleGroup;
    }

    public void ChangeSpawner(Spawner spawnerData)
    {
        for (int i = 0; i < content.childCount; ++i)
        {
            SpawnerButton button = content.GetChild(i).GetComponent<SpawnerButton>();
            if (button.uniqueId == spawnerData.uniqueId)
            {
                button.spawnName.text = spawnerData.spawnName;
                button.area = spawnerData.area;
                button.position = spawnerData.position;
                button.SpawnerSlots(spawnerData, spawnerData.groups.Length);
            }
        }
    }

    public void DestroySpawnerButton(string uniqueId)
    {
        for (int i = 0; i < content.childCount; ++i)
        {
            SpawnerButton slot = content.GetChild(i).GetComponent<SpawnerButton>();
            if (slot.uniqueId == uniqueId)
            {
                Destroy(slot.gameObject);

                // scroll to the beginning
                GetStartPosition();
            }
        }
    }

    public void DestroySpawnerButtons()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            GameObject slot = content.GetChild(i).gameObject;
            Destroy(slot);
        }
    }

    public void NoSpawners()
    {
        UIUtils.BalancePrefabs(spawnerButton.gameObject, 0, content);
    }

    public void LoadSpawners(Spawner[] spawners)
    {
        UIUtils.BalancePrefabs(spawnerButton.gameObject, spawners.Length, content);

        ToggleGroup toggleGroup = content.GetComponent<ToggleGroup>();
        if (toggleGroup.AnyTogglesOn())
        {
            toggleGroup.SetAllTogglesOff();
        }

        for (int i = 0; i < spawners.Length; ++i)
        {
            SpawnerButton button = content.GetChild(i).GetComponent<SpawnerButton>();
            Spawner spawner = spawners[i];

            button.spawnName.text = spawner.spawnName;
            button.uniqueId = spawner.uniqueId;
            button.area = spawner.area;
            button.position = spawner.position;
            button.toggleGroup = toggleGroup;
            button.SpawnerSlots(spawner, spawner.groups.Length);
        }
        // scroll to the beginning
        GetStartPosition();
    }

    public void GetStartPosition()
    {
        if (rectTransform.anchoredPosition != Vector2.zero)
        {
            StartCoroutine(Move(rectTransform.anchoredPosition, Vector2.zero));
        }
    }

    public void MovePanel(Vector2 targetPos)
    {
        StartCoroutine(Move(rectTransform.anchoredPosition, targetPos));
    }

    IEnumerator Move(Vector2 startPos, Vector2 targetPos)
    {
        float step = 0;
        while (step < 1)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, step += Time.deltaTime);
            yield return AddonManagement.EndOfFrame;
        }
    }
}
