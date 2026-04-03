using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class UICompleteChatEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public TMP_Text text;

    // keep all the message info in case it's needed to reply etc.
    [HideInInspector] public ChatMessage message;
    //public FontStyle mouseOverStyle = FontStyle.Italic;
    public FontStyles mouseOverStyle = FontStyles.Italic;//.Italic;
    //FontStyle defaultStyle;
    FontStyles defaultStyle;

    public UIShowToolTip toolTip;
    
    void Awake()
    {
        toolTip = GetComponent<UIShowToolTip>();
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        // can we reply to this message?
        if (!string.IsNullOrWhiteSpace(message.replyPrefix))
        {
            defaultStyle = text.fontStyle;
            text.fontStyle = mouseOverStyle;
        }

        string stringToCheck = message.Construct();
        int start = stringToCheck.IndexOf("[");
        int stop = stringToCheck.IndexOf("]");

        if(stop > -1)
        {
            string output = stringToCheck.Substring(start + 1, stop - start - 1);
            int key = output.GetStableHashCode();

            if(ScriptableItem.All.TryGetValue(key, out var data))
            {
                Item item = new Item(data);
                toolTip.text = new ItemSlot(item).ToolTip();
            }
        }
    }

    //Detect when Cursor leaves the GameObject
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        text.fontStyle = defaultStyle;
    }

    public void OnPointerClick(PointerEventData data)
    {
        // find the chat component in the parents
        GetComponentInParent<UICompleteChat>().OnEntryClicked(this);
    }
}
