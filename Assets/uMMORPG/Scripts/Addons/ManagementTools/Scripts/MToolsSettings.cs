using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

[HelpURL("https://docs.google.com/document/d/1ZUeeQfKkVsWGxMFlWiLC9jY5Q_BDMx9efVx5y96QXeQ/edit")]
public class MToolsSettings : ScriptableObject
{
    static MToolsSettings cache;
    public static MToolsSettings Settings => cache ?? (cache = Resources.Load<MToolsSettings>("MToolsSettings"));

#if UNITY_EDITOR
    [HideInInspector]
    public bool isInstalled;
#endif

    [Header("Key Bindings")]
    [Tooltip("Opens the main panel. Requires access level 2 or 3.")]
    public KeyCode mainPanelKey = KeyCode.M;

    [Tooltip("Opens the Support panel. Anyone can use it to send help request or report a bug.")]
    public KeyCode supportPanelKey = KeyCode.H;

    [Tooltip("Opens the Password panel. Anyone can use it to change the password.")]
    public KeyCode passwordPanelKey = KeyCode.N;

    [Tooltip("3D version - rotates an object created with the Creator panel.")]
    public KeyCode rotateLeftKey = KeyCode.LeftBracket;

    [Tooltip("3D version - rotates an object created with the Creator panel.")]
    public KeyCode rotateRightKey = KeyCode.RightBracket;

    [Header("Prefabs")]
    public GameObject managementTools;
    public GameObject onlinePlayerCounter;
    public GameObject spawner;
    public GameObject spawnMarker;
    public BuffSkill muteBuff;

    [Header("Others")]
    [Tooltip("Map panning speed for the Teleport and Spawn Map panel.")]
    [Min(1f)] public int cameraDragSpeed = 250;

    [Header("Templates")]
    #region Item Manager
    public ItemManager itemManager;
    [Serializable]
    public struct ItemManager
    {
#if UNITY_EDITOR
        [InspectorButton(nameof(LoadItemTemplates))] public bool loadItems;
#endif
        public ItemManagerCategory[] categories;
    }

    [Serializable]
    public struct ItemManagerCategory
    {
        public string category;
        public ScriptableItem[] items;
    }
    #endregion

    #region Creator
    public Creator creator;
    [Serializable]
    public struct Creator
    {
        [TextArea(1, 10)] public string folderMonsters;     // = "Assets/uMMORPG/Prefabs/Entities/Monsters";
        [TextArea(1, 10)] public string folderNpcs;         // = "Assets/uMMORPG/Prefabs/Entities/Npcs";
        [TextArea(1, 10)] public string folderEnvironments; // = "Assets/uMMORPG/Scripts/Addons/ManagementTools/Environments/3D";
#if UNITY_EDITOR
        [InspectorButton(nameof(LoadObjects))] public bool loadObjects;
#endif
        public CreatorCategory[] categories;
    }

    [Serializable]
    public struct CreatorCategory
    {
        public string category;
        public Transform[] objects;
    }
    #endregion

    [Header("Extras")]
    [Tooltip("Additional properties such as item names color based on the rarity of the item in the Item Manager panel, number of items on the server, save and delete in the Database Information panel.")]
    public ItemDropAddon itemDrop;
    [Serializable]
    public struct ItemDropAddon
    {
        public bool isActive;
    }
       
#if UNITY_EDITOR
    readonly string tag = "Environment";

    #region Default Settings
    // true/false for the first installation, 
    // because the effects of changes in the ScriptingDefineSymbols will not be included in the codeblock right after the change
    public void DefaultSettings(bool default3D = true)
    {
        mainPanelKey = KeyCode.M;
        supportPanelKey = KeyCode.H;
        passwordPanelKey = KeyCode.N;
        rotateLeftKey = KeyCode.LeftBracket;
        rotateRightKey = KeyCode.RightBracket;

        cameraDragSpeed = 250;

#if UMMORPG_2D_C || UMMORPG_2D_R
        default3D = false;
#endif
        creator.folderMonsters = "Assets/uMMORPG/Prefabs/Entities/Monsters";
        creator.folderNpcs = "Assets/uMMORPG/Prefabs/Entities/Npcs";
        creator.folderEnvironments = default3D ? "Assets/uMMORPG/Scripts/Addons/ManagementTools/Environments/3D" : "Assets/uMMORPG/Scripts/Addons/ManagementTools/Environments/2D";

        itemDrop.isActive = false;
    }
    #endregion

    #region Load Item Templates
    // automatically loads all items and divides them by category
    public void LoadItemTemplates()
    {
#if UMMORPG_3D_C
        List<ScriptableItem> templates = (from name in ScriptableItem.dict select name.Value).ToList();
#if ITEM_DROP_C || ITEM_DROP_R
        List<string> categories = templates.Where(go => !go.gold).Select(item => item.GetType().ToString().Replace("Item", "").Trim()).Distinct().ToList();
#else
        List<string> categories = templates.Select(item => item.GetType().ToString().Replace("Item", "").Trim()).Distinct().ToList();
#endif
        itemManager.categories = new ItemManagerCategory[categories.Count];

        for (int i = 0; i < categories.Count; ++i)
        {
            for (int j = 0; j < itemManager.categories.Length; ++j)
            {
                itemManager.categories[j].category = categories[j];
                itemManager.categories[j].items = templates.Select(item => item).Where(go => go.GetType().ToString().Replace("Item", "").Trim() == categories[j]).ToArray();
            }
        }
#endif

#if UMMORPG_2D_C || UMMORPG_3D_R || UMMORPG_2D_R
        List<ScriptableItem> templates = (from name in ScriptableItem.All select name.Value).ToList();
#if ITEM_DROP_C || ITEM_DROP_R
        List<string> categories = templates.Where(go => !go.gold).Select(item => item.GetType().ToString().Replace("Item", "").Trim()).Distinct().ToList();
#else
        List<string> categories = templates.Select(item => item.GetType().ToString().Replace("Item", "").Trim()).Distinct().ToList();
#endif
        itemManager.categories = new ItemManagerCategory[categories.Count];

        for (int i = 0; i < categories.Count; ++i)
        {
            for (int j = 0; j < itemManager.categories.Length; ++j)
            {
                itemManager.categories[j].category = categories[j];
                itemManager.categories[j].items = templates.Select(item => item).Where(go => go.GetType().ToString().Replace("Item", "").Trim() == categories[j]).ToArray();
            }
        }
#endif
        EditorUtility.SetDirty(this);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    #endregion

    #region Load Objects
    // automatically loads all monsters, npcs and environments and divides them by category
    public void LoadObjects()
    {
        if (!AssetDatabase.IsValidFolder(creator.folderMonsters))
        {
            Debug.Log("The given path does not exists (Monsters).");
            return;
        }

        if (!AssetDatabase.IsValidFolder(creator.folderNpcs))
        {
            Debug.Log("The given path does not exists (Npcs).");
            return;
        }

        if (!AssetDatabase.IsValidFolder(creator.folderEnvironments))
        {
            Debug.Log("The given path does not exists (Environments).");
            return;
        }

        #region 2D Samples
#if UMMORPG_2D_C || UMMORPG_2D_R
        string path1 = System.IO.Path.Combine(creator.folderEnvironments, "Boulder.prefab");
        GameObject boulderPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(path1, typeof(GameObject));
        if (boulderPrefab != null)
        {
            if (boulderPrefab.TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer.sortingLayerName = "Obstacles";
            }

            if (!boulderPrefab.TryGetComponent(out NavMeshObstacle2D obstacle))
            {
                NavMeshObstacle2D obstacle2D = boulderPrefab.gameObject.AddComponent<NavMeshObstacle2D>();
                obstacle2D.size.x = 0.8f;
                obstacle2D.size.y = 0.8f;
                obstacle2D.carve = true;
            }

            if (!boulderPrefab.TryGetComponent(out M_Environment environment))
            {
                boulderPrefab.AddComponent<M_Environment>();
            }
#if UMMORPG_2D_C
            if (!boulderPrefab.TryGetComponent(out NetworkProximityGridChecker gridChecker))
            {
                NetworkProximityGridChecker component = boulderPrefab.AddComponent<NetworkProximityGridChecker>();
                component.checkMethod = NetworkProximityGridChecker.CheckMethod.XY_FOR_2D;
            }
#endif
        }

        string path2 = System.IO.Path.Combine(creator.folderEnvironments, "Boulder2.prefab");
        GameObject boulderPrefab2 = (GameObject)AssetDatabase.LoadAssetAtPath(path2, typeof(GameObject));
        if (boulderPrefab2 != null)
        {
            if (boulderPrefab2.transform.GetChild(2).TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer.sortingLayerName = "BehindEntities";
            }

            if (boulderPrefab2.transform.GetChild(3).TryGetComponent(out SpriteRenderer spriteRenderer2))
            {
                spriteRenderer2.sortingLayerName = "BehindEntities";
            }

            if (!boulderPrefab2.TryGetComponent(out NavMeshObstacle2D obstacle))
            {
                NavMeshObstacle2D obstacle2D = boulderPrefab2.AddComponent<NavMeshObstacle2D>();
                obstacle2D.center.x = 0.5f;
                obstacle2D.center.y = -0.5f;
                obstacle2D.size.x = 1.5f;
                obstacle2D.size.y = 1.3f;
                obstacle2D.carve = true;
            }

            if (!boulderPrefab2.TryGetComponent(out M_Environment environment))
            {
                boulderPrefab2.gameObject.AddComponent<M_Environment>();
            }
#if UMMORPG_2D_C
            if (!boulderPrefab2.TryGetComponent(out NetworkProximityGridChecker gridChecker))
            {
                NetworkProximityGridChecker component = boulderPrefab2.AddComponent<NetworkProximityGridChecker>();
                component.checkMethod = NetworkProximityGridChecker.CheckMethod.XY_FOR_2D;
            }
#endif
        }
#endif
        #endregion

        NetworkManagerMMO manager = FindObjectOfType<NetworkManagerMMO>();
        if (manager != null)
        {
            Monster[] monsters = FindResources<Monster>(creator.folderMonsters);
            foreach (Monster prefab in monsters)
            {
                // to avoid adding duplicate elements
                if (!manager.spawnPrefabs.Contains(prefab.gameObject))
                {
                    manager.spawnPrefabs.Add(prefab.gameObject);
                }
            }
            
            Npc[] npcs = FindResources<Npc>(creator.folderNpcs);
            foreach (Npc prefab in npcs)
            {
                // to avoid adding duplicate elements
                if (!manager.spawnPrefabs.Contains(prefab.gameObject))
                {
                    manager.spawnPrefabs.Add(prefab.gameObject);
                }
            }

#if _SQLITE
            M_Environment[] environments = FindResources<M_Environment>(creator.folderEnvironments);
            foreach (M_Environment prefab in environments)
            {
                // assign tag
                if (prefab.tag != tag)
                {
                    prefab.tag = tag;
                }

                // assign layer
                int index = LayerMask.NameToLayer(tag);
                if (prefab.gameObject.layer != index)
                {
                    prefab.gameObject.layer = index;
                }

                // to avoid adding duplicate elements
                if (!manager.spawnPrefabs.Contains(prefab.gameObject))
                {
                    manager.spawnPrefabs.Add(prefab.gameObject);
                }
            }
#endif

#if _SQLITE
            List<Transform> objects = manager.RegisteredPrefabs();
            List<string> categories = objects.Select(item => item.tag).Distinct().ToList();

            creator.categories = new CreatorCategory[categories.Count];

            for (int i = 0; i < categories.Count; ++i)
            {
                for (int j = 0; j < creator.categories.Length; ++j)
                {
                    creator.categories[j].category = categories[j];
                    creator.categories[j].objects = objects.Select(item => item).Where(go => go.tag == categories[j]).ToArray();
                }
            }
#endif

            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);

            EditorUtility.SetDirty(this);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
    #endregion

    public void RemoveTagAndLayer()
    {
        if (AssetDatabase.IsValidFolder(creator.folderEnvironments))
        {
#if _SQLITE
            M_Environment[] environments = FindResources<M_Environment>(creator.folderEnvironments);
            foreach (M_Environment prefab in environments)
            {
                prefab.tag = "Untagged";
                prefab.gameObject.layer = 0;
            }
#endif
        }
    }

    /// <summary>
    /// Returns all objects by type from the selected folder.
    /// </summary>
    public static T[] FindResources<T>(string path) where T : UnityEngine.Object
    {
        string[] guids = AssetDatabase.FindAssets("t:Object", new[] { path });
        return guids.Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid))).Where(asset => asset != null).ToArray();
    }
#endif
}
