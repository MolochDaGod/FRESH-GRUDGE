#if _SQLITE
using UnityEngine;
using UnityEngine.UI;

public class UITeleportMap : MonoBehaviour
{
    public static UITeleportMap singleton;
    public Canvas canvas;

    public Camera teleportCamera;
    public Slider zoomSlider;

    void Awake()
    {
        // initialize singleton
        if (singleton == null) singleton = this;
    }

    void Start()
    {
        teleportCamera.backgroundColor = Camera.main.backgroundColor;

        // item drop
#if ITEM_DROP_C || ITEM_DROP_R
        teleportCamera.cullingMask = AddonItemDrop.DisableLayers(teleportCamera.cullingMask);
#endif
#if UMMORPG_3D_R
        teleportCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("TextOverlay"));
#endif
#if UMMORPG_2D_C || UMMORPG_2D_R
        // it's just for easier integration with the 2d version
        // you can make these changes manually and then delete the following lines
        Vector3 cameraPosition = teleportCamera.transform.position;
        cameraPosition.z = -1;
        teleportCamera.transform.position = cameraPosition;

        Quaternion cameraRotation = teleportCamera.transform.rotation;
        cameraRotation.x = 0;
        teleportCamera.transform.rotation = cameraRotation;

        zoomSlider.minValue = 10;
        zoomSlider.maxValue = 30;
        zoomSlider.value = 15;

        teleportCamera.cullingMask ^= 1 << LayerMask.NameToLayer("Player");
        teleportCamera.cullingMask ^= 1 << LayerMask.NameToLayer("Monster");
        teleportCamera.cullingMask ^= 1 << LayerMask.NameToLayer("Npc");
#endif
    }

    // triggered by clicking the teleport button on the toolbar
    public void PanelActivation()
    {
        if (!canvas.enabled)
        {
            teleportCamera.enabled = true;

            // set the location of the camera to my location
            ShowMyLocation();
        }
        else
        {
            teleportCamera.enabled = false;
        }
        UIManagement.singleton.PanelActivation(canvas);
        AddonManagement.ShowCanvasContent(canvas);
    }

    // executed when the value of the zoomSlider is changed
    public void ZoomLevelUpdate()
    {
        // set the zoom level
        teleportCamera.orthographicSize = zoomSlider.value;

#if UMMORPG_3D_C || UMMORPG_3D_R
        // camera drag speed depending on the zoom level
        TeleportMapClick.singleton.cameraDragSpeed = (int)zoomSlider.value * 6;
#endif
    }

    public void ToggleValueChanged()
    {
#if UMMORPG_3D_C || UMMORPG_3D_R
        // enable or disable minimap markers
        teleportCamera.cullingMask ^= 1 << LayerMask.NameToLayer("MinimapMarker");
#else
        //teleportCamera.cullingMask ^= 1 << LayerMask.NameToLayer("Player");
        teleportCamera.cullingMask ^= 1 << LayerMask.NameToLayer("Monster");
        teleportCamera.cullingMask ^= 1 << LayerMask.NameToLayer("Npc");
#endif
    }

    public void ShowMyLocation()
    {        
        Vector3 myLocation = UIManagement.singleton.player.transform.position;

        // set the location of the camera to my location
        Vector3 cameraLocation = teleportCamera.transform.position;
        cameraLocation.x = myLocation.x;
#if UMMORPG_3D_C || UMMORPG_3D_R
        cameraLocation.z = myLocation.z;
#else
        cameraLocation.y = myLocation.y;
#endif
        teleportCamera.transform.position = cameraLocation;
    }
}

#endif
