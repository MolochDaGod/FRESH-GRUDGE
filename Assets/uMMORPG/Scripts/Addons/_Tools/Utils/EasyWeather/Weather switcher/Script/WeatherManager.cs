using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    public Material SkyBox;

    public GameObject defaultSun;

    [Header("[-=-=[ Zone Sun Parameter ]-=-=-]")]
    public GameObject zoneSun;
    public Color sunColor = Color.yellow;
    [Range(0,3)] public float sunintensity = 1;


    [Header("[-=-=-[ Wheather ]-=-=-]")]
    public GameObject Weather;

    [Header("[-=-=-[ Fog Parameter ]-=-=-]")]
    public bool enebleFog = true;
    [Range(0,1)] public float density = 0.06f;
    public FogMode fogMode = FogMode.Exponential;
    public Color fogColor;

    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null && player.isLocalPlayer)
        {
            Debug.Log("Local player entered the trigger");
            RenderSettings.skybox = SkyBox;
            DynamicGI.UpdateEnvironment(); // Ensure the skybox updates visually
            defaultSun.SetActive(false);
            zoneSun.SetActive(true);
            zoneSun.GetComponent<Light>().intensity = sunintensity;
            zoneSun.GetComponent<Light>().color = sunColor;

            if (Weather != null)
                Weather.SetActive(true);

            if (enebleFog)
            {
                RenderSettings.fog = true;
                RenderSettings.fogDensity = density;
                RenderSettings.fogColor = fogColor;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null && player.isLocalPlayer)
        {

            if (Weather != null)
                Weather.SetActive(false);
        }
    }
}