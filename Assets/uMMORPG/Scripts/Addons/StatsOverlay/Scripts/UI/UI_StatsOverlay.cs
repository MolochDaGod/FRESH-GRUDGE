using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_StatsOverlay : MonoBehaviour
{
    public GameObject show;
    public GameObject healthCanvas;
    public Health health;
    public Slider sliderHealth;

    private float currentHealth;
    public float refreshSpeed = 0.12f;
    // Update is called once per frame


    // LateUpdate so that all camera updates are finished.
    private void LateUpdate()
    {
        transform.forward = Camera.main.transform.forward;
    }

    public void Start()
    {
        StartCoroutine("CR_StatsOverlay");
    }

    private IEnumerator CR_StatsOverlay()
    {
        while (true)
        {

            Player player = Player.localPlayer;
            if (player)
            {
                healthCanvas.SetActive((health.current == 0 || currentHealth == health.max) ? false : true);
                SetHealth(health.current);
            }
            else healthCanvas.SetActive(false);

            yield return new WaitForSeconds(refreshSpeed);
        }
    }
    // Sets up our visible overlay for health.
    private void SetHealth(float amount)
    {
        //Check for not calcul if not necessary
        if (amount != currentHealth)
        {
            currentHealth = amount;
            float displayedHealth = currentHealth / health.max;
            sliderHealth.value = displayedHealth;
        }
    }
}
