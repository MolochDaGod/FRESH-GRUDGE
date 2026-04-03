using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UIHelpRequests : MonoBehaviour
{
    public static UIHelpRequests singleton;
    public Canvas canvas;

    public Transform content;
    public HelpButton helpSlot;

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
            UIManagement.singleton.player.CmdShowHelpRequests();
        }
        // find and close the previously opened panel
        UIManagement.singleton.ClosePanelsExceptOne(canvas);
        AddonManagement.ShowCanvasContent(canvas);
    }

    public void AddHelpRequest(HelpRequest request)
    {
        HelpButton slot = Instantiate(helpSlot, content);
        ToggleGroup group = content.GetComponent<ToggleGroup>();
        string title()
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine($"<size=80%>by <size=100%><color=#5E99CC>{request.characterName}</color><indent=72%>{request.date}");
            return text.ToString();
        }
        slot.headerText.text = title();
        slot.playerName = request.characterName;
        slot.descriptionInputField.text = request.description;
        slot.uniqueId = request.uniqueId;
        slot.position = request.position;
        slot.toggleGroup = group;
    }

    public void DestroyHelpRequestButton(string uniqueId)
    {
        for (int i = 0; i < content.childCount; ++i)
        {
            HelpButton slot = content.GetChild(i).GetComponent<HelpButton>();
            if (slot.uniqueId == uniqueId)
            {
                Destroy(slot.gameObject);

                // scroll to the beginning
                GetStartPosition();
            }
        }
    }

    public void DestroyHelpRequestButtons()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            GameObject slot = content.GetChild(i).gameObject;
            Destroy(slot);
        }
    }

    public void NoHelpRequests()
    {
        UIUtils.BalancePrefabs(helpSlot.gameObject, 0, content);
    }

    public void LoadHelpRequests(HelpRequest[] requests)
    {
        UIUtils.BalancePrefabs(helpSlot.gameObject, requests.Length, content);

        ToggleGroup group = content.GetComponent<ToggleGroup>();
        if (group.AnyTogglesOn())
        {
            group.SetAllTogglesOff();
        }

        for (int i = 0; i < requests.Length; ++i)
        {
            HelpButton slot = content.GetChild(i).GetComponent<HelpButton>();
            HelpRequest request = requests[i];
            string title()
            {
                StringBuilder text = new StringBuilder();
                text.AppendLine($"<size=80%>by <size=100%><color=#5E99CC>{request.characterName}</color><indent=72%>{request.date}");
                return text.ToString();
            }
            slot.headerText.text = title();
            slot.playerName = request.characterName;
            slot.descriptionInputField.text = request.description;
            slot.uniqueId = request.uniqueId;
            slot.position = request.position;
            slot.toggleGroup = group;
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

    public void MovePanel(Vector2 targetPos)
    {
        StartCoroutine(Move(rectTransform.anchoredPosition, targetPos));
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
