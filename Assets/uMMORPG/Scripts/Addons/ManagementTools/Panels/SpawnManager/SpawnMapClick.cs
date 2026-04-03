using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpawnMapClick : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public static SpawnMapClick singleton;
    public Camera miniMapCam;

    [HideInInspector] public int cameraDragSpeed;

    RawImage m_RawImage;
    RectTransform rectTransform;
    Texture texture;

    bool onMinimap;

    void Awake()
    {
        // initialize singleton
        if (singleton == null) singleton = this;
    }

    void Start()
    {
        m_RawImage = GetComponent<RawImage>();
        rectTransform = m_RawImage.rectTransform;
        texture = m_RawImage.texture;
        cameraDragSpeed = MToolsSettings.Settings.cameraDragSpeed;
    }

    void Update()
    {
        // hold down the right mouse button to pan the map
        if (Input.GetMouseButton(1))
        {
            // check if the mouse cursor is on the map area
            if (onMinimap)
            {
                float speed = cameraDragSpeed * Time.deltaTime;
#if UMMORPG_3D_C || UMMORPG_3D_R
                miniMapCam.transform.position -= new Vector3(Input.GetAxis("Mouse X") * speed, 0, Input.GetAxis("Mouse Y") * speed);
#else
                Vector2 position = miniMapCam.transform.position;
                position -= new Vector2(Input.GetAxis("Mouse X") * speed, Input.GetAxis("Mouse Y") * speed);
                Vector3 newPosition = miniMapCam.transform.position;
                newPosition.x = position.x;
                newPosition.y = position.y;
                miniMapCam.transform.position = newPosition;
#endif
            }
        }
    }

    public void OnPointerEnter(PointerEventData data)
    {
        onMinimap = true;
    }

    public void OnPointerExit(PointerEventData data)
    {
        onMinimap = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // left click to place a spawn marker
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Vector2 localCursor = new Vector2(0, 0);

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.pressPosition, eventData.pressEventCamera, out localCursor))
            {
                Rect r = rectTransform.rect;

                // using the size of the texture and the local cursor, clamp the X,Y coords between 0 and width - height of texture
                float coordX = Mathf.Clamp(0, (localCursor.x - r.x) * texture.width / r.width, texture.width);
                float coordY = Mathf.Clamp(0, (localCursor.y - r.y) * texture.height / r.height, texture.height);

                // convert coordX and coordY to % (0.0-1.0) with respect to texture width and height
                float recalcX = coordX / texture.width;
                float recalcY = coordY / texture.height;

                localCursor = new Vector2(recalcX, recalcY);

                CastMiniMapRayToWorld(localCursor);
            }
        }
    }

    void CastMiniMapRayToWorld(Vector2 localCursor)
    {
#if UMMORPG_3D_C || UMMORPG_3D_R
        Ray miniMapRay = miniMapCam.ScreenPointToRay(new Vector2(localCursor.x * miniMapCam.pixelWidth, localCursor.y * miniMapCam.pixelHeight));

        if (Physics.Raycast(miniMapRay, out var miniMapHit))
        {
            // check if this position is on the NavMesh
            if (NavMesh.SamplePosition(miniMapHit.point, out var navMeshHit, 1, 1))
            {
                if (UISpawnMap.singleton.targetPin != null)
                {
                    UISpawnMap.singleton.targetPin.transform.position = miniMapHit.point;
                    UISpawnMap.singleton.positionValueText.text = miniMapHit.point.ToString();
                }
            }
        }
#endif
#if UMMORPG_2D_C || UMMORPG_2D_R
        Vector3 worldPoint = miniMapCam.ScreenToWorldPoint(new Vector2(localCursor.x * miniMapCam.pixelWidth, localCursor.y * miniMapCam.pixelHeight));
        Vector3Int cellPosition = UIManagement.singleton.grid.WorldToCell(worldPoint);

        Vector2 position = UIManagement.singleton.grid.GetCellCenterWorld(cellPosition);
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero);
        if (!hit.collider)
        {
            // check if this position is on the NavMesh
            if (NavMesh2D.SamplePosition(position, out var navMeshHit, 1, 1))
            {
                if (UISpawnMap.singleton.targetPin != null)
                {
                    UISpawnMap.singleton.targetPin.transform.position = position;
                    UISpawnMap.singleton.positionValueText.text = position.ToString();
                }
            }
        }
#endif
    }
}
