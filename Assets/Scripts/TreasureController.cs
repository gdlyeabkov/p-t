using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureController : MonoBehaviour
{
    
    void OnCollisionEnter(Collision other)
    {
        GameObject detectedObject = other.gameObject;
        string objectTag = detectedObject.tag;
        bool isIsland = objectTag == "Island";
        if (isIsland)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        GameObject detectedObject = other.gameObject;
        string objectTag = detectedObject.tag;
        bool isPlayer = objectTag == "Player";
        if (isPlayer)
        {
            
        }
    }

}
