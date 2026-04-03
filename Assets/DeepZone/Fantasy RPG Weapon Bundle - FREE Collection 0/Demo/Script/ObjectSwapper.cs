using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectSwapper : MonoBehaviour
{
    public List<GameObject> gameObjects;
    public float changeInterval = 60.0f;
    private float timer;
    private int currentIndex;

    void Start()
    {
        timer = changeInterval;
        currentIndex = 0;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            // Deactivate the current game object
            gameObjects[currentIndex].SetActive(false);

            // Move to the next game object
            currentIndex++;

            // If we've reached the end of the list, stop changing objects
            if (currentIndex >= gameObjects.Count)
            {
                enabled = false;
                return;
            }

            // Activate the next game object
            gameObjects[currentIndex].SetActive(true);

            // Reset the timer
            timer = changeInterval;
        }
    }
}
