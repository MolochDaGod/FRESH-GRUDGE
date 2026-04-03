using UnityEngine;
using System.Collections;

public class MaterialChangeTB : MonoBehaviour
{
    public Material[] mats;
    public GameObject dragonTerrorBringer;



    void Awake()
    {
        dragonTerrorBringer.GetComponent<Renderer>().material = mats[0];
    }





    public void MatChangeToBlue()
    {
        dragonTerrorBringer.GetComponent<Renderer>().material = mats[0];
    }

    public void MatChangeToRed()
    {
        dragonTerrorBringer.GetComponent<Renderer>().material = mats[1];
    }

    public void MatChangeToGreen()
    {
        dragonTerrorBringer.GetComponent<Renderer>().material = mats[2];
    }

    public void MatChangeToPurple()
    {
        dragonTerrorBringer.GetComponent<Renderer>().material = mats[3];
    }

}
