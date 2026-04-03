// this component handles the construction preview while a structure item is
// selected on the hotbar
using UnityEngine;
using Mirror;

public class PlayerConstruction : NetworkBehaviour
{
    [Header("Components")]
    //public PlayerHotbar hotbar;
    //public PlayerLook look;

    [Header("Configuration")]
    public KeyCode rotationKey = KeyCode.R;
    public Color canBuildColor = Color.cyan;
    public Color cantBuildColor = Color.red;

    public LayerMask raycastLayers;
    // current preview (on client)
    // -> no need to sync it to server. server can estimate it via:
    //    - position from look.raycast direction
    //    - rotation from rotationIndex
    //    - bounds from structure.previewPrefab
    [HideInInspector] public GameObject preview;

    // rotation index needs to be known on the server to decide final build
    // position / rotation
    //[SyncVar] int rotationIndex;

    [SyncVar] Quaternion rotation;

    [SyncVar] Vector3 position;           //FIX add position

    [HideInInspector] public int inventoryIndex;

    // helper function to get the current structure in hands (if any)
    public StructureItem CurrentStructure;

    void Update()
    {
        if (!isLocalPlayer) return;

        // selected a structure in hotbar right now?
        StructureItem structure = CurrentStructure;
        if (structure != null)
        {
            // destroy preview if not matching anymore
            if (preview != null && preview.name != structure.structurePrefab.name)
                Destroy(preview);

            // load preview if not loaded yet
            if (preview == null)              //FIX REMOVE
            {
                RaycastHit hit1;
                Ray ray1;
                Vector3 pos = Vector3.zero;
                ray1 = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray1, out hit1, Mathf.Infinity)) //check if the ray hit something
                {
                    pos = hit1.point; //use this position for what you want to do


                }
                preview = Instantiate(structure.structurePrefab,
                    CalculatePreviewPosition(structure, pos),
                    CalculatePreviewRotation());
                // avoid "(Clone)"
                preview.name = structure.structurePrefab.name;

                preview.layer = 15; // set it to your layerSCORGE

                // remove all script logic. it's only used as a preview.
                // (except NetworkIdentity, which would throw an error)
                // -> remove components from bottom to top to avoid
                //    RequireComponent errors like: "Can't remove Combat
                //    (Script) because Structure (Script) depends on it"
                // -> in other words, we assume that RequireComponent flow goes
                //    from bottom to top. that's fine for most projects.
                Behaviour[] behaviours = preview.GetComponentsInChildren<Behaviour>();
                for (int i = behaviours.Length - 1; i >= 0; --i)
                    if (!(behaviours[i] is NetworkIdentity) && !(behaviours[i] is boundingCollider))
                        Destroy(behaviours[i]);

                // remove all colliders. it's only used as a preview.
                foreach (Collider co in preview.GetComponentsInChildren<Collider>())
                {
                    if (!co.CompareTag("BuildPreview"))  //check and make sure the collider doesnt have this tag
                    {
                        Destroy(co);
                    }
                }
            }
            RaycastHit hit;
            Ray ray;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity)) //check if the ray hit something
            {
                Vector3 pos = hit.point; //use this position for what you want to do  
                if (Vector3.Distance(Player.localPlayer.transform.position, pos) < 6)
                {
                    preview.transform.position = pos;


                    Quaternion rot = preview.transform.rotation;
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        rot.y += Input.GetAxis("Mouse ScrollWheel") * 100f * Time.deltaTime;
                    }

                    preview.transform.rotation = rot;


                    CmdSetRotationIndex(rot, pos);          //FIX pass position aswell and move this into raycast


                }
            }


            // set color depending on if we can build there or not
            // (use sharedMaterial for prefabs and material for runtime)
            Bounds bounds = preview.GetComponent<boundingCollider>().col.bounds;
            bool canBuild = structure.CanBuildThere(preview.transform.position, bounds, raycastLayers);
            foreach (Renderer renderer in preview.GetComponentsInChildren<Renderer>())
                renderer.material.color = canBuild ? canBuildColor : cantBuildColor;

            if (Input.GetMouseButtonDown(0) && !Utils.IsCursorOverUserInterface())
            {
                if (canBuild)
                {
                    CmdTrySet(Player.localPlayer, inventoryIndex);
                }
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (preview != null) Destroy(preview);
                CurrentStructure = null;
                structure = null;
                preview = null;
                inventoryIndex = -1;
                return;
            }

            ItemSlot item = Player.localPlayer.inventory.slots[inventoryIndex];

            if (item.amount > 0)
            {
                if (item.item.data is StructureItem)
                {


                }
                else
                {
                    if (preview != null) Destroy(preview);
                    CurrentStructure = null;
                    structure = null;
                    preview = null;
                    inventoryIndex = -1;
                    return;
                }
            }
            else
            {
                if (preview != null) Destroy(preview);
                CurrentStructure = null;
                structure = null;
                preview = null;
                inventoryIndex = -1;
                return;
            }
        }
        // no more structure selected. destroy preview (if any)
        else if (preview != null) Destroy(preview);
    }
    [Command]
    void CmdTrySet(Player player, int inventoryIndex)
    {
        ItemSlot item = player.inventory.slots[inventoryIndex];

        if (item.amount > 0 && !Utils.IsCursorOverUserInterface())
        {

            if (item.item.data is StructureItem)
            {
                StructureItem sI = item.item.data as StructureItem;
                sI.Use(player, inventoryIndex);
            }
        }
    }



    [Command]
    void CmdSetRotationIndex(Quaternion rot, Vector3 pos)      //FIX add vector3 pass
    {
        rotation = rot;
        position = pos;        //FIX add position
    }

    static float RoundToGrid(float value, int resolution)
    {
        // if we want to round to 0, 0.5, 1, 1.5 etc. then:
        //   multiply by 2, round, divide by 2
        return Mathf.Round(value * resolution) / resolution;
    }

    public Vector3 CalculatePreviewPosition(StructureItem structure, Vector3 pos)
    {
        // snap to grid so we have some kind of order when building
        pos.x = RoundToGrid(pos.x, structure.gridResolution);
        pos.y = RoundToGrid(pos.y, structure.gridResolution);
        pos.z = RoundToGrid(pos.z, structure.gridResolution);
        return pos;
    }

    public Quaternion CalculatePreviewRotation()
    {

        // convert to quaternion
        return rotation;
    }



    public Vector3 GetPosition()  //FIX add getPos
    {

        // convert to quaternion
        return position;
    }


    void OnDrawGizmos()
    {
        // show the renderer.bounds check that is used to check StructureItem's
        // CanBuildAt check
        if (preview != null)
        {
            StructureItem structure = CurrentStructure;
            Bounds bounds = preview.GetComponent<boundingCollider>().col.bounds;

            // regular bounds
            Gizmos.color = Color.gray;
            Gizmos.DrawWireCube(bounds.center, bounds.size);

            // bounds - build tolerance (for collision check)
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(bounds.center, bounds.size * (1 - structure.buildToleranceCollision));

            // bounds + build tolerance (for air check)
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(bounds.center, bounds.size * (1 + structure.buildToleranceAir));
        }
    }
}