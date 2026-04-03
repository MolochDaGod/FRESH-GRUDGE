using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenControl : MonoBehaviour
{
    public Button clickToStart;

    [Header("[-=-[ Force auto start ]-=-]")]
    public bool enableStartAutoload = false;

    [Header("[-=-[ Slider ]-=-]")]
    public GameObject loadingScreen;
    public Slider slider;
    public Image sliderFill;

    [Header("[-=-[ Text ]-=-]")]
    public TMP_Text percentLoad;
    public TMP_Text showWait;

    [Space(20)]
    [Header("[-=-[ Scene for load ]-=-]")]
    [SerializeField] private MethodLoading methodLoading;
    public UnityScene unityScene;
    public int sceneId;
    public string sceneName;

    public void Start()
    {
        if (enableStartAutoload)
            StartCoroutine(RunLoad());
        else if (clickToStart != null)
        {
            clickToStart.onClick.RemoveListener(LoadScreen);
            clickToStart.onClick.AddListener(LoadScreen);
        }
    }

    public void LoadScreen()
    {
        if (clickToStart != null)
            clickToStart.gameObject.SetActive(false);
        StartCoroutine(RunLoad());
    }

    // Single unified coroutine — replaces three near-identical copies
    private IEnumerator RunLoad()
    {
        yield return new WaitForSeconds(1f);

        AsyncOperation op;
        switch (methodLoading)
        {
            case MethodLoading.sceneid:
                op = SceneManager.LoadSceneAsync(sceneId);
                break;
            case MethodLoading.sceneName:
                op = SceneManager.LoadSceneAsync(sceneName);
                break;
            default: // UnityScene
                op = SceneManager.LoadSceneAsync(unityScene.SceneName);
                break;
        }

        if (op == null)
        {
            Debug.LogError("[LoadingScreenControl] LoadSceneAsync returned null — check scene name/id is in Build Settings.");
            yield break;
        }

        if (loadingScreen != null) loadingScreen.SetActive(true);
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            UpdateUI(op.progress);
            if (op.progress >= 0.9f)
                op.allowSceneActivation = true;
            yield return null;
        }
    }

    private void UpdateUI(float loadValue)
    {
        float pct = Mathf.Clamp01(loadValue / 0.9f) * 100f;

        if (loadValue >= 0.9f)
        {
            if (slider != null)     slider.value = 1f;
            if (percentLoad != null) percentLoad.gameObject.SetActive(false);
            if (showWait != null)   showWait.gameObject.SetActive(true);
        }
        else
        {
            if (slider != null)     slider.value = loadValue;
            if (percentLoad != null) percentLoad.text = pct.ToString("f0") + "%";
        }
    }

    enum MethodLoading
    {
        UnityScene, sceneid, sceneName
    }
}