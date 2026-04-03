using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    public GameObject[] gameObjects; // Array of game objects to manage

    // Call this method from outside to activate a specific GameObject  
    public void ActivateGameObject(GameObject objToActivate)
    {
        foreach (GameObject obj in gameObjects)
        {
            if (obj == objToActivate)
            {
                obj.SetActive(true); // Activate the selected object  
            }
            else
            {
                obj.SetActive(false); // Disable all other objects  
            }
        }
    }
}
