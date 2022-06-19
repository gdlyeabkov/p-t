using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Monetization;
using UnityEngine.Advertisements;
using UnityEngine.EventSystems;

public class AdController : MonoBehaviour
{
    
    private string gameId = "4173107";
    public bool testingMode = true;
    public GameManager gameManager;

    void Start()
    {
        if (Monetization.isSupported)
        {
            Monetization.Initialize(gameId, testingMode);
            if (PlayerPrefs.HasKey("ShowAd"))
            {
                ShowAd();
            }
        }
    }

    public void ShowAd()
    {
        if (Monetization.IsReady("rewardedVideo"))
        {
            ShowAdCallbacks value = new ShowAdCallbacks();
            value.finishCallback = new UnityEngine.Monetization.ShowAdFinishCallback(HandleShowResult);
            (Monetization.GetPlacementContent("rewardedVideo") as ShowAdPlacementContent).Show(new ShowAdCallbacks?(value));
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (Monetization.IsReady("rewardedVideo"))
        {
            ShowAdCallbacks value = new ShowAdCallbacks();
            value.finishCallback = new UnityEngine.Monetization.ShowAdFinishCallback(HandleShowResult);
            (Monetization.GetPlacementContent("rewardedVideo") as ShowAdPlacementContent).Show(new ShowAdCallbacks?(value));
        }
    }

    private void HandleShowResult(UnityEngine.Monetization.ShowResult result)
    {
        UnityEngine.Monetization.ShowResult adFinished = UnityEngine.Monetization.ShowResult.Finished;
        bool isAdFinished = result == adFinished;
        if (isAdFinished)
        {
            this.Reclaim();
        }
    }

    private void Reclaim()
    {
        PlayerPrefs.DeleteKey("ShowAd");
    }

    public void ScheduleAd()
    {
        PlayerPrefs.SetInt("ShowAd", 1);
    }

}
