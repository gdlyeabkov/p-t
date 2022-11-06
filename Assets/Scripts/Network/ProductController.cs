using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductController : MonoBehaviour
{

    public NetworkController networkController;
    public int index;
    public int price;

    public void OnMouseUpAsButton()
    {
        networkController.SelectProduct(index);
    }

}
