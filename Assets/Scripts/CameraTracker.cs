using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTracker : MonoBehaviour
{

    // camera will follow this object
    public Transform Target;
    //camera transform
    public Transform camTransform;
    // offset between camera and target
    public Vector3 Offset;
    // change this value to get desired smoothness
    public float SmoothTime = 0.3f;
    // This value will change at the runtime depending on target movement. Initialize with zero vector.
    private Vector3 velocity = Vector3.zero;
    public GameManager gameManager;

    private void Start()
    {
        if (Target != null)
        {
            Offset = camTransform.position - Target.position;
        }
    }

    private void LateUpdate()
    {
        // update position
        if (Target != null)
        {
            Offset = new Vector3(0f, 7.5f, -10.0f);
            Vector3 targetPosition = Target.position + Offset;
            camTransform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, SmoothTime);
        }
    }

}
