#if _SQLITE
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class UICreator : MonoBehaviour
{
    public static UICreator singleton;
    public Canvas canvas;

    [HideInInspector] public GameObject selectedObject;

    public Transform categoryContent;
    public Transform objectsContent;

    public GameObject objectHeader;
    public TMP_Text currentObject;

    public ScrollRect objectsScrollRect;
    public Button categorySlotPrefab;
    public UIManagementButton objectSlotPrefab;
    public TMP_InputField nameObjectInput;

    int currentCategory;
    /*
    [Serializable]
    public struct CreatorCategory
    {
        public string category;
        public Transform[] objects;
    }
    public CreatorCategory[] creatorCategories;
    */
    void Awake()
    {
        // initialize singleton
        if (singleton == null) singleton = this;
    }

    void Start()
    {
        // create category buttons
        ShowCategories();
    }

    // triggered by clicking the creator button on the toolbar
    public void PanelActivation()
    {
        UIManagement.singleton.PanelActivation(canvas);
        AddonManagement.ShowCanvasContent(canvas);
    }

    void ShowCategories()
    {
        UIUtils.BalancePrefabs(categorySlotPrefab.gameObject, MToolsSettings.Settings.creator.categories.Length, categoryContent);

        for (int i = 0; i < MToolsSettings.Settings.creator.categories.Length; ++i)
        {
            UIManagementButton button = categoryContent.GetChild(i).GetComponent<UIManagementButton>();

            int icopy = i;
            string buttonName = MToolsSettings.Settings.creator.categories[icopy].category;
            button.name = buttonName;
            button.nameText.text = buttonName;

            button.button.onClick.SetListener(() =>
            {
                // change the category and then scroll up again
                TaskOnClick(icopy);
                UIManagement.singleton.ScrollToBeginning(objectsScrollRect);
            });
        }
        // show objects from the first category
        ShowObjects();
    }

    // invoked when the value of the text field changes or after choosing a category
    public void ShowObjects()
    {
        if (MToolsSettings.Settings.creator.categories.Length > 0)
        {
            // show objects from the current category
            //Transform[] objects = MToolsSettings.Settings.creator.categories[currentCategory].objects;

            // show found objects in the current category
            //Transform[] objects = string.IsNullOrWhiteSpace(nameObjectInput.text) ? MToolsSettings.Settings.creator.categories[currentCategory].objects : MToolsSettings.Settings.creator.categories[currentCategory].objects.Where(i => i.name.Contains(nameObjectInput.text)).ToArray();

            // show objects from the current category or search for them
            Transform[] objects = string.IsNullOrWhiteSpace(nameObjectInput.text) ? MToolsSettings.Settings.creator.categories[currentCategory].objects : GetObjectsByName(nameObjectInput.text);
            
            UIUtils.BalancePrefabs(objectSlotPrefab.gameObject, objects.Length, objectsContent);

            for (int i = 0; i < objects.Length; ++i)
            {
                UIManagementButton slot = objectsContent.GetChild(i).GetComponent<UIManagementButton>();
                Transform item = objects[i];

                string objName = item.name;
                slot.name = objName;
                slot.nameText.text = objName;

                slot.button.onClick.SetListener(() =>
                {
                    SelectedName(objName);
                });
            }
        }
    }

    void TaskOnClick(int index)
    {
        //Debug.Log(index);
        UIManagement.singleton.ClearButton(categoryContent, currentCategory);
        currentCategory = index;
        UIManagement.singleton.SetButton(categoryContent, index);

        // refresh objects
        ShowObjects();
    }

    // keyword searching
    public Transform[] GetObjectsByName(string objectName)
    {
        List<Transform> list = new List<Transform>();        
        foreach (MToolsSettings.CreatorCategory category in MToolsSettings.Settings.creator.categories)
        {
            foreach (Transform obj in category.objects)
            {
                if (obj.name.Contains(objectName))
                    list.Add(obj);
            }
        }
        return list.ToArray();
    }

    // automatically loads all monsters, npcs and environments and divides them by category
    public void LoadObjects()
    {
        /*
        List<Transform> objects = FindObjectOfType<NetworkManagerMMO>().RegisteredPrefabs();
        List<string> categories = objects.Select(item => item.tag).Distinct().ToList();

        creatorCategories = new CreatorCategory[categories.Count];

        for (int i = 0; i < categories.Count; ++i)
        {
            for (int j = 0; j < creatorCategories.Length; ++j)
            {
                creatorCategories[j].category = categories[j];
                creatorCategories[j].objects = objects.Select(item => item).Where(go => go.tag == categories[j]).ToArray();
            }
        }
        */
    }

    public void SelectedName(string objName)
    {
        if (!objectHeader.activeSelf)
        {
            objectHeader.SetActive(true);
        }
        currentObject.text = objName;

        DestroyObject(); // destroy previous

        InstantiateObject(objName);
    }

    public void CancelObject()
    {
        if (objectHeader.activeSelf)
        {
            objectHeader.SetActive(false);
        }
        currentObject.text = "";

        Destroy(selectedObject.gameObject);
        selectedObject = null;
    }

    public void RestoreObject()
    {
        if (objectHeader.activeSelf)
        {
            InstantiateObject(currentObject.text);
        }
    }

    public void InstantiateObject(string objName)
    {
        Transform clone = ((NetworkManagerMMO)NetworkManager.singleton).RegisteredPrefabs().Find(p => p.name == objName);
        if (clone == null)
            return;

        selectedObject = Instantiate(clone.gameObject);
#if UMMORPG_3D_C || UMMORPG_3D_R
        foreach (Collider collider in selectedObject.GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }
#endif
#if UMMORPG_2D_C || UMMORPG_2D_R
        foreach (Collider2D collider in selectedObject.GetComponentsInChildren<Collider2D>())
        {
            collider.enabled = false;
        }

        foreach (NavMeshObstacle2D m in selectedObject.GetComponentsInChildren<NavMeshObstacle2D>())
        {
            m.enabled = false;
        }

        foreach (NavMeshAgent2D m in selectedObject.GetComponentsInChildren<NavMeshAgent2D>())
        {
            m.enabled = false;
        }

        foreach (NetworkNavMeshAgent2D m in selectedObject.GetComponentsInChildren<NetworkNavMeshAgent2D>())
        {
            m.enabled = false;
        }
#endif
    }

    public void DestroyObject()
    {
        if (selectedObject != null)
        {
            Destroy(selectedObject.gameObject);
        }
    }

    public Vector3 MouseToTerrainPosition()
    {
#if UMMORPG_2D_C || UMMORPG_2D_R
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = UIManagement.singleton.grid.WorldToCell(worldPoint);

        Vector2 position = UIManagement.singleton.grid.GetCellCenterWorld(cellPosition);
#else
        Vector3 position = Vector3.zero;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit info, Mathf.Infinity, LayerMask.GetMask("Default")))
        {
            if (NavMesh.SamplePosition(info.point, out var navMeshHit, Mathf.Infinity, 1))
            {
                position = info.point;
            }
        }
#endif
        return position;
    }
    
    public void RotateObj(float value)
    {
        Vector3 _rotation = selectedObject.transform.eulerAngles;
        _rotation.y += value;
        selectedObject.transform.eulerAngles = _rotation;
    }

    public void SetObjectViaPosition()
    {
        if (UIManagement.singleton.player.CheckAccess())
        {
            if (!string.IsNullOrWhiteSpace(currentObject.text))
            {
#if UMMORPG_3D_C || UMMORPG_3D_R
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out var hit))
                {
                    if (hit.transform.name != currentObject.text)
                    {
                        // check if this position is on the NavMesh
                        if (NavMesh.SamplePosition(hit.point, out var navMeshHit, 1, 1))
                        {
                            UIManagement.singleton.player.CmdMsgCreateObject(currentObject.text, navMeshHit.position, selectedObject.transform.rotation);
                        }
                        else
                        {
                            UIManagement.singleton.player.CmdSendNavMeshMessage("You can create objects only on the walkable area!");
                        }
                    }
                    else UIManagement.singleton.player.CmdSendNavMeshMessage("Choose a different area!");
                }
#endif
#if UMMORPG_2D_C || UMMORPG_2D_R
                Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int cellPosition = UIManagement.singleton.grid.WorldToCell(worldPoint);

                Vector2 position = UIManagement.singleton.grid.GetCellCenterWorld(cellPosition);
                RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero);
                if (hit.collider != null)
                {
                    UIManagement.singleton.player.CmdSendNavMeshMessage("Choose a different area!");
                }
                else
                {
                    // check if this position is on the NavMesh
                    if (NavMesh2D.SamplePosition(position, out var navMeshHit, 1, 1))
                    {
                        UIManagement.singleton.player.CmdMsgCreateObject(currentObject.text, position);
                    }
                    else
                    {
                        UIManagement.singleton.player.CmdSendNavMeshMessage("You can create objects only on the walkable area!");
                    }
                }
#endif
            }
        }
    }
}

#endif
