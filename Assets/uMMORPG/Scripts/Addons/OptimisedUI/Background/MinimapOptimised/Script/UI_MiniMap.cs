using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public partial class UI_MiniMap : MonoBehaviour
{
    public GameObject panel;
    public float zoomMin = 5;
    public float zoomMax = 50;
    public float zoomStepSize = 5;
    public Text sceneText;
    public Button plusButton;
    public Button minusButton;
    public Camera minimapCamera;

    public float refreshSpeed = .5f;


    public void Start()
    {
        plusButton.onClick.SetListener(() => {
            minimapCamera.orthographicSize = Mathf.Max(minimapCamera.orthographicSize - zoomStepSize, zoomMin);
        });
        minusButton.onClick.SetListener(() => {
            minimapCamera.orthographicSize = Mathf.Min(minimapCamera.orthographicSize + zoomStepSize, zoomMax);
        });
        StartCoroutine(CR_MiniMap());
    }

    private IEnumerator CR_MiniMap()
    {
        while (true)
        {

            Player player = Player.localPlayer;
            if (player != null)
            {
                panel.SetActive(true);
                sceneText.text = SceneManager.GetActiveScene().name;
            }
            else panel.SetActive(false);

            yield return new WaitForSeconds(refreshSpeed);

        }
    }
}