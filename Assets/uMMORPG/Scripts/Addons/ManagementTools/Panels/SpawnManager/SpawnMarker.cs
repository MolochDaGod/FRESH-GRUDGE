using UnityEngine;

public class SpawnMarker : MonoBehaviour
{
    public Transform spawnArea;

#if UMMORPG_2D_C || UMMORPG_2D_R
    void Start()
    {
        // it's just for easier integration with the 2d version
        // you can make these changes manually and then delete the following lines
        Vector3 markerScale = transform.GetChild(0).localScale;
        markerScale.x = 0.25f;
        markerScale.y = 0.25f;
        transform.GetChild(0).localScale = markerScale;

        Quaternion markerRotation = transform.GetChild(0).rotation;
        markerRotation.x = 0;
        transform.GetChild(0).rotation = markerRotation;

        Quaternion areaRotation = transform.GetChild(1).rotation;
        areaRotation.x = 0;
        transform.GetChild(1).rotation = areaRotation;
    }
#endif
}
