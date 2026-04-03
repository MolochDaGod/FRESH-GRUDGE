using UnityEngine;
using Mirror;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Linq;
using System;

// NETWORK MANAGER
public class NetworkManagerMMOAutoRegister : MonoBehaviour
{
    public NetworkManagerMMO networkManagerMMO;

    [Header("[Path Search Spawnlable (look Tooltip for help)]")]
    public bool searchByPath = false;
    public bool showSkippedPrefabLog = false;


    [Tooltip("Add a folder to scan not to scan the whole project, which will be that much faster! \r\n\r\n Exemple Scan full folder and subfolder uMMORPG add : \r\nAssets/uMMORPG\r\n\r\n or if all prefab is in folder prefab add : \r\n Assets/uMMORPG/Prefabs")]
    public string[] pathSearch;
    [Header("[Exclude Folder name for search prefabs")]
    [Tooltip("in 3d we'll exclude \"2D only\", and in 2d we'll exclude \"3D only\"")]
    public string[] excludeFolderName;
    // -----------------------------------------------------------------------------------
    // AutoRegisterSpawnables
    // @Editor
    // -----------------------------------------------------------------------------------
    public void AutoRegisterSpawnables()
    {
#if UNITY_EDITOR
   
        var guids = (searchByPath) ? AssetDatabase.FindAssets("t:Prefab",pathSearch) : AssetDatabase.FindAssets("t:Prefab");

        int spanwPrefabMMOOld = networkManagerMMO.spawnPrefabs.Count();
        List<GameObject> toSelect = new List<GameObject>();
        networkManagerMMO.spawnPrefabs.Clear();

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            UnityEngine.Object[] toCheck = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (UnityEngine.Object obj in toCheck)
            {
                string fullPath = AssetDatabase.GetAssetPath(obj);
                if (fullPath != string.Empty)
                {
                    var lastPartOfCurrentDirectoryName = Path.GetDirectoryName(fullPath).Split(Path.DirectorySeparatorChar).Last();
                    var go = obj as GameObject;
                    if (go == null)
                    {
                        continue;
                    }
                    if (Array.IndexOf(excludeFolderName, lastPartOfCurrentDirectoryName) >= 0)
                    {
                        if(showSkippedPrefabLog)
                            Debug.Log(fullPath + " was skipped, currently in folder excluded");
                        continue;
                    }
                    NetworkIdentity comp = go.GetComponent<NetworkIdentity>();
                    if (comp != null && !comp.serverOnly)
                    {

                        NetworkSpawnable sp = go.GetComponent<NetworkSpawnable>();

                        if (sp == null)
                        {
                            toSelect.Add(go);
                        }
                    }
                }

            }
        }
        networkManagerMMO.spawnPrefabs.AddRange(toSelect.ToArray());
        Debug.Log("Added [" + toSelect.Count + "] network prefabs to spawnables list (Previous Spanwable count :"+ spanwPrefabMMOOld + ")");
#endif
    }
    // -----------------------------------------------------------------------------------
}
