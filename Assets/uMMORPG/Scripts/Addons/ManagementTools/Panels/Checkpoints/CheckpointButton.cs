#if _SQLITE
using TMPro;
using UnityEngine;

public class CheckpointButton : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text positionText;

    [HideInInspector] public string uniqueId;
#if UMMORPG_3D_C || UMMORPG_3D_R
    [HideInInspector] public Vector3 position;
#else
    [HideInInspector] public Vector2 position;
#endif

    public void GoToLocation()
    {
        UIManagement.singleton.player.CmdMsgGoToLocation(position);
    }

    public void CheckpointDelete()
    {
        UICommandConfirmation.singleton.Show($"Delete <color=#00FA9A>{nameText.text}</color> checkpoint?", () => { UIManagement.singleton.player.CmdMsgCheckpointDelete(uniqueId); });
    }
}

public struct Checkpoint
{
    public string checkpointName, uniqueId;
#if UMMORPG_3D_C || UMMORPG_3D_R
    public Vector3 position;

    public Checkpoint(string _checkpointName, string _uniqueId, Vector3 _position)
    {
        checkpointName = _checkpointName;
        uniqueId = _uniqueId;
        position = _position;
    }
#else
    public Vector2 position;

    public Checkpoint(string _checkpointName, string _uniqueId, Vector2 _position)
    {
        checkpointName = _checkpointName;
        uniqueId = _uniqueId;
        position = _position;
    }
#endif
}

#endif
