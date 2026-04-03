using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountManager : MonoBehaviour
{
    public static MountManager Instance = null;

    public List<GameObject> mountPrefabs = new List<GameObject>();
    public void Awake()
    {
        Instance = this;
    }

}
