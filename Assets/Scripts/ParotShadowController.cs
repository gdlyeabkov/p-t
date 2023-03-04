using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ParotShadowController : MonoBehaviour
{

    public GameManager gameManager;
    public bool isFly = false;
    public float speed = 3f;
    public float delta = 0f;

    public void Fly()
    {

        /*
        NavMeshHit hit;
        if (GetComponent<NavMeshAgent>().Raycast(Vector3.down, out hit))
        {
            Vector3 hitPosition = hit.position;
            float hitPositionHeight = hitPosition.y;
            Vector3 transformPosition = transform.position;
            float transformPositionHeight = transformPosition.y;
            delta = transformPositionHeight - hitPositionHeight;
            isFly = true;
        }*/
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
            float hitPositionHeight = target.y;
            // Vector3 transformPosition = transform.position;
            Vector3 transformPosition = gameManager.localPirate.gameObject.transform.position;
            float transformPositionHeight = transformPosition.y;
            delta = transformPositionHeight - hitPositionHeight;
            isFly = true;
            StartCoroutine(SetFlatShadow());
        }
    }

    void Update()
    {
        if (isFly)
        {
            if (gameManager != null)
            {
                Vector3 target = gameManager.shovel.transform.position;
                // Quaternion targetRotation = gameManager.shovel.transform.rotation;
                if (gameManager.treasureInst != null)
                {
                    target = gameManager.treasureInst.transform.position;
                    // targetRotation = gameManager.treasureInst.transform.rotation;
                }
                else
                {
                    target = gameManager.shovel.transform.position;
                    // targetRotation = gameManager.shovel.transform.rotation;
                }
                float step = speed * Time.deltaTime;
                // Vector3 targetPosition = new Vector3(target.x, target.y, target.z);
                // Vector3 targetPosition = new Vector3(target.x, target.y + 1.5f, target.z);
                Vector3 targetPosition = new Vector3(target.x, target.y + delta, target.z);
                // Vector3 targetPosition = new Vector3(target.x, 25f + delta, target.z);
                // Vector3 targetPosition = new Vector3(target.x, delta, target.z);
                // transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
                // transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
                // Vector3 parotShadowRotationAxes = new Vector3(1f, 0f, 1f);
                // transform.RotateAround(gameManager.islandSphereTransform.position, parotShadowRotationAxes, 2 * Time.deltaTime);
                // transform.RotateAround(gameManager.islandSphereTransform.position, parotShadowRotationAxes, Vector3.Distance(transform.position, target) * Time.deltaTime);
                // transform.RotateAround(gameManager.islandSphereTransform.position, new Vector3(1f, 0f, 0f), Mathf.Clamp(transform.position.x - target.x * Time.deltaTime, -1f, 1f));
                // transform.RotateAround(gameManager.islandSphereTransform.position, new Vector3(0f, 0f, 1f), Mathf.Clamp(transform.position.z - target.z * Time.deltaTime, -1f, 1f));
                /*var targetRotation = Quaternion.LookRotation(targetPosition - transform.position);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, speed * Time.smoothDeltaTime);*/
                bool isFounded = Vector3.Distance(transform.position, target) <= 1f;
                if (isFounded)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    public void OnTriggerEnter (Collider other)
    {
        if (gameManager != null)
        {
            if (gameManager.treasureInst != null)
            {
                Transform target = gameManager.shovel.transform;
                if (gameManager.treasureInst != null)
                {
                    target = gameManager.treasureInst.transform;
                }
                else
                {
                    target = gameManager.shovel.transform;
                }
                if (target == other.gameObject.transform && other.gameObject.tag == "Treasure" || other.gameObject.tag == "Shovel")
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    public IEnumerator SetFlatShadow()
    {
        yield return new WaitForSeconds(2f);
        GetComponent<Projector>().orthographic = true;
        GetComponent<Projector>().orthographicSize = 0.7f;
    }

}
