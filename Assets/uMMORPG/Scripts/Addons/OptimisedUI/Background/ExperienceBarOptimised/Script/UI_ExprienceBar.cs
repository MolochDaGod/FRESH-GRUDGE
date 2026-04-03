using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.Events;

public class UI_ExprienceBar : MonoBehaviour
{
    public GameObject panel;
    public Slider slider;
    public TMP_Text statusText;
    /*public static UI_ExprienceBar Instance { get; private set; }
    public UnityEvent onPlayerExperienceChange;*/
    public float refreshSpeed = 0.3f;

    private float _playerExperience = 0;
    private int _playerLevel = 0;

    public void Start()
    {
        StartCoroutine(nameof(CR_ExprienceBar));
    }

    private IEnumerator CR_ExprienceBar()
    {
        while (true)
        {

            Player player = Player.localPlayer;
            if (player)
            {
                panel.SetActive(true);
                if (_playerExperience != player.experience.Percent() || _playerLevel != player.level.current)
                {
                    slider.value = player.experience.Percent();
                    statusText.text = "Lv." + player.level.current + " (" + (player.experience.Percent() * 100).ToString("F2") + "%)";
                    _playerExperience = player.experience.Percent();
                    _playerLevel = player.level.current;
                }
            }
            else panel.SetActive(false);

            yield return new WaitForSeconds(refreshSpeed);
        }
    }
    //TODO faut modifier Experience de la même maniere que Energy afin de passer par des event pour changer l'UI
    /*
    private void OnEnable()
    {
        UpdateEvent();
    }
    public void UpdateEvent()
    {
        player player = player.localPlayer;
        if (player)
        {
            panel.SetActive(true);
            //if (_playerExperience != player.experience.Percent() || _playerLevel != player.level.current)
            //{
                slider.value = player.experience.Percent();
                statusText.text = "Level : " + player.level.current + " (" + (player.experience.Percent() * 100).ToString("F2") + "%)";
                _playerExperience = player.experience.Percent();
                _playerLevel = player.level.current;
            //}
        }
        else panel.SetActive(false);
    }*/
}