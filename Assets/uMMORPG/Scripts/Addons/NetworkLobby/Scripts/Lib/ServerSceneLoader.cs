using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ServerSceneLoader : Singleton<ServerSceneLoader>
//public class ServerSceneLoader : MonoBehaviour
{
    //public static ServerSceneLoader Instance;
    // Loading Progress: private setter, public getter
    private float _loadingProgress;
    private string _loadingSceneName;
    private string _loadingTips;
    private Sprite _loadingSceneSprite;
    private bool _activeUI = false;
    public float LoadingProgress { get { return _loadingProgress; } }
    public string LoadingSceneName { get { return _loadingSceneName; } }
    public string LoadingTips { get { return _loadingTips; } }
    public Sprite LoadingSceneSprite { get { return _loadingSceneSprite; }}
    public bool LoadingUIActive { get { return _activeUI; } }
    public bool LoadedScene { get { return sceneActive(); } }

    /*public ServerSceneLoader()
    {
        // assign singleton only once (to work with DontDestroyOnLoad when
        // using Zones / switching scenes)
        if (Instance == null) Instance = this;
    }*/

    private bool sceneActive()
    {
        bool loaded = false;
        for (int n = 0; n < SceneManager.sceneCount; ++n)
        {
            Scene scene = SceneManager.GetSceneAt(n);
            //output += scene.name;
            //output += scene.isLoaded ? " (Loaded, " : " (Not Loaded, ";
            //output += scene.isDirty ? "Dirty, " : "Clean, ";
            //output += scene.buildIndex >= 0 ? " in build)\n" : " NOT in build)\n";
            if (scene.name == _loadingSceneName && scene.isLoaded)
            {
                loaded = true;
                _activeUI = false;
            }
        }

        return loaded;
        
    }
    public void LoadScene(string sceneName, Sprite background, string tips)
    {
        _activeUI = true;
        _loadingSceneName = sceneName;
        _loadingSceneSprite = background;
        _loadingTips = tips;
        // kick-off the one co-routine to rule them all
        StartCoroutine(LoadScenesInOrder(sceneName));
    }

    private IEnumerator LoadScenesInOrder(string sceneName)
    {
        // LoadSceneAsync() returns an AsyncOperation, 
        // so will only continue past this point when the Operation has finished
        //yield return SceneManager.LoadSceneAsync("Loading");

        // as soon as we've finished loading the loading screen, start loading the game scene
        yield return StartCoroutine(_LoadScene(sceneName));
    }

    private IEnumerator _LoadScene(string sceneName)
    {
        var asyncScene = SceneManager.LoadSceneAsync(sceneName);
        //var asyncScene = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        // this value stops the scene from displaying when it's finished loading
        asyncScene.allowSceneActivation = false;

        while (!asyncScene.isDone)
        {
            // loading bar progress
            _loadingProgress = Mathf.Clamp01(asyncScene.progress / 0.9f) * 100;

            // scene has loaded as much as possible, the last 10% can't be multi-threaded
            if (asyncScene.progress >= 0.9f)
            {

                // Wait for the specified delay
                yield return new WaitForSeconds(1); 
                // we finally show the scene
                asyncScene.allowSceneActivation = true;

                Tools_UI_Tools.FadeInScreen(2f);
            }

            yield return null;
        }
    }
}
