using UnityEngine;
using UnityEngine.UI;

public class UIMailMessageSlot : MonoBehaviour
{
    public Text textReceived;
    public Text textFrom;
    public Text textSubject;
    public Button readButton;
    public Button itemSlot;
    public Toggle toggle;
    [HideInInspector] public int mailIndex;
}