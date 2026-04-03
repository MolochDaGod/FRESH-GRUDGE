using UnityEngine;
using UnityEngine.UI;
using Mirror;
public class URLbutton : MonoBehaviour
{
    public string Url = "https://www.jznation.com/grudge/";
    public Button Button;

    private void Start()
    {
        // Subscribe to the button's onClick event once
        Button.onClick.AddListener(OnBuyButtonClick);
    }

    private void OnBuyButtonClick()
    {
        // This function will be called when the button is clicked
        Application.OpenURL(Url);
    }
}
