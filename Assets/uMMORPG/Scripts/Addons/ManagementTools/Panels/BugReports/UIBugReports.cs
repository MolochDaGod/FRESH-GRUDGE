using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBugReports : MonoBehaviour
{
    public static UIBugReports singleton;
    public Canvas canvas;

    public TMP_Dropdown dropDown;
    public Transform content;
    public ReportButton reportSlot;

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

    // triggered by clicking the bugs button on the toolbar
    public void PanelActivation()
    {
        canvas.enabled = !canvas.enabled;

        if (canvas.enabled)
        {
            // show a list of buttons created on the basis of data loaded from the database
            UIManagement.singleton.player.CmdShowBugReports(dropDown.value);
        }
        // find and close the previously opened panel
        UIManagement.singleton.ClosePanelsExceptOne(canvas);
        AddonManagement.ShowCanvasContent(canvas);
    }

    // select categories by clicking one of the options in the dropdown list
    public void DropdownValueChanged()
    {
        switch (dropDown.value)
        {
            case 0:
                // new reports
                UIManagement.singleton.player.CmdShowBugReports(0);
                break;
            case 1:
                // verified
                UIManagement.singleton.player.CmdShowBugReports(1);
                break;
            case 2:
                // fixed
                UIManagement.singleton.player.CmdShowBugReports(2);
                break;
        }
    }

    public void AddBugReport(BugReport report)
    {
        ReportButton slot = Instantiate(reportSlot, content);
        ToggleGroup group = content.GetComponent<ToggleGroup>();
        string title()
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine($"<size=80%>by <size=100%><color=#5E99CC>{report.characterName}</color><indent=72%>{report.date}");
            return text.ToString();
        }
        slot.headerText.text = title();
        slot.playerName = report.characterName;
        slot.noteInputField.text = report.description;
        slot.uniqueId = report.uniqueId;
        slot.position = report.position;
        slot.date = report.date;
        slot.toggleGroup = group;
        slot.SetCategoryButton(dropDown.value);
    }

    public void DestroyBugReportButton(string uniqueId)
    {
        for (int i = 0; i < content.childCount; ++i)
        {
            ReportButton slot = content.GetChild(i).GetComponent<ReportButton>();
            if (slot.uniqueId == uniqueId)
            {
                Destroy(slot.gameObject);

                // scroll to the beginning
                GetStartPosition();
            }
        }
    }

    public void DestroyBugReportButtons()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            GameObject slot = content.GetChild(i).gameObject;
            Destroy(slot);
        }
    }

    public void EditBugReport(string uniqueId, string description)
    {
        for (int i = 0; i < content.childCount; ++i)
        {
            ReportButton slot = content.GetChild(i).GetComponent<ReportButton>();
            if (slot.uniqueId == uniqueId)
            {
                slot.noteInputField.text = description;

                // turn off text edit mode
                if (slot.editToggle.isOn)
                {
                    slot.editToggle.isOn = false;
                }
            }
        }
    }

    public void NoBugReports()
    {
        UIUtils.BalancePrefabs(reportSlot.gameObject, 0, content);
    }

    public void LoadBugReports(BugReport[] bugReports)
    {
        UIUtils.BalancePrefabs(reportSlot.gameObject, bugReports.Length, content);

        ToggleGroup group = content.GetComponent<ToggleGroup>();
        if (group.AnyTogglesOn())
        {
            group.SetAllTogglesOff();
        }

        for (int i = 0; i < bugReports.Length; ++i)
        {
            ReportButton slot = content.GetChild(i).GetComponent<ReportButton>();
            BugReport report = bugReports[i];
            
            string title()
            {
                StringBuilder text = new StringBuilder();
                text.AppendLine($"<size=80%>by <size=100%><color=#5E99CC>{report.characterName}</color><indent=72%>{report.date}");
                return text.ToString();
            }
            slot.headerText.text = title();
            slot.playerName = report.characterName;
            slot.noteInputField.text = report.description;
            slot.uniqueId = report.uniqueId;
            slot.position = report.position;
            slot.toggleGroup = group;
            slot.SetCategoryButton(dropDown.value);
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
