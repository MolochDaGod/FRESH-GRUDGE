using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplayLoadingProgressPanel : MonoBehaviour
{

    [Header("Scene manager")]
    //public ScriptableListScene sceneLoader;
    //public GameObject loadingzScreenPanel;
    public GameObject panel;
    //private float _slider;
    public Slider slider;
    public Image background; // pour modifier l'image en fonction de la scene ou l'on va (la scene doit être ajouté au scriptableScene)
    public GameObject showPercent; // pour desactivé le gameobject
    public string _percentLoad; // affiche le pourcentage dans la barre de progression (50%, 55% ...)
    public Text percentLoad; // affiche le pourcentage dans la barre de progression (50%, 55% ...)
    public GameObject showWait; // affiche l'object contenant un texte (veuillez patienté)
    public TMP_Text tips;

    /**
     * assignation des valeur
     */
    private float _sliderValue;
    private string _percentValue;
     private string _sceneName;
 
    void Start()
    {
        //int sceneCount = sceneLoader.sceneList;
       /* foreach (var item in sceneLoader.sceneList)
        {
            background.sprite = item.background;
        }*/

    }

    // Update is called once per frame
    void Update()
    {
        bool updateActivePanel = ServerSceneLoader.Instance.LoadedScene;
        bool ActivePanel = ServerSceneLoader.Instance.LoadingUIActive;
        background.sprite = ServerSceneLoader.Instance.LoadingSceneSprite;
        tips.text = ServerSceneLoader.Instance.LoadingTips;
        
        if (ActivePanel)
        {
            assignValue();
            UpdateUIValue();
        }
        panel.SetActive(ActivePanel);
       /* Debug.Log("slider value : " + _sliderValue);
        Debug.Log("pourcent load : " + _percentValue);
        Debug.Log("scene name : " + _sceneName);
        Debug.Log("panel active : " + ActivePanel.ToString());*/
    }

    private void assignValue()
    {
        _sliderValue = ServerSceneLoader.Instance.LoadingProgress;
        _percentValue = ServerSceneLoader.Instance.LoadingProgress.ToString("F0") + "%";
        _sceneName = ServerSceneLoader.Instance.LoadingSceneName;
    }

    private void UpdateUIValue()
    {
        if(_sliderValue == 100)
        {
            showWait.SetActive(true);
            showPercent.SetActive(false);
        }
        /*foreach (var item in sceneLoader.sceneList)
        {
            if(item.sceneName == _sceneName) background.sprite = item.background;
        }*/
        //sceneLoader.sceneList.
        //background.sprite = sceneLoader.sceneList.
        slider.value = _sliderValue;
        percentLoad.text = _percentValue;
    }
}
