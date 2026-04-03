using UnityEngine;
using System.Collections;

public class CameraControlTB : MonoBehaviour
{
    public Animator anim;
    int cameraGoUp;
    int cameraGoDown;

    void Start()
    {
        anim = GetComponent<Animator>();
        cameraGoUp = Animator.StringToHash("CameraGoUp");
        cameraGoDown = Animator.StringToHash("CameraGoDown");
    }

    public void CameraGoUp()
    {
        anim.SetTrigger(cameraGoUp);
    }

    public void CameraGoDown()
    {
        anim.SetTrigger(cameraGoDown);
    }

}
