using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using TMPro;

public class Translate : MonoBehaviour
{
    [Header("[-=-[ Configuration ]-=-]")]
    public bool enableTranslator = false;
    public Button button;
    public string uriTranslator = "http://localhost/translate/translate.php"; // this is not included in this assets

    //string target_lang = "en";
    public TMP_Text text;
    private string original;
    private string translated;
    private bool isTranslated = false;
    private string _name;
    private string _text;

    private void Start()
    {
        if (!enableTranslator && button != null) button.gameObject.SetActive(false);
    }
    public void translateTo()
    {
        //Debug.Log("lang :" + Application.systemLanguage);
        string uri = uriTranslator;
        string texte = text.text;

        if (original == null)
        {
            original = text.text;
        }

        if (!isTranslated)
        {

            parseText();
            isTranslated = true;
            if (translated == null)
            {
                StartCoroutine(getRequest(uri, Application.systemLanguage.ToString(), _text));
            }
            else
            {
                //Debug.Log("Translated cache");
                text.text = translated;
            }
        }
        else
        {
            isTranslated = false;
            //Debug.Log("Original");
            text.text = original;
        }
    }
    private void parseText()
    {
        string html = text.text;

        bool playerFound = false;
        foreach (Match match in Regex.Matches(html, "<b>(.*?):</b>(.*?)", RegexOptions.IgnoreCase))
        {
            if(!playerFound){
                _name = match.Groups[1].Value;
                //_text = match.Groups[2].Value;
                //Debug.Log(match.Groups[1].Value + " - " + match.Groups[2].Value);
            }
            
        }
        _text = html.Substring(("<b>" + _name + ":<b>").Length + 1);// html.StartsWith("<b>" + _name + ":<b> ");
    }
    IEnumerator getRequest(string uri, string target_lang, string texte)
    {
 
        WWWForm form = new WWWForm();
        
        form.AddField("target_lang", target_lang);
        form.AddField("text", texte);

        UnityWebRequest www = UnityWebRequest.Post(uri, form);
        //{
            //Debug.Log("WWW lancé");
            yield return www.SendWebRequest();
#if UNITY_2020_1_OR_NEWER
            if (www.result != UnityWebRequest.Result.Success)
#else
            if (www.isNetworkError || www.isHttpError)
#endif
            {
                Debug.Log("Erreur :" + www.error);
            }
            else
            {
                translated = "<b>"+_name+":</b> "+www.downloadHandler.text;
                text.text = "<b>"+_name+":</b> "+www.downloadHandler.text;
                //Debug.Log(www.downloadHandler.text);
            }
        //}
    }
}
