using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompasController : MonoBehaviour
{

    public GameManager gameManager;

    void Update()
    {
        if (gameManager.isInit)
        {
            // transform.Rotate(new Vector3(0f, gameManager.localPirate.transform.localEulerAngles.y, 0f));
            // transform.RotateAround(gameManager.islandSphereTransform.position, Vector3.up, gameManager.rotationJoystick.Horizontal);
            float mouseXDelta = gameManager.rotationJoystick.Horizontal / 5;
            float yawDelta = 10f * mouseXDelta;
            transform.RotateAround(gameManager.islandSphereTransform.position, Vector3.up, yawDelta);
        }
    }
}
