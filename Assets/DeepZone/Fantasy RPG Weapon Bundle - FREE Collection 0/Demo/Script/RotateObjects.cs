using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObjects : MonoBehaviour
{
    public List<GameObject> objectsToSpin;
    public float spinSpeed = 10.0f;

    void Update()
    {
        foreach (GameObject obj in objectsToSpin)
        {
            obj.transform.Rotate(0, spinSpeed * Time.deltaTime, 0);
        }
    }
}
