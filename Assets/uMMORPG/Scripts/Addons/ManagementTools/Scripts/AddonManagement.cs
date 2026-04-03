using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using System.Collections;

public static class AddonManagement
{
    static M_Spawner cache;
    public static M_Spawner SpawnerPrefab => cache ?? (cache = Resources.Load<M_Spawner>("Spawner"));

    static List<Monster> cacheMonsters;
    public static List<Monster> Monsters => cacheMonsters ?? (cacheMonsters = ((NetworkManagerMMO)NetworkManager.singleton).RegisteredMonsters());

    static List<Npc> cacheNpcs;
    public static List<Npc> Npcs => cacheNpcs ?? (cacheNpcs = ((NetworkManagerMMO)NetworkManager.singleton).RegisteredNpcs());

    static List<M_Environment> cacheEnvironments;
    public static List<M_Environment> Environments => cacheEnvironments ?? (cacheEnvironments = ((NetworkManagerMMO)NetworkManager.singleton).RegisteredEnvironments());

    public static Dictionary<string, M_Spawner> spawners = new Dictionary<string, M_Spawner>();

    public static WaitForEndOfFrame EndOfFrame { get; } = new WaitForEndOfFrame();
    public static WaitForSeconds OneSecond { get; } = new WaitForSeconds(1);

    public static void ShowCanvasContent(Canvas panel)
    {
        // get last object the parent Transform has
        panel.transform.GetChild(0).GetChild(panel.transform.GetChild(0).childCount - 1).gameObject.SetActive(panel.enabled);
    }
}

public partial class Player
{
    [HideInInspector] public bool canBeSaved = true;

    internal bool resurrectRequested;
    bool EventResurrect()
    {
        bool result = resurrectRequested;
        resurrectRequested = false; // reset
        return result;
    }

    #region Access Level
    [HideInInspector, SyncVar] public int _access = 1;
    public int Access
    {
        get { return _access; }
        set { _access = Mathf.Clamp(value, 0, 3); }
    }

    public bool CheckAccess() => Access == 2 || Access == 3;
    public bool OwnerAccess() => Access == 3;
    #endregion

    #region Dungeon Instance (3D only)
#if UMMORPG_3D_C || UMMORPG_3D_R
    [HideInInspector, SyncVar] public bool dungeon;

    public bool DungeonControl
    {
        get { return dungeon; }
        set { dungeon = value; }
    }

    public bool InDungeon() => DungeonControl;
#endif
    #endregion    

    #region Mute
    [HideInInspector, SyncVar] public bool muted;

    public float StartMute()
    {
        if (MToolsSettings.Settings.muteBuff != null)
        {
#if UMMORPG_3D_C || UMMORPG_2D_C
            AddOrRefreshBuff(new Buff(MToolsSettings.Settings.muteBuff, 1));
            return buffs[GetBuffIndexByName(MToolsSettings.Settings.muteBuff.name)].BuffTimeRemaining();
#endif
#if UMMORPG_3D_R || UMMORPG_2D_R
            skills.AddOrRefreshBuff(new Buff(MToolsSettings.Settings.muteBuff, 1));
            return skills.buffs[skills.GetBuffIndexByName(MToolsSettings.Settings.muteBuff.name)].BuffTimeRemaining();
#endif
        }
        return 0;
    }

    public bool IsMuted()
    {
        return MToolsSettings.Settings.muteBuff != null
#if UMMORPG_3D_C || UMMORPG_2D_C
            && buffs.Any(buff => buff.name == MToolsSettings.Settings.muteBuff.name)
#endif
#if UMMORPG_3D_R || UMMORPG_2D_R
            && skills.buffs.Any(buff => buff.name == MToolsSettings.Settings.muteBuff.name)
#endif
            ;
    }

    public IEnumerator MuteCountdown(float counter)
    {
        while (counter > 0)
        {
            yield return AddonManagement.OneSecond;
            counter--;
        }
        //if (UICompleteChat.singleton)
           // UICompleteChat.singleton.ChatUnmute();
    }

    void OnStartLocalPlayer_Mute()
    {
        if (IsMuted())
        {
            //UICompleteChat.singleton.ChatMute();
#if UMMORPG_3D_C || UMMORPG_2D_C
            float counter = buffs[GetBuffIndexByName(MToolsSettings.Settings.muteBuff.name)].BuffTimeRemaining();
            StartCoroutine(nameof(MuteCountdown), counter);
#endif
#if UMMORPG_3D_R || UMMORPG_2D_R
            float counter = skills.buffs[skills.GetBuffIndexByName(MToolsSettings.Settings.muteBuff.name)].BuffTimeRemaining();
            StartCoroutine(nameof(MuteCountdown), counter);
#endif
        }
    }
    #endregion

    #region Creator
    public void UpdateClient_SetObjectViaPosition()
    {
        if (isLocalPlayer)
        {
            UIManagement uiManagement = UIManagement.singleton;
            UICreator uiCreator = UICreator.singleton;

            if (uiManagement != null && uiManagement.canvas != null && uiManagement.canvas.enabled)
            {
                if (!Utils.IsCursorOverUserInterface())
                {
                    if (uiCreator != null && uiCreator.selectedObject != null)
                    {
#if UMMORPG_3D_C || UMMORPG_3D_R
                        if (MToolsSettings.Settings != null && Input.GetKeyDown(MToolsSettings.Settings.rotateLeftKey))
                        {
                            uiCreator.RotateObj(-10);
                        }

                        if (MToolsSettings.Settings != null && Input.GetKeyDown(MToolsSettings.Settings.rotateRightKey))
                        {
                            uiCreator.RotateObj(10);
                        }
#endif
                        Vector3 position = uiCreator.MouseToTerrainPosition();
                        uiCreator.selectedObject.transform.position = position;

                        if (Input.GetMouseButtonDown(2) && Input.touchCount <= 1)
                        {
                            uiCreator.SetObjectViaPosition();
                        }
                    }
                }
            }

            if (Input.GetMouseButtonDown(1) && !Utils.IsCursorOverUserInterface() && Input.touchCount <= 1)
            {
                RayHit();
            }
        }
    }

    [Command]
    public void CmdSendNavMeshMessage(string message)
    {
        chat.TargetMsgInfo(message);
    }
    #endregion

    #region Manipulation Tools
    public void RayHit()
    {
        if (!CheckAccess())
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

#if UMMORPG_3D_C
        bool cast = localPlayerClickThrough ? Utils.RaycastWithout(ray, out var hit, gameObject) : Physics.Raycast(ray, out hit);
        if (cast)
        {
            if (hit.transform.CompareTag("Monster"))
            {
                if (hit.transform.TryGetComponent(out Monster monster))
                {
                    if (monster.uniqueId != "")
                    {
                        UIManipulationTools.singleton.Show(monster.gameObject);
                    }
                    else
                    {
                        UIManipulationTools.singleton.Show(monster.gameObject, false);
                    }
                }
            }
            else if (hit.transform.CompareTag("Npc"))
            {
                if (hit.transform.TryGetComponent(out Npc npc))
                {
                    if (npc.uniqueId != "")
                    {
                        UIManipulationTools.singleton.Show(npc.gameObject);
                    }
                }
            }
            else if (hit.transform.CompareTag("Environment"))
            {
                if (hit.transform.TryGetComponent(out M_Environment environment))
                {
                    if (environment.uniqueId != "")
                    {
                        UIManipulationTools.singleton.Show(environment.gameObject);
                    }
                }
            }
        }
#endif
#if UMMORPG_3D_R
        bool cast = localPlayerClickThrough ? Utils.RaycastWithout(ray.origin, ray.direction, out var hit, Mathf.Infinity, gameObject) : Physics.Raycast(ray, out hit);
        if (cast)
        {
            if (hit.transform.CompareTag("Monster"))
            {
                if (hit.transform.TryGetComponent(out Monster monster))
                {
                    if (monster.uniqueId != "")
                    {
                        UIManipulationTools.singleton.Show(monster.gameObject);
                    }
                    else
                    {
                        UIManipulationTools.singleton.Show(monster.gameObject, false);
                    }
                }
            }
            else if (hit.transform.CompareTag("Npc"))
            {
                if (hit.transform.TryGetComponent(out Npc npc))
                {
                    if (npc.uniqueId != "")
                    {
                        UIManipulationTools.singleton.Show(npc.gameObject);
                    }
                }
            }
            else if (hit.transform.CompareTag("Environment"))
            {
                if (hit.transform.TryGetComponent(out M_Environment environment))
                {
                    if (environment.uniqueId != "")
                    {
                        UIManipulationTools.singleton.Show(environment.gameObject);
                    }
                }
            }
        }
#endif
#if UMMORPG_2D_C || UMMORPG_2D_R
        RaycastHit2D hit = localPlayerClickThrough ? Utils.Raycast2DWithout(ray, gameObject) : Physics2D.GetRayIntersection(ray);
        if (hit.collider != null)
        {
            if (hit.transform.CompareTag("Monster"))
            {
                if (hit.transform.TryGetComponent(out Monster monster))
                {
                    if (monster.uniqueId != "")
                    {
                        UIManipulationTools.singleton.Show(monster.gameObject);
                    }
                    else
                    {
                        UIManipulationTools.singleton.Show(monster.gameObject, false);
                    }
                }
            }
            else if (hit.transform.CompareTag("Npc"))
            {
                if (hit.transform.TryGetComponent(out Npc npc))
                {
                    if (npc.uniqueId != "")
                    {
                        UIManipulationTools.singleton.Show(npc.gameObject);
                    }
                }
            }
            else if (hit.transform.CompareTag("Environment"))
            {
                if (hit.transform.TryGetComponent(out M_Environment environment))
                {
                    if (environment.uniqueId != "")
                    {
                        UIManipulationTools.singleton.Show(environment.gameObject);
                    }
                }
            }
        }
#endif
    }
    #endregion
}

public partial class Monster
{
    [HideInInspector, SyncVar] public string uniqueId;
}

public partial class Npc
{
    [HideInInspector, SyncVar] public string uniqueId;
}

public partial class Entity
{
    [HideInInspector, SyncVar] public bool invisible = false;
    [HideInInspector, SyncVar] public bool frozen = false;

    public bool IsFrozen() => frozen;

    public bool IsImmortal()
    {
#if !UMMORPG_3D_C && !UMMORPG_2D_C && !UMMORPG_3D_R && !UMMORPG_2D_R
        return true;
#endif
#if UMMORPG_3D_C || UMMORPG_2D_C
        return invincible;
#endif
#if UMMORPG_3D_R || UMMORPG_2D_R
        return combat.invincible;
#endif
    }

    [Server]
    public void Resurrect(float healthPercentage = 1, float manaPercentage = 1)
    {
#if UMMORPG_3D_C || UMMORPG_2D_C
        health = Mathf.RoundToInt(healthMax * healthPercentage);
        mana = Mathf.RoundToInt(manaMax * manaPercentage);
#endif
#if UMMORPG_3D_R || UMMORPG_2D_R
        health.current = Mathf.RoundToInt(health.max * healthPercentage);
        mana.current = Mathf.RoundToInt(mana.max * manaPercentage);
#endif
    }

    /*
    void Awake_Management()
    {
        // name fix
        transform.name = transform.name.Replace("(Clone)", "").Trim();
    }
    */
}

public partial class UIChat
{
    public void ChatMute()
    {
        Player player = Player.localPlayer;
        if (player != null)
        {
            sendButton.interactable = false;
            messageInput.interactable = false;
            player.CmdMsgMute();
        }
    }

    public void ChatUnmute()
    {
        Player player = Player.localPlayer;
        if (player != null)
        {
            sendButton.interactable = true;
            messageInput.interactable = true;
            player.CmdMsgUnmute();
        }
    }
}
