using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinController : MonoBehaviour
{

    Vector3 lastMousePos;

    void OnMouseDrag()
    {
        if (Input.mousePosition != lastMousePos)
        {
            Vector3 diffVector = Input.mousePosition - lastMousePos;
            float amount = diffVector.y;
            transform.Rotate(new Vector3(0f, amount, 0f));
            lastMousePos = Input.mousePosition;
        }
    }
}
