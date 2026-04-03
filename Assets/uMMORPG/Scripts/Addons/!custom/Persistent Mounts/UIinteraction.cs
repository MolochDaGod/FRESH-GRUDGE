using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIinteraction : MonoBehaviour
{
    public GameObject panel;
    public static UIinteraction singleton;
    public UIinteraction()
    {
        // assign singleton only once (to work with DontDestroyOnLoad when
        // using Zones / switching scenes)
        if (singleton == null) singleton = this;
    }


    public void Show()
    {
        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}
