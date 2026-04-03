using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossManager : NetworkBehaviour
{
    public Monster boss1;
    public Monster boss2;
    public Monster boss3;
    public Monster boss4;

    public GameObject port1;
    public GameObject port2;
    public GameObject port3;
    public GameObject port4;


    [SyncVar]
    bool portal1Active, portal2Active, portal3Active, portal4Active;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isServer)
        {
            if (boss1.health.current > 0)
            {
                portal1Active = true;
            }
            else
                portal1Active = false;
            if (boss2.health.current > 0)
            {
                portal2Active = true;
            }
            else
                portal2Active = false;
            if (boss3.health.current > 0)
            {
                portal3Active = true;
            }
            else
                portal3Active = false;
            if (boss4.health.current > 0)
            {
                portal4Active = true;
            }
            else
                portal4Active = false;
        }
        if (portal1Active)
            port1.SetActive(true);
        else
            port1.SetActive(false);
        if (portal2Active)
            port2.SetActive(true);
        else
            port2.SetActive(false);
        if (portal3Active)
            port3.SetActive(true);
        else
            port3.SetActive(false);
        if (portal4Active)
            port4.SetActive(true);
        else
            port4.SetActive(false);

    }
}
