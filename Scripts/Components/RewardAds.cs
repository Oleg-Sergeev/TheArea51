using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class RewardAds : MonoBehaviour, IUnityAdsListener
{
    public static Dictionary<string, RewardedButton> RewardedButtons { get; private set; }
    private const string GAME_ID = "3370763";

    private void Awake() => RewardedButtons = new Dictionary<string, RewardedButton>();

    private void Start()
    {
        foreach (var b in RewardedButtons)
        {
            if (!b.Value) continue;

            b.Value.Button.interactable = Advertisement.IsReady(b.Key);
            b.Value.Button.onClick.AddListener(() => ShowRewardedVideo(b.Key));
        }

        Advertisement.AddListener(this);
        Advertisement.Initialize(GAME_ID, Application.isEditor);

        MyDebug.Log(Application.isEditor);

        CheckAdReady();
    }

    private async void CheckAdReady()
    {
        while (Application.isPlaying)
        {
            foreach (var b in RewardedButtons)
            {
                if (b.Value) b.Value.Button.interactable = Advertisement.IsReady(b.Key);
            }

            await System.Threading.Tasks.Task.Delay(5000);
        }
    }

    private void ShowRewardedVideo(string placementId)
    {
        Advertisement.Show(placementId);
    }


    public void OnUnityAdsReady(string placementId)
    {
        if (!RewardedButtons.ContainsKey(placementId)) return;

        RewardedButtons[placementId].Button.interactable = true;
    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        if (showResult == ShowResult.Finished)
        {
            EventManager.eventManager.FinishAd(RewardedButtons[placementId].rewardBonus);
        }
        else if (showResult == ShowResult.Skipped)
        {
            MyDebug.Log("Skipped");
        }
        else if (showResult == ShowResult.Failed)
        {
            MyDebug.LogWarning("The ad did not finish due to an error.");
        }
    }

    public void OnUnityAdsDidError(string message)
    {
        MyDebug.LogError($"*** Error: {message} ***");
    }

    public void OnUnityAdsDidStart(string placementId){}
}
