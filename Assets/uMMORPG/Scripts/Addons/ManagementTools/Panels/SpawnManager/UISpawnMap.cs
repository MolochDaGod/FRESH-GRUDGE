using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UISpawnMap : MonoBehaviour
{
    public static UISpawnMap singleton;
    public Canvas canvas;

    public Camera spawnCamera;

    public Button saveButton;

    public Slider zoomSlider;
    public Slider spawnAreaSlider;

    [HideInInspector] public int spawnArea;

    public SpawnMarker spawnMarker;
    [HideInInspector] public SpawnMarker targetPin;

    public TMP_Text positionValueText;
    public TMP_Text areaSizeText;

    public TMP_InputField labelInputField;

    void Awake()
    {
        // initialize singleton
        if (singleton == null) singleton = this;
    }

    void Start()
    {
        spawnCamera.backgroundColor = Camera.main.backgroundColor;

        // item drop
#if ITEM_DROP_C || ITEM_DROP_R
        spawnCamera.cullingMask = AddonItemDrop.DisableLayers(spawnCamera.cullingMask);
#endif
#if UMMORPG_3D_R
        spawnCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("TextOverlay"));
#endif
#if UMMORPG_2D_C || UMMORPG_2D_R
        // it's just for easier integration with the 2d version
        // you can make these changes manually and then delete the following lines
        Vector3 cameraPosition = spawnCamera.transform.position;
        cameraPosition.z = -1;
        spawnCamera.transform.position = cameraPosition;

        Quaternion cameraRotation = spawnCamera.transform.rotation;
        cameraRotation.x = 0;
        spawnCamera.transform.rotation = cameraRotation;

        zoomSlider.minValue = 5;
        zoomSlider.maxValue = 30;
        zoomSlider.value = 10;
#endif
    }

#if UMMORPG_3D_C || UMMORPG_3D_R
    public void Show(string spawnName, Vector3 spawnPos, int area, UnityAction onConfirm)
#else
    public void Show(string spawnName, Vector2 spawnPos, int area, UnityAction onConfirm)
#endif
    {
        labelInputField.text = spawnName;
        positionValueText.text = spawnPos.ToString();
        spawnAreaSlider.value = area;

        if (!targetPin)
        {
            SpawnMarker prefab = Instantiate(spawnMarker, spawnPos, Quaternion.identity);
            targetPin = prefab;
            targetPin.spawnArea.localScale = area * Vector3.one;            
        }
        else
        {
            targetPin.transform.position = spawnPos;
            targetPin.spawnArea.localScale = area * Vector3.one;
        }
        // set the location of the camera to spawn point location
        ShowSpawnLocation();

        saveButton.onClick.SetListener(onConfirm);

        spawnCamera.enabled = true;
        canvas.enabled = true;
    }

    // executed when the value of the zoomSlider is changed
    public void ZoomLevelUpdate()
    {
        // set the zoom level
        spawnCamera.orthographicSize = zoomSlider.value;

#if UMMORPG_3D_C || UMMORPG_3D_R
        // camera drag speed depending on the zoom level
        SpawnMapClick.singleton.cameraDragSpeed = (int)zoomSlider.value * 6;
#endif
    }

    public void ValueChangeCheckArea()
    {
        // change the size of the spawn area
        spawnArea = (int)spawnAreaSlider.value;
        areaSizeText.text = spawnArea.ToString();
        if (targetPin != null)
        {
            targetPin.spawnArea.localScale = spawnArea * Vector3.one;
        }
    }

    public void ToggleValueChanged()
    {
#if UMMORPG_3D_C || UMMORPG_3D_R
        // enable or disable minimap markers
        spawnCamera.cullingMask ^= 1 << LayerMask.NameToLayer("MinimapMarker");
#else
        spawnCamera.cullingMask ^= 1 << LayerMask.NameToLayer("Player");
        spawnCamera.cullingMask ^= 1 << LayerMask.NameToLayer("Monster");
        spawnCamera.cullingMask ^= 1 << LayerMask.NameToLayer("Npc");
#endif
    }

    // invoked when the value of the text field changes
    public void ValueChangeCheckLabel()
    {
        // save button works only if you enter text
        if (string.IsNullOrWhiteSpace(labelInputField.text))
        {
            saveButton.interactable = false;
        }
        else if (!saveButton.IsInteractable())
        {
            saveButton.interactable = true;
        }
    }

    public void ShowSpawnLocation()
    {        
        Vector3 myLocation = targetPin.transform.position;

        // set the location of the camera to spawn point location
        Vector3 cameraLocation = spawnCamera.transform.position;
        cameraLocation.x = myLocation.x;
#if UMMORPG_3D_C || UMMORPG_3D_R
        cameraLocation.z = myLocation.z;
#else
        cameraLocation.y = myLocation.y;
#endif
        spawnCamera.transform.position = cameraLocation;
    }

    public void Hide()
    {
        spawnCamera.enabled = false;
        canvas.enabled = false;
    }
}
