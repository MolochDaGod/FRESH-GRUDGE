using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReportButton : MonoBehaviour
{
    public RectTransform rectTransform;
    public GameObject content;

    public TMP_Text headerText;
    public TMP_InputField noteInputField;

    [HideInInspector] public string playerName;
    [HideInInspector] public string uniqueId;
    [HideInInspector] public string date;
#if UMMORPG_3D_C || UMMORPG_3D_R
    [HideInInspector] public Vector3 position;
#else
    [HideInInspector] public Vector2 position;
#endif

    public Toggle m_Toggle;
    [HideInInspector] public ToggleGroup toggleGroup;

    public Toggle editToggle;

    public GameObject background;
    public TMP_Text statusText;

    Image image;
    Vector2 targetPos;

    void Start()
    {
        m_Toggle.group = toggleGroup;
        image = GetComponent<Image>();
    }

    public void GoToPlayer()
    {
        UIManagement.singleton.player.CmdMsgGoTo(playerName);
    }

    public void GoToLocation()
    {
        UIManagement.singleton.player.CmdMsgGoToLocation(position);
    }

    public void EditDescription()
    {
        // edit description
        UIManagement.singleton.player.CmdMsgEditBugReportDescription(uniqueId, noteInputField.text);
    }

    public void ChangeReportCategory()
    {
        BugReport report = new BugReport
        {
            characterName = playerName,
            uniqueId = uniqueId,
            date = date,
            description = noteInputField.text,
            position = position
        };

        if (UIBugReports.singleton.dropDown.value == 0)
        {
            // verify
            report.status = 1;
        }
        else if (UIBugReports.singleton.dropDown.value == 1)
        {
            // fixed
            report.status = 2;
        }
        // move the report to the next category
        UIManagement.singleton.player.CmdMsgEditBugReportStatus(report);
    }

    // depending on the category in which the report is currently located
    public void SetCategoryButton(int status)
    {
        switch (status)
        {
            case 0:
                // new report
                statusText.text = "Verified";
                background.SetActive(true);
                break;
            case 1:
                // verified                
                statusText.text = "Fixed";
                background.SetActive(true);
                break;
            case 2:
                // fixed
                background.SetActive(false);
                break;
        }
    }

    public void ToggleValueChanged()
    {
        // enable text edit mode
        if (editToggle.isOn)
        {
            noteInputField.readOnly = false;
            noteInputField.interactable = true;
        }
        else
        {
            noteInputField.readOnly = true;
            noteInputField.interactable = false;
        }
    }

    public void BugReportDelete()
    {
        UICommandConfirmation.singleton.Show("Delete this bug report?", () => { UIManagement.singleton.player.CmdMsgBugReportDelete(uniqueId); });
    }

    public void ExpandButton()
    {
        // the index is not called in Start() function because the order of buttons may change
        int index = transform.GetSiblingIndex();
        targetPos = Vector2.up * (29 * index);

        if (m_Toggle.isOn)
        {
            content.SetActive(true);

            // scroll to the target position
            UIBugReports.singleton.MovePanel(targetPos);

            SetSize(rectTransform, 171);
        }
        else
        {
            // scroll to the beginning
            UIBugReports.singleton.GetStartPosition();

            SetSize(rectTransform, 26);
        }
    }

    public void SetSize(RectTransform rectTransform, float maximum)
    {
        StartCoroutine(Resize(rectTransform, rectTransform.sizeDelta.y, maximum));
    }

    IEnumerator Resize(RectTransform rectTransform, float minimum, float maximum)
    {
        float step = 0;
        while (step < 1)
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.CeilToInt(Mathf.SmoothStep(minimum, maximum, step += Time.deltaTime)));
            yield return AddonManagement.EndOfFrame;
        }
        yield return AddonManagement.EndOfFrame;
        if (maximum == 26)
        {
            content.SetActive(false);
        }
    }
}

public struct BugReport
{
    public string characterName, uniqueId, date, description;
    public int status;
#if UMMORPG_3D_C || UMMORPG_3D_R
    public Vector3 position;

    public BugReport(string _characterName, string _uniqueId, int _status, string _date, string _description, Vector3 _position)
    {
        characterName = _characterName;
        uniqueId = _uniqueId;
        status = _status;
        date = _date;
        description = _description;
        position = _position;
    }
#else
    public Vector2 position;

    public BugReport(string _characterName, string _uniqueId, int _status, string _date, string _description, Vector2 _position)
    {
        characterName = _characterName;
        uniqueId = _uniqueId;
        status = _status;
        date = _date;
        description = _description;
        position = _position;
    }
#endif
}
