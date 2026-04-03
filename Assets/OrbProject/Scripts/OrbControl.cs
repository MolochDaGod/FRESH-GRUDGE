using UnityEngine;
using System.Collections;

public class OrbControl : MonoBehaviour
{

    public GameObject[] OrbObject;
    public GameObject[] Text;
    int change = 0;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            change += 1;

        }

        if (change >= 10)
            change = 0;

        switch (change)
        {
            case 0:
                Text[9].SetActive(true);
                OrbObject[8].SetActive(false);
                Text[8].SetActive(false);
                break;
            case 1:
                Text[9].SetActive(false);
                OrbObject[0].SetActive(true);
                Text[0].SetActive(true);
                break;
            case 2:
                OrbObject[0].SetActive(false);
                Text[0].SetActive(false);
                OrbObject[1].SetActive(true);
                Text[1].SetActive(true);
                break;
            case 3:
                OrbObject[1].SetActive(false);
                Text[1].SetActive(false);
                OrbObject[2].SetActive(true);
                Text[2].SetActive(true);
                break;
            case 4:
                OrbObject[2].SetActive(false);
                Text[2].SetActive(false);
                OrbObject[3].SetActive(true);
                Text[3].SetActive(true);
                break;
            case 5:
                OrbObject[3].SetActive(false);
                Text[3].SetActive(false);
                OrbObject[4].SetActive(true);
                Text[4].SetActive(true);
                break;
            case 6:
                OrbObject[4].SetActive(false);
                Text[4].SetActive(false);
                OrbObject[5].SetActive(true);
                Text[5].SetActive(true);
                break;
            case 7:
                OrbObject[5].SetActive(false);
                Text[5].SetActive(false);
                OrbObject[6].SetActive(true);
                Text[6].SetActive(true);
                break;
            case 8:
                OrbObject[6].SetActive(false);
                Text[6].SetActive(false);
                OrbObject[7].SetActive(true);
                Text[7].SetActive(true);
                break;
            case 9:
                OrbObject[7].SetActive(false);
                Text[7].SetActive(false);
                OrbObject[8].SetActive(true);
                Text[8].SetActive(true);
                break;

        }

    }
}
