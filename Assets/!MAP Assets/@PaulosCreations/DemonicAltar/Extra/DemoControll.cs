using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoControll : MonoBehaviour
{

    public Transform altarTF;
    public Light mainLight;
    public DemonicAltarController altarScript;

    private float lightMaxIntens, lightTargetIntens, timer;
    private bool altarStarted, fadeInLight, altarStopped;

    // Use this for initialization
    void Start()
    {
        lightMaxIntens = mainLight.intensity;
        //mainLight.intensity = 0;
        lightTargetIntens = lightMaxIntens;
    }

    // Update is called once per frame
    void Update()
    {
        altarTF.Rotate(Vector3.up, 10 * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.A))
        {
            altarScript.ActivateDeactivateAltar();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            if (lightTargetIntens != 0)
                lightTargetIntens = 0;
            else lightTargetIntens = lightMaxIntens;
        }

        mainLight.intensity = Mathf.MoveTowards(mainLight.intensity, lightTargetIntens, Time.deltaTime * 0.15f);
    }

    public void ButtonToggleAltar()
    {
        altarScript.ActivateDeactivateAltar();
    }

    public void ButtonToggleLight()
    {
        if (lightTargetIntens != 0)
            lightTargetIntens = 0;
        else lightTargetIntens = lightMaxIntens;
    }
}
