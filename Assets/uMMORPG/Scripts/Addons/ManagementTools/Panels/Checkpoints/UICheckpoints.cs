#if _SQLITE
using System;
using System.Collections;
using UnityEngine;

public class UICheckpoints : MonoBehaviour
{
    public static UICheckpoints singleton;
    public Canvas canvas;

    public Transform content;
    public CheckpointButton checkpointButton;

    RectTransform rectTransform;

    void Awake()
    {
        // initialize singleton
        if (singleton == null) singleton = this;
    }

    void Start()
    {
        rectTransform = content.GetComponent<RectTransform>();
    }

    // triggered by clicking the teleport button on the toolbar
    public void PanelActivation()
    {
        canvas.enabled = !canvas.enabled;

        if (canvas.enabled)
        {
            // show a list of buttons created on the basis of data loaded from the database
            UIManagement.singleton.player.CmdShowCheckpoints();
        }
        // find and close the previously opened panel
        UIManagement.singleton.ClosePanelsExceptOne(canvas);
        AddonManagement.ShowCanvasContent(canvas);
    }

    public void AddCheckpoint()
    {
        UICheckpointConfirmation.singleton.Show(() =>
        {
            Checkpoint checkpoint = new Checkpoint
            {
                checkpointName = UICheckpointConfirmation.singleton.nameInputField.text,
                uniqueId = Guid.NewGuid().ToString(),
                position = UIManagement.singleton.player.transform.position
            };
            UIManagement.singleton.player.CmdMsgAddCheckpoint(checkpoint);
            
            UICheckpointConfirmation.singleton.Hide();
        });
    }

    public void AddCheckpointButton(Checkpoint checkpoint)
    {
        CheckpointButton slot = Instantiate(checkpointButton, content);
        slot.nameText.text = checkpoint.checkpointName;
        slot.uniqueId = checkpoint.uniqueId;
        slot.position = checkpoint.position;
        slot.positionText.text = checkpoint.position.ToString();
    }

    public void DestroyCheckpointButton(string uniqueId)
    {
        for (int i = 0; i < content.childCount; ++i)
        {
            CheckpointButton slot = content.GetChild(i).GetComponent<CheckpointButton>();
            if (slot.uniqueId == uniqueId)
            {
                Destroy(slot.gameObject);

                // scroll to the beginning
                GetStartPosition();
            }
        }
    }

    public void DestroyCheckpointButtons()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            GameObject slot = content.GetChild(i).gameObject;
            Destroy(slot);
        }
    }

    public void NoCheckpoints()
    {
        UIUtils.BalancePrefabs(checkpointButton.gameObject, 0, content);
    }

    public void LoadCheckpoints(Checkpoint[] checkpoints)
    {
        UIUtils.BalancePrefabs(checkpointButton.gameObject, checkpoints.Length, content);

        for (int i = 0; i < checkpoints.Length; ++i)
        {
            CheckpointButton button = content.GetChild(i).GetComponent<CheckpointButton>();
            Checkpoint checkpoint = checkpoints[i];

            button.nameText.text = checkpoint.checkpointName;
            button.uniqueId = checkpoint.uniqueId;
            button.position = checkpoint.position;
            button.positionText.text = checkpoint.position.ToString();
        }
        // scroll to the beginning
        GetStartPosition();
    }

    public void GetStartPosition()
    {
        if (rectTransform.anchoredPosition != Vector2.zero)
        {
            StartCoroutine(Move(rectTransform.anchoredPosition, Vector2.zero));
        }
    }

    IEnumerator Move(Vector2 startPos, Vector2 targetPos)
    {
        float step = 0;
        while (step < 1)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, step += Time.deltaTime);
            yield return AddonManagement.EndOfFrame;
        }
    }
}

#endif
