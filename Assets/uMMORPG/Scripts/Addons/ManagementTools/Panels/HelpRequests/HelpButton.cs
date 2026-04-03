using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HelpButton : MonoBehaviour
{
    public RectTransform rectTransform;
    public GameObject content;

    public TMP_Text headerText;
    public TMP_InputField descriptionInputField;

    [HideInInspector] public string playerName;
    [HideInInspector] public string uniqueId;
#if UMMORPG_3D_C || UMMORPG_3D_R
    [HideInInspector] public Vector3 position;
#else
    [HideInInspector] public Vector2 position;
#endif

    public Toggle m_Toggle;
    [HideInInspector] public ToggleGroup toggleGroup;

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

    public void HelpRequestDelete()
    {
        UICommandConfirmation.singleton.Show("Delete this help request?", () => { UIManagement.singleton.player.CmdMsgHelpRequestDelete(uniqueId); });
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
            UIHelpRequests.singleton.MovePanel(targetPos);

            Color tempColor = m_Toggle.image.color;
            tempColor.a = 0f;
            m_Toggle.image.color = tempColor;

            SetSize(rectTransform, 171);
        }
        else
        {
            // scroll to the beginning
            UIHelpRequests.singleton.GetStartPosition();

            Color tempColor = m_Toggle.image.color;
            tempColor.a = 1f;
            m_Toggle.image.color = tempColor;

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

public struct HelpRequest
{
    public string characterName, uniqueId, date, description;
#if UMMORPG_3D_C || UMMORPG_3D_R
    public Vector3 position;

    public HelpRequest(string _characterName, string _uniqueId, string _date, string _description, Vector3 _position)
    {
        characterName = _characterName;
        uniqueId = _uniqueId;
        date = _date;
        description = _description;
        position = _position;
    }
#else
    public Vector2 position;

    public HelpRequest(string _characterName, string _uniqueId, string _date, string _description, Vector2 _position)
    {
        characterName = _characterName;
        uniqueId = _uniqueId;
        date = _date;
        description = _description;
        position = _position;
    }
#endif
}
