//using Mirror;
using System;
using UnityEngine;

// PLAYER
public class PlayerMeshSwitcher : MonoBehaviour// NetworkBehaviour
{
#if _iMMOMESHSWITCHER
    public Player player;
    public PlayerEquipment playerEquipment;

    protected Renderer[] cachedRenderers;


    // -----------------------------------------------------------------------------------
    // OnStartClient_Tools_MeshSwitcher -> to Start
    // -----------------------------------------------------------------------------------
    private void Start()
    {
        if (player == null)
            player = GetComponent<Player>();

        if (playerEquipment == null && player != null)
            playerEquipment = player.equipment as PlayerEquipment;

        if (player == null || player.equipment == null || playerEquipment == null)
        {
            enabled = false;
            return;
        }

        EquipmentInfo[] infoList = playerEquipment.slotInfo;
        for (int index = 0; index < infoList.Length; ++index)
        {
            EquipmentInfo info = infoList[index];
            for (int i = 0; i < info.mesh.Length; ++i)
            {
                if (info.mesh[i].mesh != null)
                {
                    Renderer renderer = info.mesh[i].mesh.GetComponent<Renderer>();
                    if (renderer != null)
                        info.mesh[i].defaultMaterial = renderer.sharedMaterial;
                }
            }
            RefreshMesh(index);
        }
        playerEquipment.onRefreshLocation.AddListener(OnRefreshLocation_MeshSwitcher);
    }

    // -----------------------------------------------------------------------------------
    // OnRefreshLocation_MeshSwitcher
    // -----------------------------------------------------------------------------------
    public void OnRefreshLocation_MeshSwitcher(int index)
    {
        RefreshMesh(index);
    }

    // -----------------------------------------------------------------------------------
    // RefreshMesh
    // -----------------------------------------------------------------------------------
    private void RefreshMesh(int index)
    {
        if (player == null || player.equipment == null || playerEquipment == null)
            return;

        if (index < 0 || index >= playerEquipment.slotInfo.Length)
            return;

        EquipmentInfo info = playerEquipment.slotInfo[index];
        ItemSlot slot = index < player.equipment.slots.Count ? player.equipment.slots[index] : default;
        cachedRenderers = new Renderer[info.mesh.Length];

        for (int i = 0; i < info.mesh.Length; ++i)
        {
            if (info.mesh[i].mesh != null)
                cachedRenderers[i] = info.mesh[i].mesh.GetComponent<Renderer>();
        }

        if (info.requiredCategory != "" && info.mesh.Length > 0)
        {
            //Debug.Log("JE passe ici");
            if (slot.amount > 0)
            {
                EquipmentItem itemData = (EquipmentItem)slot.item.data;
                SwitchToMesh(info, itemData.meshIndex, itemData.meshMaterial, itemData.switchableColors);
            }
            else
            {
                SwitchToMesh(info, 0, null, null);
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // SwitchToMesh
    // -----------------------------------------------------------------------------------
    private void SwitchToMesh(EquipmentInfo info, int[] meshIndex, Material mat, SwitchableColor[] colors)
    {
        for (int i = 0; i < info.mesh.Length; ++i)
        {
            if (info.mesh[i].mesh != null)
            {
                if (Array.Exists(meshIndex, e => e == i))
                {
                    info.mesh[i].mesh.SetActive(true);

                    if (cachedRenderers[i] == null)
                        continue;

                    if (mat != null)
                    {
                        cachedRenderers[i].material = mat;
                    }
                    else if (info.mesh[i].defaultMaterial != null)
                    {
                        cachedRenderers[i].material = info.mesh[i].defaultMaterial;
                    }

                    SwitchToColor(cachedRenderers[i].material, colors);
                }
                else
                {
                    info.mesh[i].mesh.SetActive(false);
                }
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // SwitchToMesh
    // -----------------------------------------------------------------------------------
    private void SwitchToMesh(EquipmentInfo info, int meshIndex, Material mat, SwitchableColor[] colors)
    {
        for (int i = 0; i < info.mesh.Length; ++i)
        {
            if (info.mesh[i].mesh != null)
            {
                if (meshIndex == i)
                {
                    info.mesh[i].mesh.SetActive(true);

                    if (cachedRenderers[i] == null)
                        continue;

                    if (mat != null)
                    {
                        cachedRenderers[i].material = mat;
                    }
                    else if (info.mesh[i].defaultMaterial != null)
                    {
                        cachedRenderers[i].material = info.mesh[i].defaultMaterial;
                    }

                    SwitchToColor(cachedRenderers[i].material, colors);
                }
                else
                {
                    info.mesh[i].mesh.SetActive(false);
                }
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // SwitchToColor
    // -----------------------------------------------------------------------------------
    private void SwitchToColor(Material material, SwitchableColor[] colors)
    {

        if (material == null || colors == null || colors.Length == 0) return;
        foreach (SwitchableColor color in colors)
        {
            material.SetColor(color.propertyName.ToString(), color.color);
           ///   Debug.LogError("switch color...? " + color.ToString());
        }
    }
#else
    [ReadOnly] public string enableMEshSwitcher = "Mesh Switcher is not enabled";
#endif
    // -----------------------------------------------------------------------------------
}
