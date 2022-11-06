using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinController : MonoBehaviour
{

    Vector3 lastMousePos;

    void OnMouseDrag()
    {
        Vector3 mousePos = Input.mousePosition;
        bool isChange = mousePos != lastMousePos;
        if (isChange)
        {
            // Vector3 diffVector = mousePos - lastMousePos;
            Vector3 diffVector = lastMousePos - mousePos;
            float amount = diffVector.x;
            transform.Rotate(new Vector3(0f, amount, 0f));
            lastMousePos = mousePos;
        }
    }
}
