using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public float zoomSpeed = 1.0f;
    public float minZoom = 5.0f;
    public float maxZoom = 20.0f;
    public float smoothTime = 0.3f;

    private float zoomVelocity;

    void Update()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        float zoomAmount = scrollInput * zoomSpeed;
        float newZoom = Mathf.Clamp(Camera.main.fieldOfView - zoomAmount, minZoom, maxZoom);

        Camera.main.fieldOfView = Mathf.SmoothDamp(Camera.main.fieldOfView, newZoom, ref zoomVelocity, smoothTime);
    }
}
