using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoneysController : MonoBehaviour
{

    public Text moneysLabel;
    public NetworkController networkController;

    void Start()
    {
        bool isMoneysExists = PlayerPrefs.HasKey("Moneys");
        bool isNotMoneys = !isMoneysExists;
        if (isNotMoneys)
        {
            if (networkController.isDebug)
            {
                PlayerPrefs.SetInt("Moneys", 150);
            }
            else
            {
                PlayerPrefs.SetInt("Moneys", 0);
            }
        }
        GetMoneys();
    }

    public void GetMoneys ()
    {
        int moneys = PlayerPrefs.GetInt("Moneys");
        string parsedMoneys = moneys.ToString();
        moneysLabel.text = parsedMoneys;
    }

}
