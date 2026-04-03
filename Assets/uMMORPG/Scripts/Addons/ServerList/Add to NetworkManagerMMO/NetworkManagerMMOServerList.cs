using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManagerMMOServerList : MonoBehaviour
{
    public NetworkManagerMMO networkManagerMMO;
    public string uriListServer = "http://localhost/remoteComand/serverList.php";
    public bool removeDefaultServer = false;
    private string listServerJson = "";
    private ServerAPI serverList;

    // Start is called before the first frame update
    void Start()
    {
        if (uriListServer != "")
            StartCoroutine(GetRequest(uriListServer, Application.systemLanguage.ToString(), "lol"));
    }

    public void CreateFromJSON(string jsonString)
    {

        serverList = JsonUtility.FromJson<ServerAPI>("{\"server\":" + jsonString + "}");
        //Debug.Log(">> "+ serverList.server.Count);
    }
    IEnumerator GetRequest(string uri, string target_lang, string texte)
    {

        WWWForm form = new WWWForm();

        form.AddField("xApp", "game");
        form.AddField("xkey", "AbCdEfGhIjKlMnOprStUvWxYz");

        UnityWebRequest www = UnityWebRequest.Post(uri, form);
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
            listServerJson = www.downloadHandler.text;
            CreateFromJSON(listServerJson);

            if (removeDefaultServer)
                networkManagerMMO.serverList.Clear();
            foreach (var item in serverList.server)
            {
                //Debug.Log("name add :" + item.name + ", ip:" + item.ip);
                networkManagerMMO.serverList.Add(new NetworkManagerMMO.ServerInfo { name = item.name, ip = item.ip });
            }
            networkManagerMMO.listServerLoaded = true;
            //Debug.Log(www.downloadHandler.text);
        }
    }

}
public partial class NetworkManagerMMO
{
    [HideInInspector] public bool listServerLoaded = false;
}

public partial class UILogin
{
    private bool isReloaded = false;

    private void LateUpdate()
    {
        if (manager.listServerLoaded && !isReloaded)
        {
            UpdageSelectDropdown();
            //Debug.Log("reload fini");
        }
    }

    public void UpdageSelectDropdown()
    {
        // load last server by name in case order changes some day.
        if (PlayerPrefs.HasKey("LastServer"))
        {
            string last = PlayerPrefs.GetString("LastServer", "");
            serverDropdown.value = manager.serverList.FindIndex(s => s.name == last);
        }
        isReloaded = true;
    }
}
[Serializable]
public class ServerListAPI
{
    public string name;
    public string ip;
}
[Serializable]
public class ServerAPI
{
    public List<ServerListAPI> server;
}