using UnityEngine;
using UnityEngine.UI;

public class MacroControl : MonoBehaviour
{
    public GameObject slot;
    public Button button;
    public InputField macro;
    public Button editMacro;
    public Image imglock;
    public Image ingUnlock;

    private void Start()
    {
        button.onClick.AddListener(SendToChat);
        editMacro.onClick.AddListener(EditMacro);
        LoadMacro();
    }

    private void SendToChat()
    {
        GetComponentInParent<UICompleteChat>().btnSendMacro(macro.text);
    }
    private void EditMacro()
    {
        //saveMacro only if macro is already interactable
        if (macro.interactable)
            SaveMacro();
        macro.interactable = !macro.interactable;
        imglock.gameObject.SetActive(!macro.interactable);
        ingUnlock.gameObject.SetActive(macro.interactable);
        button.gameObject.SetActive(!button.gameObject.activeSelf);
    }

    private void LoadMacro()
    {
        if (PlayerPrefs.HasKey(slot.name))
        {
            macro.text = PlayerPrefs.GetString(slot.name);
        }
    }

    private void SaveMacro()
    {
        PlayerPrefs.SetString(slot.name, macro.text);
        PlayerPrefs.Save();
    }
}