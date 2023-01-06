using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParotShadowController : MonoBehaviour
{

    public GameManager gameManager;
    public bool isFly = false;
    public float speed = 1f;

    public void Fly()
    {
        isFly = true;
    }

    void Update()
    {
        if (isFly)
        {
            if (gameManager != null)
            {
                Vector3 target = gameManager.shovel.transform.position;
                if (gameManager.treasureInst != null)
                {
                    target = gameManager.treasureInst.transform.position;
                }
                else
                {
                    target = gameManager.shovel.transform.position;
                }
                float step = speed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, target, step);
            }
        }
    }

}
