using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLabelController : MonoBehaviour
{

    void LateUpdate()
    {
        Camera mainCamera = Camera.main;
        transform.LookAt(mainCamera.transform, Vector3.up);
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + 180f, transform.eulerAngles.z);
    }

}
