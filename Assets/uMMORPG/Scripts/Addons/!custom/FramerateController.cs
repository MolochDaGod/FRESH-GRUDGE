using UnityEngine;

public class FramerateController : MonoBehaviour
{
    public int clientFramerate = 30; // Target framerate for client builds  
    public int serverFramerate = 60; // Target framerate for server builds

    void Start()
    {
        SetTargetFramerate();
    }

    void SetTargetFramerate()
    {
#if UNITY_SERVER  
        Application.targetFrameRate = serverFramerate; // Set to server framerate  
        Debug.Log("Running in Server mode. Target framerate: " + serverFramerate);
#else  
        Application.targetFrameRate = clientFramerate; // Set to client framerate  
        Debug.Log("Running in Client mode. Target framerate: " + clientFramerate);
#endif  
    }
}