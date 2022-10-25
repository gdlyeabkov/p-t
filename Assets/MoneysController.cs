using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoneysController : MonoBehaviour
{

    public Text moneysLabel;

    void Start()
    {
        bool isMoneysExists = PlayerPrefs.HasKey("Moneys");
        bool isNotMoneys = !isMoneysExists;
        if (isNotMoneys)
        {
            PlayerPrefs.SetInt("Moneys", 0);
        }
        int moneys = PlayerPrefs.GetInt("Moneys");
        string parsedMoneys = moneys.ToString();
        Debug.Log("parsedMoneys: " + parsedMoneys);
        moneysLabel.text = parsedMoneys;
    }

}
