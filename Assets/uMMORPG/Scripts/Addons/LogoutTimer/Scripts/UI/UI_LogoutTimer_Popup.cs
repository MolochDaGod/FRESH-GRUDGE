using UnityEngine;

public class UI_LogoutTimer_Popup : MonoBehaviour
{
    public GameObject panel;

    public static UI_LogoutTimer_Popup singleton;

    // -----------------------------------------------------------------------------------
    // Show
    // -----------------------------------------------------------------------------------
    private void Start()
    {
        if (singleton == null) singleton = this;
    }

    // -----------------------------------------------------------------------------------
    // Show
    // -----------------------------------------------------------------------------------
    public void Show()
    {
        if (panel.activeSelf) return;
        panel.SetActive(true);
    }

    // -----------------------------------------------------------------------------------
    // Hide
    // -----------------------------------------------------------------------------------
    public void Hide()
    {
        if (!panel.activeSelf) return;
        panel.SetActive(false);
    }

    // -----------------------------------------------------------------------------------
}